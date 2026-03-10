/*
 * RiverGenerator.cs
 * Generates river paths on the world map using greedy descent.
 *
 * Uses the HeightValues from WorldMap to simulate water flowing downhill.
 * Sources are found by dividing the map into zones and picking the highest
 * point in each zone, ensuring even spatial distribution across the map.
 *
 * KEY MEMBERS:
 *      - RiverGenerator(WorldMap map, int seed) - constructor
 *      - List<List<(int x, int y)>> GenerateRivers(int zoneCount) - generates one river per zone
 *      - private (int x, int y) FindBestSourceInZone(int x1, int y1, int x2, int y2)
 *      - private List<(int x, int y)> TraceRiver(int startX, int startY)
 *      - private List<(int x, int y)> TraceRiverUntil(int startX, int startY, HashSet<(int,int)>? stopSet)
 *      - private List<List<(int x, int y)>> GenerateTributaries(List<(int x, int y)> mainRiver)
 *      - private (int x, int y) FindEscape(int stuckX, int stuckY, HashSet<(int,int)> globalSeen)
 *      - private List<(int x, int y)> GetNeighbors(int x, int y)
 *      - private float Align(float dx1, float dy1, float dx2, float dy2)
 */

namespace WorldMapGenerator
{
	public class RiverGenerator
	{
		private readonly WorldMap _map;
		private readonly Random _rng;

		public RiverGenerator(WorldMap map, int seed)
		{
			_map = map;
			_rng = new Random(seed);
		}

		// Main entry point — splits map into a grid and generates one river per zone
		public List<List<(int x, int y)>> GenerateRivers(int zoneCount = 4)
		{
			var allRivers = new List<List<(int x, int y)>>();

			int zoneWidth = _map.Width / zoneCount;
			int zoneHeight = _map.Height / zoneCount;

			for (int zx = 0; zx < zoneCount; zx++)
			{
				for (int zy = 0; zy < zoneCount; zy++)
				{
					int x1 = zx * zoneWidth;
					int y1 = zy * zoneHeight;
					int x2 = x1 + zoneWidth;
					int y2 = y1 + zoneHeight;

					var (sx, sy) = FindBestSourceInZone(x1, y1, x2, y2);

					// Skip zones whose best tile is water or beach — nothing to flow from
					var sourceTerrain = _map.GetTerrainAt(sx, sy);
					if (sourceTerrain == TerrainType.DeepOcean ||
						sourceTerrain == TerrainType.ShallowWater ||
						sourceTerrain == TerrainType.Beach)
						continue;

					var mainRiver = TraceRiver(sx, sy);
					if (mainRiver.Count <= 20) continue;

					allRivers.Add(mainRiver);

					// Trace tributaries that flow into this main river
					var tributaries = GenerateTributaries(mainRiver);
					allRivers.AddRange(tributaries);
				}
			}

			return allRivers;
		}

		// Scans every tile in the zone and returns the coordinates of the highest one
		private (int x, int y) FindBestSourceInZone(int x1, int y1, int x2, int y2)
		{
			(int x, int y) best = (x1, y1);
			float bestHeight = float.MinValue;

			for (int x = x1; x < x2; x++)
			{
				for (int y = y1; y < y2; y++)
				{
					float h = _map.GetHeightAt(x, y);
					if (h > bestHeight)
					{
						bestHeight = h;
						best = (x, y);
					}
				}
			}

			return best;
		}

		// Finds confluence points along an existing river and traces tributaries into them
		private List<List<(int x, int y)>> GenerateTributaries(List<(int x, int y)> mainRiver)
		{
			var tributaries = new List<List<(int x, int y)>>();
			var mainRiverSet = new HashSet<(int, int)>(mainRiver);

			// Walk the main river skipping the first and last 20 tiles (source and mouth)
			for (int i = 20; i < mainRiver.Count - 20; i++)
			{
				var (rx, ry) = mainRiver[i];

				// Only consider every 15th tile as a potential confluence
				if (i % 15 != 0) continue;

				(int x, int y) tributarySource = (-1, -1);
				float bestHeight = _map.GetHeightAt(rx, ry) + 0.05f; // Must be meaningfully uphill

				// Search an 8-tile radius around this confluence point for a high source
				for (int dx = -8; dx <= 8; dx++)
				{
					for (int dy = -8; dy <= 8; dy++)
					{
						int nx = rx + dx;
						int ny = ry + dy;

						if (nx < 0 || nx >= _map.Width || ny < 0 || ny >= _map.Height) continue;
						if (mainRiverSet.Contains((nx, ny))) continue;

						// Any non-water tile can feed a tributary
						var terrain = _map.GetTerrainAt(nx, ny);
						if (terrain == TerrainType.DeepOcean ||
							terrain == TerrainType.ShallowWater ||
							terrain == TerrainType.Beach) continue;

						float h = _map.GetHeightAt(nx, ny);
						if (h > bestHeight) { bestHeight = h; tributarySource = (nx, ny); }
					}
				}

				if (tributarySource == (-1, -1)) continue;

				var tributary = TraceRiverUntil(tributarySource.x, tributarySource.y, mainRiverSet);
				if (tributary.Count > 10)
					tributaries.Add(tributary);
			}

			return tributaries;
		}

		private List<(int x, int y)> TraceRiver(int startX, int startY)
		{
			return TraceRiverUntil(startX, startY, null);
		}

		private List<(int x, int y)> TraceRiverUntil(int startX, int startY, HashSet<(int, int)>? stopSet)
		{
			const int maxSteps = 10000;
			const float momentumWeight = 0.075f; // Resistance to sharp turns — higher = smoother curves
			const float jitter = 0.075f; // Random nudge to break ties on flat ground
			const int recentWindow = 150;    // How many recent steps count as "expensive" to revisit
			const float revisitPenalty = 0.5f;   // Cost added for stepping onto a recent tile
			const float deadEndPenalty = 0.4f;   // Extra cost for stepping into a near-dead-end

			var path = new List<(int x, int y)>();
			var globalSeen = new HashSet<(int, int)>(); // Hard block — never revisit any tile ever
			var recentTiles = new Dictionary<(int, int), int>(); // tile -> step last visited

			// Pre-populate globalSeen with stopSet so tributaries treat
			// the main river as permanently blocked from the start
			if (stopSet != null)
				foreach (var tile in stopSet)
					globalSeen.Add(tile);

			int cx = startX;
			int cy = startY;
			float momX = 0f;
			float momY = 0f;

			for (int steps = 0; steps < maxSteps; steps++)
			{
				// Stop if we've reached the target set (tributaries joining main river)
				if (stopSet != null && stopSet.Contains((cx, cy))) break;

				path.Add((cx, cy));
				globalSeen.Add((cx, cy));
				recentTiles[(cx, cy)] = steps;

				// Evict the tile that just fell out of the recent window
				if (steps >= recentWindow)
				{
					var old = path[steps - recentWindow];
					recentTiles.Remove(old);
				}

				var terrain = _map.GetTerrainAt(cx, cy);
				if (terrain == TerrainType.DeepOcean || terrain == TerrainType.ShallowWater)
					break;

				var neighbors = GetNeighbors(cx, cy);
				(int x, int y) best = (-1, -1);
				float bestScore = float.MaxValue;
				float currentH = _map.GetHeightAt(cx, cy);

				foreach (var (nx, ny) in neighbors)
				{
					// Hard block — never step on a globally visited tile
					if (globalSeen.Contains((nx, ny))) continue;

					// Lookahead — count unblocked exits from this neighbor.
					// Hard-skip tiles with zero exits (stepping there corners us immediately).
					// Soft-penalize tiles with very few exits (narrow corridors).
					int freeExits = GetNeighbors(nx, ny)
						.Count(n => !globalSeen.Contains(n) && n != (cx, cy));
					if (freeExits == 0) continue;
					float exitPenalty = freeExits <= 2 ? deadEndPenalty : 0f;

					float h = _map.GetHeightAt(nx, ny) + (float)(_rng.NextDouble() * jitter);
					float score = h + exitPenalty;

					// Soft recency penalty — discourages approaching recent path segments
					// even before they would technically be revisited
					if (recentTiles.TryGetValue((nx, ny), out int lastVisited))
					{
						float recency = 1f - (float)(steps - lastVisited) / recentWindow;
						score += revisitPenalty * recency;
					}

					// Momentum penalty — only meaningful on flat ground.
					// On steep slopes gravity fully overrides direction preference.
					float slopeDrop = currentH - h;
					float flatness = 1f - Math.Clamp(slopeDrop / 0.4f, 0f, 1f);
					float dirX = nx - cx;
					float dirY = ny - cy;
					score += momentumWeight * flatness * (1f - Align(dirX, dirY, momX, momY));

					if (score < bestScore) { bestScore = score; best = (nx, ny); }
				}

				// Genuinely cornered — all neighbors blocked or dead ends.
				// FindEscape teleports to the nearest low unvisited tile.
				// This should be rare thanks to the lookahead above.
				if (best == (-1, -1))
				{
					best = FindEscape(cx, cy, globalSeen);
					if (best == (-1, -1)) break;
					momX = 0f;
					momY = 0f;
				}

				// Smoothly update momentum toward the direction we just moved
				float newDirX = best.x - cx;
				float newDirY = best.y - cy;
				momX = momX * 0.6f + newDirX * 0.4f;
				momY = momY * 0.6f + newDirY * 0.4f;

				cx = best.x;
				cy = best.y;
			}

			return path;
		}

		// Last resort when the river is fully cornered by its own global history.
		// Scans outward and returns the nearest low unvisited tile with room to continue.
		private (int x, int y) FindEscape(int stuckX, int stuckY, HashSet<(int, int)> globalSeen)
		{
			const int maxRadius = 30;

			(int x, int y) best = (-1, -1);
			float bestScore = float.MaxValue;

			for (int dx = -maxRadius; dx <= maxRadius; dx++)
			{
				for (int dy = -maxRadius; dy <= maxRadius; dy++)
				{
					int nx = stuckX + dx;
					int ny = stuckY + dy;

					if (nx < 0 || nx >= _map.Width || ny < 0 || ny >= _map.Height) continue;
					if (globalSeen.Contains((nx, ny))) continue;

					// Must have onward options so we don't immediately get stuck again
					int freeExits = GetNeighbors(nx, ny)
						.Count(n => !globalSeen.Contains(n));
					if (freeExits < 2) continue;

					float h = _map.GetHeightAt(nx, ny);
					float distPenalty = MathF.Sqrt(dx * dx + dy * dy) * 0.01f;
					float score = h + distPenalty;

					if (score < bestScore) { bestScore = score; best = (nx, ny); }
				}
			}

			return best;
		}

		private List<(int x, int y)> GetNeighbors(int x, int y)
		{
			var result = new List<(int, int)>();
			int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
			int[] dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

			for (int i = 0; i < 8; i++)
			{
				int nx = x + dx[i];
				int ny = y + dy[i];
				if (nx >= 0 && nx < _map.Width && ny >= 0 && ny < _map.Height)
					result.Add((nx, ny));
			}
			return result;
		}

		// Returns how aligned two direction vectors are in range [0, 1]
		// 1 = same direction, 0 = perpendicular or opposite
		private float Align(float dx1, float dy1, float dx2, float dy2)
		{
			float len1 = MathF.Sqrt(dx1 * dx1 + dy1 * dy1);
			float len2 = MathF.Sqrt(dx2 * dx2 + dy2 * dy2);
			if (len1 == 0 || len2 == 0) return 1f;
			float dot = (dx1 / len1) * (dx2 / len2) + (dy1 / len1) * (dy2 / len2);
			return (dot + 1f) / 2f; // Remap [-1, 1] to [0, 1]
		}
	}
}
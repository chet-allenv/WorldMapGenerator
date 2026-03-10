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
 *      - private List<(int x, int y)> GetNeighbors(int x, int y)
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

					var sourceTerrain = _map.GetTerrainAt(sx, sy);
					if (sourceTerrain == TerrainType.DeepOcean ||
						sourceTerrain == TerrainType.ShallowWater ||
						sourceTerrain == TerrainType.Beach)
						continue;

					var mainRiver = TraceRiver(sx, sy);
					if (mainRiver.Count <= 20) continue;

					allRivers.Add(mainRiver);

					// Generate tributaries that flow into this main river
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

		// Finds confluence points along an existing river and traces tributaries into them
		private List<List<(int x, int y)>> GenerateTributaries(List<(int x, int y)> mainRiver)
		{
			var tributaries = new List<List<(int x, int y)>>();
			var mainRiverSet = new HashSet<(int, int)>(mainRiver);

			// Walk the main river and look for high ground nearby that could feed a tributary
			// Skip the first and last 20 tiles — don't branch at the source or mouth
			for (int i = 20; i < mainRiver.Count - 20; i++)
			{
				var (rx, ry) = mainRiver[i];

				// Only consider every Nth tile as a potential confluence to avoid over-branching
				if (i % 15 != 0) continue;

				// Search in a small radius around this river tile for a high neighbor
				(int x, int y) tributarySource = (-1, -1);
				float bestHeight = _map.GetHeightAt(rx, ry) + 0.05f; // Must be meaningfully higher

				for (int dx = -8; dx <= 8; dx++)
				{
					for (int dy = -8; dy <= 8; dy++)
					{
						int nx = rx + dx;
						int ny = ry + dy;

						if (nx < 0 || nx >= _map.Width || ny < 0 || ny >= _map.Height) continue;
						if (mainRiverSet.Contains((nx, ny))) continue;

						var terrain = _map.GetTerrainAt(nx, ny);
						if (terrain != TerrainType.Mountain && terrain != TerrainType.Snow &&
							terrain != TerrainType.Forest) continue;

						float h = _map.GetHeightAt(nx, ny);
						if (h > bestHeight)
						{
							bestHeight = h;
							tributarySource = (nx, ny);
						}
					}
				}

				if (tributarySource == (-1, -1)) continue;

				// Trace a river from that high point — stop when it hits the main river
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

			var path = new List<(int x, int y)>();
			var visited = new HashSet<(int, int)>();

			int cx = startX;
			int cy = startY;

			for (int steps = 0; steps < maxSteps; steps++)
			{
				if (stopSet != null && stopSet.Contains((cx, cy))) break;

				path.Add((cx, cy));
				visited.Add((cx, cy));

				var terrain = _map.GetTerrainAt(cx, cy);
				if (terrain == TerrainType.DeepOcean || terrain == TerrainType.ShallowWater)
					break;

				var neighbors = GetNeighbors(cx, cy);
				(int x, int y) best = (-1, -1);
				float bestHeight = float.MaxValue;

				foreach (var (nx, ny) in neighbors)
				{
					if (visited.Contains((nx, ny))) continue;
					float h = _map.GetHeightAt(nx, ny) + (float)(_rng.NextDouble() * 0.01);
					if (h < bestHeight) { bestHeight = h; best = (nx, ny); }
				}

				if (best != (-1, -1))
				{
					cx = best.x;
					cy = best.y;
					continue;
				}

				// --- Stuck in a local minimum: attempt to escape via flood fill ---
				var escape = FindEscapePoint(cx, cy, visited);
				if (escape == (-1, -1)) break; // Truly landlocked, give up

				// Fill the basin path from current position to the escape point
				var basinPath = FillBasinTo(cx, cy, escape, visited);
				foreach (var tile in basinPath)
				{
					path.Add(tile);
					visited.Add(tile);
				}

				cx = escape.x;
				cy = escape.y;
			}

			return path;
		}

		// Expands outward from the stuck point using BFS to find the lowest
		// unvisited neighbor that is outside the current basin
		private (int x, int y) FindEscapePoint(int stuckX, int stuckY, HashSet<(int, int)> visited)
		{
			const int maxSearchRadius = 40;

			var queue = new Queue<(int x, int y)>();
			var searched = new HashSet<(int, int)>();

			queue.Enqueue((stuckX, stuckY));
			searched.Add((stuckX, stuckY));

			(int x, int y) bestEscape = (-1, -1);
			float bestHeight = float.MaxValue;

			while (queue.Count > 0)
			{
				var (cx, cy) = queue.Dequeue();

				// If this tile is unvisited and outside the basin, its a candidate escape
				if (!visited.Contains((cx, cy)))
				{
					float h = _map.GetHeightAt(cx, cy);
					if (h < bestHeight)
					{
						bestHeight = h;
						bestEscape = (cx, cy);
					}
					continue; // Don't expand further past the basin boundary
				}

				foreach (var (nx, ny) in GetNeighbors(cx, cy))
				{
					if (searched.Contains((nx, ny))) continue;

					// Limit search radius to avoid searching the entire map
					if (Math.Abs(nx - stuckX) > maxSearchRadius ||
						Math.Abs(ny - stuckY) > maxSearchRadius) continue;

					searched.Add((nx, ny));
					queue.Enqueue((nx, ny));
				}
			}

			return bestEscape;
		}

		// Traces the lowest-height path from the stuck point to the escape point
		// so the basin fill looks like a lake draining rather than teleporting
		private List<(int x, int y)> FillBasinTo(int fromX, int fromY, (int x, int y) target, HashSet<(int, int)> visited)
		{
			// Simple greedy walk toward the target, preferring lower tiles
			var basinPath = new List<(int x, int y)>();
			int cx = fromX;
			int cy = fromY;
			const int maxSteps = 200;
			int steps = 0;

			while ((cx, cy) != target && steps++ < maxSteps)
			{
				var neighbors = GetNeighbors(cx, cy);

				(int x, int y) best = target;
				float bestScore = float.MaxValue;

				foreach (var (nx, ny) in neighbors)
				{
					if (visited.Contains((nx, ny)) && (nx, ny) != target) continue;

					// Score = height + distance to target, nudges path toward escape
					float dist = MathF.Sqrt(MathF.Pow(nx - target.x, 2) + MathF.Pow(ny - target.y, 2));
					float score = _map.GetHeightAt(nx, ny) + dist * 0.1f;

					if (score < bestScore) { bestScore = score; best = (nx, ny); }
				}

				basinPath.Add(best);
				visited.Add(best);
				cx = best.x;
				cy = best.y;
			}

			return basinPath;
		}
	}
}
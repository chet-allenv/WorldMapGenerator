/*
 * RiverGenerator.cs
 * Generates rivers using downhill flow simulation.
 *
 * Finds high-elevation source points, then from each source traces a path by
 * repeatedly stepping to the lowest neighboring cell (with small noise jitter on
 * the ranking so rivers meander on flat ground).
 *
 * When the tracer hits a local minimum on land it calls FindDepressionEscape(),
 * which BFS-floods the depression in height order (like water filling a bowl)
 * and returns the lowest cell that has a downhill exit — the overflow point.
 * The tracer jumps there and continues. This lets rivers cross flat plateaus and
 * inland basins that would otherwise kill the path immediately.
 *
 * The raw downhill path is then subsampled into control points and smoothed
 * with a Catmull-Rom spline for a clean, organic final curve.
 *
 * After each main river is generated, SpawnBranches() fans out 2–4 distributary
 * branches from the lower portion of the river, creating a delta-like effect.
 *
 * KEY MEMBERS:
 *      - RiverGenerator(WorldMap map, MapConfig config, int seed)
 *      - List<List<(int x, int y)>> GenerateRivers()
 *      - private List<(int x, int y)> FindSourcePoints(int count)
 *      - private List<(int x, int y)> TraceDownhill(int startX, int startY)
 *      - private (int x, int y)? FindDepressionEscape(int sx, int sy, HashSet<(int,int)> visited)
 *      - private void SpawnBranches(List<(int x, int y)> mainRaw, List<List<(int x, int y)>> allRivers)
 *      - private List<(int x, int y)> SmoothPath(List<(int x, int y)> raw)
 *      - private List<(int x, int y)> SampleCatmullRom(List<(int x, int y)> pts)
 */

namespace WorldMapGenerator
{
	public class RiverGenerator
	{
		private readonly WorldMap _map;
		private readonly MapConfig _config;
		private readonly Random _rng;

		// 8-directional neighbors
		private static readonly (int dx, int dy)[] Neighbors =
		{
			(-1, -1), (0, -1), (1, -1),
			(-1,  0),           (1,  0),
			(-1,  1), (0,  1), (1,  1)
		};

		public RiverGenerator(WorldMap map, MapConfig config, int seed)
		{
			_map = map;
			_config = config;
			_rng = new Random(seed);
		}

		// Main entry point
		public List<List<(int x, int y)>> GenerateRivers()
		{
			var allRivers = new List<List<(int x, int y)>>();

			// Generate extra candidates so we have enough after filtering short ones
			var sources = FindSourcePoints(_config.RiverCount * 4);

			int mainRiverCount = 0;
			foreach (var src in sources)
			{
				// RiverCount caps main rivers only — branches are always added on top
				if (mainRiverCount >= _config.RiverCount) break;

				var raw = TraceDownhill(src.x, src.y);
				if (raw.Count < _config.RiverMinLength) continue;

				var smooth = SmoothPath(raw);
				if (smooth.Count < 10) continue;

				allRivers.Add(smooth);
				mainRiverCount++;

				// Spawn delta branches from the lower portion of this river
				SpawnBranches(raw, allRivers);
			}

			return allRivers;
		}

		// Spawns branch rivers (distributaries) from the lower portion of a main river,
		// creating a delta-like fan near the mouth. Each branch starts offset
		// perpendicularly from the main channel so it can diverge along its own
		// downhill path rather than retracing the parent.
		private void SpawnBranches(List<(int x, int y)> mainRaw, List<List<(int x, int y)>> allRivers)
		{
			int branchCount = _rng.Next(2, 5);

			// Only branch from the lower 40–80% of the river
			int rangeStart = mainRaw.Count * 4 / 10;
			int rangeEnd   = mainRaw.Count * 8 / 10;
			if (rangeEnd <= rangeStart + 4) return;

			for (int b = 0; b < branchCount; b++)
			{
				int idx = _rng.Next(rangeStart, rangeEnd);
				var (cx, cy) = mainRaw[idx];

				// Compute local river direction and its perpendicular
				int prevIdx = Math.Max(0, idx - 3);
				int nextIdx = Math.Min(mainRaw.Count - 1, idx + 3);
				float ddx = mainRaw[nextIdx].x - mainRaw[prevIdx].x;
				float ddy = mainRaw[nextIdx].y - mainRaw[prevIdx].y;
				float len = MathF.Sqrt(ddx * ddx + ddy * ddy);
				if (len < 0.001f) continue;

				// Alternate left/right across successive branches
				float side = (b % 2 == 0) ? 1f : -1f;
				float perpX = (-ddy / len) * side;
				float perpY = ( ddx / len) * side;

				int offset = _rng.Next(3, 7);
				int bx = Math.Clamp((int)(cx + perpX * offset), 0, _map.Width  - 1);
				int by = Math.Clamp((int)(cy + perpY * offset), 0, _map.Height - 1);

				var raw = TraceDownhill(bx, by);
				if (raw.Count < _config.RiverMinLength / 4) continue;

				var smooth = SmoothPath(raw);
				if (smooth.Count < 5) continue;

				allRivers.Add(smooth);
			}
		}

		// Selects source points from the highest-elevation land tiles on the island.
		// Using terrain type (Mountain/Snow) is too fragile because many seeds
		// produce islands with no mountain tiles at all. Instead we gather ALL
		// non-ocean, non-beach land tiles, sort by height, take the top 20 %,
		// then greedily pick well-spaced ones so rivers fan across the island.
		private List<(int x, int y)> FindSourcePoints(int count)
		{
			// Collect every land tile with its height
			var land = new List<(int x, int y, float h)>();
			for (int x = 0; x < _map.Width; x++)
			for (int y = 0; y < _map.Height; y++)
			{
				var t = _map.GetTerrainAt(x, y);
				if (t == TerrainType.DeepOcean ||
					t == TerrainType.ShallowWater)
				{
					continue;
				}
				land.Add((x, y, _map.GetHeightAt(x, y)));
			}

			if (land.Count == 0) return new List<(int x, int y)>();

			// Keep only the top 20 % by height — these are the high-ground cells
			// that would realistically be river sources
			land.Sort((a, b) => b.h.CompareTo(a.h));
			int topCount = Math.Max(count * 4, land.Count / 5);
			var candidates = land.Take(topCount)
								 .Select(c => (c.x, c.y))
								 .ToList();

			Shuffle(candidates);

			// Minimum spacing — small enough to fill the island but large enough
			// that rivers don't all start from the same hilltop
			float minDist = Math.Max(30f, _map.Width / 20f);

			var selected = new List<(int x, int y)>();
			foreach (var c in candidates)
			{
				bool tooClose = false;
				foreach (var s in selected)
				{
					float d = MathF.Sqrt(MathF.Pow(c.x - s.x, 2) + MathF.Pow(c.y - s.y, 2));
					if (d < minDist) { tooClose = true; break; }
				}

				if (!tooClose)
				{
					selected.Add(c);
					if (selected.Count >= count) break;
				}
			}

			return selected;
		}

		// Traces a river from (startX, startY) by stepping downhill at each cell.
		// A small noise jitter on the height ranking (not detection) breaks ties on
		// flat terrain, producing natural meanders.
		// When the path hits a true local minimum, FindDepressionEscape() BFS-floods
		// the depression to find the lowest overflow point, and the tracer jumps there.
		private List<(int x, int y)> TraceDownhill(int startX, int startY)
		{
			var path    = new List<(int x, int y)>();
			var visited = new HashSet<(int, int)>();

			int x = startX, y = startY;
			int maxSteps    = _map.Width * 3;
			int escapeCount = 0;
			const int MaxEscapes = 6; // max depression crossings per river

			for (int step = 0; step < maxSteps; step++)
			{
				if (!visited.Add((x, y))) break; // loop detected
				path.Add((x, y));

				var terrain = _map.GetTerrainAt(x, y);
				if (terrain == TerrainType.ShallowWater ||
					terrain == TerrainType.DeepOcean)
					break;

				float currentH = _map.GetHeightAt(x, y);

				// Build the downhill candidate list using true heights for detection
				// and a tiny jitter only for ranking, so flat ties resolve randomly.
				var downhill = new List<(int nx, int ny, float h)>();

				foreach (var (dx, dy) in Neighbors)
				{
					int nx = x + dx, ny = y + dy;
					if (nx < 0 || nx >= _map.Width || ny < 0 || ny >= _map.Height) continue;
					if (visited.Contains((nx, ny))) continue;

					float trueH = _map.GetHeightAt(nx, ny);
					if (trueH < currentH)
					{
						// Jitter only for ranking — encourages meanders without hiding downhill neighbours
						float ranked = trueH + (float)(_rng.NextDouble() * 0.008 - 0.004);
						downhill.Add((nx, ny, ranked));
					}
				}

				if (downhill.Count == 0)
				{
					// True local minimum — BFS-flood to find the depression's overflow point
					if (escapeCount >= MaxEscapes) break;
					escapeCount++;

					var escape = FindDepressionEscape(x, y, visited);
					if (escape == null) break;

					x = escape.Value.x;
					y = escape.Value.y;
					continue;
				}

				// Sort by jittered height ascending (steepest first, with natural variation)
				downhill.Sort((a, b) => a.h.CompareTo(b.h));

				// Pick from the top 3 downhill neighbors with decreasing probability
				// so the river mostly follows gravity but occasionally meanders
				int pick;
				double roll = _rng.NextDouble();
				if (downhill.Count == 1 || roll < 0.70)
					pick = 0;
				else if (downhill.Count == 2 || roll < 0.92)
					pick = 1;
				else
					pick = Math.Min(2, downhill.Count - 1);

				x = downhill[pick].nx;
				y = downhill[pick].ny;
			}

			return path;
		}

		// BFS-floods the depression starting at (startX, startY) using a min-heap
		// (priority queue keyed by height) so it always expands the lowest frontier
		// cell first — just like water filling a bowl from the bottom up.
		// Returns the first land cell found that either:
		//   (a) has an unvisited downhill neighbour (can resume flowing), or
		//   (b) is adjacent to a water tile (river reaches the sea directly).
		// Returns null if no escape is found within MaxCells explored cells.
		private (int x, int y)? FindDepressionEscape(int startX, int startY, HashSet<(int, int)> visited)
		{
			var pq   = new PriorityQueue<(int x, int y), float>();
			var seen = new HashSet<(int, int)>();

			pq.Enqueue((startX, startY), _map.GetHeightAt(startX, startY));
			seen.Add((startX, startY));

			const int MaxCells = 4000;

			while (pq.Count > 0 && seen.Count < MaxCells)
			{
				var (cx, cy) = pq.Dequeue();
				float ch = _map.GetHeightAt(cx, cy);

				// Check if this cell has any neighbour OUTSIDE the depression
				// (not in `seen`, not in main-path `visited`) that is lower or is water.
				// The `seen` exclusion is critical: without it, cells inside the bowl
				// would appear to have lower neighbours (each other), giving false escapes.
				foreach (var (dx, dy) in Neighbors)
				{
					int nx = cx + dx, ny = cy + dy;
					if (nx < 0 || nx >= _map.Width || ny < 0 || ny >= _map.Height) continue;
					if (visited.Contains((nx, ny))) continue;
					if (seen.Contains((nx, ny))) continue; // still inside the depression

					var nt = _map.GetTerrainAt(nx, ny);
					float nh = _map.GetHeightAt(nx, ny);

					if (nh < ch || nt == TerrainType.ShallowWater || nt == TerrainType.DeepOcean)
						return (cx, cy); // overflow from (cx, cy) into (nx, ny)
				}

				// Expand to unvisited land neighbors (flood the depression upward)
				foreach (var (dx, dy) in Neighbors)
				{
					int nx = cx + dx, ny = cy + dy;
					if (nx < 0 || nx >= _map.Width || ny < 0 || ny >= _map.Height) continue;
					if (seen.Contains((nx, ny))) continue;
					if (visited.Contains((nx, ny))) continue;

					var nt = _map.GetTerrainAt(nx, ny);
					if (nt == TerrainType.ShallowWater || nt == TerrainType.DeepOcean) continue;

					seen.Add((nx, ny));
					pq.Enqueue((nx, ny), _map.GetHeightAt(nx, ny));
				}
			}

			return null; // no escape found within search budget
		}

		// Subsamples the raw downhill path into evenly-spaced control points
		// and passes them through a Catmull-Rom spline for a smooth final curve.
		private List<(int x, int y)> SmoothPath(List<(int x, int y)> raw)
		{
			if (raw.Count < 4) return raw;

			// Pick one control point every ~stride cells
			int stride = Math.Max(2, raw.Count / 24);
			var ctrl = new List<(int x, int y)>();

			for (int i = 0; i < raw.Count; i += stride)
				ctrl.Add(raw[i]);

			// Always include the exact mouth of the river
			if (ctrl[ctrl.Count - 1] != raw[raw.Count - 1])
				ctrl.Add(raw[raw.Count - 1]);

			return SampleCatmullRom(ctrl);
		}

		// Fisher-Yates shuffle
		private void Shuffle<T>(List<T> list)
		{
			for (int i = list.Count - 1; i > 0; i--)
			{
				int j = _rng.Next(i + 1);
				(list[i], list[j]) = (list[j], list[i]);
			}
		}

		// Catmull-Rom spline — passes through every control point.
		// Produces smooth natural curves with no extra tangent math.
		private List<(int x, int y)> SampleCatmullRom(List<(int x, int y)> points)
		{
			var result = new List<(int x, int y)>();
			if (points.Count < 2) return result;

			// Duplicate first and last point so the spline starts/ends cleanly
			var pts = new List<(int x, int y)>();
			pts.Add(points[0]);
			pts.AddRange(points);
			pts.Add(points[points.Count - 1]);

			for (int i = 1; i < pts.Count - 2; i++)
			{
				var p0 = pts[i - 1];
				var p1 = pts[i];
				var p2 = pts[i + 1];
				var p3 = pts[i + 2];

				float segDist = MathF.Sqrt(
					MathF.Pow(p2.x - p1.x, 2) +
					MathF.Pow(p2.y - p1.y, 2));
				int samples = Math.Max(2, (int)segDist);

				for (int s = 0; s < samples; s++)
				{
					float t = (float)s / samples;
					float t2 = t * t;
					float t3 = t2 * t;

					float b0 = -0.5f * t3 + t2 - 0.5f * t;
					float b1 =  1.5f * t3 - 2.5f * t2 + 1f;
					float b2 = -1.5f * t3 + 2f  * t2 + 0.5f * t;
					float b3 =  0.5f * t3 - 0.5f * t2;

					int rx = Math.Clamp(
						(int)(b0 * p0.x + b1 * p1.x + b2 * p2.x + b3 * p3.x),
						0, _map.Width - 1);
					int ry = Math.Clamp(
						(int)(b0 * p0.y + b1 * p1.y + b2 * p2.y + b3 * p3.y),
						0, _map.Height - 1);

					var last = result.Count > 0 ? result[result.Count - 1] : (-1, -1);
					if ((rx, ry) != last)
						result.Add((rx, ry));
				}
			}

			return result;
		}
	}
}

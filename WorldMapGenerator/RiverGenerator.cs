/*
 * RiverGenerator.cs
 * Generates river paths on the world map using greedy descent.
 * 
 * Uses the HeightValues from WorldMap to simulate water flowing downhill.
 * First finds a high-elevation source via greedy ascent, then traces
 * a path downhill to water via greedy descent.
 * 
 * KEY MEMBERS:
 *      - RiverGenerator(WorldMap map, int seed) - constructor
 *      - List<(int x, int y)> GenerateRiver() - returns a list of river tile coordinates
 */
namespace WorldMapGenerator
{
	public class RiverGenerator
	{
		private readonly Random _rng;
		private readonly WorldMap _map;

		public RiverGenerator(WorldMap map, int seed)
		{
			_map = map;
			_rng = new Random(seed);
		}

		/*
		* Stretch Goal: River Generation
		* This method implements a river generation algorithm that creates river paths across the map.
		* Because my current implementation of the world map is based on noise values, I can use the height values to determine where rivers should flow.
		* Additionally, because the generated picture is 1024x1024, using A* or Dijkstra's pathfinding algorithms are costly.
		* Best approach here is using a greedy decent algorithm that starts at a high elevation point and moves to the lowest adjacent point until it reaches sea level, marking the path as a river.
		*/
		public List<(int x, int y)> GenerateRivers()
		{
			// Find best starting point
			(int x, int y) riverSource = FindRiverSource();

			int cx = riverSource.x;
			int cy = riverSource.y;

			// Create river path
			var path = new List<(int x, int y)>();
			var visited = new HashSet<(int, int)>();

			while (true)
			{
				path.Add((cx, cy));
				visited.Add((cx, cy));

				var terrain = _map.GetTerrainAt(cx, cy);

				if (terrain == TerrainType.DeepOcean || terrain == TerrainType.ShallowWater)
				{
					// Reached the ocean, river is complete
					break;
				}

				var neighbors = GetAdjacentPoints(cx, cy);
				(int x, int y) lowestNeighbor = (cx, cy);
				float bestHeight = float.MaxValue;

				foreach (var (nx, ny) in neighbors)
				{
					float h = _map.GetHeightAt(nx, ny) + (float)(_rng.NextDouble() * 0.01);
					if (h < bestHeight)
					{
						lowestNeighbor = (nx, ny);
						bestHeight = h;
					}
				}

				if (lowestNeighbor == (-1, -1))
				{
					// Stuck in local minimum, river cannot continue
					break;
				}

				cx = lowestNeighbor.x;
				cy = lowestNeighbor.y;
			}

			return path;
		}

		/*
         * FindRiverSource Method:
         * This method finds a suitable starting point for a river, which is typically a high elevation point in the terrain.
         * It would scan through the HeightValues array to find points that are above a certain threshold and return their coordinates as potential river sources.
         */
		private (int x, int y) FindRiverSource()
		{
			// Implementation to find a suitable river source based on height values
			int startX, startY;

			do
			{
				startX = _rng.Next(_map.Width);
				startY = _rng.Next(_map.Height);
			} while (_map.GetTerrainAt(startX, startY) != TerrainType.DeepOcean || _map.GetTerrainAt(startX, startY) != TerrainType.ShallowWater);

			int cx = startX;
			int cy = startY;

			while (true)
			{
				var neighbors = GetAdjacentPoints(cx, cy);

				(int x, int y) lowestNeighbor = (cx, cy);
				float bestHeight = _map.GetHeightAt(cx, cy);

				foreach (var (nx, ny) in neighbors)
				{
					float h = _map.GetHeightAt(nx, ny);
					if (_map.GetHeightAt(nx, ny) < lowestNeighbor.x)
					{
						lowestNeighbor = (nx, ny);
						bestHeight = h;
					}
				}

				if (lowestNeighbor == (cx, cy) || bestHeight >= _map.GetHeightAt(cx, cy))
				{
					// No lower neighbor found, this is a local minimum
					break;
				}
				cx = lowestNeighbor.x;
				cy = lowestNeighbor.y;
			}

			return (cx, cy);
		}

		/*
         * GetAdjacentPoints Method:
         * This method returns a list of adjacent points (up, down, left, right, and diagonals) for a given coordinate (x, y) while ensuring that the points are within the bounds of the map.
         * It would check the coordinates around the given point and return those that are valid and can be considered for river flow.
         */
		private List<(int x, int y)> GetAdjacentPoints(int x, int y)
		{
			// Implementation to get adjacent points (up, down, left, right) while ensuring they are within the bounds of the map
			var res = new List<(int x, int y)>();

			int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
			int[] dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

			for (int i = 0; i < dx.Length; i++)
			{
				int newX = x + dx[i];
				int newY = y + dy[i];
				if (newX >= 0 && newX < _map.Width && newY >= 0 && newY < _map.Height)
				{
					res.Add((newX, newY));
				}
			}

			return res;
		}
	}
}
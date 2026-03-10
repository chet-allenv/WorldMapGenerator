/*
 * WorldMap.cs
 * Serves as the map data grid.
 * 
 * Represents the world map as a 2D array of TerrainType values. 
 * This class is responsible for generating the terrain types based on the noise values and the thresholds defined in MapConfig.
 * 
 * KEY FUNCTIONALITY:
 *      - Stores a 2D array of TerrainType values
 *      - Stores a 2D array of raw float nosie values
 *      - Has Generate() method that fills both arrays using NoiseGenerator and TerrainClassifier.
 *      - Exposes width and height properties.
 *      
 * KEY MEMBERS:
 *      - TerrainType[,] TerrainMap - a 2D array that holds the terrain type    
 *      - float[,] HeightValues - a 2D array that holds the raw noise values (height values) for each point on the map.
 *      - int Width - the width of the map in pixels.
 *      - int Height - the height of the map in pixels.
 *      - void Generate(NoiseGenerator noiseGen, TerrainClassifier classifier) - a method that generates the terrain map using the provided NoiseGenerator and TerrainClassifier.
 *      - TerrainType GetTerrainAt(int x, int y) - a method that returns the terrain type at the specified coordinates.
 *      - float GetHeightAt(int x, int y) - a method that returns the raw noise value (height) at the specified coordinates.
 *      
 *      Stretch Goal Members:
 *      - bool[,] IsRiver() - a 2D array that indicates whether each point on the map is part of a river or not.
 *      - void GenerateRivers(int count) - a method that generates river paths across the map.
 */

namespace WorldMapGenerator
{
    public class WorldMap
    {
        // Properties
        private readonly MapConfig _config;

        public TerrainType[,] TerrainMap { get; private set; }

        public float[,] HeightValues { get; private set; }

        public int Width { get; }
        public int Height { get; }

        // Constructor
        public WorldMap(MapConfig config)
        {
            _config = config;
            Width = _config.Width;
            Height = _config.Height;
            TerrainMap = new TerrainType[_config.Width, _config.Height];
            HeightValues = new float[_config.Width, _config.Height];
        }

        /*
         * Generate Method:
         * Fills both arrays (TerrainMap and HeightValues) using the provided NoiseGenerator and TerrainClassifier.
         */
        public void Generate(NoiseGenerator ng, TerrainClassifier tc)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    // Island falloff Calculation
                    // Calculate the distance from the center of the map
                    float centerX = Width / 2f;
                    float centerY = Height / 2f;

                    float distanceX = x - centerX;
                    float distanceY = y - centerY;

                    float distanceFromCenter = MathF.Sqrt(distanceX * distanceX + distanceY * distanceY);

                    // Normalize the distance from the center to a range of 0 to 1
                    float distanceToCorner = MathF.Sqrt(centerX * centerX + centerY * centerY);
                    float normalizedDistance = distanceFromCenter / distanceToCorner;

                    // Calculate the falloff factor based on the distance from the center and the maximum distance
                    float falloffFactor = MathF.Pow(normalizedDistance, _config.FallOffStrength);

                    //System.Diagnostics.Debug.WriteLine($"Distance from center: {distanceFromCenter}, Normalized Distance: {normalizedDistance}, Falloff Factor: {falloffFactor}");

                    // Get the sample of the noise value @ (x,y)
                    var heightValue = ng.SampleNoise(x, y);

                    var adjustedHeightValue = heightValue - falloffFactor;

                    HeightValues[x, y] = adjustedHeightValue;

                    // Classify the terrain type based on the noise value and store it in the TerrainMap
                    TerrainMap[x, y] = tc.Classify(adjustedHeightValue);
                }
            }
        }

        /*
         * GetTerrainAt Method:
         * Returns the terrain type at the specified coordinates.
         */
        public TerrainType GetTerrainAt(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                throw new ArgumentOutOfRangeException("Coordinates are out of bounds of the map.");
            return TerrainMap[x, y];
        }

        /*
         * GetHeightAt Method:
         * Returns the raw noise value (height) at the specified coordinates.
         */
        public float GetHeightAt(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                throw new ArgumentOutOfRangeException("Coordinates are out of bounds of the map.");
            return HeightValues[x, y];
        }

        /*
         * Stretch Goal: River Generation
         * This method implements a river generation algorithm that creates river paths across the map.
         * Because my current implementation of the world map is based on noise values, I can use the height values to determine where rivers should flow.
         * Additionally, because the generated picture is 1024x1024, using A* or Dijkstra's pathfinding algorithms are costly.
         * Best approach here is using a greedy decent algorithm that starts at a high elevation point and moves to the lowest adjacent point until it reaches sea level, marking the path as a river.
         */
        public void GenerateRivers(int count)
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

                var terrain = GetTerrainAt(cx, cy);

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
                    float h = GetHeightAt(nx, ny) + (float)(_rng.NextDouble() * 0.01) ;
                    if (h < bestHeight)
                    {
                        lowestNeighbor = (nx, ny);
                        bestHeight = h;
                    }
                }
            }
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
                startX = new Random().Next(0, Width);
                startY = new Random().Next(0, Height);
            } while (GetTerrainAt(startX, startY) != TerrainType.DeepOcean || GetTerrainAt(startX, startY) != TerrainType.ShallowWater);

            int cx = startX;
            int cy = startY;

            while (true)
            {
                var neighbors = GetAdjacentPoints(cx, cy);

                (int x, int y) lowestNeighbor = (cx, cy);
                float bestHeight = GetHeightAt(cx, cy);

                foreach (var (nx, ny) in neighbors)
                {
                    float h = GetHeightAt(nx, ny);
                    if (GetHeightAt(nx, ny) < lowestNeighbor.x)
                    {
                        lowestNeighbor = (nx, ny);
                        bestHeight = h;
                    }
                }

                if (lowestNeighbor == (cx, cy) || bestHeight >= GetHeightAt(cx, cy))
                {
                    // No lower neighbor found, this is a local minimum
                    break;
                })
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
                if (newX >= 0 && newX < Width && newY >= 0 && newY < Height)
                {
                    res.Add((newX, newY));
                }
            }
        }
    }
}

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
 */

namespace WorldMapGenerator
{
    public class WorldMap
    {
        // Properties
        public TerrainType[,] TerrainMap { get; private set; 

        public float[,] HeightValues { get; private set; }

        public int Width { get; }
        public int Height { get; }

        // Constructor
        public WorldMap(int width, int height)
        {
            Width = width;
            Height = height;
            TerrainMap = new TerrainType[width, height];
            HeightValues = new float[width, height];
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
                    // Get the sample of the noise value @ (x,y)
                    var heightValue = ng.SampleNoise(x, y);

                    HeightValues[x, y] = heightValue;

                    // Classify the terrain type based on the noise value and store it in the TerrainMap
                    TerrainMap[x, y] = tc.Classify(heightValue);
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
    }
}

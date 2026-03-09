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

/*
 * MapConfig.cs
 * The tunable parameters for the world map generator. This is used to configure the world map generator and make it more flexible.
 * 
 * a Record that holds every tunable value. Only stores settings.
 * 
 *      - Defines map width and height in pixels
 *      - Stores noise seed
 *      - Stores noise frequency & octave count
 *      - Controls the island falloff (how quickly the terrain falls off towards the edges of the map)
 *      - Stores the output file path.
 *      
 *      
 * KEY PARAMS:
 *      - Width (int)
 *      - Height (int)
 *      - seed (int)
 *      - frequency (float)
 *      - octaves (int)
 *      - falloffStrength (float)
 *      - outputFilePath (string)
 *      
 *      ALSO:
 *      
 *      Hold float thresholds for terrain classification.
 *      This allows us to easily adjust the thresholds for different terrain types without changing the code in TerrainClassifier.cs.
 *      
 *      - DeepOceanMax (float)
 *      - ShallowWaterMax (float)
 *      - BeachMax (float)
 *      - GrasslandMax (float)
 *      - ForestMax (float)
 *      - MountainMax (float)
 *      // Anything higher than MountainMax is considered Snow.
 */
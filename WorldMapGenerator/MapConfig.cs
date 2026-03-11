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

namespace WorldMapGenerator
{

    public record MapConfig
    {
        public int Width = 1024;                                        // In pixels
        public int Height = 1024;                                       // In pixels

        public int Seed = 1337;                                         // Seed for noise generation

        public float Frequency = 0.003f;                                // Frequency for noise generation
        public int Octaves = 5;                                         // Number of octaves for noise generation

        public float FallOffStrength = 3.0f;                            // Controls how quickly the terrain falls off towards the edges of the map
        public string OutputFilePath = @"..\..\..\output\worldmap.png"; // Output file path for the generated map

        public int RiverCount = 8; // Number of main rivers radiating from peak

        // Flow accumulation threshold � lower = more rivers, higher = fewer but more prominent
        // On a 1024x1024 map, good starting values are between 400 and 1200
        public int RiverAccumulationThreshold = 400; // Lower = more/longer rivers
        public int RiverMinLength = 30;  // Discard rivers shorter than this

        // Terrain classification thresholds
        // Noise returns values in the range of -1.0 to 1.0

        public float DeepOceanMax = -0.5f;     // -1 to -0.5
        public float ShallowWaterMax = -0.3f;   // -0.5 to 0
        public float BeachMax = -0.2f;           // 0 to 0.1
        public float GrasslandMax = 0.1f;    // 0.1 to 0.3
        public float ForestMax = 0.3f;       // 0.3 to 0.5
        public float MountainMax = 0.6f;     // 0.5 to 0.7
                                             // Anything higher than MountainMax is considered Snow.
    }
}

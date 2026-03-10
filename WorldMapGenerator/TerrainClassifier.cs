/*
 * TerrainClassifier.cs
 * takes a float and turns it into a TerrainType. This is used to classify the terrain based on the noise value.
 * 
 * Converts raw float nosie into a TerrainType enum value based on the thresholds defined in MapConfig.
 * Strictly logic - no drawing or noise sampling.
 * 
 * KEY FUNCTIONALITY:
 *      - Accepts a MapConfig object to access the terrain thresholds.
 *      - public method Classify(float heightValue) that takes a float noise value and returns the corresponding TerrainType based on the thresholds.
 *      - Checks the heightValue against the thresholds in MapConfig in a specific order to determine the correct TerrainType.
 * 
 * KEY MEMBERS:
 *      - TerrainClassifier(MapConfig config) - constructor that takes in a MapConfig object to initialize the classifier with the terrain thresholds.
 *      - TerrainType Classify(float heightValue) - method that takes a float noise value and returns the corresponding TerrainType based on the thresholds defined in MapConfig.
 *          // Can approach this as a series of if-else statements that check the heightValue against each threshold in order, starting from the lowest (DeepOcean) to the highest (Snow)
 *          // or can look up through sorted thresholds. For simplicity and readability, if-else statements are likely sufficient given the small number of terrain types.
 *          
 *          
 *      What ranges should we approach for each terrain type? This is somewhat subjective and can be adjusted based on the desired look of the map.
 *      
 *      These thresholds are stored in the MapConfig class, 
 *      allowing for easy adjustment without changing the code in TerrainClassifier.cs. 
 *      The Classify method will check the heightValue against these thresholds in order to determine the correct TerrainType to return.
 */

namespace WorldMapGenerator
{
    public class TerrainClassifier
    {
        // Instance of MapConfig to access the terrain thresholds
        private readonly MapConfig _mapConfig;

        // Constructor that takes in a MapConfig object to initialize the classifier with the terrain thresholds.
        public TerrainClassifier(MapConfig config)
        {
            _mapConfig = config;
        }

        /*
         * Classify Method:
         * Takes a float noise value and returns the corresponding TerrainType based on the thresholds defined in MapConfig.
         * Checks the heightValue against the thresholds in MapConfig in a specific order to determine the correct TerrainType.
         *      (FROM MAPCONFIG)
         *      
         *      public float DeepOceanMax = -.5f;     // -1 to -0.5
                public float ShallowWaterMax = 0f;   // -0.5 to 0
                public float BeachMax = 0.1f;        // 0 to 0.1
                public float GrasslandMax = 0.3f;    // 0.1 to 0.3
                public float ForestMax = 0.5f;       // 0.3 to 0.5
                public float MountainMax = 0.7f;     // 0.5 to 0.7
                                                     // Anything higher than MountainMax is considered Snow.
        *
        */
        public TerrainType Classify(float heightValue)
        {
            if (heightValue < _mapConfig.DeepOceanMax)
                return TerrainType.DeepOcean;
            else if (heightValue < _mapConfig.ShallowWaterMax)
                return TerrainType.ShallowWater;
            else if (heightValue < _mapConfig.BeachMax)
                return TerrainType.Beach;
            else if (heightValue < _mapConfig.GrasslandMax)
                return TerrainType.Grassland;
            else if (heightValue < _mapConfig.ForestMax)
                return TerrainType.Forest;
            else if (heightValue < _mapConfig.MountainMax)
                return TerrainType.Mountain;
            else
                return TerrainType.Snow;
        }
    }
}
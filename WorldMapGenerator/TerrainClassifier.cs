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
 *      What ranges should we approach for each terrain type? This is somewhat subjective and can be adjusted based on the desired look of the map. Here's a common approach:
 *      
 *      All Terrain Types:
 *  
 *      DeepOcean: 0.0 to 0.2
 *      ShallowWater: 0.2 to 0.4
 *      Beach: 0.4 to 0.5
 *      Grassland: 0.5 to 0.7
 *      Forest: 0.7 to 0.85
 *      Mountain: 0.85 to 0.95
 *      Snow: 0.95 to 1.0
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
         * 
         *      DeepOcean: 0.0 to 0.2
         *      ShallowWater: 0.2 to 0.4
         *      Beach: 0.4 to 0.5
         *      Grassland: 0.5 to 0.7
         *      Forest: 0.7 to 0.85
         *      Mountain: 0.85 to 0.95
         *      Snow: 0.95 to 1.0
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
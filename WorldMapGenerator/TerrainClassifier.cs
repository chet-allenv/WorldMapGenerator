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
 */
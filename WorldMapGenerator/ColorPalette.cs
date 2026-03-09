/* 
 * ColorPalette.cs
 * Takes in a TerrainType and returns a Color. This is used to color the map based on the terrain type.
 * 
 * Maps a TerrainType to a specific SKColor. This is the only class to be aware of colors. By centralizing color mapping here,
 * I can easily change the theme of the map by modifying this class without affecting the rest of the codebase.
 * 
 * KEY FUNCTIONALITY: 
 *      - Contains a public method GetColor(TerrainType terrain) that takes a TerrainType and returns the corresponding SKColor.
 *      - Defines all colors as named private constants for easy modification and readability.
 *      - Does NOT call any noise or terrain logic
 *      
 * KEY MEMBERS:
 *      - SKColor GetColor(TerrainType terrain) - method that takes a TerrainType and returns the corresponding SKColor based on a predefined mapping.
 *          // private static readonly SKColor DeepOceanColor = new SKColor(0x1A, 0x4A, 0x7A); // Dark Blue
 *          // etc etc for each terrain type, using hex color codes for clarity and ease of modification.
 */
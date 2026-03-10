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

using SkiaSharp;

namespace WorldMapGenerator
{
    public class ColorPalette
    {
        // Define colors for each terrain type as private constants
        private static readonly SKColor DeepOceanColor = new SKColor(0x1A, 0x4A, 0x7A); // Dark Blue
        private static readonly SKColor ShallowWaterColor = new SKColor(0x2E, 0x8B, 0xC8); // Medium Blue
        private static readonly SKColor BeachColor = new SKColor(0xF4, 0xE1, 0xB6); // Sand
        private static readonly SKColor GrasslandColor = new SKColor(0x7C, 0xFC, 0x00); // Light Green
        private static readonly SKColor ForestColor = new SKColor(0x22, 0x8B, 0x22); // Dark Green
        private static readonly SKColor MountainColor = new SKColor(0xA9, 0xA9, 0xA9); // Gray
        private static readonly SKColor SnowColor = new SKColor(0xFF, 0xFF, 0xFF); // White

        /*
         * GetColor Method:
         * Takes a TerrainType and returns the corresponding SKColor based on a predefined mapping.
         * Uses a switch expression to map each TerrainType to its corresponding color. 
         * Throws an ArgumentOutOfRangeException if an unexpected TerrainType is provided, ensuring that all cases are handled explicitly.
         */
        public SKColor GetColor(TerrainType terrain)
        {
            return terrain switch
            {
                TerrainType.DeepOcean => DeepOceanColor,
                TerrainType.ShallowWater => ShallowWaterColor,
                TerrainType.Beach => BeachColor,
                TerrainType.Grassland => GrasslandColor,
                TerrainType.Forest => ForestColor,
                TerrainType.Mountain => MountainColor,
                TerrainType.Snow => SnowColor,
                _ => throw new ArgumentOutOfRangeException(nameof(terrain), $"Unexpected terrain type: {terrain}")
            };
        }
    }
}

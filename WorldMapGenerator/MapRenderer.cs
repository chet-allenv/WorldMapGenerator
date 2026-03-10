/*
 * MapRenderer.cs
 * Draws the bitmap based on the terrain types and color palette. This is used to render the world map.
 * 
 * Takes a WorldMap and a ColorPalette, and generates an SKBitmap where each pixel's color is determined by the terrain type at that location on the WorldMap.
 * The only class that utilizes SkiaSharp drawing. All other classes are unaware of the rendering details, which allows for better separation of concerns and easier maintenance.
 * 
 * KEY FUNCTIONALITY:
 *      - Constructor accepts a MapConfig object to access map dimensions and output file path.
 *      - Primary method Render(WorldMap map, ColorPalette palette) that generates an SKBitmap based on the terrain types in the WorldMap and the corresponding colors from the ColorPalette.
 *      - Loops over every pixel, asks palette for a color, and paints the pixel
 *      - Separate method handles saving the SKBitmap to a file using the output file path from MapConfig.
 * 
 * KEY MEMBERS:
 *      - MapRenderer(MapConfig config) - constructor that takes in a MapConfig object to initialize the renderer with the map dimensions and output file path.
 *      - SKBitmap Render(WorldMap map, ColorPalette palette) - method that takes a WorldMap and a ColorPalette, and generates an SKBitmap where each pixel's color is determined by the terrain type at that location on the WorldMap using the ColorPalette.
 *      - void SaveBitmap(SKBitmap bitmap) - method that saves the generated SKBitmap to a file using the output file path specified in the MapConfig.
 *          // PHASE 3 METHOD - TO DO LATER
 *      - void ApplyElevationShading(SKBitmap bitmap, WorldMap map) - method that applies elevation-based shading to the SKBitmap to add depth and visual interest to the map. 
 *      This method would analyze the height values from the WorldMap and adjust the brightness of each pixel accordingly to create a shaded effect that enhances the perception of terrain elevation.    
 *      
 */
using SkiaSharp;

namespace WorldMapGenerator
{
    public class MapRenderer
    {
        private readonly MapConfig _config;

        public MapRenderer(MapConfig config)
        {
            _config = config;
        }

        /*
         * Render Method:
         * Method that takes a WorldMap and a ColorPalette, and generates an SKBitmap where each pixel's color is determined by the terrain type at that location on the WorldMap using the ColorPalette.
         */
        public SKBitmap Render(WorldMap map, ColorPalette palette)
        {
            var _bitmap = new SKBitmap(map.Width, map.Height);

            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    var terrainType = map.GetTerrainAt(x, y);
                    var color = palette.GetColor(terrainType);
                    _bitmap.SetPixel(x, y, color);
                }
            }
            return _bitmap;
        }
    }
}
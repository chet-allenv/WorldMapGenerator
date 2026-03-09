/*
 * TerrainType.cs
 * an enum that represents the different types of terrain that can be generated. 
 * This is used to classify the terrain based on the noise value.
 * 
 * Delcares every terrain type as a value.
 * 
 * just enum declaration.
 * 
 * Types of terrain:
 *		- DeepOcean
 *		- ShallowWater
 *		- Beach
 *		- Grassland
 *		- Forest
 *		- Mountain
 *		- Snow
 */

namespace WorldMapGenerator 
{
    public enum TerrainType
    {
        DeepOcean,
        ShallowWater,
        Beach,
        Grassland,
        Forest,
        Mountain,
        Snow
    }
}
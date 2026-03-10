/*
 * NoiseGenerator.cs
 * Wraps FastNoiseLite to provide noise generation functionality for the world map generator.
 * 
 * Rest of code should NOT directly reference FastNoiseLite, but should instead use this class to generate
 * 
 * If needed to swap noise libraries in the future, only need to change this class and not the rest of the codebase.
 * 
 * KEY FUNCTIONALITY:
 *		- Accepts a MapConfig object to configure the noise generation parameters.
 *		- Provides a method to generate a 2D array of noise values based on the map configuration.
 *		- Responsible for normalization of nosie values.
 *		
 *	KEY MEMBERS:
 *		- NoiseGenerator(MapConfig config) - constructor that takes in a MapConfig object to initialize the noise generator.
 *		- float SampleNoise(int x, int y) - method to sample noise value at given coordinates.
 *		- private: FastNoiseLite noise - the instance of the FastNoiseLite noise generator.
 */

/* 
 * Understanding Perlin Noise: Information taken from watching https://www.youtube.com/watch?v=DxUY42r_6Cg
 * 
 * Perlin noise is a method of generating smooth, natural noise. It is not as scattered and crazy as white noise.
 * it is also a close relative of value noise. Essentially, Perlin noise is a type of gradient noise that uses bilinear interpolation to 
 * create smooth transitions between random values. It uses the dot product of gradient vectors and distance vectors to calculate the noise value at a given point.
 * 
 * Perlin's algorithm is useful in terrain generation because it allows for natural looking landscapes with hills, valleys, and other features. By adjusting the 
 * frequency and amplitude of the noise, you can create different types of terrain, such as mountains, plains, or oceans.
 * Additionally, an improvement to the noise algorithm uses a new interpolant function f(x) = 6x^5 - 15x^4 + 10x^3, which has zero first and second derivatives at the endpoints,
 * resulting in smoother transitions between noise values.
 */

namespace WorldMapGenerator
{
    public class NoiseGenerator
    {
        // The instance of the FastNoiseLite noise generator that this class wraps around.
        private FastNoiseLite _noise;

        // Constructor that takes in a MapConfig object to initialize the noise generator.
        public NoiseGenerator(MapConfig config)
        {
            _noise = new FastNoiseLite();

            _noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            _noise.SetFractalType(FastNoiseLite.FractalType.FBm);

            _noise.SetSeed(config.Seed);
            _noise.SetFrequency(config.Frequency);
            _noise.SetFractalOctaves(config.Octaves);
        }


        /*
         * SampleNoise Method:
         * Leverages the FastNoiseLite instance to sample noise values at given coordinates (x,y).
         */
        public float SampleNoise(int x, int y)
        {
            // Sample the noise at a given point (x,y)
            _noise.GetNoise(x, y, out float noiseValue);
            return noiseValue;
        }
    }
}
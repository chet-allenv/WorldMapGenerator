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
using System;

namespace ProjectRoguelike.Procedural
{
    /// <summary>
    /// Manages the seed for procedural generation. Ensures deterministic generation.
    /// </summary>
    public sealed class LevelSeed
    {
        private readonly int _seed;
        private System.Random _random;

        public int Seed => _seed;

        public LevelSeed(int seed)
        {
            _seed = seed;
            _random = new System.Random(seed);
        }

        public LevelSeed() : this(UnityEngine.Random.Range(int.MinValue, int.MaxValue))
        {
        }

        public int NextInt(int min, int max)
        {
            return _random.Next(min, max);
        }

        public int NextInt(int max)
        {
            return _random.Next(max);
        }

        public float NextFloat()
        {
            return (float)_random.NextDouble();
        }

        public float NextFloat(float min, float max)
        {
            return min + (float)_random.NextDouble() * (max - min);
        }

        public bool NextBool()
        {
            return _random.Next(2) == 1;
        }

        public void Reset()
        {
            _random = new System.Random(_seed);
        }
    }
}


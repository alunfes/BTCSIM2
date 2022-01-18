using System;
using System.Collections.Generic;
using System.Linq;

namespace BTCSIM2
{
    public static class RandomSeed
    {
        public static Random rnd { get; set; }

        public static void initialize()
        {
            rnd = new Random();
        }
    }


    public class RandomGenerator
    {
        public RandomGenerator()
        {
        }
        public double getRandomVal()
        {
            return RandomSeed.rnd.NextDouble();
        }
    }
}

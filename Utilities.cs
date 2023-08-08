using SFML.System;
using SpaceInvadersClone;

namespace Utilities
{
    static class Utilities
    {
        public static int Modulus(int a, int b)
        {
            if (a < 0)
            {
                return b + a % b;
            }
            else
            {
                return a % b;
            }
        }

        public static double Magnitude(Vector2f v)
        { 
            return Math.Sqrt(v.X * v.X + v.Y * v.Y);
        }
    }

    class Die
    {
        public Die(int n) 
        { 
            // Creates an n-sided dice
            random = new Random();
            this.n = n;
            secretNumber = random.Next(1, n + 1);
        }

        public int Roll()
        {
            return random.Next(1, n + 1);
        }

        public bool RollGiven(int number)
        {
            return Roll() == number;
        }

        public bool RandomRoll()
        {
            return RollGiven(secretNumber);
        }

        Random random;
        int n, secretNumber;
    }
}

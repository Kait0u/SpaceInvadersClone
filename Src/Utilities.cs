using SFML.Graphics;
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

        public static View CalcView(Vector2u windowCurrSize, Vector2u baseSize)
        {
            FloatRect viewport = new FloatRect(0f, 0f, 1f, 1f);

            float sw = windowCurrSize.X / (float)baseSize.X;
            float sh = windowCurrSize.Y / (float)baseSize.Y;

            if (sw > sh) 
            { 
                viewport.Width = sh / sw;
                viewport.Left = (1f - viewport.Width) / 2f;
            }

            else if (sw < sh)
            {
                viewport.Height = sw / sh;
                viewport.Top = (1f - viewport.Height) / 2f;
            }

            View view = new View(new FloatRect(0f, 0f, baseSize.X, baseSize.Y));
            view.Viewport = viewport;
            return view;
        }

        public static float DegRadConversionConstant => degRadConversionCostant;

        const float degRadConversionCostant = 0.0174532925199432957692369076848861271344f;
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

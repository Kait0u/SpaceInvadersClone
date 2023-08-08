using SFML.Graphics;

namespace SpaceInvadersClone
{
    static class FontBank
    {
        private static string assetPath = "./Assets/Fonts/";

        static Font pixelColecoFont = new Font(assetPath + "PixelColeco.ttf");

        public static Font PixelColeco {  get { return pixelColecoFont; }
        }
    }
}

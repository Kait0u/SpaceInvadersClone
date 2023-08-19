using SFML.Graphics;

namespace SpaceInvadersClone
{
    static class TextureBank
    {
        static TextureBank()
        {
            meteoriteTextures = new List<Texture>();
            for (int i = 1; i <= 4; ++i) meteoriteTextures.Add(new Texture(assetPath + $"meteorite{i}.png"));
        }

        static string assetPath = "./Assets/Sprites/";

        static Texture backgroundTexture = new Texture(assetPath + "space.png");

        static Texture playerTexture = new Texture(assetPath + "spaceship.png");
        static Texture enemyTexture1 = new Texture(assetPath + "alien1.png");
        static Texture enemyTexture2 = new Texture(assetPath + "alien2.png");
        static Texture enemyTexture3 = new Texture(assetPath + "alien3.png");

        static Texture playerBulletTexture = new Texture(assetPath + "player_bullet.png");
        static Texture enemyBulletTexture = new Texture(assetPath + "enemy_bullet.png");

        static Texture medkitTexture = new Texture(assetPath + "medkit.png");
        static Texture powerupTexture = new Texture(assetPath + "powerup.png");

        static List<Texture> meteoriteTextures;


        public static Texture BackgroundTexture { get { return backgroundTexture; } }

        public static Texture PlayerTexture { get { return playerTexture; } }
        public static Texture EnemyTexture1 { get { return enemyTexture1; } }
        public static Texture EnemyTexture2 { get { return enemyTexture2; } }
        public static Texture EnemyTexture3 { get { return enemyTexture3; } }

        public static Texture MeteoriteTexture1 => meteoriteTextures[0];
        public static Texture MeteoriteTexture2 => meteoriteTextures[1];
        public static Texture MeteoriteTexture3 => meteoriteTextures[2];
        public static Texture MeteoriteTexture4 => meteoriteTextures[3];

        public static Texture PlayerBulletTexture { get { return playerBulletTexture; } }
        public static Texture EnemyBulletTexture { get { return enemyBulletTexture; } }

        public static Texture MedkitTexture { get { return medkitTexture; } }
        public static Texture PowerupTexture { get { return powerupTexture; } }
    }
}

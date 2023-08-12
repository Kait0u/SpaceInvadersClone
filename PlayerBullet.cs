using SFML.Graphics;
using SFML.System;

namespace SpaceInvadersClone
{
    internal class PlayerBullet: IBullet
    {
        public PlayerBullet(Vector2f startingPoint) 
        {
            renderWindow = Game.GameWindowInstance;
            position = startingPoint;

            direction = new Vector2f(0, -1);
            sprite = new Sprite(TextureBank.PlayerBulletTexture);
            sprite.Position = position;
        }

        public PlayerBullet Copy()
        {
            PlayerBullet bullet = new PlayerBullet(position);
            return bullet;
        }

        public void Draw()
        {
            renderWindow.Draw(sprite);
        }

        public void Update()
        {
            Vector2f deltaPos = velocity * direction;
            position += deltaPos;
            sprite.Position = position;
            
            bool doesCollide = DetectCollision();
            
            if (doesCollide)
            {
                Game.PlayerBulletController.DeleteBullet(this);
                Game.WaveController.Damage(enemyCollided);
                enemyCollided.AddLifebar(Enemy.LifebarPositions.Above);
            }
        }

        bool DetectCollision()
        {
            bool doesCollide = false;

            foreach (Hostile enemy in Game.WaveController.EnemyGroup.EnemyList) 
            { 
                if (sprite.GetGlobalBounds().Intersects(enemy.EnemySprite.GetGlobalBounds()))
                {
                    doesCollide = true;
                    enemyCollided = enemy;
                    break;
                }
            }

            return doesCollide;
        }

        public int XSize { get { return (int)sprite.Texture.Size.X; } }
        public int YSize { get { return (int)sprite.Texture.Size.Y; } }

        public float X { get { return position.X; } set { position.X = value; } }
        public float Y { get { return position.Y; } set { position.Y = value; } }

        RenderWindow renderWindow;
        Vector2f position;
        Vector2f direction;
        Sprite sprite;
        Hostile? enemyCollided;
        const float velocity = 12f;
    }
}

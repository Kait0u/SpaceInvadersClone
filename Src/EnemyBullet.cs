﻿using SFML.Graphics;
using SFML.System;

namespace SpaceInvadersClone
{
    internal class EnemyBullet : IBullet
    {
        public EnemyBullet(Vector2f startingPoint)
        {
            renderWindow = Game.GameWindowInstance;
            player = Game.PlayerInstance;

            position = startingPoint;
            initialPosition = position;
            sprite = new Sprite(TextureBank.EnemyBulletTexture);
            sprite.Position = position;
        }


        public EnemyBullet Copy()
        {
            EnemyBullet bullet = new EnemyBullet(new Vector2f(X, Y));

            if (verticalDownfall)
            {
                bullet.SetVerticalDownfall(this.velocity);
            }

            else if (targetted)
            {
                bullet.SetTargetted(this.velocity);
            }

            else if (directed)
            {
                bullet.SetDirected(this.direction, this.velocity);
            }

            else if (verticalSine)
            {
                bullet.SetVerticalSine(this.velocity, this.peakToPeakAmpl);
            }

            else if (verticalCosine)
            {
                bullet.SetVerticalCosine(this.velocity, this.peakToPeakAmpl);
            }

            return bullet;
        }

        public void Draw()
        {
            renderWindow.Draw(sprite);
        }

        public void Update()
        {
            Move();
            sprite.Position = position;

            bool doesCollide = sprite.GetGlobalBounds().Intersects(player.PlayerSprite.GetGlobalBounds());
            
            if (doesCollide)
            {
                Game.EnemyBulletController.DeleteBullet(this);
                player.Damage(playerDamage);
            }
        }

        void Move()
        {
            if (verticalDownfall || targetted || directed)
            {
                position += velocity * direction;
            }

            else if (verticalSine)
            {
                position.Y += velocity;
                float verticalDistanceFromStart = position.Y - initialPosition.Y;

                position.X = initialPosition.X + peakToPeakAmpl * (float)Math.Sin(verticalDistanceFromStart/(velocity * Math.Max(2, velocity)));
            }

            else if (verticalCosine)
            {
                position.Y += velocity;
                float verticalDistanceFromStart = position.Y - initialPosition.Y;

                position.X = initialPosition.X + peakToPeakAmpl * (float)Math.Cos(verticalDistanceFromStart/(velocity * Math.Max(2, velocity)));
            }
        }

        public void SetVerticalDownfall(float velocity)
        {
            direction = new Vector2f(0, 1);
            this.velocity = velocity;

            verticalDownfall = true;
            targetted = false;
            directed = false;
            verticalSine = false;
            verticalCosine = false;
        }

        public void SetTargetted(float velocity)
        {
            direction = new Vector2f(player.X, player.Y) - position;
            float magnitude = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
            direction /= magnitude;
            this.velocity = velocity;

            verticalDownfall = false;
            targetted = true;
            directed = false;
            verticalSine = false;
            verticalCosine = false;
        }

        public void SetDirected(Vector2f direction, float velocity)
        {
            this.direction = direction / (float)Utilities.Utilities.Magnitude(direction);
            this.velocity = velocity;

            verticalDownfall = false;
            targetted = false;
            directed = true;
            verticalSine = false;
            verticalCosine = false;
        }

        public void SetVerticalSine(float velocity, float peakToPeakAmplitude)
        {
            this.velocity = velocity;
            peakToPeakAmpl = peakToPeakAmplitude;

            verticalDownfall = false;
            targetted = false;
            directed = false;
            verticalSine = true;
            verticalCosine = false;
        }

        public void SetVerticalCosine(float velocity, float peakToPeakAmplitude)
        {
            this.velocity = velocity;
            peakToPeakAmpl = peakToPeakAmplitude;

            verticalDownfall = false;
            targetted = false;
            directed = false;
            verticalSine = false;
            verticalCosine = true;
        }

        public int XSize => (int)sprite.GetGlobalBounds().Width;
        public int YSize => (int)sprite.GetGlobalBounds().Height;

        public float X { get { return position.X; } set { position.X = value; } }
        public float Y { get { return position.Y; } set { position.Y = value; } }

        public int BulletModePoints
        {
            get
            {
                if (verticalDownfall) return 100;
                else if (targetted) return 500;
                else if (directed) return 200;
                else if (verticalSine) return 250;
                else if (verticalCosine) return 250;
                else return 0;
            }
        }

        RenderWindow renderWindow;
        Player player;
        Vector2f position, initialPosition, direction;
        Sprite sprite;
        
        float velocity, peakToPeakAmpl;
        bool verticalDownfall, targetted, directed, verticalSine, verticalCosine;

        const int playerDamage = 10;
    }
}

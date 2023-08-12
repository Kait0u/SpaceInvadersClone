using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using static SpaceInvadersClone.Enemy;

namespace SpaceInvadersClone
{
    abstract class Hostile
    {
        public Hostile(Vector2f startingPosition, int health, Texture texture)
        {
            renderWindow = Game.GameWindowInstance;

            sprite = new Sprite(texture);
            position = startingPosition;
            sprite.Position = position;

            RecalculateBonusSpawnPosition();

            this.health = health;
            maxHealth = health;
            pointValue = health;

            lifebarTime = Time.FromSeconds(10);
        }

        private void RecalculateBonusSpawnPosition()
        {
            bonusSpawnPosition = position + new Vector2f(XSize, YSize) / 2f
                                 - new Vector2f(TextureBank.MedkitTexture.Size.X, TextureBank.MedkitTexture.Size.Y) / 2f;
        }

        public virtual void Draw() 
        { 
            renderWindow.Draw(sprite);
            lifebar?.Draw(renderWindow);
        }
        public abstract void Update();

        public abstract void AddLifebar(LifebarPositions lbPosition);
        public abstract void RemoveLifebar();

        public abstract void Fire();

        public virtual float X { get => position.X; set { position.X = value; sprite.Position = position; RecalculateBonusSpawnPosition(); } }
        public virtual float Y { get => position.Y; set { position.Y = value; sprite.Position = position; RecalculateBonusSpawnPosition(); } }
        public virtual int XSize => (int)sprite.GetGlobalBounds().Width;
        public virtual int YSize => (int)sprite.GetGlobalBounds().Height;

        public virtual Sprite EnemySprite => sprite;
        public virtual EnemyBullet? BulletBP => bulletBP;
        public virtual bool IsAggressive => isAggressive;

        public virtual Vector2f BonusSpawnPoint => bonusSpawnPosition;
        public virtual int EnemyHealth { get => health; set { health = value; } }
        public abstract int PointValue { get; }

        protected Sprite sprite;

        protected int health, maxHealth, pointValue;
        protected Vector2f position, bonusSpawnPosition;

        protected bool isAggressive; // Is it supposed to shoot?
        protected EnemyBullet? bulletBP;

        public enum LifebarPositions { Above, Below }
        protected Lifebar? lifebar;
        protected LifebarPositions? lifebarPosition;
        protected Clock? lifebarClock;
        protected Time lifebarTime;

        protected RenderWindow renderWindow;
    }

    class Enemy: Hostile
    {
        public Enemy(Vector2f startingPosition, int health, Texture texture): base(startingPosition, health, texture)
        {
            renderWindow = Game.GameWindowInstance;
            sound = new Sound();

            bulletBP = new EnemyBullet(PositionBullet());
            isAggressive = true;
        }
        
        public override void Draw()
        {
            renderWindow.Draw(sprite);
            lifebar?.Draw(renderWindow);
        }

        public override void Update()
        {
            bulletBP.X = X + XSize / 2;
            bulletBP.Y = Y + YSize;

            if (lifebar != null)
            {
                UpdateLifebar();
            }
        }

        Vector2f PositionBullet()
        {
            return new Vector2f(X + XSize / 2, Y + YSize);
        }

        public override void AddLifebar(LifebarPositions lbPosition)
        {
            lifebarPosition = lbPosition;

            Vector2f dim = new Vector2f(XSize, YSize * 0.2f);
            Vector2f lifebarTopLeftCorner = new Vector2f(X, 0);
            
            if (lifebarPosition == LifebarPositions.Above) lifebarTopLeftCorner.Y = Y - 2 - dim.Y;
            else if (lifebarPosition == LifebarPositions.Below) lifebarTopLeftCorner.Y = Y + dim.Y + 2;

            lifebar = new Lifebar(lifebarTopLeftCorner, dim, health / maxHealth);

            lifebarClock = new Clock();
        }

        void UpdateLifebar()
        {
            if (lifebarClock.ElapsedTime >= lifebarTime)
            {
                RemoveLifebar();
                return;
            }

            lifebar.Percentage = (float)health / maxHealth;

            lifebar.X = X;
            if (lifebarPosition == LifebarPositions.Above) lifebar.Y = Y - 2 - lifebar.Dimensions.Y;
            else if (lifebarPosition == LifebarPositions.Below) lifebar.Y = Y + lifebar.Dimensions.Y + 2;
        }

        public override void RemoveLifebar()
        {
            lifebar = null;
            lifebarPosition = null;
        }

        public override void Fire()
        {
            EnemyBullet bullet = bulletBP.Copy();
            Game.EnemyBulletController.RegisterBullet(bullet);
        }

        public override int PointValue { get { return pointValue * bulletBP.BulletModePoints; } }

        Sound sound;
    }

    class Meteor: Hostile
    {
        public Meteor(Vector2f startingPosition, Vector2f direction, float speed, int health, Texture texture): base(startingPosition, health, texture)
        {
            Random random = new Random();
            int x = 0;
            while (x == 0) x = random.Next(-2, 3);
            float scale = (x < 0 ? -1f / x : x);
            sprite.Scale = new Vector2f(scale, scale);

            this.direction = new Vector2f(direction.X, direction.Y);
            this.speed = speed;

            isAggressive = false;

            sound = new Sound();
        }

        public override void Update()
        {

            if (lifebar != null)
            {
                UpdateLifebar();
            }
        }

        public void Move()
        {
            Console.WriteLine($"{sprite.CPointer} --> {position}");
            position += direction * speed;
        }

        public override void Draw()
        {
            renderWindow.Draw(sprite);
            lifebar?.Draw(renderWindow);
        }
        
        public override void AddLifebar(LifebarPositions lbPosition)
        {
            lifebarPosition = lbPosition;

            Vector2f dim = new Vector2f(XSize, YSize * 0.2f);
            Vector2f lifebarTopLeftCorner = new Vector2f(X, 0);

            if (lifebarPosition == LifebarPositions.Above) lifebarTopLeftCorner.Y = Y - 2 - dim.Y;
            else if (lifebarPosition == LifebarPositions.Below) lifebarTopLeftCorner.Y = Y + dim.Y + 2;

            lifebar = new Lifebar(lifebarTopLeftCorner, dim, health / maxHealth);

            lifebarClock = new Clock();
        }

        void UpdateLifebar()
        {
            if (lifebarClock?.ElapsedTime >= lifebarTime)
            {
                RemoveLifebar();
                return;
            }

            lifebar.Percentage = (float)health / maxHealth;

            lifebar.X = X;
            if (lifebarPosition == LifebarPositions.Above) lifebar.Y = Y - 2 - lifebar.Dimensions.Y;
            else if (lifebarPosition == LifebarPositions.Below) lifebar.Y = Y + lifebar.Dimensions.Y + 2;
        }

        public override void RemoveLifebar()
        {
            lifebar = null;
            lifebarPosition = null;
        }

        public override void Fire()
        {

        }

        public override int PointValue { get { return (int)(pointValue * sprite.Scale.X); } }

        Vector2f direction;
        float speed;

        Sound sound;

    }
}

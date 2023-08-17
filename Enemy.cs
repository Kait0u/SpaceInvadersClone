using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using System.Diagnostics.Metrics;
using System.Net;
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
            if (isProtected) renderWindow.Draw(shield);
            renderWindow.Draw(sprite);
            lifebar?.Draw(renderWindow);
        }
        public abstract void Update();

        public abstract void AddLifebar(LifebarPositions lbPosition);
        public virtual void RemoveLifebar()
        {
            lifebar = null;
            lifebarPosition = null;
        }

        public abstract void Fire();

        public virtual void Shield()
        {
            isProtected = true;
            shield = new CircleShape(XSize / 2f)
            {
                OutlineColor = new Color(223, 24, 0, 200),
                OutlineThickness = 2,
                FillColor = new Color(0, 0, 0, 0)
            };
        }

        public virtual void Shield(Color color)
        {
            Shield();
            shield.OutlineColor = color;
        }

        public virtual void UpdateShield()
        {
            shield.Position = new Vector2f(X, Y);
        }

        public virtual void Unshield()
        {
            isProtected = false;
            shield = null;
        }

        protected Vector2f PositionBullet()
        {
            return new Vector2f(X + XSize / 2, Y + YSize);
        }

        public virtual float X { get => position.X; set { position.X = value; sprite.Position = position; RecalculateBonusSpawnPosition(); } }
        public virtual float Y { get => position.Y; set { position.Y = value; sprite.Position = position; RecalculateBonusSpawnPosition(); } }
        public virtual int XSize => (int)sprite.GetGlobalBounds().Width;
        public virtual int YSize => (int)sprite.GetGlobalBounds().Height;

        public virtual Sprite EnemySprite => sprite;
        public virtual EnemyBullet? BulletBP => bulletBP;
        public virtual bool IsAggressive => isAggressive;

        public virtual bool IsProtected { get { return isProtected; } }
        public virtual CircleShape ShieldSprite => shield;

        public virtual Vector2f BonusSpawnPoint => bonusSpawnPosition;
        public virtual int EnemyHealth { get => health; set { health = value; } }
        public abstract int PointValue { get; }


        protected Sprite sprite;

        protected int health, maxHealth, pointValue;
        protected Vector2f position, bonusSpawnPosition;

        protected bool isAggressive; // Is it supposed to shoot?
        protected EnemyBullet? bulletBP;

        protected bool isProtected = false; // Is it supposed to take damage?
        protected CircleShape shield;

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

        public override void Update()
        {
            bulletBP.X = X + XSize / 2;
            bulletBP.Y = Y + YSize;

            if (lifebar != null)
            {
                UpdateLifebar();
            }

            if (shield != null)
            {
                UpdateShield();
            }
        }

        public override void AddLifebar(LifebarPositions lbPosition)
        {
            if (isProtected) return;

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
            sprite.Origin = new Vector2f(XSize, YSize) / 2f;
            sprite.Rotation = random.Next(0, 180);
            sprite.Scale = new Vector2f(scale, scale);

            this.direction = new Vector2f(direction.X, direction.Y);
            this.speed = speed;

            rotationDirection = random.Next(2) * 2 - 1;

            isAggressive = false;

            sound = new Sound();
        }

        public override void Update()
        {
            sprite.Position = position;
            bonusSpawnPosition.X = position.X;

            if (lifebar != null)
            {
                UpdateLifebar();
            }
        }

        public void Move()
        {
            position += direction * speed;
            sprite.Rotation += rotationDirection * speed * 0.75f;
        }
        
        public override void AddLifebar(LifebarPositions lbPosition)
        {
            lifebarPosition = lbPosition;

            Vector2f dim = new Vector2f(XSize, YSize * 0.2f);
            float x = sprite.GetGlobalBounds().Left;
            float y = sprite.GetGlobalBounds().Top;
            if (lifebarPosition == LifebarPositions.Above) y = y - 2 - dim.Y;
            else if (lifebarPosition == LifebarPositions.Below) y = y + dim.Y + 2;
            Vector2f lifebarTopLeftCorner = new Vector2f(x, y);

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

            lifebar.X = sprite.GetGlobalBounds().Left;
            if (lifebarPosition == LifebarPositions.Above) lifebar.Y = sprite.GetGlobalBounds().Top - 2 - lifebar.Dimensions.Y;
            else if (lifebarPosition == LifebarPositions.Below) lifebar.Y = sprite.GetGlobalBounds().Top + lifebar.Dimensions.Y + 2;
        }

        public override void Fire()
        {
            return;
        }

        public override int PointValue { get { return (int)(pointValue * sprite.Scale.X * 100); } }

        public int RotationDirection { get => rotationDirection; set { rotationDirection = value / Math.Abs(value); } }

        Vector2f direction;
        int rotationDirection;
        float speed;

        Sound sound;

    }

    class BossEnemy : Hostile
    {
        public BossEnemy(Vector2f startingPosition, int health, int weaponLv) : base(startingPosition, health, TextureBank.EnemyTexture3)
        {
            random = new Random();

            sprite.Scale = new Vector2f(scale, scale);

            isAggressive = true;
            bulletBPs = new List<EnemyBullet>();
            directedBulletBPs = new List<EnemyBullet>();
            directedAngles = new Dictionary<EnemyBullet, float>();
            straightBulletBP = new EnemyBullet(PositionBullet());
            straightBulletBP.SetVerticalDownfall(bulletSpeed);
            bulletBPs.Add(straightBulletBP);
            targettedBulletBP = new EnemyBullet(PositionBullet());
            targettedBulletBP.SetTargetted(bulletSpeed);
            bulletBPs.Add(targettedBulletBP);

            float deltaTheta = 360f / weaponLv;
            for (int i = 0; i < weaponLv; ++i)
            {
                float theta = i * deltaTheta * Utilities.Utilities.DegRadConversionConstant;
                Vector2f dir = new Vector2f((float)Math.Cos(theta), (float)Math.Sin(theta));

                EnemyBullet b = new EnemyBullet(PositionBullet());
                b.SetDirected(dir, bulletSpeed);
                directedBulletBPs.Add(b);
                directedAngles.Add(b, theta);
                bulletBPs.Add(b);
            }

            sound = new Sound();

            Shield();
        }

        public override void Update()
        {
            if (shield != null) UpdateShield();
            if (lifebar != null) UpdateLifebar();

            Vector2f pos = PositionBullet();
            foreach (EnemyBullet b in bulletBPs) (b.X, b.Y) = (pos.X, pos.Y);

            foreach (EnemyBullet b in directedBulletBPs)
            {
                directedAngles[b] += revolutionSpeedDegrees * Utilities.Utilities.DegRadConversionConstant;
                float theta = directedAngles[b];
                Vector2f dir = new Vector2f((float)Math.Cos(theta), (float)Math.Sin(theta));
                b.SetDirected(dir, bulletSpeed);
            }

        }

        public override void Shield()
        {
            isProtected = true;
            shield = new CircleShape(XSize / 2f)
            {
                OutlineColor = new Color(223, 24, 0, 200),
                OutlineThickness = 6,
                FillColor = new Color(0, 0, 0, 0)
            };
        }

        public override void Fire()
        {
            if ((double)health / maxHealth < 0.3f) FireDirected();
            else
            {
                int x = random.Next(3);
                if (x == 0) FireDirected();
                else if (x == 1) FireStraight();
                else if (x == 2) FireTargetted();
            }
        }

        void FireStraight()
        {
            EnemyBullet bullet = straightBulletBP.Copy();
            Game.EnemyBulletController.RegisterBullet(bullet);
        }

        void FireDirected()
        {
            foreach (EnemyBullet b in directedBulletBPs)
            {
                EnemyBullet bullet = b.Copy();
                Game.EnemyBulletController.RegisterBullet(bullet);
            }
        }

        void FireTargetted()
        {
            EnemyBullet bullet = targettedBulletBP.Copy();
            Game.EnemyBulletController.RegisterBullet(bullet);
        }

        public override void AddLifebar(LifebarPositions lbPosition)
        {
            if (isProtected) return;

            lifebarPosition = lbPosition;

            Vector2f dim = new Vector2f(XSize, YSize * 0.2f);
            float x = sprite.GetGlobalBounds().Left;
            float y = sprite.GetGlobalBounds().Top;
            if (lifebarPosition == LifebarPositions.Above) y = y - 2 - dim.Y;
            else if (lifebarPosition == LifebarPositions.Below) y = y + dim.Y + 2;
            Vector2f lifebarTopLeftCorner = new Vector2f(x, y);

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

        public override int PointValue => health * 5000;

        Game game;
        Random random;

        List<EnemyBullet> directedBulletBPs;
        Dictionary<EnemyBullet, float> directedAngles;
        const float revolutionSpeedDegrees = 15f;
        EnemyBullet straightBulletBP, targettedBulletBP;
        List<EnemyBullet> bulletBPs;


        Sound sound;
        const int scale = 4;
        const int bulletSpeed = 8;
    }
}

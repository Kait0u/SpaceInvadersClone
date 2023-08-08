using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using static SpaceInvadersClone.Enemy;

namespace SpaceInvadersClone
{
    class Hostile
    {
        public virtual void Draw() { }
        public virtual void Update() { }

        public virtual void AddLifebar(LifebarPositions lbPosition) { }
        public virtual void RemoveLifebar() { }

        public virtual void Fire() { }

        public virtual float X { get; set; }
        public virtual float Y { get; set; }
        public virtual int XSize { get; }
        public virtual int YSize { get; }

        public virtual Sprite EnemySprite => sprite;
        public virtual RectCollider Collider => collider;
        public virtual EnemyBullet BulletBP => bulletBP;

        public virtual Vector2f BonusSpawnPoint { get; }
        public virtual float BonusSpawnPointX { get; set; }
        public virtual float BonusSpawnPointY { get; set; }
        public virtual int EnemyHealth { get; set; }
        public virtual int PointValue { get; }

        Sprite sprite;
        RectCollider collider;
        EnemyBullet bulletBP;

    }

    class Enemy: Hostile
    {
        public Enemy(Vector2f startingPosition, int health, Texture texture)
        {
            renderWindow = Game.GameWindowInstance;

            sprite = new Sprite(texture);
            position = startingPosition;
            sprite.Position = position;
            collider = new RectCollider(position, position + new Vector2f(XSize, YSize));
            bulletBP = new EnemyBullet(PositionBullet());
            bonusSpawnPosition = position + new Vector2f(sprite.Texture.Size.X, sprite.Texture.Size.Y) / 2f 
                                 - new Vector2f(TextureBank.MedkitTexture.Size.X, TextureBank.MedkitTexture.Size.Y) / 2f;

            this.health = health;
            maxHealth = health;
            pointValue = health;

            lifebarTime = Time.FromSeconds(10);

            sound = new Sound();
        }
        
        public override void Draw()
        {
            renderWindow.Draw(sprite);

            lifebar?.Draw(renderWindow);
        }

        public override void Update()
        {
            BulletBP.X = X + XSize / 2;
            BulletBP.Y = Y + YSize;

            if (lifebar != null)
            {
                UpdateLifebar();
            }
        }

        Vector2f PositionBullet()
        {
            return new Vector2f(X + XSize / 2, Y + YSize);
        }

        public enum LifebarPositions { Above, Below }
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

        public override float X { get { return position.X; } set { position.X = value; sprite.Position = position; } }
        public override float Y { get { return position.Y; } set { position.Y = value; sprite.Position = position; } }

        public override int XSize { get { return (int)sprite.Texture.Size.X; } }
        public override int YSize { get { return (int)sprite.Texture.Size.Y; } }

        public override Sprite EnemySprite { get { return sprite; } }
        public override RectCollider Collider { get { return collider; } }
        public override EnemyBullet BulletBP { get { return bulletBP; } }

        public override Vector2f BonusSpawnPoint { get { return bonusSpawnPosition; } }
        public override float BonusSpawnPointX { get { return bonusSpawnPosition.X; } set { bonusSpawnPosition.X = value;  } }
        public override float BonusSpawnPointY { get { return bonusSpawnPosition.Y; } set { bonusSpawnPosition.Y = value;  } }

        public override int EnemyHealth { get { return health; } set { health = value; } }

        public override int PointValue { get { return pointValue * bulletBP.BulletModePoints; } }

        int health, maxHealth, pointValue;
        Lifebar? lifebar;
        LifebarPositions? lifebarPosition;
        Clock lifebarClock;
        Time lifebarTime;

        Vector2f position, bonusSpawnPosition;
        Sprite sprite;
        RectCollider collider;
        EnemyBullet bulletBP;

        RenderWindow renderWindow;
        Sound sound;
    }

    class Meteor: Hostile
    {
        public Meteor(Vector2f startingPosition, Vector2f direction, float speed, int health, Texture texture) 
        {
            renderWindow = Game.GameWindowInstance;

            sprite = new Sprite(texture);
            position = startingPosition;
            sprite.Position = position;

            Random random = new Random();
            int x = 0;
            while (x == 0) x = random.Next(-2, 3);
            float scale = (x < 0 ? -1f / x : x);
            sprite.Scale = new Vector2f(scale, scale);

            collider = new RectCollider(position, position + new Vector2f(XSize, YSize));

            this.direction = new Vector2f(direction.X, direction.Y);
            this.speed = speed;

            this.health = health;
            maxHealth = health;
            pointValue = health;

            lifebarTime = Time.FromSeconds(10);

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
        public enum LifebarPositions { Above, Below }
        
        public void AddLifebar(LifebarPositions lbPosition)
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

        }

        public override float X { get { return position.X; } set { position.X = value; sprite.Position = position; } }
        public override float Y { get { return position.Y; } set { position.Y = value; sprite.Position = position; } }

        public override int XSize { get { return (int)sprite.GetGlobalBounds().Width; } }
        public override int YSize { get { return (int)sprite.GetGlobalBounds().Height; } }

        public override Sprite EnemySprite { get { return sprite; } }
        public override RectCollider Collider { get { return collider; } }
        public override EnemyBullet BulletBP { get { return bulletBP; } }

        public override Vector2f BonusSpawnPoint { get { return bonusSpawnPosition; } }
        public override float BonusSpawnPointX { get { return bonusSpawnPosition.X; } set { bonusSpawnPosition.X = value; } }
        public override float BonusSpawnPointY { get { return bonusSpawnPosition.Y; } set { bonusSpawnPosition.Y = value; } }

        public override int EnemyHealth { get { return health; } set { health = value; } }

        public override int PointValue { get { return pointValue * bulletBP.BulletModePoints; } }

        int health, maxHealth, pointValue;
        Lifebar? lifebar;
        LifebarPositions? lifebarPosition;
        Clock? lifebarClock;
        Time lifebarTime;

        Vector2f position, bonusSpawnPosition, direction;
        float speed;
        Sprite sprite;
        RectCollider collider;
        EnemyBullet? bulletBP;

        RenderWindow renderWindow;
        Sound sound;

    }
}

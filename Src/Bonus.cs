using SFML.Graphics;
using SFML.System;

namespace SpaceInvadersClone
{
    abstract class Bonus
    {
        public Bonus(Vector2f initialPosition, float velocity)
        {
            this.velocity = velocity;
            position = new Vector2f(initialPosition.X, initialPosition.Y);
            direction = new Vector2f(0, 1);
        }

        public abstract void Update();

        public abstract void Action(Player player); // What happens when picked up

        public float X { get { return position.X; } set { position.X = value; } }
        public float Y { get { return position.Y; } set { position.Y = value; } }

        public virtual int XSize => (int)sprite.GetGlobalBounds().Width;
        public virtual int YSize => (int)sprite.GetGlobalBounds().Height;

        public Sprite BonusSprite { get { return sprite; } set { sprite = value; } }

        protected Vector2f Position { get { return position; } set { position = value; } }
        protected Vector2f Direction { get { return direction; } }
        protected float Velocity { get { return velocity; } set { velocity = value; } }

        float velocity;
        Vector2f position, direction;
        Sprite sprite;
    }

    class PowerUp : Bonus
    {
        public PowerUp(Vector2f initialPosition, float velocity) : base(initialPosition, velocity)
        {
            BonusSprite = new Sprite(TextureBank.PowerupTexture);
        }

        public override void Update()
        {
            base.Position += base.Direction * base.Velocity;
            BonusSprite.Position = base.Position;
        }

        public override void Action(Player player)
        {
            // Enhance player's weapon
            ++player.WeaponLevel;
        }

        public override int XSize => (int)BonusSprite.GetGlobalBounds().Width;
        public override int YSize => (int)BonusSprite.GetGlobalBounds().Height;
    }

    class Medkit: Bonus
    {
        public Medkit(Vector2f initialPosition, float velocity) : base(initialPosition, velocity)
        {
            BonusSprite = new Sprite(TextureBank.MedkitTexture);
        }
        public override void Update()
        {
            base.Position += base.Direction * base.Velocity;
            BonusSprite.Position = base.Position;
        }

        public override void Action(Player player)
        {
            // Heal player
            player.Heal(healValue);
        }

        public override int XSize => (int)BonusSprite.GetGlobalBounds().Width;
        public override int YSize => (int)BonusSprite.GetGlobalBounds().Height;

        const int healValue = 10;
    }
}

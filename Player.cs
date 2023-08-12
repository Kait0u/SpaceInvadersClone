using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;

namespace SpaceInvadersClone
{
    internal class Player
    {
        public Player() 
        {
            renderWindow = Application.GameWindowInstance;

            sprite = new Sprite(TextureBank.PlayerTexture);
            position = new Vector2f(0, 0);
            bulletBP = new PlayerBullet(new Vector2f(X + XSize / 2, Y));
            clock = new Clock();
            sound = new Sound();

            weaponLevel = 1;
            FireRate = 4f;
            canFire = true;

            currHealth = health;
            isDead = false;
            deathSound = new Sound(SoundBank.PlayerExplosion);
            
            isProtected = false;
            shieldClock = new Clock();
            shieldDuration = Time.FromSeconds(0);
            shield = new CircleShape(XSize / 2f) { OutlineColor = Color.Cyan, FillColor = new Color(255, 255, 255, 0), OutlineThickness = 2f, Position = sprite.Position };

            controlLost = false;
            forcedDirection = new Vector2f(0, 0);
            forcedPosition = new Vector2f(0, 0);
        }

        public void Reset()
        {
            sprite = new Sprite(TextureBank.PlayerTexture);
            position = new Vector2f(0, 0);
            bulletBP = new PlayerBullet(new Vector2f(X + XSize / 2, Y));
            clock = new Clock();
            sound = new Sound();

            weaponLevel = 1;
            FireRate = 4f;
            canFire = true;

            currHealth = health;
            isDead = false;
            deathSound = new Sound(SoundBank.PlayerExplosion);

            isProtected = false;
            shieldClock = new Clock();
            shieldDuration = Time.FromSeconds(0);
            shield = new CircleShape(XSize / 2f) { OutlineColor = Color.Cyan, FillColor = new Color(255, 255, 255, 0), OutlineThickness = 2f, Position = sprite.Position };

            controlLost = false;
        }

        public void Draw()
        {
            if (isDead) return;
            if (isProtected) renderWindow.Draw(shield);
            renderWindow.Draw(sprite);
        }

        public void Update()
        {
            if (currHealth <= 0) isDead = true;
            if (isDead)
            {
                deathSound.Play();
                Application.SoundController.RegisterSound(sound);
                while (deathSound.Status == SoundStatus.Playing) { }
                return;
            }
                

            canFire = clock.ElapsedTime.AsMilliseconds() >= fireCooldown.AsMilliseconds();

            if (isProtected && !(shieldClock.ElapsedTime <= shieldDuration)) Unshield();

            if (!controlLost) Controls();
            else
            {
                Shield(Time.FromSeconds(0.25f));
                Vector2f delta = forcedDirection * 2 * speed;
                if (Math.Abs(fullForcedDirection.X) - Math.Abs(delta.X) >= speed || Math.Abs(fullForcedDirection.Y) - Math.Abs(delta.Y) >= speed)
                {
                    position += delta;
                    fullForcedDirection -= delta;

                    //Console.WriteLine(position);
                    //Console.WriteLine(fullForcedDirection);
                }
                else
                {
                    controlLost = false;
                    position.X = forcedPosition.X;
                    position.Y = forcedPosition.Y;
                    forcedDirection = new Vector2f(0, 0);
                }
                

            }

            position.X = Math.Max(0, position.X);
            position.X = Math.Min(renderWindow.Size.X - sprite.GetGlobalBounds().Width, position.X);
            position.Y = Math.Max(0, position.Y);
            position.Y = Math.Min(renderWindow.Size.Y - sprite.GetGlobalBounds().Height, position.Y);


            sprite.Position = position;
            shield.Position = position;
            bulletBP.X = position.X + XSize / 2 - bulletBP.XSize / 2;
            bulletBP.Y = position.Y;

        }

        void Controls()
        {
            var ip = Keyboard.IsKeyPressed;
            int h = 0, v = 0;
            bool boost = false;

            if (ip(Keyboard.Key.W) || ip(Keyboard.Key.Up)) v = -1;
            else if (ip(Keyboard.Key.S) || ip(Keyboard.Key.Down)) v = 1;

            if (ip(Keyboard.Key.D) || ip(Keyboard.Key.Right)) h = 1;
            else if (ip(Keyboard.Key.A) || ip(Keyboard.Key.Left)) h = -1;

            if (ip(Keyboard.Key.LShift)) boost = true;

            if (ip(Keyboard.Key.Space)) Fire();


            if (h != 0 || v != 0) Move(h, v, boost);
        }

        void Move(int horizMult, int vertiMult, bool boost = false)
        {
            Vector2f delta = new Vector2f(0, 0);

            if (horizMult == 0 || vertiMult == 0) 
            {
                delta.X = horizMult * speed;
                delta.Y = vertiMult * speed;
            }

            else
            {
                delta.X += horizMult * speed * revSqrt2;
                delta.Y += vertiMult * speed * revSqrt2;
            }

            Vector2f temp = new Vector2f(position.X, position.Y);
            delta *= boost ? boostMultiplier : 1;
            position += delta;

            if (position.X < 0 || position.X >= renderWindow.Size.X - XSize)
            {
                position.X = temp.X;
            }

            if (position.Y < 0 || position.Y >= renderWindow.Size.Y - YSize)
            {
                position.Y = temp.Y;
            }
        }

        public void ForceMove(Vector2f forcedPosition)
        {
            if (!controlLost)
            {
                controlLost = true;
                this.forcedPosition = forcedPosition;

                if (this.forcedPosition.X < 0) this.forcedPosition.X = 0;
                else if (this.forcedPosition.X >= renderWindow.Size.X - XSize) this.forcedPosition.X = renderWindow.Size.X - XSize;

                if (this.forcedPosition.Y < 0) this.forcedPosition.Y = 0;
                else if (this.forcedPosition.Y >= renderWindow.Size.Y - YSize) this.forcedPosition.Y = renderWindow.Size.Y - YSize;
                this.fullForcedDirection = this.forcedPosition - this.position;
                this.forcedDirection = this.fullForcedDirection / (float)Utilities.Utilities.Magnitude(fullForcedDirection);

                //Console.WriteLine($"P: {this.position}");
                //Console.WriteLine($"FP:{this.forcedPosition}");
                //Console.WriteLine($"FFD: {this.fullForcedDirection} | {(float)Utilities.Utilities.Magnitude(fullForcedDirection)}");
                //Console.WriteLine($"FD: {this.forcedDirection}");
                //Console.WriteLine();
            }
        }

        void Fire()
        {
            if (canFire)
            {
                sound.SoundBuffer = SoundBank.PlayerFire;

                PlayerBullet bullet = bulletBP.Copy();
                Game.PlayerBulletController.RegisterBullet(bullet);
                canFire = false;
                clock.Restart();

                sound.Play();
                Application.SoundController.RegisterSound(sound);
            }
        }

        public void Damage(int damage)
        {
            if (isProtected) return;

            sound.SoundBuffer = SoundBank.PlayerDamaged;

            CurrentHealth -= damage;

            sound.Play();
            Application.SoundController.RegisterSound(sound);
        }

        public void Heal(int amount)
        {
            CurrentHealth += amount;
        }

        public void Shield(Time duration)
        {
            shieldClock.Restart();
            shieldDuration = duration;
            isProtected = true;
        }

        public void Unshield()
        {
            shieldClock.Restart();
            shieldDuration = Time.FromSeconds(0);
            isProtected = false;
        }

        public float X { get { return position.X; } set { position.X = value; } }
        public float Y { get { return position.Y; } set { position.Y = value; } }

        public int XSize { get { return (int) sprite.Texture.Size.X; } }
        public int YSize { get { return (int) sprite.Texture.Size.Y; } }

        float FireRate { get { return fireRate; } set { fireRate = value; fireCooldown = Time.FromSeconds(1 / fireRate); } }
        public int WeaponLevel { get { return weaponLevel; } set { weaponLevel = value; FireRate = FireRate + 0.5f * (weaponLevel - 1); } }

        public int CurrentHealth { get { return currHealth; } private set { currHealth = Math.Max(0, Math.Min(value, 100)); } }
        public bool IsDead { get { return isDead; } }
        public bool IsProtected { get { return isProtected; } }
        public bool LostControl { get { return controlLost; } }

        public Sprite PlayerSprite { get { return  sprite; } }
        public Sound Sound { get { return sound; } }

        Sprite sprite;
        RenderWindow renderWindow;
        Vector2f position;
        PlayerBullet bulletBP;


        int currHealth;
        const int health = 100;
        const int speed = 5;
        const float boostMultiplier = 1.5f;
        int weaponLevel;
        float fireRate; // How many shots per second
        Time fireCooldown;
        Clock clock;
        bool canFire, isDead, isProtected;

        Clock shieldClock;
        Time shieldDuration;
        CircleShape shield;

        bool controlLost;
        Vector2f forcedPosition, fullForcedDirection, forcedDirection;

        Sound sound;
        Sound deathSound;

        const float revSqrt2 = 0.7071067811865475244008443621048490392848359376884740365883398689f;
    }
}

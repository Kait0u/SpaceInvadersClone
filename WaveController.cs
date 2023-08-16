using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using System;
using System.Diagnostics.Metrics;
using Utilities;

namespace SpaceInvadersClone
{
    abstract class EnemyGroup
    {
        public EnemyGroup() 
        {
            renderWindow = Game.GameWindowInstance;
            enemies = new List<Hostile>();
        }

        public abstract void Update();

        public abstract void Move();

        //public virtual void ForceMove()
        //{ 

        //}

        public virtual void PlayIntro() { if (!intro) intro = true; }
        public abstract void Intro();

        protected RenderWindow renderWindow;

        List<Hostile> enemies;

        public virtual List<Hostile> EnemyList { get { return enemies; } }

        public virtual Vector2f Direction { get { return direction; } set { direction = value; } }
        public virtual Time MovementCooldown { get { return movementBreak; } set { movementBreak = value; } }

        public virtual Vector2f TopLeftCorner { get { return topLeftCorner; } set { topLeftCorner = value; } }
        public virtual Vector2f BottomRightCorner { get { return bottomRightCorner; } set { bottomRightCorner = value; } }

        public virtual bool IsAggressive => isAggressive;
        public virtual bool IsIntro => intro;

        Time movementBreak;
        Vector2f topLeftCorner, bottomRightCorner, direction, tempDirection;
        bool isAggressive;
        protected bool intro = false;

        const int enemySize = 32;
    }

    class PlaceholderWave: EnemyGroup
    {
        public override void PlayIntro() { }
        public override void Intro() { }
        public override void Update() { }
        public override void Move() { }
    }

    class Wave: EnemyGroup
    {
        public Wave(int health = 1) 
        {
            random = new Random();
            renderWindow = Game.GameWindowInstance;
            sound = new Sound();
            movementClock = new Clock();
            movementBreak = Time.FromMilliseconds(500);
            enemies = base.EnemyList;

            maxSpaceWidth = (int)(renderWindow.Size.X - 2 * enemySize);
            maxWidth = (int)(renderWindow.Size.X - 2 * enemySize) / enemySize;
            maxSpaceHeight = (int)(renderWindow.Size.Y / 2 - enemySize);
            maxHeight = (int)((renderWindow.Size.Y / 2 - enemySize) / enemySize);
            int width = random.Next(1, maxWidth + 1);
            int height = random.Next(1, maxHeight + 1);

            int paddingX = (maxSpaceWidth - width * enemySize) / width;
            int paddingY = (maxSpaceHeight - height * enemySize) / height;

            direction = new Vector2f(random.Next(0, 2) * 2 - 1, 0);

            topLeftCorner = new Vector2f(enemySize, enemySize);
            bottomRightCorner = new Vector2f();

            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    float x = paddingX / 2 + topLeftCorner.X + j * enemySize + Math.Max(0, j * paddingX);
                    float y = paddingY / 2 + topLeftCorner.Y + i * enemySize + Math.Max(0, i * paddingY);

                    Vector2f startingPosition = new Vector2f(x, y);
                    Enemy enemy = new Enemy(startingPosition, health, textures[random.Next(textures.Length)]);
                    enemies.Add(enemy);

                    bottomRightCorner.X = x + enemy.XSize;
                    bottomRightCorner.Y = y + enemy.YSize;
                }
            }

            descent = false;
            descentBuffer = enemySize;
            tempDirection = new Vector2f(direction.X, direction.Y);
        }

        public override void Update()
        {
            if (intro) { Intro(); return; }

            float minX = EnemyList[0].X, 
                maxX = EnemyList[0].X + EnemyList[0].XSize,
                minY = EnemyList[0].Y,
                maxY = EnemyList[0].Y + EnemyList[0].YSize;
            
            Move();
            
            foreach (Enemy enemy in EnemyList)
            {
                enemy.Update();

                minX = Math.Min(minX, enemy.X);
                maxX = Math.Max(maxX, enemy.X + enemy.XSize);
                minY = Math.Min(minY, enemy.Y);
                maxY = Math.Max(maxY, enemy.Y + enemy.YSize);

            }

            TopLeftCorner = new Vector2f(minX, minY);
            BottomRightCorner = new Vector2f(maxX, maxY);
        }

        public override void Move()
        {
            if (direction == null) return;
            if (movementClock.ElapsedTime < movementBreak) return;

            sound.SoundBuffer = SoundBank.EnemyMove;
            sound.Play();
            Application.SoundController.RegisterSound(sound);

            foreach (Enemy enemy in enemies)
            {
                enemy.X += movementSpeed * direction.X;
            }

            if (descent)
            {
                Lower();
                return;
            }

            else if (topLeftCorner.X + movementSpeed * direction.X <= 0 || bottomRightCorner.X + movementSpeed * direction.X >= renderWindow.Size.X)
            {
                descent = true;
                Lower();
                return;
            }

            if (!descent)
            {
                topLeftCorner += movementSpeed * direction;
                bottomRightCorner += movementSpeed * direction;
            }

            movementClock.Restart();
        }

        void Lower()
        {
            if (direction == null) return;

            direction = new Vector2f(0, 1);

            
            if (descent)
            {
                
                if (descentBuffer > movementSpeed) 
                {
                    foreach (Enemy enemy in enemies)
                    {
                        enemy.Y += movementSpeed;

                        // Kill those who are too far below the screen --> WaveController
                    }

                    descentBuffer -= movementSpeed;
                }

                else
                {
                    foreach (Enemy enemy in enemies)
                    {
                        enemy.Y += descentBuffer;

                        // Kill those who are too far below the screen --> WaveController
                    }

                    descentBuffer = enemySize;
                    descent = false;
                    direction = -tempDirection;
                    tempDirection = new Vector2f(direction.X, direction.Y);
                }

            }
            movementClock.Restart();
        }

        public override void Intro()
        {
            if (introCountdown <= 0) 
            { 
                intro = false;
                enemies.ForEach(e => e.Unshield());
                return; 
            }

            if (introClock.ElapsedTime >= introDeltaTime)
            { 
                foreach (Enemy enemy in enemies)  enemy.Y += introDeltaY;

                sound.SoundBuffer = SoundBank.EnemyMove;
                sound.Play();
                Application.SoundController.RegisterSound(sound);

                --introCountdown;
                introClock.Restart();
                
            }
        }

        public override void PlayIntro()
        {
            if (intro) return;

            intro = true;

            float displacementY = renderWindow.Size.Y / 2f;

            foreach (Enemy enemy in enemies)
            {
                enemy.Y -= displacementY;
                enemy.Shield();
            }

            Application.SoundController.RegisterPlaySound(new Sound(SoundBank.WaveEntrance));

            introClock = new Clock();
            introDeltaTime = movementBreak / 4f;
            introDeltaY = displacementY / 40f;
            introCountdown = 40;
        }



        public override List<Hostile> EnemyList { get { return enemies; } }
        public override Vector2f Direction { get { return direction; } set { direction = value; } }
        public override Time MovementCooldown { get { return this.movementBreak; } set { movementBreak = value; } }

        public override Vector2f TopLeftCorner { get { return topLeftCorner; } set { topLeftCorner = value; } }
        public override Vector2f BottomRightCorner { get { return bottomRightCorner; } set { bottomRightCorner = value; } }

        public override bool IsAggressive => this.isAggressive;

        bool isAggressive = true;
        Random random;
        RenderWindow renderWindow;
        List<Hostile> enemies;

        const int enemySize = 32;
        int maxSpaceWidth, maxWidth,maxSpaceHeight, maxHeight, width, height;
        bool descent;
        float descentBuffer;
        float movementSpeed = 5f;
        Clock movementClock;
        Time movementBreak;
        Vector2f topLeftCorner, bottomRightCorner, direction, tempDirection;
        static Texture[] textures = new Texture[2] { TextureBank.EnemyTexture1, TextureBank.EnemyTexture2 };

        Clock introClock;
        Time introDeltaTime;
        float introDeltaY;
        int introCountdown;

        Sound sound;
    }

    class Rain: EnemyGroup
    {
        private class Droplet
        {
            public Droplet(uint enemyCount, float speed, int health = 1)
            {
                renderWindow = Game.GameWindowInstance;
                random = new Random();
                
                enemies = new Queue<Hostile>();
                movementSpeed = speed;
                
                int x = random.Next((int)renderWindow.Size.X - enemySize);
                for (int i = 0; i < enemyCount; ++i)
                {    
                    int y = 0 - enemySize * (i + 1) - paddingY * i;
                    Texture texture = textures[random.Next(textures.Length)];
                    Vector2f startPos = new Vector2f(x, y);
                    enemies.Enqueue(new Enemy(startPos, health, texture));
                }
            }

            public void Move()
            {
                foreach (Enemy enemy in enemies)
                {
                    enemy.Y += movementSpeed;
                }
            }

            Random random;
            RenderWindow renderWindow;

            Queue<Hostile> enemies;
            public Queue<Hostile> EnemyQueue { get { return enemies; } }

            float movementSpeed;

            static Texture[] textures = new Texture[2] { TextureBank.EnemyTexture1, TextureBank.EnemyTexture2 };

            const int enemySize = 32;
            const int paddingY = 4;
        }

        public Rain(int health = 1) 
        {
            random = new Random();
            renderWindow = Game.GameWindowInstance;
            enemies = base.EnemyList;
            droplets = new List<Droplet>();
            movingDroplets = new List<Droplet>();
            dropletCount = random.Next(4, (int)(renderWindow.Size.X - 2 * enemySize) / (enemySize * 2) + 1);
            dropletDelay = new Dictionary<Droplet, Time>();
            Time dropletBreak = Time.FromMilliseconds(0);

            for (int i = 0; i < dropletCount; ++i)
            {
                uint perDroplet = (uint)random.Next(1, 12 + 1);
                float speed = random.Next(45, 96) / 10f;

                Droplet droplet = new Droplet(perDroplet, speed, health);
                
                foreach (Enemy enemy in droplet.EnemyQueue)
                {
                    enemies.Add(enemy);
                }

                droplets.Add(droplet);

                dropletDelay.Add(droplet, Time.FromMilliseconds(dropletBreak.AsMilliseconds()));

                int r = random.Next(50, 2000) * (i / 2 + 1);
                dropletBreak = Time.FromMilliseconds(r);

            }

            movementClock = new Clock();
            dropletDeployClock = new Clock();
            movementBreak = Time.FromMilliseconds(300);

            garbageList = new List<Droplet>();

            sound = new Sound(SoundBank.EnemyMove);
        }

        public override void Update()
        {
            if (intro) { Intro(); return; }

            foreach (Droplet droplet in droplets)
            {
                if (droplet.EnemyQueue.Count <= 0) garbageList.Add(droplet);
            }

            foreach (Droplet droplet in garbageList)
            {
                droplets.Remove(droplet);
                if (movingDroplets.Contains(droplet)) movingDroplets.Remove(droplet);
            }
            garbageList.Clear();

            foreach (Droplet droplet in droplets)
            {
                if (!movingDroplets.Contains(droplet) && dropletDeployClock.ElapsedTime >= dropletDelay[droplet])
                {
                    movingDroplets.Add(droplet);
                    dropletDeployClock.Restart();
                    break;
                }
            }

            Move();
        }

        public override void Move()
        {
            if (movementClock.ElapsedTime < movementBreak) return;

            sound.Play();
            Application.SoundController.RegisterSound(sound);

            foreach (Droplet droplet in movingDroplets)
            {
                droplet.Move();
            }
            
            movementClock.Restart();
        }

        public override void Intro()
        {
            if (introClock.ElapsedTime >= introDuration) intro = false;
        }

        public override void PlayIntro()
        {
            base.PlayIntro();

            introDuration = Time.FromSeconds(1.5f);
            introClock = new Clock();
            Application.SoundController.RegisterPlaySound(new Sound(SoundBank.RainfallEntrance));
        }

        public override Time MovementCooldown { get { return this.movementBreak; } set { this.movementBreak = value; } }

        public override bool IsAggressive => this.isAggressive;

        bool isAggressive = true;
        Random random;
        RenderWindow renderWindow;

        int dropletCount;
        List<Droplet> droplets, movingDroplets, garbageList;
        Dictionary<Droplet, Time> dropletDelay;

        List<Hostile> enemies;

        Clock movementClock, dropletDeployClock;
        Time movementBreak;

        Sound sound;

        Vector2f topLeftCorner, bottomRightCorner, direction;

        Clock introClock;
        Time introDuration;


        const int enemySize = 32;
    }

    class MeteorShower : EnemyGroup
    {
        class SpawnLine
        {
            public SpawnLine(int alphaDeg, int r)
            {
                renderWindow = Application.GameWindowInstance;

                this.alphaDeg = alphaDeg;
                this.alphaRad = DegToRad(alphaDeg);
                this.r = r;

                C = (Vector2f)renderWindow.Size / 2;
                A = new Vector2f((float)(r * Math.Cos(alphaRad)), -(float)(r * Math.Sin(alphaRad))) + C;
                B = new Vector2f((float)(r * Math.Cos(alphaRad + DegToRad(90))), -(float)(r * Math.Sin(alphaRad + DegToRad(90)))) + C;

                m = (float)(A.Y - B.Y) / (A.X - B.X);
                intercept = A.Y - A.X * m;
                thetaRad = (float)Math.Atan(m);

                f = (x) => m * x + intercept;

                //Vector2f t = new Vector2f((float)Math.Sin(thetaRad), (float)Math.Cos(thetaRad));
                //directionVector = t / (float)Utilities.Utilities.Magnitude(t);

                Vector2f t = A - B;
                t /= (float)Utilities.Utilities.Magnitude(t);
                directionVector = t;
                orthogonalVector = new Vector2f(directionVector.Y, -directionVector.X);

                //Vector2f u = new Vector2f(-(float)Math.Cos(thetaRad), (float)Math.Sin(thetaRad));
                //orthogonalVector = u / (float)Utilities.Utilities.Magnitude(u);

                //orthogonalVector = new Vector2f(-directionVector.Y, directionVector.X);
            }

            public float Evaluate(float xValue) => f(xValue);

            float DegToRad(float d) => d * degRadConversionCostant;

            RenderWindow renderWindow;
            Vector2f A, B, C;
            int alphaDeg, r;
            float alphaRad, thetaRad;

            float m, intercept;
            Func<float, float> f;
            Vector2f directionVector, orthogonalVector;

            public Vector2f DirectionVector => directionVector;
            public Vector2f OrthogonalVector => orthogonalVector;

            public Vector2f LowerEnd => -A.Y <= -B.Y ? A : B;
            public Vector2f HigherEnd => -A.Y > -B.Y ? A : B;



            const float degRadConversionCostant = 0.0174532925199432957692369076848861271344f;
        }

        public MeteorShower(int health = 1)
        {
            random = new Random();
            renderWindow = Game.GameWindowInstance;
            List<Texture> meteorTextures = new List<Texture>()
            {
                TextureBank.MeteoriteTexture1,
                TextureBank.MeteoriteTexture2,
                TextureBank.MeteoriteTexture3,
                TextureBank.MeteoriteTexture4
            };

            enemies = new List<Hostile>();
            movingMeteors = new List<Meteor>();
            meteorDelay = new Dictionary<Meteor, Time>();
            enemyCount = 15 * random.Next(4, 5 + (int)(Math.Exp(health) / 2));

            int alphaDeg = random.Next(15, 75 + 1);
            int r = (int)Utilities.Utilities.Magnitude(new Vector2f(renderWindow.Size.X/2 + maxEnemySize, renderWindow.Size.Y/2 + maxEnemySize)) + 1;
            spawnLine = new SpawnLine(alphaDeg, r);

            Vector2f dir = new Vector2f(spawnLine.OrthogonalVector.X, spawnLine.OrthogonalVector.Y);
            dir.X = spawnLine.HigherEnd.X < spawnLine.LowerEnd.X ? -Math.Abs(dir.X) : Math.Abs(dir.X);
            dir.Y = Math.Abs(dir.Y);

            float left = Math.Min(spawnLine.LowerEnd.X, spawnLine.HigherEnd.X);
            float right = Math.Max(spawnLine.LowerEnd.X, spawnLine.HigherEnd.X);

            for (int i = 0; i < enemyCount; ++i)
            {

                int x = random.Next((int)left, (int)right + 1);
                float y = spawnLine.Evaluate(x);
                Vector2f pos = new Vector2f(x, y);

                float speed = random.Next(35, 165 + 1) / 10f;

                int randInd = random.Next(meteorTextures.Count);

                Meteor meteor = new Meteor(pos, dir, speed, health, meteorTextures[randInd]);
                enemies.Add(meteor);

                Time meteorBreak = Time.FromMilliseconds(random.Next(150, 1750)) / 2;
                meteorDelay.Add(meteor, Time.FromMilliseconds(meteorBreak.AsMilliseconds()));
            }

            movementClock = new Clock();
            meteorDeployClock = new Clock();
            sound = new Sound(SoundBank.EnemyMove);
        }

        public override void Update()
        {
            if (intro) { Intro(); return; }

            foreach (Meteor meteor in enemies)
            {
                if (!movingMeteors.Contains(meteor) && meteorDeployClock.ElapsedTime >= meteorDelay[meteor])
                {
                    movingMeteors.Add(meteor);
                    meteorDeployClock.Restart();
                    break;
                }
            }

            Move();
        }

        public override void Move()
        {
            if (movementClock.ElapsedTime < movementBreak) return;

            sound.Play();
            Application.SoundController.RegisterSound(sound);

            foreach (Meteor meteor in movingMeteors)
            {
                meteor.Move();
            }

            movementClock.Restart();
        }

        public override void Intro()
        {
            if (introClock.ElapsedTime >= introDuration) intro = false;
        }

        public override void PlayIntro()
        {
            base.PlayIntro();

            introDuration = Time.FromSeconds(1f);
            introClock = new Clock();
            Application.SoundController.RegisterPlaySound(new Sound(SoundBank.MeteorEntrance));
        }

        public override List<Hostile> EnemyList => this.enemies;

        public override Time MovementCooldown { get { return this.movementBreak; } set { this.movementBreak = value; } }

        public override bool IsAggressive => this.isAggressive;

        bool isAggressive = false;
        Random random;
        RenderWindow renderWindow;
        SpawnLine spawnLine;
        List<Hostile> enemies;
        List<Meteor> movingMeteors;
        Dictionary<Meteor, Time> meteorDelay;
        int enemyCount;

        Clock movementClock, meteorDeployClock;
        Time movementBreak;

        Clock introClock;
        Time introDuration;

        Sound sound;

        const int maxEnemySize = 4 * 32;
    }

    // -------------------------------------------------------------------

    class WaveController
    {
        public WaveController() 
        {
            random = new Random();
            renderWindow = Application.GameWindowInstance;
            waveNumber = 0;
            isWave = false;
            isBreak = false;

            garbageList = new List<Hostile>();

            sound = new Sound();
        }

        public void Reset()
        {
            random = new Random();

            waveNumber = 0;
            isWave = false;
            isBreak = false;

            garbageList = new List<Hostile>();

            sound = new Sound();
        }

        public void StartWithBreak()
        {
            enemyGroup = new PlaceholderWave();
        }
        
        public void Start()
        {
            List<Action> models = new List<Action>() { StartWave, StartRain, StartMeteorShower };
            int choice = random.Next(models.Count);
            models[choice]();

            enemyGroup?.PlayIntro();
        }

        void StartWave()
        {
            if (!isWave && !isBreak)
            {
                Game.PlayerInstance.Shield(Time.FromSeconds(5));
                enemyGroup = new Wave(Math.Max(1, (int)(waveNumber / 2)));
                ++waveNumber;
                waveInitialSize = enemyGroup.EnemyList.Count;
                ArmEnemies();
                isWave = true;
                isBreak = false;
                clock = new Clock();
                // waveClock = clock; // ???
                CalculateTimeBoundaries();
                attackBreak = Time.FromMilliseconds(random.Next(800, 4500));
                enemyGroup.MovementCooldown = Time.FromMilliseconds(500);
                initialMovementCooldown = enemyGroup.MovementCooldown;
            }
        }

        void StartRain()
        {
            if (!isWave && !isBreak)
            {
                Game.PlayerInstance.Shield(Time.FromSeconds(5));
                enemyGroup = new Rain(Math.Max(1, (int)(waveNumber / 2)));
                ++waveNumber;
                waveInitialSize = enemyGroup.EnemyList.Count;
                ArmEnemies();
                isWave = true;
                isBreak = false;
                clock = new Clock();
                // waveClock = clock; // ???
                CalculateTimeBoundaries();
                attackBreak = Time.FromMilliseconds(random.Next(800, 4500 / 2));
                enemyGroup.MovementCooldown = Time.FromMilliseconds(300);
                initialMovementCooldown = enemyGroup.MovementCooldown;
            }
        }

        void StartMeteorShower()
        {
            if (!isWave && !isBreak)
            {
                Game.PlayerInstance.Shield(Time.FromSeconds(5));
                enemyGroup = new MeteorShower(2 * Math.Max(1, (int)(waveNumber / 2)));
                ++waveNumber;
                waveInitialSize = enemyGroup.EnemyList.Count;

                isWave = true;
                isBreak = false;
                clock = new Clock();
                // waveClock = clock; // ???
                CalculateTimeBoundaries();
                enemyGroup.MovementCooldown = Time.FromMilliseconds(150);
                initialMovementCooldown = enemyGroup.MovementCooldown;
            }
        }

        void ArmEnemies()
        {
            if (enemyGroup == null) return;
            if (!enemyGroup.IsAggressive) return;

            foreach (Enemy enemy in enemyGroup.EnemyList)
            {
                int x = random.Next(4);
                float vel = random.Next(200, 1001) / 100;
                switch (x)
                {
                    case 0:
                        enemy.BulletBP.SetVerticalDownfall(vel);
                        break;
                    case 1:
                        enemy.BulletBP.SetTargetted(vel);
                        break;
                    case 2:
                        enemy.BulletBP.SetVerticalSine(vel, random.Next(10, 70));
                        break;
                    case 3:
                        enemy.BulletBP.SetVerticalCosine(vel, random.Next(10, 70));
                        break;
                    default:
                        break;
                }
            }
        }

        public void Update()
        {
            if (isWave)
            {
                // if (enemyGroup.IsIntro) --> do the intro stuff 

                enemyGroup.Update();

                foreach (Hostile enemy in enemyGroup.EnemyList)
                {
                    enemy.Update();

                    if (!Game.PlayerInstance.LostControl && enemy.EnemySprite.GetGlobalBounds().Intersects(Game.PlayerInstance.PlayerSprite.GetGlobalBounds()))
                    {
                        Damage(enemy, Math.Max(1, (int)(waveNumber / 2)));
                        enemy.AddLifebar(Enemy.LifebarPositions.Above);

                        Game.PlayerInstance.Damage(40);
                        Application.SoundController.RegisterPlaySound(new Sound(SoundBank.PlayerExplosion));
                        Game.PlayerInstance.ForceMove(Game.PlayerSpawnPoint);
                        continue;
                    }

                    // Kill those who are too far below the screen
                    if (enemy.Y >= renderWindow.Size.Y + 2 * enemy.EnemySprite.Texture.Size.Y)
                    {
                        Kill(enemy);
                    }
                }

                if (enemyGroup.IsAggressive)
                {
                    if (clock.ElapsedTime >= attackBreak)
                    {
                        int attackerIndex = random.Next(enemyGroup.EnemyList.Count);

                        sound.SoundBuffer = SoundBank.EnemyFire;

                        enemyGroup.EnemyList[attackerIndex].Fire();

                        sound.Play();
                        Application.SoundController.RegisterSound(sound);

                        CalculateTimeBoundaries();
                        attackBreak = Time.FromMilliseconds(random.Next(timeBoundaryLeft, timeBoundaryRight + 1));
                        clock.Restart();
                    }
                }

                foreach (Hostile enemy in garbageList)
                {
                    enemyGroup.EnemyList.Remove(enemy);
                }
                garbageList.Clear();

                if (enemyGroup.EnemyList.Count == 0) isWave = false;

            }
            else
            {
                if (!isBreak)
                {
                    clock = new Clock();
                    isBreak = true;
                }

                else if (clock != null && clock.ElapsedTime >= breakTime)
                {
                    isBreak = false;

                    Start();
                    ArmEnemies();
                }
            }
            
        }

        public void Draw()
        {
            if (isBreak) return;

            foreach (Hostile enemy in enemyGroup.EnemyList)
            {
                enemy.Draw();
            }
        }

        public void Damage(Hostile enemy, int damageValue = 1)
        {
            if (enemy.IsProtected) 
            {
                Application.SoundController.RegisterPlaySound(new Sound(SoundBank.ShieldProtect));
                return;
            }

            sound.SoundBuffer = SoundBank.EnemyDamaged;

            enemy.EnemyHealth -= damageValue;
            if (enemy.EnemyHealth <= 0)
            {
                sound.SoundBuffer = SoundBank.EnemyExplosion;

                Kill(enemy);
                Game.Score += (ulong)enemy.PointValue;

                // 2% chance to drop a PowerUp on death
                Die die50 = new Die(50);
                if (die50.RandomRoll()) 
                    Game.BonusController.IssuePowerup(enemy.BonusSpawnPoint);

                // 0.5% chance to drop a MedKit on death
                Die die200 = new Die(200);
                if (die200.RandomRoll())
                    Game.BonusController.IssueMedkit(enemy.BonusSpawnPoint);
            }

            sound.Play();
            Application.SoundController.RegisterSound(sound);
        }

        public void Kill(Hostile enemy)
        {
            garbageList.Add(enemy);
            enemyGroup.MovementCooldown = Time.FromMilliseconds(Math.Max(100, (int)(((float)enemyGroup.EnemyList.Count / waveInitialSize) * initialMovementCooldown.AsMilliseconds())));
        }

        public EnemyGroup EnemyGroup { get { return enemyGroup; } }

        void CalculateTimeBoundaries()
        {
            if (enemyGroup == null) return;
            timeBoundaryLeft = Math.Max(250, 500 - 20 * waveNumber);
            timeBoundaryRight = Math.Max(500, (int)(4000 * enemyGroup.EnemyList.Count / waveInitialSize) - 20 * waveNumber);
        }

        public int WaveNumber { get { return waveNumber; } }

        Random random;
        RenderWindow renderWindow;

        EnemyGroup? enemyGroup;
        int waveNumber, waveInitialSize;
        int timeBoundaryLeft, timeBoundaryRight;
        bool isWave;
        bool isBreak;
        Clock? clock;
        Clock waveClock;
        Time breakTime = Time.FromSeconds(3.5f);
        Time attackBreak, initialMovementCooldown;

        List<Hostile> garbageList;

        Sound sound;
    }
}

using NAudio.Vorbis;
using NAudio.Wave;

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
            movementClock = new Clock();
            movementBreak = Time.FromMilliseconds(500);
            enemies = base.EnemyList;

            maxSpaceWidth = (int)(renderWindow.GetView().Size.X - 2 * enemySize);
            maxWidth = (int)(renderWindow.GetView().Size.X - 2 * enemySize) / enemySize;
            maxSpaceHeight = (int)(renderWindow.GetView().Size.Y / 2 - enemySize);
            maxHeight = (int)((renderWindow.GetView().Size.Y / 2 - enemySize) / enemySize);
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

            PlayIntro();
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

            // Recalculate the two defining corners to know where a wave ends and where it begins for movement control
            TopLeftCorner = new Vector2f(minX, minY);
            BottomRightCorner = new Vector2f(maxX, maxY);
        }

        public override void Move()
        {
            if (direction == null) return;
            if (movementClock.ElapsedTime < movementBreak) return; // Only take action if the clock allows it

            Application.SoundController.Play(SoundBank.EnemyMove);

            if (descent)
            {
                Lower();
            }

            else if (topLeftCorner.X + movementSpeed * direction.X <= 0 || bottomRightCorner.X + movementSpeed * direction.X >= renderWindow.GetView().Size.X)
            {
                descent = true;
                Lower();
            }

            foreach (Enemy enemy in enemies)
            {
                enemy.X += movementSpeed * direction.X;
            }

            topLeftCorner += movementSpeed * direction;
            bottomRightCorner += movementSpeed * direction;

            movementClock.Restart();
        }

        void Lower() // Start going down instead of left/right
        {
            if (direction == null) return;

            direction = new Vector2f(0, 1);

            
            if (descent)
            {
                
                if (descentBuffer > movementSpeed) // For as long as the enemies can, they will move down by their usual speed
                {
                    foreach (Enemy enemy in enemies)
                    {
                        enemy.Y += movementSpeed;

                        // Kill those who are too far below the screen --> WaveController
                    }

                    descentBuffer -= movementSpeed;
                }

                else // If they'd go too far by going with their usual speed
                {
                    foreach (Enemy enemy in enemies)
                    {
                        enemy.Y += descentBuffer; // Move down by what's left to move

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

                Application.SoundController.Play(SoundBank.EnemyMove);

                --introCountdown;
                introClock.Restart();
                
            }
        }

        public override void PlayIntro()
        {
            if (intro) return;

            intro = true;

            float displacementY = renderWindow.GetView().Size.Y / 2f;

            foreach (Enemy enemy in enemies)
            {
                enemy.Y -= displacementY;
                enemy.Shield();
            }

            Application.SoundController.Play(SoundBank.WaveEntrance);

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
    }

    class Rain: EnemyGroup
    {
        private class Droplet // A droplet is a unit of enemies of width 1 and varying length, coming down
        {
            public Droplet(uint enemyCount, float speed, int health = 1)
            {
                renderWindow = Game.GameWindowInstance;
                random = new Random();
                
                enemies = new Queue<Hostile>();
                movementSpeed = speed;
                
                int x = random.Next((int)renderWindow.GetView().Size.X - enemySize);
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
            dropletCount = random.Next(4, (int)(renderWindow.GetView().Size.X - 2 * enemySize) / (enemySize * 2) + 1);
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

                // Add some delay for individual droplets so that not all of them come down at once
                dropletDelay.Add(droplet, Time.FromMilliseconds(dropletBreak.AsMilliseconds()));

                int r = random.Next(50, 2000) * (i / 2 + 1);
                dropletBreak = Time.FromMilliseconds(r);

            }

            movementClock = new Clock();
            dropletDeployClock = new Clock();
            movementBreak = Time.FromMilliseconds(300);

            garbageList = new List<Droplet>();

            PlayIntro();
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

            Application.SoundController.Play(SoundBank.EnemyMove);

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
            Application.SoundController.Play(SoundBank.RainfallEntrance);
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

        Vector2f topLeftCorner, bottomRightCorner, direction;

        Clock introClock;
        Time introDuration;


        const int enemySize = 32;
    }

    class MeteorShower : EnemyGroup
    {
        private class SpawnLine // The line from which meteors will be spawned
        {
            public SpawnLine(int alphaDeg, int r) 
            {
                renderWindow = Application.GameWindowInstance;

                this.alphaDeg = alphaDeg;
                this.alphaRad = DegToRad(alphaDeg);
                this.r = r;

                C = renderWindow.GetView().Size / 2; // The center of the window
                // Two ends of the line
                A = new Vector2f((float)(r * Math.Cos(alphaRad)), -(float)(r * Math.Sin(alphaRad))) + C;
                B = new Vector2f((float)(r * Math.Cos(alphaRad + DegToRad(90))), -(float)(r * Math.Sin(alphaRad + DegToRad(90)))) + C;

                m = (float)(A.Y - B.Y) / (A.X - B.X);
                intercept = A.Y - A.X * m;

                f = (x) => m * x + intercept;

                Vector2f t = A - B;
                t /= (float)Utilities.Utilities.Magnitude(t);
                directionVector = t;
                orthogonalVector = new Vector2f(directionVector.Y, -directionVector.X);
            }

            public float Evaluate(float xValue) => f(xValue);
            public float FindX(float yValue) => (yValue - intercept) / m;

            float DegToRad(float d) => d * Utilities.Utilities.DegRadConversionConstant;

            RenderWindow renderWindow;
            Vector2f A, B, C;
            int alphaDeg, r;
            float alphaRad;

            float m, intercept;
            Func<float, float> f;
            Vector2f directionVector, orthogonalVector;

            public Vector2f DirectionVector => directionVector;
            public Vector2f OrthogonalVector => orthogonalVector;

            public Vector2f LowerEnd => -A.Y <= -B.Y ? A : B;
            public Vector2f HigherEnd => -A.Y > -B.Y ? A : B;
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
            enemyCount = Math.Min(1200, 20 * random.Next(4, 5 + health));

            int alphaDeg = random.Next(15, 75 + 1);
            int r = (int)Utilities.Utilities.Magnitude(new Vector2f(renderWindow.GetView().Size.X/2 + maxEnemySize, renderWindow.GetView().Size.Y/2 + maxEnemySize)) + 1;
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

                // Check if it'll ever appear on the screen:
                bool shouldAdd = false;
                for (float t = 0f; !shouldAdd && t * spawnLine.OrthogonalVector.Y <= renderWindow.GetView().Size.Y; t += speed)
                {
                    Vector2f p = t * spawnLine.OrthogonalVector;
                    Vector2f lims = renderWindow.GetView().Size;
                    shouldAdd = (0 <= p.X && p.X <= lims.X
                              && 0 <= p.Y && p.Y <= lims.Y);
                }

                if (!shouldAdd) continue;

                int randInd = random.Next(meteorTextures.Count);

                Meteor meteor = new Meteor(pos, dir, speed, health, meteorTextures[randInd]);
                enemies.Add(meteor);

                Time meteorBreak = Time.FromMilliseconds(random.Next(150, 1750)) / 2;
                meteorDelay.Add(meteor, Time.FromMilliseconds(meteorBreak.AsMilliseconds()));
            }

            movementClock = new Clock();
            meteorDeployClock = new Clock();

            PlayIntro();
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

            Application.SoundController.Play(SoundBank.EnemyMove);

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
            Application.SoundController.Play(SoundBank.MeteorEntrance);
        }

        public override List<Hostile> EnemyList => this.enemies;

        public override Time MovementCooldown { get { return this.movementBreak; } set { this.movementBreak = value; } }

        public override bool IsAggressive => this.isAggressive;

        bool isAggressive = false;
        Random random;
        SpawnLine spawnLine;
        List<Hostile> enemies;
        List<Meteor> movingMeteors;
        Dictionary<Meteor, Time> meteorDelay;
        int enemyCount;

        Clock movementClock, meteorDeployClock;
        Time movementBreak;

        Clock introClock;
        Time introDuration;

        const int maxEnemySize = 4 * 32;
    }

    class BossFight : EnemyGroup
    {
        private class ProtectorRing
        {
            public ProtectorRing(BossEnemy bossEnemy, int enemyCount, int health)
            {
                Random random = new Random();
                this.bossEnemy = bossEnemy;

                enemyCount = Math.Min(enemyCount, 12);
                enemies = new List<Enemy>();
                enemyAngles = new Dictionary<Enemy, float>();

                ringCenter = CalculateRingCenter();
                r = bossSize * 1.5f / 2;
                deltaTheta = 360 / enemyCount * Utilities.Utilities.DegRadConversionConstant;

                for (int i = 0; i < enemyCount; ++i)
                {
                    float theta = i * deltaTheta;
                    Vector2f pos = PositionEnemy(theta);
                    Texture texture = textures[random.Next(textures.Length)];
                    Enemy enemy = new Enemy(pos, health, texture);
                    enemies.Add(enemy);
                    enemyAngles.Add(enemy, theta);
                }

                revolutionClock = new Clock();

                garbageList = new List<Enemy>();
            }

            public void Update()
            {
                if (!enRoute) ringCenter = CalculateRingCenter();

                enemies.RemoveAll((enemy) => garbageList.Contains(enemy));
                garbageList.Clear();

                foreach (Enemy enemy in enemies)
                {
                    if (enemy.EnemyHealth <= 0)
                    {
                        garbageList.Add(enemy);
                        continue;
                    }
                }

                if (revolutionClock.ElapsedTime >= revolutionCooldown)
                {
                    foreach (Enemy enemy in enemies)
                    {
                        Vector2f pos = PositionEnemy(enemyAngles[enemy]);
                        enemy.X = pos.X;
                        enemy.Y = pos.Y;

                        //enemy.Update();

                        enemyAngles[enemy] += revolutionSpeedDegrees * Utilities.Utilities.DegRadConversionConstant;
                    }

                    revolutionClock.Restart();
                }
            }

            Vector2f CalculateRingCenter() => new Vector2f(bossEnemy.X + bossSize / 2f, bossEnemy.Y + bossSize / 2f);

            Vector2f PositionEnemy(float theta)
            {
                Vector2f result = new Vector2f((float)Math.Cos(theta), (float)Math.Sin(theta)) * r;
                result -= new Vector2f(enemySize, enemySize) / 2f;
                result += ringCenter;
                return result;
            }

            public void Move(Vector2f delta)
            {
                foreach (Enemy enemy in enemies)
                {
                    enemy.X += delta.X;
                    enemy.Y += delta.Y;
                }
            }

            public void Rush()
            {
                if (enRouteCountdown <= 0)
                {
                    enRoute = false;
                    UnshieldAll();
                    Application.SoundController.Stop(reinforcementsSound);

                    return;
                }

                if (enRouteClock.ElapsedTime >= enRouteDeltaTime)
                {
                    foreach (Enemy enemy in enemies) { enemy.Y += enRouteDeltaY; enemy.X += CalculateRingCenter().X - ringCenter.X; };
                    ringCenter.Y += enRouteDeltaY;
                    ringCenter.X = CalculateRingCenter().X;

                    Application.SoundController.Play(SoundBank.EnemyMove);

                    --enRouteCountdown;
                    enRouteClock.Restart();
                }
            }

            public void ShieldAll()
            {
                foreach (Enemy enemy in enemies) enemy.Shield();
            }


            public void UnshieldAll()
            {
                foreach (Enemy enemy in enemies) enemy.Unshield();
            }


            BossEnemy bossEnemy;
            Vector2f ringCenter;
            public Vector2f RingCenter { get => ringCenter; set => ringCenter = value; }

            List<Enemy> enemies, garbageList;

            public List<Enemy> EnemyList => enemies;
            
            float r, deltaTheta;
            Dictionary<Enemy, float> enemyAngles;
            const float revolutionSpeedDegrees = 10f;
            Clock revolutionClock;
            Time revolutionCooldown = Time.FromMilliseconds(100);
            public Time RevolutionCooldown { get => revolutionCooldown; set => revolutionCooldown = value; }


            bool enRoute;
            public bool EnRoute { get { return enRoute; } set { enRoute = value; } }
            Clock enRouteClock;
            public Clock EnRouteClock { get { return enRouteClock; } set { enRouteClock = value; } }
            Time enRouteDeltaTime;
            public Time EnRouteDeltaTime { get { return enRouteDeltaTime; } set { enRouteDeltaTime = value; } }
            float enRouteDeltaY;
            public float EnRouteDeltaY { get { return enRouteDeltaY; } set { enRouteDeltaY = value; } }

            int enRouteCountdown; // 40
            public int EnRouteCountdown { get { return enRouteCountdown; } set { enRouteCountdown = value; } }

            DirectSoundOut? reinforcementsSound;
            public DirectSoundOut ReinforcementsSound { get => reinforcementsSound; set => reinforcementsSound = value; }

            static Texture[] textures = new Texture[2] { TextureBank.EnemyTexture1, TextureBank.EnemyTexture2 };

        }

        public BossFight(int healthUnits = 1) : base()
        {
            random = new Random();
            renderWindow = Game.GameWindowInstance;
            movementBreak = Time.FromMilliseconds(500);
            enemies = base.EnemyList;
            protectorRingsLeft = Math.Min(1 + Game.BossesSlain * 2, 8);
            isAggressive = true;

            Vector2f pos = (renderWindow.GetView().Size - new Vector2f(bossSize, bossSize)) / 2f;
            boss = new BossEnemy(pos, healthUnits * 25, 3 + healthUnits);
            boss.Shield();
            enemies.Add(boss);

            Vector2f[] directions = new Vector2f[2] {new Vector2f(1, 0), new Vector2f(-1, 0)};
            movementDirection = directions[random.Next(directions.Length)];

            activeRing = new ProtectorRing(boss, 6 + Game.BossesSlain, healthUnits);
            --protectorRingsLeft;
            foreach (Enemy enemy in activeRing.EnemyList) enemies.Add(enemy);

            this.healthUnits = healthUnits;

            AlterSpeed();
            PlayIntro();
        }

        public override void Update()
        {
            if (bossDead) return;
            if (boss.EnemyHealth <= 0) 
            {
                Application.SoundController.Play(SoundBank.BossDeath);
                ++Game.BossesSlain; 
                for (int i = 0; i < random.Next(2, 3); ++i) 
                    Game.BonusController.IssuePowerup(
                        new Vector2f(
                            boss.X + boss.XSize/2 - 8 + random.Next(-16, 17), 
                            boss.Y + boss.YSize/2 - 8 + random.Next(-16, 17)));
                for (int i = 0; i < random.Next(1, 3); ++i)
                    Game.BonusController.IssueMedkit(
                        new Vector2f(
                            boss.X + boss.XSize / 2 - 8 + random.Next(-16, 17),
                            boss.Y + boss.YSize / 2 - 8 + random.Next(-16, 17)));
                return; 
            }

            if (intro) { Intro(); activeRing.Update(); return; }

            if (activeRing != null && activeRing.EnemyList.Count == 0 && protectorRingsLeft == 0) { activeRing = null; Application.SoundController.Play(SoundBank.BossShieldBroken); }
            else if (activeRing != null && activeRing.EnemyList.Count == 0 && protectorRingsLeft > 0) CallProtectors();

            if (activeRing == null) { boss.Unshield(); }
            
            Move();
            activeRing?.Update();

            foreach (Hostile enemy in EnemyList) enemy.Update();
        }

        public override void Move()
        {

            if (0 > boss.X || boss.X > renderWindow.GetView().Size.X - boss.XSize) movementDirection *= -1;
            boss.X += movementDirection.X * movementSpeed;

            if (activeRing != null)
            {
                if (activeRing.EnRoute)
                {
                    activeRing.Rush();
                }
                else
                {
                    activeRing.Move(movementDirection);
                }
            }
        }

        void CallProtectors()
        {
            if (protectorRingsLeft <= 0) { activeRing = null; Application.SoundController.Play(SoundBank.BossShieldBroken); return; }

            activeRing = new ProtectorRing(boss, 6 + Game.BossesSlain, healthUnits);
            --protectorRingsLeft;

            foreach (Enemy enemy in activeRing.EnemyList) enemies.Add(enemy);
            activeRing.ShieldAll();

            float displacementY = renderWindow.GetView().Size.Y / 2f + bossSize;
            activeRing.RingCenter -= new Vector2f(0, displacementY);
            activeRing.EnemyList.ForEach(enemy => { enemy.Y -= displacementY; });
            activeRing.EnRouteClock = new Clock();
            activeRing.EnRouteDeltaTime = movementBreak / 4f;
            activeRing.EnRouteCountdown = 20;
            activeRing.EnRouteDeltaY = displacementY / activeRing.EnRouteCountdown;
            activeRing.EnRoute = true;

            Arm();
            activeRing.ReinforcementsSound = Application.SoundController.Play(new LoopStream(SoundBank.BossReinforcements));
            AlterSpeed();
        }

        void AlterSpeed()
        {
            movementSpeed = random.Next(10, 36) / 10f;
        }

        public override void Intro()
        {
            if (introCountdown <= 0)
            {
                intro = false;
                activeRing.UnshieldAll();
                return;
            }

            if (introClock.ElapsedTime >= introDeltaTime)
            {
                foreach (Hostile enemy in enemies) enemy.Y += introDeltaY;

                Application.SoundController.Play(SoundBank.EnemyMove);

                --introCountdown;
                introClock.Restart();
            }
        }

        public override void PlayIntro()
        {
            if (intro) return;

            intro = true;

            float displacementY = renderWindow.GetView().Size.Y / 2f + bossSize;

            foreach (Hostile enemy in enemies)
            {
                enemy.Y -= displacementY;
                enemy.Shield();
            }

            Application.SoundController.Play(SoundBank.BossEntrance);

            introClock = new Clock();
            introDeltaTime = movementBreak / 4f;
            introDeltaY = displacementY / 40f;
            introCountdown = 40;
        }

        public void Arm()
        {
            foreach (Hostile enemy in enemies)
            {
                if (enemy == boss) continue;

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

        Random random;
        RenderWindow renderWindow;
        List<Hostile> enemies;
        int healthUnits;

        bool isAggressive = true;
        public override bool IsAggressive => isAggressive;

        BossEnemy boss;
        bool bossDead;
        ProtectorRing? activeRing;
        int protectorRingsLeft;

        Vector2f movementDirection;
        Time movementBreak;

        const int enemySize = 32;
        const int bossSize = 4 * 32;
        float movementSpeed;

        Clock introClock;
        Time introDeltaTime;
        float introDeltaY;
        int introCountdown; 
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
        }

        public void Reset()
        {
            random = new Random();

            waveNumber = 0;

            isWave = false;
            isBreak = false;

            garbageList = new List<Hostile>();
        }

        public void StartWithBreak()
        {
            enemyGroup = new PlaceholderWave();
        }
        
        public void Start()
        {
            List<Action> models;
            if (waveNumber > 0 && waveNumber % 3 == 0) models = new List<Action>() { StartBoss };
            else models = new List<Action>() { StartWave, StartRain, StartMeteorShower };
            
            int choice = random.Next(models.Count);
            models[choice]();

            isWave = true;
            isBreak = false;
            clock = new Clock();

            // PlayIntro moved to individual constructors
        }

        void StartWave()
        {
            if (!isWave && !isBreak)
            {
                Game.PlayerInstance.Shield(Time.FromSeconds(5));
                enemyGroup = new Wave(1 + Game.BossesSlain);
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
                enemyGroup = new Rain(1 + Game.BossesSlain);
                ++waveNumber;
                waveInitialSize = enemyGroup.EnemyList.Count;
                ArmEnemies();
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
                enemyGroup = new MeteorShower(2 * (1 + Game.BossesSlain));
                ++waveNumber;
                waveInitialSize = enemyGroup.EnemyList.Count;

                isWave = true;
                isBreak = false;
                clock = new Clock();
                CalculateTimeBoundaries();
                enemyGroup.MovementCooldown = Time.FromMilliseconds(150);
                initialMovementCooldown = enemyGroup.MovementCooldown;
            }
        }

        void StartBoss()
        {
            if (!isWave && !isBreak)
            {
                Game.PlayerInstance.Shield(Time.FromSeconds(5));
                enemyGroup = new BossFight(1 + Game.BossesSlain);
                ((BossFight)enemyGroup).Arm();
                ++waveNumber;
                waveInitialSize = enemyGroup.EnemyList.Count;
                CalculateTimeBoundaries();
                enemyGroup.MovementCooldown = Time.FromMilliseconds(150);
                initialMovementCooldown = enemyGroup.MovementCooldown;
            }
        }

        void ArmEnemies()
        {
            if (enemyGroup == null) return;
            if (!enemyGroup.IsAggressive) return;

            foreach (Hostile enemy in enemyGroup.EnemyList)
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
                        Damage(enemy, 1 + Game.BossesSlain);
                        enemy.AddLifebar(Enemy.LifebarPositions.Above);

                        Game.PlayerInstance.Damage(40);
                        Application.SoundController.Play(SoundBank.PlayerExplosion);
                        Game.PlayerInstance.ForceMove(Game.PlayerSpawnPoint);
                        continue;
                    }

                    // Kill those who are too far below the screen
                    if (enemy.Y >= renderWindow.GetView().Size.Y + 2 * enemy.EnemySprite.Texture.Size.Y)
                    {
                        Kill(enemy);
                    }
                }

                if (enemyGroup.IsAggressive)
                {
                    if (clock.ElapsedTime >= attackBreak)
                    {
                        int attackerIndex = random.Next(enemyGroup.EnemyList.Count);

                        enemyGroup.EnemyList[attackerIndex].Fire();

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
                    //ArmEnemies();
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
                Application.SoundController.Play(SoundBank.ShieldProtect);
                return;
            }

            VorbisWaveReader sound = SoundBank.EnemyDamaged;

            enemy.EnemyHealth -= damageValue;
            if (enemy.EnemyHealth <= 0)
            {
                sound = SoundBank.EnemyExplosion;

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

            Application.SoundController.Play(sound);
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
    }
}

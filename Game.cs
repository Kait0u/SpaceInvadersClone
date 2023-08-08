using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;

namespace SpaceInvadersClone
{
    internal class Game: IDisposable
    {
        public Game(RenderWindow window)
        {
            random = new Random();

            this.window = window;
            gameWindowInstance = window;

            bgSprite = new Sprite(TextureBank.BackgroundTexture);

            player.Reset();
            playerSpawnPoint = new Vector2f(window.Size.X / 2f - player.XSize / 2f, window.Size.Y - player.YSize - 10);
            player.X = playerSpawnPoint.X;
            player.Y = playerSpawnPoint.Y;

            playerBulletController.Reset();
            enemyBulletController.Reset();
            bonusController.Reset();
            waveController.Reset();

            gameOver = false;
            pause = false;
            
            escPauseMenu = (sender, e) =>
            {
                pause = (e.Code == Keyboard.Key.Escape);
            };
            window.KeyPressed += escPauseMenu;

            score = 0;

            infoBar = new InfoBar();

            waveController.Start();
        }

        public void Dispose()
        {
            window.KeyPressed -= escPauseMenu;
        }

        public void Run()
        {
            while (window.IsOpen)
            {
                if (window.HasFocus())
                {
                    if (gameOver) Application.State = Application.ApplicationStates.GameOver;

                    if (Application.State != Application.ApplicationStates.Game)
                    {
                        player.Sound.Stop();
                        break;
                    }

                    window.DispatchEvents();
                    Update();
                    Draw();
                }
            }
        }

        private void Update()
        {
            player.Update();
            if (player.IsDead)
            {
                gameOver = true;
                return;
            }

            if (pause)
            {
                PauseMenu pauseMenu = new PauseMenu(window, this);
                pauseMenu.Run();
                pauseMenu.Dispose();
                pause = false;
            }

            // Medkits for points - abandoned

            playerBulletController.UpdateBullets();
            enemyBulletController.UpdateBullets();
            waveController.Update();
            bonusController.Update();

            infoBar.Update();
        }

        private void Draw()
        {
            window.Clear(new Color(0, 0, 50));
            window.Draw(bgSprite);
            player.Draw();
            playerBulletController.DrawBullets();
            enemyBulletController.DrawBullets();
            waveController.Draw();
            bonusController.Draw();

            infoBar.Draw();
            // Add above this line!!
            window.Display();
        }


        RenderWindow window;
        Sprite bgSprite;
        
        InfoBar infoBar;

        Random random;

        static ulong score = 0;
        public static ulong Score { get { return score; } set { score = value; } }

        bool gameOver, pause;
        EventHandler<KeyEventArgs> escPauseMenu;


        static RenderWindow gameWindowInstance;
        static Player player = new Player();
        static Vector2f playerSpawnPoint;
        static BulletController<PlayerBullet> playerBulletController = new BulletController<PlayerBullet>();
        static BulletController<EnemyBullet> enemyBulletController = new BulletController<EnemyBullet>();
        static WaveController waveController = new WaveController();
        static BonusController bonusController = new BonusController();

        public static RenderWindow GameWindowInstance { get { return gameWindowInstance; } }
        public static Player PlayerInstance { get { return player; } }
        public static Vector2f PlayerSpawnPoint { get { return playerSpawnPoint; } }
        public static BulletController<PlayerBullet> PlayerBulletController { get { return playerBulletController; } }
        public static BulletController<EnemyBullet> EnemyBulletController { get { return enemyBulletController; } }
        public static WaveController WaveController { get { return waveController; } }
        public static BonusController BonusController { get { return bonusController; } }

    }
}

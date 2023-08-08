using SFML.Audio;
using SFML.Graphics;
using SFML.Window;
using System.Data;

namespace SpaceInvadersClone
{
    class Application
    {
        public Application()
        {
            gameWindowInstance.Closed += (sender, args) => gameWindowInstance.Close();
            gameWindowInstance.SetVerticalSyncEnabled(true);
            soundController = new SoundController();
            soundControllerThread = new Thread(() => { while (gameWindowInstance.IsOpen) SoundController.Update(); });
        }

        public void Run()
        {
            soundControllerThread.Start();

            while (gameWindowInstance.IsOpen)
            {
                if (state == ApplicationStates.MainMenu)
                {
                    MainMenu mainMenu = new MainMenu(gameWindowInstance);
                    mainMenu.Run();
                    mainMenu.Dispose();
                }
                else if (state == ApplicationStates.Game)
                {
                    Game game = new Game(gameWindowInstance);
                    game.Run();
                    game.Dispose();
                }
                else if (state == ApplicationStates.GameOver) { new GameOverScreen(gameWindowInstance).Run(); }
                else if (state == ApplicationStates.Terminated) { Close(); }
            }
        }

        void Close() 
        {
            gameWindowInstance.Close();
        }

        static VideoMode videoMode = new VideoMode(width, height);
        static RenderWindow gameWindowInstance = new RenderWindow(videoMode, title, Styles.Close);
        public static RenderWindow GameWindowInstance { get { return gameWindowInstance; } }

        public enum ApplicationStates
        {
            MainMenu,
            Game,
            GameOver,
            Terminated
        }

        static ApplicationStates state = ApplicationStates.MainMenu;
        public static ApplicationStates State { get { return state; } set { state = value; } }

        static SoundController soundController = new SoundController();
        Thread soundControllerThread;
        public static SoundController SoundController { get {  return soundController; } }


        const int width = 1024, height = 768;
        const string title = "Space Invaders";
    }


    class Program
    {
        static void Main(string[] args)
        {
            new Application().Run();
        }
    }
}

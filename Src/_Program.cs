using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Text.Json;

namespace SpaceInvadersClone
{
    class Application
    {
        public Application()
        {
            InitializeFromJSON(); // Load settings from a JSON or create a new one if it's missing

            Styles style;
            if (windowed)
            {
                videoMode = new VideoMode(defaultSize.X, defaultSize.Y);
                style = Styles.Default;
            }
            else
            {
                uint w = VideoMode.DesktopMode.Width;
                uint h = VideoMode.DesktopMode.Height;

                videoMode = new VideoMode(w, h);
                style = Styles.Fullscreen;
            }
            gameWindowInstance = new RenderWindow(videoMode, title, style);

            gameWindowInstance.Closed += (sender, args) => gameWindowInstance.Close();
            gameWindowInstance.SetVerticalSyncEnabled(true);
            soundController = new SoundController();
            soundControllerThread = new Thread(() => { while (gameWindowInstance.IsOpen) soundController.Update(); });

            View view = Utilities.Utilities.CalcView(gameWindowInstance.Size, defaultSize);
            gameWindowInstance.SetView(view);

            gameWindowInstance.Resized += (sender, e) =>
            {
                View view = Utilities.Utilities.CalcView(new Vector2u(e.Width, e.Height), defaultSize); // Recalculate the view and viewport
                gameWindowInstance.SetView(view);
            };
        }

        public void Run()
        {
            soundControllerThread.Start(); // Start an audio controller for audio handling in a separate thread

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

        void InitializeFromJSON() // Load settings
        {
            try
            { 
                string jsonText = File.ReadAllText(configFilePath);
                JsonDocument json = JsonDocument.Parse(jsonText);
                JsonElement root = json.RootElement;
                windowed = !root.GetProperty("FullScreen").GetBoolean();
            }
            catch (Exception e) // Something's wrong with the file?
            {
                File.WriteAllText(configFilePath, defaultConfigContent);
                InitializeFromJSON();
            }
        }

        const uint defaultWidth = 1024, defaultHeight = 768; // Default game dimensions - beyond that, everything will have to maintain the aspect ratio
        static readonly Vector2u defaultSize = new Vector2u(defaultWidth, defaultHeight);
        public static Vector2u DefaultSize => defaultSize;

        static VideoMode videoMode;
        static RenderWindow gameWindowInstance;
        
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

        bool windowed = true;

        static SoundController soundController = new SoundController();
        Thread soundControllerThread;
        public static SoundController SoundController { get {  return soundController; } }

        const string title = "Space Invaders";
        const string configFilePath = "./config.json";
        const string defaultConfigContent = @"
{
  ""FullScreen"": false
}";
    }


    class Program
    {
        static void Main(string[] args)
        {
            new Application().Run();
        }
    }
}

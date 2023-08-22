using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace SpaceInvadersClone
{
    internal class CreditsScreen
    {
        public CreditsScreen() 
        {
            drawables = new List<Drawable>();

            string[] lines = File.ReadAllLines(creditsFilePath);

            // Background
            RectangleShape shape = new RectangleShape()
            {
                FillColor = Color.Black,
                Size = (Vector2f)window.Size,
            };
            drawables.Add(shape);

            // Logo
            Text logo = new Text()
            {
                FillColor = new Color(255, 225, 98),
                Font = FontBank.PixelColeco,
                CharacterSize = 100,
                DisplayedString = "Space Invaders"
            };
            Vector2f pos = new Vector2f((window.GetView().Size.X - logo.GetLocalBounds().Width) / 2f, 0);
            logo.Position = new Vector2f(pos.X, pos.Y);
            drawables.Add(logo);

            // Actual Credits:

            const float padding = 10;
            float startY = logo.Position.Y + logo.GetGlobalBounds().Height + padding * 4;
            foreach (string line in lines)
            {
                line.Trim();
                Text entry = new Text();
                entry.DisplayedString = line;
                entry.Font = FontBank.PixelColeco;

                if (line.Length > 0 && line[0] == '[') // Header
                {
                    entry.CharacterSize = 30;
                    entry.FillColor = Color.Red;
                }

                else // Mentions or a blank line
                {
                    entry.CharacterSize = 20;
                    entry.FillColor = Color.White;
                }

                Vector2f dims = new Vector2f(entry.GetGlobalBounds().Width, entry.GetGlobalBounds().Height);
                float x = (window.GetView().Size.X - dims.X) / 2f;
                float y = startY + padding;
                entry.Position = new Vector2f(x, y);

                drawables.Add(entry);
                startY += padding + dims.Y;
            }

            List<Text> credits = drawables.GetRange(2, drawables.Count - 2).Cast<Text>().ToList();

            // Make sure the credits are centered along the Y axis
            float h = (credits.Last().GetGlobalBounds().Top 
                + credits.Last().GetGlobalBounds().Height) 
                - credits.First().GetGlobalBounds().Top;

            h = (window.GetView().Size.Y - h) / 2f;
            h = credits.First().GetGlobalBounds().Top - h;
            foreach (Text credit in credits) credit.Position -= new Vector2f(0, h);

            // Prompt
            startY = credits.Last().GetGlobalBounds().Top + credits.Last().GetGlobalBounds().Height +  4 * padding;
            Text prompt = new Text();
            prompt.Font = FontBank.PixelColeco;
            prompt.CharacterSize = 25;
            prompt.FillColor = Color.Yellow;
            prompt.DisplayedString = "Press ENTER to continue...";
            prompt.Position = new Vector2f((window.GetView().Size.X - prompt.GetGlobalBounds().Width) / 2f, startY);

            drawables.Add(prompt);
        }

        public void Run()
        {
            while (window.IsOpen && !skipped)
            {
                if (!window.HasFocus()) continue;

                window.DispatchEvents();
                Update();
                Draw();
            }
            
            Application.State = Application.ApplicationStates.MainMenu;
            Application.SoundController.Play(SoundBank.Select);
        }

        void Update()
        {
            if (skipClock.ElapsedTime >= skipTime && Keyboard.IsKeyPressed(Keyboard.Key.Enter)) skipped = true;
        }

        void Draw()
        {
            window.Clear();

            drawables.GetRange(0, drawables.Count - 1).ForEach(window.Draw);
            if (skipClock.ElapsedTime >= skipTime) window.Draw(drawables.Last());

            window.Display();
        }

        RenderWindow window = Application.GameWindowInstance;
        List<Drawable> drawables;

        bool skipped = false;
        Clock skipClock = new Clock();
        Time skipTime = Time.FromSeconds(5);

        const string creditsFilePath = "./Assets/credits.txt";
    }
}

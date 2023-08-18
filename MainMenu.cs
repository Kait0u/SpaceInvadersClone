﻿
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace SpaceInvadersClone
{
    class MainMenu: IDisposable
    {
        public MainMenu(RenderWindow window) 
        { 
            this.window = window;

            controls = (sender, e) =>
            {
                switch (e.Code)
                {
                    case Keyboard.Key.Up:
                        MoveUp();
                        break;
                    case Keyboard.Key.W:
                        MoveUp();
                        break;
                    case Keyboard.Key.Down:
                        MoveDown();
                        break;
                    case Keyboard.Key.S:
                        MoveDown();
                        break;
                    case Keyboard.Key.Enter:
                        // Execute selected
                        Execute();
                        break;
                    default:
                        break;
                }
            };
            window.KeyPressed += controls;

            drawables = new List<Drawable>();
            options = new Dictionary<Options, Drawable>();
            

            // Background
            RectangleShape shape = new RectangleShape() {
                FillColor = Color.Black,
                Size = (Vector2f)window.Size,
            };
            drawables.Add(shape);

            // Logo
            Text logo = new Text() { 
                FillColor = new Color(255, 225, 98),
                Font = FontBank.PixelColeco,
                CharacterSize = 100,
                DisplayedString = "Space Invaders"
            };
            Vector2f pos = new Vector2f((window.Size.X - logo.GetLocalBounds().Width) / 2f, 0);
            logo.Position = new Vector2f(pos.X, pos.Y);
            drawables.Add(logo);

            // Options
            Text playOption = new Text() { 
                FillColor = Color.White,
                Font = FontBank.PixelColeco,
                CharacterSize = 40,
                DisplayedString = "Play"
            };

            Text creditsOption = new Text(playOption) { 
                DisplayedString = "Credits"
            };

            Text exitOption = new Text(playOption) { 
                DisplayedString = "Exit"
            };

            float interoptionPadding = 20;
            float y = window.Size.Y - playOption.GetLocalBounds().Height - creditsOption.GetLocalBounds().Height - exitOption.GetLocalBounds().Height - 4 * interoptionPadding;
            pos = new Vector2f((window.Size.X - playOption.GetLocalBounds().Width) / 2f, y);
            playOption.Position = new Vector2f(pos.X, pos.Y);
            y += playOption.GetLocalBounds().Height + interoptionPadding;
            options.Add(Options.Play, playOption);
            drawables.Add(playOption);

            pos = new Vector2f((window.Size.X - creditsOption.GetLocalBounds().Width) / 2f, y);
            creditsOption.Position = new Vector2f(pos.X, pos.Y);
            y += creditsOption.GetLocalBounds().Height + interoptionPadding;
            options.Add(Options.Credits, creditsOption);
            drawables.Add(creditsOption);

            pos = new Vector2f((window.Size.X - exitOption.GetLocalBounds().Width) / 2f, y);
            exitOption.Position = new Vector2f(pos.X, pos.Y);
            options.Add(Options.Exit, exitOption);
            drawables.Add(exitOption);

            selected = Options.Play;
            selectionExecuted = false;
        }

        public void Dispose() 
        { 
            window.KeyPressed -= controls;
        }


        public void Run()
        {
            while (window.IsOpen)
            {
                if (window.HasFocus())
                {
                    window.DispatchEvents();
                    Update();
                    Draw();

                    if (selectionExecuted) break;
                }
            }
        }

        void Update()
        {
            foreach (KeyValuePair<Options, Drawable> pair in options)
            {
                Text t = pair.Value as Text;
                t.FillColor = Color.White;
            }

            (options[selected] as Text).FillColor = Color.Red;
        }

        void Draw() 
        { 
            foreach (Drawable drawable in drawables)
            {
                window.Draw(drawable);
            }

            window.Display();
        }

        void MoveUp()
        {

            --selected;
            selected = (Options)Utilities.Utilities.Modulus((int)selected, options.Count());

            //sound.Play();
            //Application.SoundController.RegisterSound(sound);
            Application.SoundController.Play(SoundBank.MoveSelection);
        }

        void MoveDown() 
        {

            ++selected;
            selected = (Options)Utilities.Utilities.Modulus((int)selected, options.Count());

            //sound.Play();
            //Application.SoundController.RegisterSound(sound);
            Application.SoundController.Play(SoundBank.MoveSelection);
        }

        void Execute()
        {
            if (selected == Options.Play) 
            {
                Application.State = Application.ApplicationStates.Game;
                selectionExecuted = true;
            }
            
            else if (selected == Options.Credits) { }
            
            else if (selected == Options.Exit) 
            {
                Application.State = Application.ApplicationStates.Terminated;
                selectionExecuted = true;
            }
            Application.SoundController.Play(SoundBank.Select);
        }

        RenderWindow window;
        List<Drawable> drawables;

        enum Options { Play, Credits, Exit}

        Dictionary<Options, Drawable> options;
        Options selected;
        bool selectionExecuted;

        EventHandler<KeyEventArgs> controls;
    }
}

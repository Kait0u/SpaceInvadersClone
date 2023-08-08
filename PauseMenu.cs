using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Linq;

namespace SpaceInvadersClone
{
    class PauseMenu: IDisposable
    {
        public PauseMenu(RenderWindow renderWindow, Game game) 
        {
            window = renderWindow;
            this.game = game;

            sound = new Sound(SoundBank.MoveSelection);
            drawables = new List<Drawable>();

            Vector2u windowSize = window.Size;
            Texture backgroundTexture = new Texture(windowSize.X, windowSize.Y);
            backgroundTexture.Update(window);
            background = new Sprite(backgroundTexture) { Position = new Vector2f(0, 0) };
            background.Color = new Color(128, 128, 128);
            drawables.Add(background);

            options = new Dictionary<Options, Drawable>();

            continueOption = new Text()
            {
                FillColor = Color.White,
                Font = FontBank.PixelColeco,
                CharacterSize = 30,
                DisplayedString = "Continue"
            };

            sacrificeOption = new Text(continueOption)
            {
                DisplayedString = "Sacrifice"
            };

            quitOption = new Text(continueOption)
            {
                DisplayedString = "Quit to Menu"
            };

            options[Options.Continue] = continueOption;         
            options[Options.Sacrifice] = sacrificeOption;
            options[Options.Quit] = quitOption;

            List<Drawable> textList = options.Values.ToList();
            
            float interoptionPadding = 10;
            float minBoxHeight = continueOption.GetGlobalBounds().Height + sacrificeOption.GetGlobalBounds().Height + quitOption.GetGlobalBounds().Height + 2 * interoptionPadding;
            float minBoxWidth = Math.Max(continueOption.GetGlobalBounds().Width, Math.Max(sacrificeOption.GetGlobalBounds().Width, quitOption.GetGlobalBounds().Width));

            float x = (window.Size.X - minBoxWidth) / 2f;
            float minX = x;
            float y = (window.Size.Y - minBoxHeight) / 2f;
            
            Vector2f tlCorner = new Vector2f(x, y);
            Vector2f brCorner;

            for (int i = 0; i < textList.Count; ++i)
            {
                Text t = (Text)textList[i];
                
                x = (window.Size.X - t.GetGlobalBounds().Width) / 2f;
                y += (i == 0 ? 0 : 1) * interoptionPadding;
                
                t.Position = new Vector2f(x, y);
                
                y += (i + 1 < textList.Count ? 1 : 0) * t.GetGlobalBounds().Height;
                minX = Math.Min(minX, x);
            }

            float boxPadding = 30;

            tlCorner.X -= boxPadding;
            tlCorner.Y -= boxPadding;

            brCorner = new Vector2f(minX + minBoxWidth + boxPadding, y + boxPadding * 2 + 8);


            menuBox = new RectangleShape(brCorner - tlCorner)
            {
                Position = new Vector2f(tlCorner.X, tlCorner.Y),
                FillColor = new Color(44, 44, 44)
            };

            drawables.Add(menuBox);
            drawables.Add(continueOption);
            drawables.Add(sacrificeOption);
            drawables.Add(quitOption);

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
                    case Keyboard.Key.Escape:
                        // Execute continue
                        selected = Options.Continue;
                        Execute();
                        break;
                    default:
                        break;
                }
            };
            window.KeyPressed += controls;

            actions = new Dictionary<Options, Action>()
            {
                { Options.Continue, () => {} },
                { Options.Sacrifice, () => {} },
                { Options.Quit, () => { Application.State = Application.ApplicationStates.MainMenu; } },
            };
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
                Text t = (Text)pair.Value;
                t.FillColor = Color.White;
            }

            ((Text)options[selected]).FillColor = Color.Red;
        }

        void Draw() 
        {
            foreach (Drawable drawable in drawables)
            {
                window.Draw(drawable);
            }

            // Add above this line
            window.Display();
        }

        void MoveUp()
        {
            sound.SoundBuffer = SoundBank.MoveSelection;

            --selected;
            selected = (Options)Utilities.Utilities.Modulus((int)selected, options.Count());

            sound.Play();
            Application.SoundController.RegisterSound(sound);
        }

        void MoveDown()
        {
            sound.SoundBuffer = SoundBank.MoveSelection;

            ++selected;
            selected = (Options)Utilities.Utilities.Modulus((int)selected, options.Count());

            sound.Play();
            Application.SoundController.RegisterSound(sound);
        }

        void Execute()
        {
            sound.SoundBuffer = SoundBank.Select;

            actions[selected]();
            selectionExecuted = true;

            sound.Play();
            Application.SoundController.RegisterSound(sound);
        }

        RenderWindow window;
        Game game;
        Sprite background;
        Sound sound;

        bool selectionExecuted;

        enum Options { Continue, Sacrifice, Quit }
        Options selected;
        Dictionary<Options, Action> actions;

        List<Drawable> drawables;
        RectangleShape menuBox;
        Text continueOption, sacrificeOption, quitOption;
        Dictionary<Options, Drawable> options;

        EventHandler<KeyEventArgs> controls;
    }
}

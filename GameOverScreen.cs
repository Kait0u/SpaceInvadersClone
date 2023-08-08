using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace SpaceInvadersClone
{
    internal class GameOverScreen
    {
        public GameOverScreen(RenderWindow window) 
        { 
            this.window = window;
            clock = new Clock();
            sound = new Sound(SoundBank.GameOver) { Loop = false };

            background = new RectangleShape()
            {
                FillColor = Color.Black,
                Size = (Vector2f)window.Size,
            };

            gameOverText = new Text() {
                Font = FontBank.PixelColeco,
                FillColor = Color.Red,
                CharacterSize = 80,
                DisplayedString = "Game Over"
            };
            
            Vector2f pos = new Vector2f((window.Size.X - gameOverText.GetLocalBounds().Width) / 2f, 
                                        window.Size.Y / 2f - gameOverText.GetLocalBounds().Height * 1.5f);
            gameOverText.Position = new Vector2f(pos.X, pos.Y);

            gameOverInstruction = new Text() {
                Font = FontBank.PixelColeco,
                FillColor = Color.White,
                CharacterSize = 30,
                DisplayedString = "Press ENTER to continue"
            };

            pos = new Vector2f((window.Size.X - gameOverInstruction.GetLocalBounds().Width) / 2f,
                               window.Size.Y / 2f + 5);

            gameOverInstruction.Position = new Vector2f(pos.X, pos.Y);

            skipped = false;
        }

        public void Run()
        {
            clock.Restart();

            sound = new Sound(SoundBank.GameOver) { Loop = false };
            Application.SoundController.RegisterSound(sound);
            sound.Play();

            while (window.IsOpen)
            {
                if (window.HasFocus())
                {
                    window.DispatchEvents();
                    Update();
                    Draw();

                    if (skipped)
                    {
                        sound.Stop();
                        sound.SoundBuffer = SoundBank.Select;
                        Application.State = Application.ApplicationStates.MainMenu;
                        sound.Play();
                        break;
                    }
                }
            }
        }

        public void Update() 
        {
            if (clock.ElapsedTime >= waitTime && Keyboard.IsKeyPressed(Keyboard.Key.Enter)) skipped = true;
        }

        public void Draw() 
        {
            window.Draw(background);
            window.Draw(gameOverText);
            if (clock.ElapsedTime >= waitTime) window.Draw(gameOverInstruction);
            window.Display();
        }

        RenderWindow window;
        Clock clock;

        RectangleShape background;
        Text gameOverText, gameOverInstruction;

        Sound sound;

        bool skipped;
        static readonly Time waitTime = Time.FromSeconds(5);
    }
}

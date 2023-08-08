using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Threading.Tasks.Sources;

namespace SpaceInvadersClone
{
    class InfoBar
    {
        public InfoBar()
        {
            renderWindow = Game.GameWindowInstance;
            player = Game.PlayerInstance;
            width = (int)renderWindow.Size.X;
            height = 30; // Due to wave layout and other cases
            font = FontBank.PixelColeco;

            background = new RectangleShape(new Vector2f(width, height));
            background.FillColor = Color.Black;

            healthInfo = new Text("", font, 26);
            healthInfo.FillColor = Color.Red;

            scoreInfo = new Text("", font, 26);
            scoreInfo.FillColor = Color.White;

            waveNumberInfo = new Text("", font, healthInfo.CharacterSize / 2 - 1);
            waveNumberInfo.FillColor = Color.White;

            weaponLevelInfo = new Text("", font, healthInfo.CharacterSize / 2 - 1);
            weaponLevelInfo.FillColor = Color.White;

            Update();
        }

        public void Update()
        {
            // Health Info
            int spaces = Math.Max(0, 2 - (int)Math.Log10(player.CurrentHealth));
            string filling = new string(' ', spaces);
            healthInfo.DisplayedString = $"Health: {filling}{player.CurrentHealth}%";
            healthInfo.Position = new Vector2f(height - healthInfo.CharacterSize / 2, top);

            // Score Info
            if (Game.Score <= 999_999_999)
            {
                spaces = Math.Max(0, 8 - (int)Math.Log10(Game.Score == 0 ? 1 : Game.Score));
                filling = new string(' ', spaces);
                scoreInfo.DisplayedString = $"Score: {filling}{Game.Score}";
            }
            else
            {
                string text = "LOTS!";
                spaces = Math.Max(0, 8 - (int)Math.Log10(text.Length));
                filling = new string(' ', spaces);
                scoreInfo.DisplayedString = $"Score: {filling}{text}";
            }
            scoreInfo.Position = new Vector2f(renderWindow.Size.X - scoreInfo.GetLocalBounds().Width - scoreInfo.CharacterSize / 2,
                                              top);

            // Wave Number Info
            if (Game.WaveController.WaveNumber <= 999)
            {
                spaces = Math.Max(0, 2 - (int)Math.Log10(Game.WaveController.WaveNumber));
                filling = new string(' ', spaces);
                waveNumberInfo.DisplayedString = $"Wave: {filling}{Game.WaveController.WaveNumber}";
            }
            else
            {
                string text = "WOW";
                waveNumberInfo.DisplayedString = $"Wave: {text}";
            }
            Vector2f pos = new Vector2f(0, 0);
            pos.X = healthInfo.GetGlobalBounds().Left + healthInfo.GetGlobalBounds().Width + paddingPx;
            pos.Y = healthInfo.Position.Y + 4;
            waveNumberInfo.Position = new Vector2f(pos.X, pos.Y);

            // Weapon Info
            if (Game.PlayerInstance.WeaponLevel <= 999)
            {
                spaces = Math.Max(0, 2 - (int)Math.Log10(Game.PlayerInstance.WeaponLevel));
                filling = new string(' ', spaces);
                weaponLevelInfo.DisplayedString = $"W.Lv: {filling}{Game.PlayerInstance.WeaponLevel}";
            }
            else
            {
                string text = "WOW";
                weaponLevelInfo.DisplayedString = $"W.Lv: {text}";
            }
            pos = new Vector2f(0, 0);
            pos.X = waveNumberInfo.Position.X;
            pos.Y = waveNumberInfo.Position.Y + waveNumberInfo.GetLocalBounds().Height + 2;
            weaponLevelInfo.Position = new Vector2f(pos.X, pos.Y);

        }

        public void Draw() 
        {
            renderWindow.Draw(background);
            renderWindow.Draw(healthInfo);
            renderWindow.Draw(scoreInfo);
            renderWindow.Draw(waveNumberInfo);
            renderWindow.Draw(weaponLevelInfo);
        }


        RenderWindow renderWindow;
        Player player;
        int width, height;
        const int paddingPx = 20;
        const int top = -2;

        Font font;
        Text healthInfo, scoreInfo, weaponLevelInfo, waveNumberInfo;
        RectangleShape background;
    }

    class UI
    {

    }
}

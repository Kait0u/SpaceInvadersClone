


using SFML.Graphics;
using SFML.System;

namespace SpaceInvadersClone
{
    internal class BonusController
    {
        public BonusController() 
        {
            renderWindow = Application.GameWindowInstance;
            player = Game.PlayerInstance;
            random = new Random();
            
            bonusList = new List<Bonus>();
            garbageList = new List<Bonus>();
        }

        public void Reset()
        {
            random = new Random();

            bonusList = new List<Bonus>();
            garbageList = new List<Bonus>();
        }

        public void IssueMedkit(Vector2f initialPosition)
        {
            float velocity = random.Next(100, 501) / 100f;
            Medkit medkit = new Medkit(initialPosition, velocity);
            bonusList.Add(medkit);
        }

        public void IssuePowerup(Vector2f initialPosition)
        {
            float velocity = random.Next(100, 501) / 100f;
            PowerUp powerup = new PowerUp(initialPosition, velocity);
            bonusList.Add(powerup);
        }

        void DeleteBonus(Bonus bonus)
        {
            garbageList.Add(bonus);
        }

        public void Update()
        {
            foreach (Bonus bonus in garbageList) bonusList.Remove(bonus);
            garbageList.Clear();

            foreach (Bonus bonus in bonusList) bonus.Update();

            foreach (Bonus bonus in bonusList)
            {
                bool touchesPlayer = bonus.BonusSprite.GetGlobalBounds().Intersects(player.PlayerSprite.GetGlobalBounds());
                bool isOffScreen = bonus.BonusSprite.Position.Y > renderWindow.GetView().Size.Y + bonus.YSize + 10;
                
                if (touchesPlayer)
                {
                    bonus.Action(player);
                    DeleteBonus(bonus);

                    Application.SoundController.Play(SoundBank.Powerup);
                }
                else if (isOffScreen) DeleteBonus(bonus);
            }
        }

        public void Draw()
        {
            foreach (Bonus bonus in bonusList)
            {
                renderWindow.Draw(bonus.BonusSprite);
            }
        }



        RenderWindow renderWindow;
        Player player;
        Random random;

        List<Bonus> bonusList, garbageList;
    }
}

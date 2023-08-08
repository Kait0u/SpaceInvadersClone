using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceInvadersClone
{
    public interface IBullet
    {
        public void Update();
        public void Draw();

        public int XSize { get; }
        public int YSize { get; }
        public float X { get; set; }
        public float Y { get; set; }


    }

    internal class BulletController<Bullet> where Bullet : IBullet
    {
        public BulletController() 
        { 
            bulletList = new List<Bullet>();
            garbageList = new List<Bullet>();
        }

        public void Reset()
        {
            bulletList = new List<Bullet>();
            garbageList = new List<Bullet>();
        }

        public void RegisterBullet(Bullet bullet) 
        { 
            bulletList.Add(bullet);
        }

        public void UpdateBullets()
        {
            
            foreach (Bullet bullet in bulletList)
            {

                bullet.Update();

                if (!(-10 <= bullet.Y && bullet.Y <= Game.GameWindowInstance.Size.Y + 10))
                {
                    DeleteBullet(bullet);
                }
            }

            foreach(Bullet bullet in garbageList)
            {
                bulletList.Remove(bullet);
            }
            garbageList.Clear();
        }

        public void DrawBullets()
        {
            foreach (Bullet bullet in bulletList)
            {
                bullet.Draw();
            }
        }

        public void DeleteBullet(Bullet bullet)
        {
            garbageList.Add(bullet);
        }

        List<Bullet> bulletList;
        List<Bullet> garbageList;
    }
}

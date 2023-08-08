using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceInvadersClone
{
    class RectCollider
    {
        public RectCollider(Vector2f topLeftCorner, Vector2f bottomRightCorner)
        {
            this.topLeftCorner = topLeftCorner;
            this.bottomRightCorner = bottomRightCorner;
        }

        public bool CollidesWith(RectCollider other)
        {
            bool doesCollide, doesCollideX, doesCollideY;

            Vector2f otherTopRightCorner = new Vector2f(other.BottomRightCorner.X, other.TopLeftCorner.Y);
            Vector2f otherBottomLeftCorner = new Vector2f(other.TopLeftCorner.X, other.BottomRightCorner.Y);

            doesCollideX = (TopLeftCorner.X <= other.TopLeftCorner.X && other.TopLeftCorner.X <= BottomRightCorner.X)
                            || (TopLeftCorner.X <= other.BottomRightCorner.X && other.BottomRightCorner.X <= BottomRightCorner.X)
                            || (TopLeftCorner.X <= otherBottomLeftCorner.X && otherBottomLeftCorner.X <= BottomRightCorner.X)
                            || (TopLeftCorner.X <= otherTopRightCorner.X && otherTopRightCorner.X <= BottomRightCorner.X);

            doesCollideY = (TopLeftCorner.Y <= other.TopLeftCorner.Y && other.TopLeftCorner.Y <= BottomRightCorner.Y)
                           || (TopLeftCorner.Y <= other.BottomRightCorner.Y && other.BottomRightCorner.Y  <= BottomRightCorner.Y)
                           || (TopLeftCorner.Y <= otherBottomLeftCorner.Y && otherBottomLeftCorner.Y <= BottomRightCorner.Y)
                           || (TopLeftCorner.Y <= otherTopRightCorner.Y && otherTopRightCorner.Y <= BottomRightCorner.Y);

            //doesCollideX = !(TopLeftCorner.X < other.BottomRightCorner.X || other.TopLeftCorner.X < BottomRightCorner.X);
            //doesCollideY = ! (BottomRightCorner.Y < other.TopLeftCorner.Y || other.BottomRightCorner.Y < TopLeftCorner.Y);

            doesCollide = doesCollideX && doesCollideY;
            return doesCollide;
        }

        public Vector2f TopLeftCorner { get { return topLeftCorner; } set  { topLeftCorner = value; } }
        public Vector2f BottomRightCorner { get { return bottomRightCorner; } set { bottomRightCorner = value; } }

        Vector2f topLeftCorner, bottomRightCorner;
    }
}

using SFML.Graphics;
using SFML.System;

namespace SpaceInvadersClone
{
    class Lifebar
    {
        public Lifebar(Vector2f topLeftCorner, Vector2f dimensions, float percent = 1f) 
        {
            percentage = percent;

            position = new Vector2f(topLeftCorner.X, topLeftCorner.Y);
            this.dimensions = new Vector2f(dimensions.X, dimensions.Y);
            
            background = new RectangleShape(dimensions)
            {
                Position = new Vector2f(position.X, position.Y),
                FillColor = Color.Red
            };

            foreground = new RectangleShape(new Vector2f(dimensions.X * percentage, dimensions.Y))
            {
                Position = new Vector2f(topLeftCorner.X, topLeftCorner.Y),
                FillColor = Color.Green
            };
        }

        public void Draw(RenderWindow renderWindow)
        {
            renderWindow.Draw(background);
            renderWindow.Draw(foreground);
        }

        void UpdatePercentageVisualization()
        {
            background = new RectangleShape(dimensions)
            {
                Position = new Vector2f(position.X, position.Y),
                FillColor = Color.Red
            };

            foreground = new RectangleShape(new Vector2f(dimensions.X * percentage, dimensions.Y))
            {
                Position = new Vector2f(position.X, position.Y),
                FillColor = Color.Green
            };
        }

        void UpdatePosition()
        {
            background.Position = new Vector2f(position.X, position.Y);
            foreground.Position = new Vector2f(position.X, position.Y);
        }

        float percentage; // 0f <= percentage <= 1f
        public float Percentage 
        { 
            get { return percentage; } 
            set { percentage = Math.Max(0, Math.Min(1, value)); UpdatePercentageVisualization(); } 
        }

        public float X 
        { 
            get { return position.X; } 
            set { position.X = value; UpdatePosition(); } 
        }

        public float Y 
        { 
            get { return position.Y; } 
            set { position.Y = value; UpdatePosition(); } 
        }

        Vector2f position, dimensions;

        public Vector2f Dimensions { get { return dimensions; } set { dimensions = value; } }

        RectangleShape background, foreground;

    }
}

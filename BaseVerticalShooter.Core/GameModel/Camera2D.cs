using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace BaseVerticalShooter.GameModel
{
    public interface ICamera2d
    {
        Microsoft.Xna.Framework.Matrix GetTransformation(Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice);
        void Move(Microsoft.Xna.Framework.Vector2 amount);
        Microsoft.Xna.Framework.Vector2 Pos { get; set; }
        float Rotation { get; set; }
        float Zoom { get; set; }
    }

    public class Camera2d : ICamera2d
    {
        protected float _zoom; // Camera Zoom
        public Matrix _transform; // Matrix Transform
        public Vector2 _pos; // Camera Position
        protected float _rotation; // Camera Rotation

        public Camera2d()
        {
            _zoom = 1f;
            _rotation = 0.0f;
            _pos = Vector2.Zero;
        }

        // Sets and gets zoom
        public float Zoom
        {
            get { return _zoom; }
            set { _zoom = value; if (_zoom < 0.1f) _zoom = 0.1f; } // Negative zoom will flip image
        }

        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        // Auxiliary function to move the camera
        public void Move(Vector2 amount)
        {
            _pos += amount;
        }
        // Get set position
        public Vector2 Pos
        {
            get { return _pos; }
            set { _pos = value; }
        }

        public Matrix GetTransformation(GraphicsDevice graphicsDevice)
        {
            var center = new Vector3(
                -((Zoom - 1f) / 4f) * graphicsDevice.Viewport.Width,
                -((Zoom - 1f) / 4f) * graphicsDevice.Viewport.Height, 
                0);

            _transform =       // Thanks to o KB o for this solution
              Matrix.CreateTranslation(new Vector3(center.X, center.Y, 0)) *
                                         Matrix.CreateRotationZ(Rotation) *
                                         Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                                         //Matrix.CreateTranslation(new Vector3(graphicsDevice.Viewport.Width * 0.5f, graphicsDevice.Viewport.Height * 0.5f, 0));
                                         Matrix.CreateTranslation(new Vector3(0, 0, 0));
            return _transform;
        }
    }
}
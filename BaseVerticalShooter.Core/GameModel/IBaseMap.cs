using BaseVerticalShooter.Core;
using System;
namespace BaseVerticalShooter.GameModel
{
    public interface IBaseMap
    {
        void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Microsoft.Xna.Framework.GameTime gameTime, int tickCount, float scrollRows);
        //System.Threading.Tasks.Task<System.Collections.Generic.List<string>> GetMapLinesAsync();
        void Initialize();
        void LoadContent(IContentHelper contentHelper);
        void LoadMapContent();
        System.Collections.Generic.List<string> MapLines { get; set; }
        void RestoreScrollStartRow();
        void SaveScrollStartRow();
        event EventHandler Scrolled;
        float ScrollRows { get; set; }
        float ScrollStartRow { get; set; }
        int SegmentCount { get; set; }
        MapState State { get; set; }
        Shooter.GameModel.CollisionResult TestCollision(IPhysicalObject that, Microsoft.Xna.Framework.Vector2 thatNewPosition, float scrollRows);
        void UnLoadContent();
        void Update(Microsoft.Xna.Framework.GameTime gameTime, int tickCount, float sr);
    }

    public enum MapState
    {
        Scrolling,
        Timer,
        Paused
    }
}

using BaseVerticalShooter.GameModel;
using System;
using System.Collections.Generic;
namespace BaseVerticalShooter.Core.GameModel
{
    public interface IMap : IBaseMap
    {
        void Draw(global::Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, global::Microsoft.Xna.Framework.GameTime gameTime, int tickCount, float scrollRows);
        global::System.Threading.Tasks.Task<global::BaseVerticalShooter.JsonModels.JsonMap> GetLayersAsync();
        global::System.Threading.Tasks.Task<List<string>> GetMapLinesAsync(int levelNumber);
        void LoadMapContent();
        global::Shooter.GameModel.CollisionResult TestCollision(global::BaseVerticalShooter.GameModel.IPhysicalObject that, global::Microsoft.Xna.Framework.Vector2 thatNewPosition, float scrollRows);
    }
}

using BaseVerticalShooter;
using BaseVerticalShooter.GameModel;
using BaseVerticalShooter.JsonModels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shooter;
using Shooter.GameModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BaseVerticalShooter.Core.GameModel
{
    public class Map : BaseMap, IMap
    {
        const int MAP_COLS = 16;
        const int MAP_ROWS = 118;
        const int MINI_TILE_WIDTH = 16;
        const int LAYER_COUNT = 3;
            
        Texture2D defaultTileTexture = null;
        Texture2D layer0Texture = null;
        Texture2D layer1Texture = null;
        public JsonMap jsonMap = null;
        IJsonMapManager jsonMapManager;

        public Map(int levelNumber, IJsonMapManager jsonMapManager): base(levelNumber)
        {
            this.jsonMapManager = jsonMapManager;
        }

        public override void LoadMapContent()
        {
            LoadTextures();
            LoadJsonMap();
        }

        public override void LoadJsonMap()
        {
            jsonMap = new JsonMap
            {
                layers = new List<JsonLayer>() 
                { 
                    new JsonLayer 
                    {
                         index = 0,
                         map = "Layer0.png",
                         tileIndexes = new List<TileInfo>()
                    },
                    new JsonLayer 
                    {
                         index = 1,
                         map = "Layer1.png",
                         tileIndexes = new List<TileInfo>()
                    },
                    new JsonLayer 
                    {
                         index = 2,
                         map = "Layer1.png",
                         tileIndexes = new List<TileInfo>()
                    },
                    new JsonLayer 
                    {
                         index = 3,
                         map = "Layer1.png",
                         tileIndexes = new List<TileInfo>()
                    },
                    new JsonLayer 
                    {
                         index = 4,
                         map = "EnemySpriteSheet.png",
                         tileIndexes = new List<TileInfo>()
                    }
                }
            };

            var index = 0;
            for (var y = 0; y < MAP_ROWS; y++)
            {
                for (var x = 0; x < MAP_COLS; x++)
                {
                    jsonMap.layers[0].tileIndexes.Add(new TileInfo
                    {
                        i = index,
                        ti = 0
                    });

                    jsonMap.layers[1].tileIndexes.Add(new TileInfo
                    {
                        i = index,
                        ti = 0
                    });

                    jsonMap.layers[2].tileIndexes.Add(new TileInfo
                    {
                        i = index,
                        ti = 0
                    });

                    jsonMap.layers[3].tileIndexes.Add(new TileInfo
                    {
                        i = index,
                        ti = 0
                    });

                    jsonMap.layers[4].tileIndexes.Add(new TileInfo
                    {
                        i = index,
                        ti = 0
                    });
                    index++;
                }
            }

            //if (jsonMapManager is DummyJsonMapManager)
                jsonMapManager.GetJsonMap(levelNumber, jsonMap);
            //else
            //    GetLayersAsync();
        }

        public override void LoadTextures()
        {
            defaultTileTexture = defaultTileTexture ?? ContentHelper.Instance.GetContent<Texture2D>("DefaultTile");
            layer0Texture = layer0Texture ?? ContentHelper.Instance.GetContent<Texture2D>("Layer0");
            layer1Texture = layer1Texture ?? ContentHelper.Instance.GetContent<Texture2D>("Layer1");
        }

        public async Task<JsonMap> GetLayersAsync()
        {
            JsonMap map = await jsonMapManager.GetJsonMap(levelNumber, jsonMap);
            return map;
        }

        public async Task<List<string>> GetMapLinesAsync(int levelNumber)
        {
            return await jsonMapManager.GetMapLinesAsync(levelNumber);
        }

        public override void Draw(SpriteBatch spriteBatch, Microsoft.Xna.Framework.GameTime gameTime, int tickCount, float scrollRows)
        {
            DrawDefaultTileTexture(spriteBatch, scrollRows);
            DrawAutoTiles(spriteBatch, scrollRows);

        }

        private void DrawDefaultTileTexture(SpriteBatch spriteBatch, float scrollRows)
        {
            for (var y = 0; y <= GameSettings.Instance.WindowTilesSize.Y / 2; y++)
            {
                var scrollTileOffset = (int)(((scrollRows / 2) - Math.Floor(scrollRows / 2))
                        * GameSettings.Instance.MapTileWidth);

                for (var x = 0; x < GameSettings.Instance.WindowTilesSize.X / 2; x++)
                {
                    spriteBatch.Draw(defaultTileTexture,
                        new Rectangle(
                                (int)TopLeftCorner.X + x * GameSettings.Instance.MapTileWidth,
                                (int)TopLeftCorner.Y + y * GameSettings.Instance.MapTileWidth
                                    - scrollTileOffset
                                , GameSettings.Instance.MapTileWidth
                                , GameSettings.Instance.MapTileWidth)
                            , Color.White);
                }
            }
        }

        private void DrawAutoTiles(SpriteBatch spriteBatch, float scrollRows)
        {
            for (var i = 0; i <= LAYER_COUNT; i++)
            {
                for (var y = 0; y <= GameSettings.Instance.WindowTilesSize.Y / 2; y++)
                {
                    var scrollTileOffset = (int)(((scrollRows / 2) - Math.Floor(scrollRows / 2))
                            * GameSettings.Instance.MapTileWidth);

                    for (var x = 0; x < GameSettings.Instance.WindowTilesSize.X / 2; x++)
                    {

                        if (jsonMap != null)
                        {
                            var tileInfo = jsonMap.layers[i].tileIndexes[(int)(x + (y + (int)((scrollRows / 2))) * MAP_COLS)];
                            tileInfo.x = x;
                            tileInfo.y = y;
                            if (tileInfo.ti != 0)
                            {
                                if (i == 0)
                                    DrawMiniTiles(spriteBatch, tileInfo, tileInfo.ti);
                                else
                                    spriteBatch.Draw(layer1Texture,
                                        new Rectangle(
                                            (int)(TopLeftCorner.X + x * GameSettings.Instance.MapTileWidth),
                                            (int)(TopLeftCorner.Y + y * GameSettings.Instance.MapTileWidth)
                                                - scrollTileOffset,
                                        GameSettings.Instance.MapTileWidth,
                                        GameSettings.Instance.MapTileWidth),
                                        new Rectangle(
                                            (int)((tileInfo.ti % 16) * GameSettings.Instance.MapTileWidth),
                                            (int)((tileInfo.ti / 16) * GameSettings.Instance.MapTileWidth),
                                        GameSettings.Instance.MapTileWidth,
                                        GameSettings.Instance.MapTileWidth),
                                        Color.White
                                );
                            }
                        }
                    }
                }
            }
        }


        TileInfo GetTile(int x, int y, int defaultTileIndex)
        {
            var index = x + y * MAP_COLS;
            if (x < 0 || x >= MAP_COLS || y < 0 || y >= MAP_ROWS) {
                return new TileInfo { i = 0, x = x, y = y, ti = defaultTileIndex };
            }
            else if (index < 0 || index > jsonMap.layers[0].tileIndexes.Count)
                return new TileInfo { i = 0, x = -1, y = -1, ti = 0 };
            else
            {
                var tile = jsonMap.layers[0].tileIndexes[x + y * MAP_COLS];
                return tile;
            }
        }

        void DrawMiniTiles(SpriteBatch spriteBatch, TileInfo tile, int selectedTileIndex) 
        {
            if (tile == null) {
                return;
            }

            var tileIndex = tile.ti;
            if (tileIndex != selectedTileIndex)
                return;

            var selectedTile = new TileInfo { x = tile.x, y = tile.y, ti = tile.ti };
                    
            var x = tile.i % MINI_TILE_WIDTH;
            var y = tile.i / MINI_TILE_WIDTH;

            var nbW = GetTile(x - 1, y, tileIndex);
            var nbNW = GetTile(x - 1, y - 1, tileIndex);
            var nbN = GetTile(x, y - 1, tileIndex);
            var nbNE = GetTile(x + 1, y - 1, tileIndex);
            var nbE = GetTile(x + 1, y, tileIndex);
            var nbSE = GetTile(x + 1, y + 1, tileIndex);
            var nbS = GetTile(x, y + 1, tileIndex);
            var nbSW = GetTile(x - 1, y + 1, tileIndex);

            var tileOffset = new Vector2(0, 0);

            if (nbNW.ti != tileIndex && nbN.ti != tileIndex && nbW.ti != tileIndex)
                tileOffset = new Vector2(0, 2);
            else if (nbN.ti == tileIndex && nbW.ti != tileIndex)
                tileOffset = new Vector2(0, 3);
            else if (nbN.ti != tileIndex && nbW.ti == tileIndex)
                tileOffset = new Vector2(1, 2);
            else if (nbNW.ti != tileIndex && nbN.ti == tileIndex && nbW.ti == tileIndex)
                tileOffset = new Vector2(2, 0);
            else if (nbNW.ti == tileIndex && nbN.ti == tileIndex && nbW.ti == tileIndex)
                tileOffset = new Vector2(1, 3);

            DrawMiniTile(spriteBatch, selectedTile, tileOffset, "NW");

            tileOffset = new Vector2(3, 2);

            if (nbNE.ti != tileIndex && nbN.ti != tileIndex && nbE.ti != tileIndex)
                tileOffset = new Vector2(3, 2);
            else if (nbN.ti == tileIndex && nbE.ti != tileIndex)
                tileOffset = new Vector2(3, 3);
            else if (nbN.ti != tileIndex && nbE.ti == tileIndex)
                tileOffset = new Vector2(1, 2);
            else if (nbNE.ti != tileIndex && nbN.ti == tileIndex && nbE.ti == tileIndex)
                tileOffset = new Vector2(3, 0);
            else if (nbNE.ti == tileIndex && nbN.ti == tileIndex && nbE.ti == tileIndex)
                tileOffset = new Vector2(2, 3);

            DrawMiniTile(spriteBatch, selectedTile, tileOffset, "NE");

            tileOffset = new Vector2(0, 5);

            if (nbSW.ti != tileIndex && nbS.ti != tileIndex && nbW.ti != tileIndex)
                tileOffset = new Vector2(0, 5);
            else if (nbS.ti == tileIndex && nbW.ti != tileIndex)
                tileOffset = new Vector2(0, 4);
            else if (nbS.ti != tileIndex && nbW.ti == tileIndex)
                tileOffset = new Vector2(1, 5);
            else if (nbSW.ti != tileIndex && nbS.ti == tileIndex && nbW.ti == tileIndex)
                tileOffset = new Vector2(2, 1);
            else if (nbSW.ti == tileIndex && nbS.ti == tileIndex && nbW.ti == tileIndex)
                tileOffset = new Vector2(1, 4);

            DrawMiniTile(spriteBatch, selectedTile, tileOffset, "SW");

            tileOffset = new Vector2(3, 5);

            if (nbSE.ti != tileIndex && nbS.ti != tileIndex && nbE.ti != tileIndex)
                tileOffset = new Vector2(3, 5);
            else if (nbS.ti == tileIndex && nbE.ti != tileIndex)
                tileOffset = new Vector2(3, 4);
            else if (nbS.ti != tileIndex && nbE.ti == tileIndex)
                tileOffset = new Vector2(2, 5);
            else if (nbSE.ti != tileIndex && nbS.ti == tileIndex && nbE.ti == tileIndex)
                tileOffset = new Vector2(3, 1);
            else if (nbSE.ti == tileIndex && nbS.ti == tileIndex && nbE.ti == tileIndex)
                tileOffset = new Vector2(2, 4);

            DrawMiniTile(spriteBatch, selectedTile, tileOffset, "SE");
        }

        void DrawMiniTile(SpriteBatch spriteBatch, TileInfo selectedTile, Vector2 sourceTileOffset, string miniTileCode)
        {
            var destTileOffset = Vector2.Zero;
            var destMiniTileOffset = Vector2.Zero;
            switch (miniTileCode) {
                case "NW":
                    destMiniTileOffset = new Vector2(0, 0);
                    break;
                case "NE":
                    destMiniTileOffset = new Vector2(1, 0);
                    break;
                case "SW":
                    destMiniTileOffset = new Vector2(0, 1);
                    break;
                case "SE":
                    destMiniTileOffset = new Vector2(1, 1);
                    break;
            }

            var sx = 64 * (selectedTile.ti % 8) + sourceTileOffset.X * MINI_TILE_WIDTH;
            var sy = 96 * (int)(selectedTile.ti / 8) + sourceTileOffset.Y * MINI_TILE_WIDTH;
            var dx = (selectedTile.x) * 32 + destMiniTileOffset.X * MINI_TILE_WIDTH;
            var dy = (selectedTile.y) * 32 + destMiniTileOffset.Y * MINI_TILE_WIDTH;

            spriteBatch.Draw(layer0Texture,
                new Rectangle(
                    (int)(TopLeftCorner.X + selectedTile.x * GameSettings.Instance.MapTileWidth)
                        + (int)(destMiniTileOffset * MINI_TILE_WIDTH).X,
                    (int)(TopLeftCorner.Y + selectedTile.y * GameSettings.Instance.MapTileWidth)
                        - (int)(((scrollRows / 2) - Math.Floor(scrollRows / 2)) * GameSettings.Instance.MapTileWidth)
                        + (int)(destMiniTileOffset * MINI_TILE_WIDTH).Y,
                    MINI_TILE_WIDTH,
                    MINI_TILE_WIDTH),
                new Rectangle(
                    (int)((selectedTile.ti % 8) * 64 + sourceTileOffset.X * MINI_TILE_WIDTH),
                    (int)((selectedTile.ti / 8) * 96 + sourceTileOffset.Y * MINI_TILE_WIDTH),
                    MINI_TILE_WIDTH,
                    MINI_TILE_WIDTH),
                Color.White);

        }

        public override CollisionResult TestCollision(IPhysicalObject that, Vector2 thatNewPosition, float scrollRows)
        {
            var collisionResult = new CollisionResult();

            if (thatNewPosition.X < 0 ||
                thatNewPosition.X + that.Size.X > GameSettings.Instance.WindowTilesSize.X ||
                thatNewPosition.Y < 0 ||
                thatNewPosition.Y + that.Size.Y > GameSettings.Instance.WindowTilesSize.Y)
            {
                collisionResult.CollisionType = CollisionType.OffRoad;
                var x = Math.Min(thatNewPosition.X, GameSettings.Instance.WindowTilesSize.X - that.Size.X);
                x = Math.Max(x, 0);
                var y = Math.Min(thatNewPosition.Y, GameSettings.Instance.WindowTilesSize.Y - that.Size.Y);
                y = Math.Max(y, 0);
            }
            else
            {
                for (var deltaX = 0; deltaX < that.Size.X; deltaX++)
                {
                    for (var deltaY = 0; deltaY < that.Size.Y; deltaY++)
                    {
                        var f = (thatNewPosition.X + deltaX) / 2f;
                        var x = (int)Math.Round(f * 2, MidpointRounding.AwayFromZero) / 2;
                        f = (scrollRows / 2) + (thatNewPosition.Y + deltaY) / 2;
                        var y = (int)Math.Round(f * 2, MidpointRounding.AwayFromZero) / 2;

                        if ((x < 0
                            || x >= MAP_COLS)
                            || y < 0
                            || y >= MAP_ROWS)
                        {
                            collisionResult.CollisionType = CollisionType.OffRoad;
                            return collisionResult;
                        }
                        else
                        {
                            var tileInfo = jsonMap.layers[0].tileIndexes[(int)(x + y * MAP_COLS)];
                            
                            if (tileInfo.ti != 0)
                            {
                                collisionResult.CollisionType = CollisionType.Blocked;
                                return collisionResult;
                            }
                        }
                    }
                }
            }

            return collisionResult;
        }

    }

    public interface IJsonMapManager
    {
        Task<JsonMap> GetJsonMap(int levelNumber, JsonMap jsonMap);
        Task<List<string>> GetMapLinesAsync(int levelNumber);
    }
}

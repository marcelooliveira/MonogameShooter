﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using xInput = Microsoft.Xna.Framework.Input;
using ScreenControlsSample;
using Microsoft.Xna.Framework.Content;
using System.IO;
using BaseVerticalShooter;
using BaseVerticalShooter.Input;
using BaseVerticalShooter.GameModel;
using BaseVerticalShooter.Core;

namespace Shooter.GameModel
{
    public class BaseMap : PhysicalObject, IBaseMap
    {
        Texture2D map01;
        //Texture2D path01;
        int tickInMS = 125;
        TimeSpan accumElapsedGameTime = TimeSpan.FromSeconds(0);
        TimeSpan accumElapsedPauseTime = TimeSpan.FromSeconds(0);
        float scrollTimeOutInMS = 125f;
        TimeSpan accumScrollTime = TimeSpan.FromSeconds(0);
        public event EventHandler Scrolled;
        protected int levelNumber;
        int mapTickCount = 0;

        float scrollStartRow = 207;
        public float ScrollStartRow
        {
            get
            {
                return scrollStartRow;
            }
            set
            {
                scrollStartRow = value;
            }
        }

        MapState state = MapState.Scrolling;
        public MapState State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
            }
        }

        protected float scrollRows = 0;
        public float ScrollRows
        {
            get { return scrollRows; }
            set { scrollRows = value; }
        }

        int segmentCount = 4;
        public int SegmentCount
        {
            get { return segmentCount; }
            set { segmentCount = value; }
        }

        List<string> mapLines = new List<string>();
        public List<string> MapLines
        {
            get
            {
                return mapLines;
            }
            set
            {
                mapLines = value;
            }
        }

        public BaseMap(int levelNumber)
            : base()
        {
            scrollRows = scrollStartRow;
            this.levelNumber = levelNumber;
            //LoadMapContent();
            Initialize();
        }

        protected override void SetNewContent(ContentManager content)
        {
            this.content = new ContentManager(content.ServiceProvider, "Content");
        }

        public void SaveScrollStartRow()
        {
            ScrollStartRow = ScrollRows;
        }

        public void RestoreScrollStartRow()
        {
            ScrollRows = ScrollStartRow;
        }

        public override void LoadContent(IContentHelper contentHelper)
        {
            LoadMapContent();
        }

        public virtual void LoadMapContent()
        {
            map01 = map01 ?? ContentHelper.Instance.GetContent<Texture2D>(string.Format("Map{0:d2}", levelNumber));
        }

        public virtual void LoadJsonMap()
        {

        }

        public virtual void LoadTextures()
        {

        }

        public void Initialize()
        {
            scrollRows = scrollStartRow;
        }

        public override void Update(GameTime gameTime, int tickCount, float sr)
        {
            if (State == MapState.Scrolling)
            {
                if (accumElapsedGameTime >= TimeSpan.FromMilliseconds(tickInMS))
                {
                    accumElapsedGameTime = TimeSpan.FromSeconds(0);
                    mapTickCount++;
                }

                accumElapsedGameTime = accumElapsedGameTime.Add(gameTime.ElapsedGameTime);

                if (scrollRows > 0)
                {
                    if (accumScrollTime >= TimeSpan.FromMilliseconds(scrollTimeOutInMS))
                    {
                        accumScrollTime = TimeSpan.FromSeconds(0);
                        scrollRows -= .125f;
                        OnScrolled(new EventArgs());
                    }
                    accumScrollTime = accumScrollTime.Add(gameTime.ElapsedGameTime);
                }
            }
            else
            {
                if (state == MapState.Timer)
                {
                    accumElapsedPauseTime = accumElapsedPauseTime.Add(gameTime.ElapsedGameTime);
                    if (accumElapsedPauseTime > TimeSpan.FromSeconds(5))
                    {
                        accumElapsedPauseTime = TimeSpan.FromSeconds(0);
                        state = MapState.Scrolling;
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, int tickCount, float scrollRows)
        {
            spriteBatch.Draw(map01,
                    TopLeftCorner,
                    new Rectangle(0
                        , (int)(scrollRowHeight * this.ScrollRows)
                        , (int)(GameSettings.Instance.WindowTilesSize.X * TileWidth)
                        , (int)(GameSettings.Instance.WindowTilesSize.Y * TileWidth)),
                    Color.White,
                    0,
                    new Vector2(0, 0),
                    1f,
                    SpriteEffects.None,
                    0);
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
                for (var deltaX = 0; deltaX < that.Size.X; deltaX++ )
                {
                    for (var deltaY = 0; deltaY < that.Size.Y; deltaY++)
                    {
                        var f = (thatNewPosition.X + deltaX) / 2f;
                        var x = (int)Math.Round(f * 2, MidpointRounding.AwayFromZero) / 2;
                        f = (scrollRows / 2) + (thatNewPosition.Y + deltaY) / 2;
                        var y = (int)Math.Round(f * 2, MidpointRounding.AwayFromZero) / 2;

                        if ((x < 0 
                            || x >= (GameSettings.Instance.WindowTilesSize.X / 2))
                            ||y < 0 
                            || y >= (mapLines.Count() * 2))
                        {
                            collisionResult.CollisionType = CollisionType.OffRoad;
                            return collisionResult;
                        }
                        else
                        {
                            var mapChar = mapLines[y][x];

                            if ("XD".Contains(mapChar))
                            {
                                collisionResult.CollisionType = CollisionType.Blocked;
                                return collisionResult;
                            }
                        }
                    }
                }

                //var colorArray = new Color[(int)(that.Size.X * that.Size.Y)];
                //var extractRegion = new Rectangle(
                //    (int)Math.Round(thatNewPosition.X),
                //    (int)Math.Round(scrollRows + thatNewPosition.Y),
                //    (int)that.Size.X,
                //    (int)that.Size.Y);
                //path01.GetData<Color>(0, extractRegion, colorArray, 0, (int)(that.Size.X * that.Size.Y));
                //foreach (var color in colorArray)
                //{
                //    if (color != Color.White)
                //    {
                //        collisionResult.CollisionType = CollisionType.Blocked;
                //        break;
                //    }
                //}                
            }

            return collisionResult;
        }

        protected virtual void OnScrolled(EventArgs e)
        {
            if (Scrolled != null)
                Scrolled(this, e);
        }

        public void UnLoadContent()
        {
            if (content != null)
                content.Unload();
        }
    }
}
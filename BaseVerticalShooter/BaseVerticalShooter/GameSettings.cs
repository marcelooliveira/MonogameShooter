using BaseVerticalShooter.Core.JsonModels;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Windows.Storage;

namespace BaseVerticalShooter
{
    public class GameSettings
    {
        private static GameSettings instance;
        Vector2 deviceScreenSize = new Vector2(800f, 480f);
        Vector2 gameScreenSize = new Vector2(512, 384);
        Vector2 gameScreenTilesSize = new Vector2(32, 30);
        Vector2 windowTilesSize = new Vector2(32, 28);
        int mapTileWidth = 32;
       
        private GameSettings()
        {

        }

        public static GameSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameSettings();
                    instance.GetJsonOppositionAsync();
                }
                return instance;
            }
        }

        public Vector2 DeviceScreenSize { get { return deviceScreenSize; } }
        public Vector2 GameScreenSize { get { return gameScreenSize; } }
        public Vector2 GameScreenTilesSize { get { return gameScreenTilesSize; } }
        public Vector2 WindowTilesSize { get { return windowTilesSize; } }
        public int MapTileWidth { get { return mapTileWidth; } }
        public JsonOpposition Opposition { get; set; }

        public async void GetJsonOppositionAsync()
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///Content/JsonOpposition.txt"));
            using (StreamReader sRead = new StreamReader(await file.OpenStreamForReadAsync()))
            {
                var json = sRead.ReadToEnd();
                Opposition = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonOpposition>(json);
            }
        }
    }
}

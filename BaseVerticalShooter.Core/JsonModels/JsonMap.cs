using System;
using System.Collections.Generic;
using System.Text;

namespace BaseVerticalShooter.JsonModels
{
    public class JsonMap
    {
        public List<JsonLayer> layers { get; set; }
    }

    public class JsonLayer
    {
        public int index { get; set; }
        public string map { get; set; }
        public List<TileInfo> tileIndexes { get; set; }
    }

    public class TileInfo
    {
        public int x { get; set; }
        public int y { get; set; }
        public int i { get; set; }
        public int ti { get; set; }
    }
}

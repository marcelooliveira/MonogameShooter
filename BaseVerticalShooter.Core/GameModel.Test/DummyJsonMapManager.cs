using BaseVerticalShooter.JsonModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseVerticalShooter.Core.GameModel.Test
{
    public class DummyJsonMapManager : IJsonMapManager
    {
        public Task<JsonMap> GetJsonMap(int levelNumber, JsonMap jsonMap)
        {
            string[] lines = new string[]
            {
            "00100",
            "00000",
            "10001",
            "00000",
            "00100"};

            var y = 0;
            foreach (var line in lines)
            {
                var x = 0;
                foreach (var c in line)
                {
                    var ti = int.Parse(c.ToString());
                    jsonMap.layers[0].tileIndexes[y * 16 + x].ti = ti;
                    x++;
                }
                y++;
            }                

            return Task.FromResult(jsonMap);
        }

        public async Task<List<string>> GetMapLinesAsync(int levelNumber)
        {
            return new List<string>();
        }
    }
}

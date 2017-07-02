using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseVerticalShooter.Core.GameModel
{
    public interface IDirectionable
    {
        Vector2 Direction { get; set; }
    }
}

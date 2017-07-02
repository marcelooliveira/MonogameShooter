using BaseVerticalShooter;
using BaseVerticalShooter.Core;
using BaseVerticalShooter.GameModel;
using BaseVerticalShooter.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace BaseVerticalShooter
{
    public class Resolver : BaseResolver
    {
        public override void RegisterGame(ContentManager content, IScreenPad screenPad)
        {
            base.RegisterGame(content, screenPad);
            RegisterInstance<IScreenPad>(screenPad);
        }
    }
}

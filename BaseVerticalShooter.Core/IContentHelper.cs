using Microsoft.Xna.Framework.Audio;
using System;

namespace BaseVerticalShooter.Core
{
    public interface IContentHelper
    {
        T GetContent<T>(string name);
        SoundEffectInstance GetSoundEffectInstance(string name);
        void Unload();
    }
}

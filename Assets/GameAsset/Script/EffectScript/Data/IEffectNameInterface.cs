using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlotEffectMaker2023.Data
{
    public enum EChangeNameType { Var, Timer, SoundID, SoundPlayer, Timeline, ColorMap, Collection, Info, None }

    interface IEffectNameInterface : SlotMaker2022.ILocalDataInterface
    {
        void Rename(EChangeNameType type, string src, string dst);
    }
}

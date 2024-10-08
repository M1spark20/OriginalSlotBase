﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlotEffectMaker2023.Data
{
    public enum EChangeNameType { Var, Timer, SoundID, SoundPlayer, Timeline, ColorMap, MapPlayer, Collection, Info, GameAchievement, None }

    interface IEffectNameInterface : SlotMaker2022.ILocalDataInterface
    {
        void Rename(EChangeNameType type, string src, string dst);
    }
}

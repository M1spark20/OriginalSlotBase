using System;
using System.IO;

namespace SlotMaker2022
{
    interface ILocalDataInterface
    {
        bool StoreData(ref BinaryWriter fs, int version);
        bool ReadData(ref BinaryReader fs, int version);
    }

    public class UserBaseData
    {
        readonly uint[] elemData;
        public uint ElemSize { get; }
        public uint BaseNum { get; }

        public UserBaseData(uint baseVal, bool fixedBase, uint dataSize)
        {
            // fixedBase:trueでBaseNumが2^baseVal(=桁数)に、falseでそのままになる
            BaseNum = fixedBase ? (uint)Math.Pow(2, baseVal) : baseVal;
            ElemSize = dataSize;
            elemData = new uint[dataSize];
            for (int count = 0; count < ElemSize; ++count) elemData[count] = 0;
        }
        public UserBaseData(UserBaseData deepCopySrc)
        {   // 引数データのディープコピー(値渡し)を生成する
            BaseNum = deepCopySrc.BaseNum;
            ElemSize = deepCopySrc.ElemSize;
            elemData = new uint[deepCopySrc.elemData.Length];
            for (int count = 0; count < elemData.Length; ++count) elemData[count] = deepCopySrc.elemData[count];
        }
        public bool SetData(uint pos, uint inputData)
        {
            if (pos >= ElemSize) return false;
            if (inputData >= BaseNum) return false;
            elemData[pos] = inputData;
            return true;
        }
        public uint GetData(uint pos)
        {
            if (pos >= ElemSize) return uint.MaxValue;
            return elemData[pos];
        }
        public decimal Export()
        {
            decimal ans = 0;
            decimal expNum = 1;
            for (int count = 0; count < ElemSize; ++count)
            {
                if (count > 0) expNum *= BaseNum;
                ans += elemData[count] * expNum;
            }
            return ans;
        }
        public void Import(decimal importData)
        {
            for (int count = 0; count < ElemSize; ++count)
            {
                elemData[count] = decimal.ToUInt32(importData % BaseNum);
                importData /= BaseNum;
            }
        }
    }

}

using System.Collections.Generic;
using System.IO;

namespace SlotEffectMaker2023.Data
{
    public class FlagCounterCond : IEffectNameInterface
    {   // 小役成立回数のカウントに使用
        public string OutVar { get; set; }  // 出力先変数
        public byte FlagMin { get; set; }   // カウントフラグ最小
        public byte FlagMax { get; set; }   // カウントフラグ最大

		public FlagCounterCond()
        {
			OutVar = string.Empty;
			FlagMin = 0;
			FlagMax = 0;
        }
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			fs.Write(OutVar);
			fs.Write(FlagMin);
			fs.Write(FlagMax);
			return true;
		}
		public bool ReadData(ref BinaryReader fs, int version)
		{
			OutVar = fs.ReadString();
			FlagMin = fs.ReadByte();
			FlagMax = fs.ReadByte();
			return true;
		}
		public void Rename(EChangeNameType type, string src, string dst) {
			if (type == EChangeNameType.Var && OutVar.Equals(src)) OutVar = dst;
		}
	}

	public class FlagCounterSet : IEffectNameInterface
    {
        public List<FlagCounterCond> elemData { get; set; }
        public string CountCond { get; set; }   // カウント条件(ActVCを指定)

		public FlagCounterSet()
        {
			elemData = new List<FlagCounterCond>();
			CountCond = string.Empty;
        }
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			fs.Write(CountCond);
			fs.Write(elemData.Count);
			foreach (var item in elemData) item.StoreData(ref fs, version);
			return true;
		}
		public bool ReadData(ref BinaryReader fs, int version)
		{
			CountCond = fs.ReadString();
			int dataCount = fs.ReadInt32();
			for (int i = 0; i < dataCount; ++i)
			{
				FlagCounterCond fc = new FlagCounterCond();
				fc.ReadData(ref fs, version);
				elemData.Add(fc);
			}
			return true;
		}
		public void Rename(EChangeNameType type, string src, string dst) {
			foreach (var item in elemData) item.Rename(type, src, dst);
			if (type == EChangeNameType.Timeline && CountCond.Equals(src)) CountCond = dst;
		}
    }
}

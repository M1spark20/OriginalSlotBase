using System;
using System.Collections.Generic;
using System.IO;


namespace SlotEffectMaker2023.Data
{
	public class BonusConfig : IEffectNameInterface
	{
		public byte BonusID { get; set; }
		public byte ComaID { get; set; }
		public byte BonusType { get; set; }

		public BonusConfig()
        {
			BonusID = 0;
			ComaID = 0;
			BonusType = 0;
        }
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			fs.Write(BonusID);
			fs.Write(ComaID);
			fs.Write(BonusType);
			return true;
		}
		public bool ReadData(ref BinaryReader fs, int version)
		{
			BonusID = fs.ReadByte();
			ComaID = fs.ReadByte();
			BonusType = fs.ReadByte();
			return true;
		}
		public void Rename(EChangeNameType type, string src, string dst) { }
	}
	public class HistoryConfig : IEffectNameInterface
    {
		// 定数
		public const int BONUS_TYPE_MAX = 3;	// 1-記載数まで、0は総計で使用

		public List<BonusConfig> BonusConfig { get; set; }	// 各ボーナスの設定
		public string LaunchEffect { get; set; }			// 成立時演出取得用変数名
		public string InGameHolder { get; set; }			// 入賞G数取得用変数名
		public List<string> BonusCountHolder { get; set; }	// ボーナス入賞回数設定先変数名

		public HistoryConfig()
        {
			BonusConfig = new List<BonusConfig>();
			InGameHolder = string.Empty;
			LaunchEffect = string.Empty;
			BonusCountHolder = new List<string>();
			for (int i = 0; i <= BONUS_TYPE_MAX; ++i) BonusCountHolder.Add(string.Empty);
        }
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			fs.Write(BonusConfig.Count);
			foreach (var item in BonusConfig) item.StoreData(ref fs, version);
			fs.Write(LaunchEffect);
			fs.Write(InGameHolder);
			fs.Write(BonusCountHolder.Count);
			foreach (var item in BonusCountHolder) fs.Write(item);
			return true;
		}
		public bool ReadData(ref BinaryReader fs, int version)
		{
			int dataCount = fs.ReadInt32();
			for (int i = 0; i < dataCount; ++i)
			{
				BonusConfig bc = new BonusConfig();
				bc.ReadData(ref fs, version);
				BonusConfig.Add(bc);
			}
			LaunchEffect = fs.ReadString();
			InGameHolder = fs.ReadString();
			dataCount = fs.ReadInt32();
			for (int i = 0; i < dataCount && i < BONUS_TYPE_MAX; ++i)
				BonusCountHolder[i] = fs.ReadString();
			return true;
		}
		public void Rename(EChangeNameType type, string src, string dst)
		{
			foreach (var item in BonusConfig) item.Rename(type, src, dst);
			if (type == EChangeNameType.Var && InGameHolder.Equals(src)) InGameHolder = dst;
			if (type == EChangeNameType.Var && LaunchEffect.Equals(src)) LaunchEffect = dst;
			for (int i = 0; i < BONUS_TYPE_MAX; ++i)
				if (type == EChangeNameType.Var && BonusCountHolder[i].Equals(src)) BonusCountHolder[i] = dst;
		}
		public BonusConfig GetConfig(int pBonusID)
		{
			foreach (var item in BonusConfig)
				if (item.BonusID == pBonusID) return item;
			return null;
		}
	}
}

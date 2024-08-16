using System;
using System.Collections.Generic;
using System.IO;

namespace SlotEffectMaker2023.Data
{
    public enum AchieveDataType { Num, Flag }

    public class GameAchievement : IEffectNameInterface
    {
        public AchieveDataType Type { get; set; }   // データか実績かを選択する
        public string DataID { get; set; }          // データIDを指定する
        public string RefData { get; set; }         // チェック対象データを選択(データ：代入先変数名/フラグ：達成条件)
        public bool UpdateOnlyBonusIn { get; set; } // ボーナス入賞時にのみ評価するか
        public bool IsAchieved { get; set; }        // <保存対象外>ゲーム内で実績達成済みか(リモート取得)
        public string Title { get; private set; }   // <保存対象外>実績タイトル(リモート取得)
        public string Desc { get; private set; }    // <保存対象外>実績の説明(リモート取得)

        public GameAchievement()
        {
            Type = AchieveDataType.Flag;
            DataID = string.Empty;
            RefData = string.Empty;
            IsAchieved = false;
            Title = string.Empty;
            Desc = string.Empty;
        }

        public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write((byte)Type);
            fs.Write(DataID);
            fs.Write(RefData);
            fs.Write(UpdateOnlyBonusIn);

            return true;
        }
        public bool ReadData(ref BinaryReader fs, int version)
        {
            Type = (AchieveDataType)fs.ReadByte();
            DataID = fs.ReadString();
            RefData = fs.ReadString();
            UpdateOnlyBonusIn = fs.ReadBoolean();
            return true;
        }
        public void Rename(EChangeNameType type, string src, string dst) {
            if (type == EChangeNameType.Var && Type == AchieveDataType.Num && RefData.Equals(src)) RefData = dst;
            if (type == EChangeNameType.Timeline && Type == AchieveDataType.Flag && RefData.Equals(src)) RefData = dst;
        }

        public void SetDetail(bool pAchievedFlag, string pTitle, string pDesc)
        {
            IsAchieved = pAchievedFlag;
            Title = pTitle;
            Desc = pDesc;
        }
    }

    public class GameAchievementList : IEffectNameInterface
    {
        public List<GameAchievement> elemData { get; set; }

        public GameAchievementList()
        {
            elemData = new List<GameAchievement>();
        }
        public bool StoreData(ref BinaryWriter fs, int version)
        {
            int elemSize = elemData.Count;
            fs.Write(elemSize);
            for (int i = 0; i < elemSize; ++i) elemData[i].StoreData(ref fs, version);
            return true;
        }
		public bool ReadData(ref BinaryReader fs, int version)
        {
			int mapSize = fs.ReadInt32();
			for (int i = 0; i < mapSize; ++i)
			{
				GameAchievement ga = new GameAchievement();
				ga.ReadData(ref fs, version);
				elemData.Add(ga);
			}
			return true;
        }
		public void Rename(EChangeNameType type, string src, string dst)
        {
			foreach (var ga in elemData) ga.Rename(type, src, dst);
        }
    }
}

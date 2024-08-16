using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SlotEffectMaker2023.Data
{
    public enum CollectionReelPattern 
    {   // 各リールのデータがどのようなものか(リール位置, コマ情報, ANY, はずれ, 目押しはずれ)
        eReelPos, eComaItem, eAny, eRotating, eHazure, eAiming, eItemMax
    }

    public class CollectionReelElem : IEffectNameInterface
    {
        public CollectionReelPattern Pattern { get; set; }  // データ指定がリール位置か(true:リール位置/false:その他)
        public List<short> ComaItem { get; set; }           // 各コマのアイテム(index下段から)、マイナスで非停止時
        public byte ReelPos { get; set; }                   // リール位置

        public CollectionReelElem()
        {
            Pattern = CollectionReelPattern.eReelPos;
            ComaItem = new List<short>();
            for (int i = 0; i < SlotMaker2022.LocalDataSet.SHOW_MAX; ++i) ComaItem.Add(0);
            ReelPos = 0;
        }
		public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write((int)Pattern);
            fs.Write(ComaItem.Count);
            foreach (var item in ComaItem) fs.Write(item);
            fs.Write(ReelPos);
            return true;
        }
		public bool ReadData(ref BinaryReader fs, int version)
        {
            Pattern = (CollectionReelPattern)fs.ReadInt32();
            int dataSize = fs.ReadInt32();
            if (dataSize != SlotMaker2022.LocalDataSet.SHOW_MAX) return false;
            for (int i = 0; i < dataSize; ++i) ComaItem[i] = fs.ReadInt16();
            ReelPos = fs.ReadByte();
            return true;
        }
        public void Rename(EChangeNameType type, string src, string dst) { }
        public List<byte> GetItemList(byte pCheckPos)
        {
            if (pCheckPos >= ComaItem.Count) return null;
            var ans = new List<byte>();
            short mask = Math.Abs(ComaItem[pCheckPos]);
            byte count = 0;
            while (mask != 0)
            {
                if ((mask & 0x1) == 1) ans.Add(count);
                mask >>= 1;
                ++count;
            }
            return ans;
        }
    }

    public class CollectionDataElem : IEffectNameInterface
    {   // データをリール数分まとめたもの
        public const int COLLECTION_LEVEL_MAX = 5;

        public List<CollectionReelElem> CollectionElem { get; set; }
        public byte Level { get; set; }

        public CollectionDataElem()
        {
            CollectionElem = new List<CollectionReelElem>();
            for (int i = 0; i < SlotMaker2022.LocalDataSet.REEL_MAX; ++i) CollectionElem.Add(new CollectionReelElem());
            Level = 1;
        }
		public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write(CollectionElem.Count);
            foreach (var item in CollectionElem)
                if (!item.StoreData(ref fs, version)) return false;
            fs.Write(Level);
            return true;
        }
		public bool ReadData(ref BinaryReader fs, int version)
        {
            int dataSize = fs.ReadInt32();
            if (dataSize != SlotMaker2022.LocalDataSet.REEL_MAX) return false;
            for (int i = 0; i < dataSize; ++i)
                if (!CollectionElem[i].ReadData(ref fs, version)) return false;
            Level = fs.ReadByte();
            return true;
        }
        public void Rename(EChangeNameType type, string src, string dst) {
            foreach (var item in CollectionElem) item.Rename(type, src, dst);
        }
    }

    public class CollectionData : IEffectNameInterface
    {
        public List<CollectionDataElem> Collections { get; set; }   // コレクションデータ(要素数、リール)
        public string JudgeCondName { get; set; }                   // 判定を行うフラグ名(変数)
        public string JudgeHazure { get; set; }                     // はずれ判定を行うフラグ名(変数)
        public string JudgeAiming { get; set; }                     // ?判定を行うフラグ名(変数)

        public CollectionData()
        {
            Collections = new List<CollectionDataElem>();
            JudgeCondName = string.Empty;
            JudgeHazure = string.Empty;
            JudgeAiming = string.Empty;
        }
		public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write(Collections.Count);
            foreach (var item in Collections) 
                if (!item.StoreData(ref fs, version)) return false;
            fs.Write(JudgeCondName);
            fs.Write(JudgeHazure);
            fs.Write(JudgeAiming);
            return true;
        }
		public bool ReadData(ref BinaryReader fs, int version)
        {
            int dataSize = fs.ReadInt32();
            for (int i = 0; i < dataSize; ++i)
            {
                var data = new CollectionDataElem();
                if(!data.ReadData(ref fs, version)) return false;
                Collections.Add(data);
            }
            JudgeCondName = fs.ReadString();
            JudgeHazure = fs.ReadString();
            JudgeAiming = fs.ReadString();
            return true;
        }
        public void Rename(EChangeNameType type, string src, string dst) {
            if (type == EChangeNameType.Var && JudgeCondName.Equals(src)) JudgeCondName = dst;
            if (type == EChangeNameType.Var && JudgeHazure.Equals(src)) JudgeHazure = dst;
            if (type == EChangeNameType.Var && JudgeAiming.Equals(src)) JudgeAiming = dst;
            foreach (var item in Collections) item.Rename(type, src, dst);
        }
    }
}

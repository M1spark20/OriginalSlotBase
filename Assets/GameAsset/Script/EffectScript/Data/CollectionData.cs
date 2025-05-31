using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SlotMaker2022;

namespace SlotEffectMaker2023.Data
{
    /// <summary>
    /// リールデータパターンを表す列挙型。
    /// 各リール要素がどのような条件で判定されるかを指定します。
    /// </summary>
    public enum CollectionReelPattern
    {   // 各リールのデータがどのようなものか(リール位置, コマ情報, ANY, はずれ, 目押しはずれ)
        eReelPos,
        eComaItem,
        eAny,
        eRotating,
        eHazure,
        eAiming,
        eItemMax
    }

    /// <summary>
    /// 単一リールの判定要素を管理するクラス。
    /// リールパターン、コマ情報、リール位置を保持し、セーブ/ロードやアイテム列挙を提供します。
    /// </summary>
    public class CollectionReelElem : IEffectNameInterface
    {
        public CollectionReelPattern Pattern { get; set; }  // データ指定がリール位置か(true:リール位置/false:その他)
        public List<short> ComaItem { get; set; }           // 各コマのアイテム(index下段から)、マイナスで非停止時
        public byte ReelPos { get; set; }                   // リール位置

        /// <summary>
        /// コンストラクタ。初期値を設定し、コマリストを生成します。
        /// </summary>
        public CollectionReelElem()
        {
            Pattern = CollectionReelPattern.eReelPos;
            ComaItem = new List<short>();
            for (int i = 0; i < LocalDataSet.SHOW_MAX; ++i) ComaItem.Add(0);
            ReelPos = 0;
        }

        /// <summary>
        /// 単一リール要素をバイナリ形式で保存します。
        /// </summary>
        /// <param name="fs">BinaryWriter の参照</param>
        /// <param name="version">保存バージョン</param>
        /// <returns>保存に成功したか（常に true）</returns>
        public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write((int)Pattern);
            fs.Write(ComaItem.Count);
            foreach (var item in ComaItem) fs.Write(item);
            fs.Write(ReelPos);
            return true;
        }

        /// <summary>
        /// バイナリ形式から単一リール要素を読み込みます。
        /// </summary>
        /// <param name="fs">BinaryReader の参照</param>
        /// <param name="version">保存バージョン</param>
        /// <returns>読み込みが成功したか</returns>
        public bool ReadData(ref BinaryReader fs, int version)
        {
            Pattern = (CollectionReelPattern)fs.ReadInt32();
            int dataSize = fs.ReadInt32();
            if (dataSize != LocalDataSet.SHOW_MAX) return false;
            for (int i = 0; i < dataSize; ++i) ComaItem[i] = fs.ReadInt16();
            ReelPos = fs.ReadByte();
            return true;
        }

        /// <summary>
        /// 効果名変更イベントに応じて名前を更新します（未実装）。
        /// </summary>
        /// <param name="type">変更タイプ</param>
        /// <param name="src">元の名前</param>
        /// <param name="dst">新しい名前</param>
        public void Rename(EChangeNameType type, string src, string dst) { }

        /// <summary>
        /// 指定した位置のアイテムマスクから有効なアイテムリストを返します。
        /// </summary>
        /// <param name="pCheckPos">チェック位置</param>
        /// <returns>有効なアイテムインデックスのリスト、範囲外の場合は null</returns>
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

    /// <summary>
    /// 複数リールの要素をまとめたデータクラス。
    /// レベル情報とリールごとの要素リストを管理します。
    /// </summary>
    public class CollectionDataElem : IEffectNameInterface
    {   // データをリール数分まとめたもの
        public const int COLLECTION_LEVEL_MAX = 5;

        public List<CollectionReelElem> CollectionElem { get; set; }
        public byte Level { get; set; }

        /// <summary>
        /// コンストラクタ。要素リストを初期化し、レベルをデフォルト値に設定します。
        /// </summary>
        public CollectionDataElem()
        {
            CollectionElem = new List<CollectionReelElem>();
            for (int i = 0; i < LocalDataSet.REEL_MAX; ++i) CollectionElem.Add(new CollectionReelElem());
            Level = 1;
        }

        /// <summary>
        /// CollectionDataElem をバイナリ形式で保存します。
        /// </summary>
        /// <param name="fs">BinaryWriter の参照</param>
        /// <param name="version">保存バージョン</param>
        /// <returns>保存に成功したか</returns>
        public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write(CollectionElem.Count);
            foreach (var item in CollectionElem)
                if (!item.StoreData(ref fs, version)) return false;
            fs.Write(Level);
            return true;
        }

        /// <summary>
        /// バイナリ形式から CollectionDataElem を読み込みます。
        /// </summary>
        /// <param name="fs">BinaryReader の参照</param>
        /// <param name="version">保存バージョン</param>
        /// <returns>読み込みが成功したか</returns>
        public bool ReadData(ref BinaryReader fs, int version)
        {
            int dataSize = fs.ReadInt32();
            if (dataSize != LocalDataSet.REEL_MAX) return false;
            for (int i = 0; i < dataSize; ++i)
                if (!CollectionElem[i].ReadData(ref fs, version)) return false;
            Level = fs.ReadByte();
            return true;
        }

        /// <summary>
        /// 効果名変更イベントに応じて内部要素の名前を更新します。
        /// </summary>
        /// <param name="type">変更タイプ</param>
        /// <param name="src">元の名前</param>
        /// <param name="dst">新しい名前</param>
        public void Rename(EChangeNameType type, string src, string dst)
        {
            foreach (var item in CollectionElem) item.Rename(type, src, dst);
        }
    }

    /// <summary>
    /// コレクション判定データ全体を管理するクラス。
    /// 要素リストと判定用フラグ名を保持します。
    /// </summary>
    public class CollectionData : IEffectNameInterface
    {
        public List<CollectionDataElem> Collections { get; set; }   // コレクションデータ(要素数、リール)
        public string JudgeCondName { get; set; }                   // 判定を行うフラグ名(変数)
        public string JudgeHazure { get; set; }                     // はずれ判定を行うフラグ名(変数)
        public string JudgeAiming { get; set; }                     // ?判定を行うフラグ名(変数)

        /// <summary>
        /// コンストラクタ。内部リストと判定用フラグ名を初期化します。
        /// </summary>
        public CollectionData()
        {
            Collections = new List<CollectionDataElem>();
            JudgeCondName = string.Empty;
            JudgeHazure = string.Empty;
            JudgeAiming = string.Empty;
        }

        /// <summary>
        /// CollectionData をバイナリ形式で保存します。
        /// </summary>
        /// <param name="fs">BinaryWriter の参照</param>
        /// <param name="version">保存バージョン</param>
        /// <returns>保存に成功したか</returns>
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

        /// <summary>
        /// バイナリ形式から CollectionData を読み込みます。
        /// </summary>
        /// <param name="fs">BinaryReader の参照</param>
        /// <param name="version">保存バージョン</param>
        /// <returns>読み込みが成功したか</returns>
        public bool ReadData(ref BinaryReader fs, int version)
        {
            int dataSize = fs.ReadInt32();
            for (int i = 0; i < dataSize; ++i)
            {
                var data = new CollectionDataElem();
                if (!data.ReadData(ref fs, version)) return false;
                Collections.Add(data);
            }
            JudgeCondName = fs.ReadString();
            JudgeHazure = fs.ReadString();
            JudgeAiming = fs.ReadString();
            return true;
        }

        /// <summary>
        /// 効果名変更イベントに応じてジャッジ用変数名および内部要素の名前を更新します。
        /// </summary>
        /// <param name="type">変更タイプ</param>
        /// <param name="src">元の名前</param>
        /// <param name="dst">新しい名前</param>
        public void Rename(EChangeNameType type, string src, string dst)
        {
            if (type == EChangeNameType.Var && JudgeCondName.Equals(src)) JudgeCondName = dst;
            if (type == EChangeNameType.Var && JudgeHazure.Equals(src)) JudgeHazure = dst;
            if (type == EChangeNameType.Var && JudgeAiming.Equals(src)) JudgeAiming = dst;
            foreach (var item in Collections) item.Rename(type, src, dst);
        }
    }
}

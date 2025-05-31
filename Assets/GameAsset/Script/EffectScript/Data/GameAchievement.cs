using System;
using System.Collections.Generic;
using System.IO;

namespace SlotEffectMaker2023.Data
{
    /// <summary>
    /// 実績データの種類を表します。数値ベース（Num）かフラグベース（Flag）を選択します。
    /// </summary>
    public enum AchieveDataType
    {
        /// <summary>数値ベースの実績</summary>
        Num,

        /// <summary>フラグベースの実績</summary>
        Flag
    }

    /// <summary>
    /// ゲーム内の実績情報を保持・管理するクラスです。
    /// </summary>
    public class GameAchievement : IEffectNameInterface
    {
        /// <summary>実績データの種類（数値またはフラグ）</summary>
        public AchieveDataType Type { get; set; }

        /// <summary>実績を識別するデータID</summary>
        public string DataID { get; set; }

        /// <summary>
        /// チェック対象データを指定します。
        /// - Num の場合：代入先変数名  
        /// - Flag の場合：達成条件（タイムライン名）
        /// </summary>
        public string RefData { get; set; }

        /// <summary>ボーナス入賞時にのみ評価を行う場合は true</summary>
        public bool UpdateOnlyBonusIn { get; set; }

        /// <summary>
        /// 実績がゲーム内で既に達成されているかどうかを示すフラグ。
        /// <para>リモート取得専用で、ストレージには保存されません。</para>
        /// </summary>
        public bool IsAchieved { get; set; }

        /// <summary>実績タイトル（リモート取得専用）</summary>
        public string Title { get; private set; }

        /// <summary>実績の説明文（リモート取得専用）</summary>
        public string Desc { get; private set; }

        /// <summary>ゲーム開始時の初期値（リモート取得専用）</summary>
        public int StartVal { get; set; }

        /// <summary>データ転送時のオフセット（リモート取得専用）</summary>
        public int Offset { get; set; }

        /// <summary>デフォルトコンストラクタ。プロパティを初期化します。</summary>
        public GameAchievement()
        {
            Type = AchieveDataType.Flag;
            DataID = string.Empty;
            RefData = string.Empty;
            UpdateOnlyBonusIn = false;
            IsAchieved = false;
            Title = string.Empty;
            Desc = string.Empty;
            StartVal = 0;
            Offset = 0;
        }

        /// <summary>
        /// 実績の基本データをバイナリに書き込みます。
        /// </summary>
        /// <param name="fs">書き込み先の BinaryWriter（ref）</param>
        /// <param name="version">データのバージョン</param>
        /// <returns>書き込み成功時に true を返します。</returns>
        public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write((byte)Type);
            fs.Write(DataID);
            fs.Write(RefData);
            fs.Write(UpdateOnlyBonusIn);
            return true;
        }

        /// <summary>
        /// バイナリから実績の基本データを読み込みます。
        /// </summary>
        /// <param name="fs">読み込み元の BinaryReader（ref）</param>
        /// <param name="version">データのバージョン</param>
        /// <returns>読み込み成功時に true を返します。</returns>
        public bool ReadData(ref BinaryReader fs, int version)
        {
            Type = (AchieveDataType)fs.ReadByte();
            DataID = fs.ReadString();
            RefData = fs.ReadString();
            UpdateOnlyBonusIn = fs.ReadBoolean();
            return true;
        }

        /// <summary>
        /// 名前変更時に、参照変数名またはタイムライン名を更新します。
        /// </summary>
        /// <param name="type">変更の種類（Var: 変数 / Timeline: フラグ条件）</param>
        /// <param name="src">元の名前</param>
        /// <param name="dst">新しい名前</param>
        public void Rename(EChangeNameType type, string src, string dst)
        {
            if (type == EChangeNameType.Var
                && Type == AchieveDataType.Num
                && RefData.Equals(src))
            {
                RefData = dst;
            }

            if (type == EChangeNameType.Timeline
                && Type == AchieveDataType.Flag
                && RefData.Equals(src))
            {
                RefData = dst;
            }
        }

        /// <summary>
        /// リモート取得した実績詳細（達成フラグ、タイトル、説明）を設定します。
        /// </summary>
        /// <param name="pAchievedFlag">実績達成フラグ</param>
        /// <param name="pTitle">実績タイトル</param>
        /// <param name="pDesc">実績の説明文</param>
        public void SetDetail(bool pAchievedFlag, string pTitle, string pDesc)
        {
            IsAchieved = pAchievedFlag;
            Title = pTitle;
            Desc = pDesc;
        }
    }

    /// <summary>
    /// 複数の GameAchievement をまとめて管理するリストクラスです。
    /// </summary>
    public class GameAchievementList : IEffectNameInterface
    {
        /// <summary>実績データのリスト</summary>
        public List<GameAchievement> elemData { get; set; }

        /// <summary>デフォルトコンストラクタ。リストを初期化します。</summary>
        public GameAchievementList()
        {
            elemData = new List<GameAchievement>();
        }

        /// <summary>
        /// 全実績データをバイナリに書き込みます。
        /// </summary>
        /// <param name="fs">書き込み先の BinaryWriter（ref）</param>
        /// <param name="version">データのバージョン</param>
        /// <returns>書き込み成功時に true を返します。</returns>
        public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write(elemData.Count);
            foreach (var ga in elemData)
                ga.StoreData(ref fs, version);
            return true;
        }

        /// <summary>
        /// バイナリから全実績データを読み込みます。
        /// </summary>
        /// <param name="fs">読み込み元の BinaryReader（ref）</param>
        /// <param name="version">データのバージョン</param>
        /// <returns>読み込み成功時に true を返します。</returns>
        public bool ReadData(ref BinaryReader fs, int version)
        {
            int mapSize = fs.ReadInt32();
            for (int i = 0; i < mapSize; ++i)
            {
                var ga = new GameAchievement();
                ga.ReadData(ref fs, version);
                elemData.Add(ga);
            }
            return true;
        }

        /// <summary>
        /// 名前変更時に、内包するすべての実績に対して Rename を実行します。
        /// </summary>
        /// <param name="type">変更の種類</param>
        /// <param name="src">元の名前</param>
        /// <param name="dst">新しい名前</param>
        public void Rename(EChangeNameType type, string src, string dst)
        {
            foreach (var ga in elemData)
                ga.Rename(type, src, dst);
        }
    }
}

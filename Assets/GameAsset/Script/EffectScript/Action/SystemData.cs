using SlotMaker2022;
using System.IO;

namespace SlotEffectMaker2023.Action
{
    /// <summary>
    /// システム設定（音量、表示位置、ロケールなど）を管理し、セーブ/ロードを提供するクラス。
    /// </summary>
    public class SystemData : ILocalDataInterface
    {   // 音量や情報表示位置などのシステムデータを管理する
        public int ReadVersion { get; set; }
        public float MasterVol { get; set; }
        public float BGMVol { get; set; }
        public float SEVol { get; set; }
        public int InfoPos { get; set; }
        public LangLocale Locale { get; set; }
        public bool WaitCut { get; set; }
        public bool ShowSlipCount { get; set; }
        public byte UseSaveDataID { get; set; }
        public bool ResetFlag { get; set; }

        /// <summary>
        /// コンストラクタ。システムデータの初期値を設定します。
        /// </summary>
        public SystemData()
        {
            ReadVersion = 0;
            MasterVol = 0.5f;
            BGMVol = 0.5f;
            SEVol = 0.5f;
            InfoPos = 16;
            Locale = LangLocale.ja;
            WaitCut = false;
            ShowSlipCount = false;
            UseSaveDataID = 0;
            ResetFlag = false;
        }

        /// <summary>
        /// システムデータをバイナリ形式で保存します。
        /// </summary>
        /// <param name="fs">BinaryWriter の参照</param>
        /// <param name="version">保存バージョン</param>
        /// <returns>保存処理が成功したか（常に true）</returns>
        public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write(MasterVol);
            fs.Write(BGMVol);
            fs.Write(SEVol);
            fs.Write(InfoPos);
            fs.Write((int)Locale);
            if (version >= 1) fs.Write(WaitCut);
            if (version >= 2) fs.Write(ShowSlipCount);
            if (version >= 3)
            {
                fs.Write(UseSaveDataID);
                fs.Write(ResetFlag);
            }
            return true;
        }

        /// <summary>
        /// バイナリ形式からシステムデータを読み込みます。
        /// </summary>
        /// <param name="fs">BinaryReader の参照</param>
        /// <param name="version">保存バージョン</param>
        /// <returns>読み込み処理が成功したか（常に true）</returns>
        public bool ReadData(ref BinaryReader fs, int version)
        {
            ReadVersion = version;
            MasterVol = fs.ReadSingle();
            BGMVol = fs.ReadSingle();
            SEVol = fs.ReadSingle();
            InfoPos = fs.ReadInt32();
            Locale = (LangLocale)fs.ReadInt32();
            if (version >= 1) WaitCut = fs.ReadBoolean();
            if (version >= 2) ShowSlipCount = fs.ReadBoolean();
            if (version >= 3)
            {
                UseSaveDataID = fs.ReadByte();
                ResetFlag = fs.ReadBoolean();
            }
            return true;
        }
    }

    /// <summary>
    /// 表示用ロケールを定義する列挙型。
    /// </summary>
    public enum LangLocale
    {
        en,
        ja
    }
}

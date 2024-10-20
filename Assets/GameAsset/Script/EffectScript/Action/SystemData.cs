using SlotMaker2022;
using System.IO;

namespace SlotEffectMaker2023.Action
{
    public enum LangLocale { en, ja }
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

        public SystemData()
        {
            ReadVersion = 0;
            MasterVol = .5f;
            BGMVol = .5f;
            SEVol = .5f;
            InfoPos = 16;
            Locale = LangLocale.ja;
            WaitCut = false;
            ShowSlipCount = false;
            UseSaveDataID = 0;
            ResetFlag = false;
        }
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
}

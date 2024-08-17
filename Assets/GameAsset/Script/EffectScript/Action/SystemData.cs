using SlotMaker2022;
using System.IO;

namespace SlotEffectMaker2023.Action
{
    public enum LangLocale { en, ja }
    public class SystemData : ILocalDataInterface
    {   // 音量や情報表示位置などのシステムデータを管理する
        public float MasterVol { get; set; }
        public float BGMVol { get; set; }
        public float SEVol { get; set; }
        public int InfoPos { get; set; }
        public LangLocale Locale { get; set; }

        public SystemData()
        {
            MasterVol = .5f;
            BGMVol = .5f;
            SEVol = .5f;
            InfoPos = 16;
            Locale = LangLocale.ja;
        }
        public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write(MasterVol);
            fs.Write(BGMVol);
            fs.Write(SEVol);
            fs.Write(InfoPos);
            fs.Write((int)Locale);
            return true;
        }
        public bool ReadData(ref BinaryReader fs, int version)
        {
            MasterVol = fs.ReadSingle();
            BGMVol = fs.ReadSingle();
            SEVol = fs.ReadSingle();
            InfoPos = fs.ReadInt32();
            Locale = (LangLocale)fs.ReadInt32();
            return true;
        }
    }
}

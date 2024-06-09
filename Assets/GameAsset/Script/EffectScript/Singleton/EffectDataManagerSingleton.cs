using System;
using System.Collections.Generic;
using System.Text;

namespace SlotEffectMaker2023.Singleton
{
    // メインROMデータ管理
    // データ上に1つだけ存在させたいためSingletonパターンを採用
    public sealed class EffectDataManagerSingleton
    {
        // ファイルバージョン
        const int FILE_VERSION = 0;

        // Singletonインスタンス
        private static EffectDataManagerSingleton ins = new EffectDataManagerSingleton();

        // データセット定義
        public List<Data.SoundID>       SoundIDList   { get; set; } // 読込音源一覧
        public List<Data.SoundPlayData> SoundPlayList { get; set; } // サウンド再生データ一覧
        public Data.VarList             VarList       { get; set; } // 生成変数一覧
        public Data.TimerList           TimerList     { get; set; } // 生成タイマ一覧
        public Data.SlotTimeline        Timeline      { get; set; } // サブ基板処理内容
        public Data.ColorMapDataManager ColorMap      { get; set; } // 演出色マップ
        public Data.HistoryConfig       HistoryConf   { get; set; } // ボーナス履歴用コンフィグ
        public Data.CollectionData      Collection    { get; set; } // リーチ目コレクション

        private EffectDataManagerSingleton()
        {
            // データセット初期化
            SoundIDList   = new List<Data.SoundID>();
            SoundPlayList = new List<Data.SoundPlayData>();
            VarList       = new Data.VarList();
            TimerList     = new Data.TimerList();
            Timeline      = new Data.SlotTimeline();
            ColorMap      = new Data.ColorMapDataManager();
            HistoryConf   = new Data.HistoryConfig();
            Collection    = new Data.CollectionData();
        }
        public static EffectDataManagerSingleton GetInstance()
        {
            return ins;
        }
        public bool ReadData()
        {
           // ファイルから読み込み処理を行う(Unityでは処理内容を変更する)
            var rd = new SlotMaker2022.ProgressRead();
            if (!rd.OpenCompressedFile("Effect.bytes")) return false;
            if (!ReadAction(rd)) return false;
            rd.Close();

            BackupData();
            return true;
        }
        public bool ReadData(UnityEngine.TextAsset data)
        {   // Unity用
            var rd = new SlotMaker2022.ProgressRead();
            if (!rd.OpenCompressedFile(data.bytes)) return false;
            if (!ReadAction(rd)) return false;
            rd.Close();
            return true;
        }
        private bool ReadAction(SlotMaker2022.ProgressRead rd)
        {
            if (!rd.ReadData(SoundIDList)) return false;
            if (!rd.ReadData(SoundPlayList)) return false;
            if (!rd.ReadData(VarList)) return false;
            if (!rd.ReadData(TimerList)) return false;
            if (!rd.ReadData(Timeline)) return false;
            if (!rd.ReadData(ColorMap)) return false;
            if (!rd.ReadData(HistoryConf)) return false;
            if (!rd.ReadData(Collection)) return false;
            return true;
        }
        public bool SaveData()
        {
            var sw = new SlotMaker2022.ProgressWrite();
            if (sw.OpenFile("Effect.bytes", FILE_VERSION))
            {
                WriteOut(sw);
                sw.FlushCompressed();
                sw.Close();
            }
            return true;
        }

        // バックアップ生成
        public bool BackupData()
        {
            var sw = new SlotMaker2022.ProgressWrite();
            if (sw.OpenFile("backup.bak", FILE_VERSION))
            {
                WriteOut(sw);
                sw.Flush();
                sw.Close();
            }
            return true;
        }

        // データ書き出し
        private bool WriteOut(SlotMaker2022.ProgressWrite sw)
        {
            // 読み込み順はReadと揃えること
            sw.WriteData(SoundIDList);
            sw.WriteData(SoundPlayList);
            sw.WriteData(VarList);
            sw.WriteData(TimerList);
            sw.WriteData(Timeline);
            sw.WriteData(ColorMap);
            sw.WriteData(HistoryConf);
            sw.WriteData(Collection);
            return true;
        }

        // データ名変更処理
        public void Rename(Data.EChangeNameType type, string src, string dst)
        {
            foreach (var item in SoundIDList) item.Rename(type, src, dst);
            foreach (var item in SoundPlayList) item.Rename(type, src, dst);
            VarList.Rename(type, src, dst);
            TimerList.Rename(type, src, dst);
            Timeline.Rename(type, src, dst);
            ColorMap.Rename(type, src, dst);
            HistoryConf.Rename(type, src, dst);
            Collection.Rename(type, src, dst);
        }

        /* プログラム内共通機能(全体での共有事項をここで定義) */
        public string[] GetSoundIDNameList()
        {
            string[] ans = new string[SoundIDList.Count];
            for (int i = 0; i < ans.Length; ++i) ans[i] = SoundIDList[i].DataName;
            return ans;
        }
        public string[] GetSoundPlayerNameList()
        {
            string[] ans = new string[SoundPlayList.Count];
            for (int i = 0; i < ans.Length; ++i) ans[i] = SoundPlayList[i].ShifterName;
            return ans;
        }
        public Data.SoundID GetSoundID(string pSoundIDName)
        {
            foreach (var item in SoundIDList)
                if (item.DataName.Equals(pSoundIDName)) return item;
            return null;
        }
        public Data.SoundPlayData GetSoundPlayer(string pPlayerName)
        {
            foreach (var item in SoundPlayList)
                if (item.ShifterName.Equals(pPlayerName)) return item;
            return null;
        }
    }
}

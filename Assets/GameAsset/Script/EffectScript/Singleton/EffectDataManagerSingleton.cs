using System;
using System.Collections.Generic;
using System.Text;

namespace SlotEffectMaker2023.Singleton
{
    /// <summary>
    /// メインROMデータ管理を行うシングルトン。
    /// データ上に1つだけ存在させるために Singleton パターンを採用しています。
    /// </summary>
    public sealed class EffectDataManagerSingleton
    {
        /// <summary>
        /// 保存ファイルのフォーマットバージョン。
        /// </summary>
        const int FILE_VERSION = 0;

        /// <summary>
        /// シングルトンインスタンスへの参照。
        /// </summary>
        private static EffectDataManagerSingleton ins = new EffectDataManagerSingleton();

        // データセット定義
        /// <summary>読込音源一覧。</summary>
        public List<Data.SoundID> SoundIDList { get; set; } // 読込音源一覧
        /// <summary>サウンド再生データ一覧。</summary>
        public List<Data.SoundPlayData> SoundPlayList { get; set; } // サウンド再生データ一覧
        /// <summary>生成変数一覧。</summary>
        public Data.VarList VarList { get; set; } // 生成変数一覧
        /// <summary>生成タイマ一覧。</summary>
        public Data.TimerList TimerList { get; set; } // 生成タイマ一覧
        /// <summary>サブ基板処理内容。</summary>
        public Data.SlotTimeline Timeline { get; set; } // サブ基板処理内容
        /// <summary>演出色マップ管理。</summary>
        public Data.ColorMapDataManager ColorMap { get; set; } // 演出色マップ
        /// <summary>ボーナス履歴用コンフィグ。</summary>
        public Data.HistoryConfig HistoryConf { get; set; } // ボーナス履歴用コンフィグ
        /// <summary>リーチ目コレクション。</summary>
        public Data.CollectionData Collection { get; set; } // リーチ目コレクション
        /// <summary>Steam用実績管理。</summary>
        public Data.GameAchievementList GameAchieve { get; set; } // Steam用実績管理
        /// <summary>フラグカウント条件。</summary>
        public Data.FlagCounterSet CounterCond { get; set; } // フラグカウント条件

        /// <summary>
        /// プライベートコンストラクタ。データセットを初期化します。
        /// </summary>
        private EffectDataManagerSingleton()
        {
            // データセット初期化
            SoundIDList = new List<Data.SoundID>();
            SoundPlayList = new List<Data.SoundPlayData>();
            VarList = new Data.VarList();
            TimerList = new Data.TimerList();
            Timeline = new Data.SlotTimeline();
            ColorMap = new Data.ColorMapDataManager();
            HistoryConf = new Data.HistoryConfig();
            Collection = new Data.CollectionData();
            GameAchieve = new Data.GameAchievementList();
            CounterCond = new Data.FlagCounterSet();
        }

        /// <summary>
        /// シングルトンインスタンスを取得します。
        /// </summary>
        /// <returns>EffectDataManagerSingleton の唯一のインスタンス。</returns>
        public static EffectDataManagerSingleton GetInstance()
        {
            return ins;
        }

        /// <summary>
        /// デフォルトの読み込み処理を実行します。
        /// Effect.bytes ファイルからデータを読み込み、バックアップを生成します。
        /// </summary>
        /// <returns>読み込みに成功した場合は true、失敗した場合は false を返します。</returns>
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

        /// <summary>
        /// Unity 用の読み込み処理を実行します。
        /// </summary>
        /// <param name="data">読み込む TextAsset。</param>
        /// <returns>読み込みに成功した場合は true、失敗した場合は false を返します。</returns>
        public bool ReadData(UnityEngine.TextAsset data)
        {   // Unity用
            var rd = new SlotMaker2022.ProgressRead();
            if (!rd.OpenCompressedFile(data.bytes)) return false;
            if (!ReadAction(rd)) return false;
            rd.Close();
            return true;
        }

        /// <summary>
        /// 各データセットの読み込み処理を実行します。
        /// </summary>
        /// <param name="rd">データ読み込み用 ProgressRead インスタンス。</param>
        /// <returns>読み込みに成功した場合は true、失敗した場合は false を返します。</returns>
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
            if (!rd.ReadData(GameAchieve)) return false;
            if (!rd.ReadData(CounterCond)) return false;
            return true;
        }

        /// <summary>
        /// データを Effect.bytes ファイルに書き出します。
        /// </summary>
        /// <returns>常に true を返します。</returns>
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

        /// <summary>
        /// データを backup.bak ファイルにバックアップとして書き出します。
        /// </summary>
        /// <returns>常に true を返します。</returns>
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

        /// <summary>
        /// ProgressWrite を使用して各データセットを書き出します。
        /// 読み込み順は ReadData と揃える必要があります。
        /// </summary>
        /// <param name="sw">データ書き出し用 ProgressWrite インスタンス。</param>
        /// <returns>書き出し処理が成功した場合は true を返します。</returns>
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
            sw.WriteData(GameAchieve);
            sw.WriteData(CounterCond);
            return true;
        }

        /// <summary>
        /// 指定された名前変更タイプに従い、各データセット内の名前を更新します。
        /// </summary>
        /// <param name="type">変更対象の名前の種類。</param>
        /// <param name="src">変更前の名前。</param>
        /// <param name="dst">変更後の名前。</param>
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
            GameAchieve.Rename(type, src, dst);
            CounterCond.Rename(type, src, dst);
        }

        /// <summary>
        /// 登録されている SoundID の名前一覧を取得します。
        /// </summary>
        /// <returns>SoundID の名前文字列配列。</returns>
        public string[] GetSoundIDNameList()
        {
            string[] ans = new string[SoundIDList.Count];
            for (int i = 0; i < ans.Length; ++i) ans[i] = SoundIDList[i].DataName;
            return ans;
        }

        /// <summary>
        /// 登録されている SoundPlayData の ShifterName 一覧を取得します。
        /// </summary>
        /// <returns>SoundPlayData の ShifterName 文字列配列。</returns>
        public string[] GetSoundPlayerNameList()
        {
            string[] ans = new string[SoundPlayList.Count];
            for (int i = 0; i < ans.Length; ++i) ans[i] = SoundPlayList[i].ShifterName;
            return ans;
        }

        /// <summary>
        /// 指定された SoundID 名に対応する SoundID オブジェクトを取得します。
        /// </summary>
        /// <param name="pSoundIDName">検索する SoundID 名。</param>
        /// <returns>対応する SoundID オブジェクト、存在しない場合は null。</returns>
        public Data.SoundID GetSoundID(string pSoundIDName)
        {
            foreach (var item in SoundIDList)
                if (item.DataName.Equals(pSoundIDName)) return item;
            return null;
        }

        /// <summary>
        /// 指定されたプレイヤー名に対応する SoundPlayData オブジェクトを取得します。
        /// </summary>
        /// <param name="pPlayerName">検索するプレイヤー名。</param>
        /// <returns>対応する SoundPlayData オブジェクト、存在しない場合は null。</returns>
        public Data.SoundPlayData GetSoundPlayer(string pPlayerName)
        {
            foreach (var item in SoundPlayList)
                if (item.ShifterName.Equals(pPlayerName)) return item;
            return null;
        }
    }
}
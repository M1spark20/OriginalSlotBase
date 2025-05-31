using System;
using System.Collections.Generic;
using System.Text;

namespace SlotMaker2022
{
    // メインROMデータ管理
    // データ上に1つだけ存在させたいためSingletonパターンを採用
    /// <summary>
    /// メインのROMデータを管理するシングルトンです。
    /// ファイルの読み書きおよび各種データセットへのアクセスを提供します。
    /// </summary>
    public sealed class MainROMDataManagerSingleton
    {
        /// <summary>ファイルフォーマットのバージョン番号</summary>
        private const int FILE_VERSION = 0;

        /// <summary>単一インスタンス</summary>
        private static MainROMDataManagerSingleton ins = new MainROMDataManagerSingleton();

        // データセット定義
        public LocalDataSet.ReelArray[][] ReelArray { get; set; }   // ジャグ配列(配列の配列、2次要素数不一致可能)
        public LocalDataSet.SoftwareInformation SoftInfo { get; set; }
        public LocalDataSet.CastCommonData CastCommonData { get; set; }
        public List<LocalDataSet.CastElemData> CastElemData { get; set; }
        public LocalDataSet.FlagCommonData FlagCommonData { get; set; }
        public List<LocalDataSet.FlagElemData> FlagElemData { get; set; }
        public List<LocalDataSet.FlagRandData> FlagRandData { get; set; }
        public LocalDataSet.RTCommonData RTCommonData { get; set; }
        public List<LocalDataSet.RTMoveData> RTMoveData { get; set; }
        public List<LocalDataSet.FreezeControlData> FreezeControlData { get; set; }
        public List<LocalDataSet.FreezeTimeData> FreezeTimeData { get; set; }
        public List<LocalDataSet.ComaCombinationData> ComaCombinationData { get; set; }
        public List<LocalDataSet.SlipBaseData> ReelSlipData { get; set; }
        public List<LocalDataSet.ReachData> ReachData { get; set; }
        public List<LocalDataSet.CombiPriorityData> CombiPriorityData { get; set; }
        public List<LocalDataSet.ReelControlData> ReelCtrlData { get; set; }

        /// <summary>
        /// コンストラクタ。初期データを生成します。
        /// </summary>
        private MainROMDataManagerSingleton()
        {
            // データセット初期化
            ReelArray = new LocalDataSet.ReelArray[LocalDataSet.REEL_MAX][];
            for (int reelC = 0; reelC < LocalDataSet.REEL_MAX; ++reelC)
            {
                ReelArray[reelC] = new LocalDataSet.ReelArray[LocalDataSet.COMA_MAX];
                for (int i = 0; i < LocalDataSet.COMA_MAX; ++i)
                {
                    ReelArray[reelC][i] = new LocalDataSet.ReelArray() { Pos = LocalDataSet.COMA_MAX - i };
                }
            }
            SoftInfo = new LocalDataSet.SoftwareInformation();
            CastCommonData = new LocalDataSet.CastCommonData();
            CastElemData = new List<LocalDataSet.CastElemData>();

            FlagCommonData = new LocalDataSet.FlagCommonData();
            FlagElemData = new List<LocalDataSet.FlagElemData>();
            FlagRandData = new List<LocalDataSet.FlagRandData>();

            RTCommonData = new LocalDataSet.RTCommonData();
            RTMoveData = new List<LocalDataSet.RTMoveData>();

            FreezeControlData = new List<LocalDataSet.FreezeControlData>();
            FreezeTimeData = new List<LocalDataSet.FreezeTimeData>();

            ComaCombinationData = new List<LocalDataSet.ComaCombinationData>();
            // ReelSlipDataは初期データとしてデフォルト値を格納しておく
            ReelSlipData = new List<LocalDataSet.SlipBaseData>();
            ReelSlipData.Add(new LocalDataSet.SlipBaseData()
            {
                Comment = "全リールビタ止まり"
            });

            // ReachData初期化
            ReachData = new List<LocalDataSet.ReachData>();
            for (int i = 0; i < LocalDataSet.REEL_MAX * LocalDataSet.COMA_MAX; ++i)
            {
                var addData = new LocalDataSet.ReachData()
                {
                    BaseReelPos = (byte)i
                };
                ReachData.Add(addData);
            }

            // CombiPriorityDataは初期データとしてデフォルト値を格納しておく
            CombiPriorityData = new List<LocalDataSet.CombiPriorityData>();
            CombiPriorityData.Add(new LocalDataSet.CombiPriorityData()
            {
                Comment = "枚数優先制御"
            });

            ReelCtrlData = new List<LocalDataSet.ReelControlData>();
        }

        /// <summary>
        /// シングルトンインスタンスを取得します。
        /// </summary>
        /// <returns>MainROMDataManagerSingleton の単一インスタンス</returns>
        public static MainROMDataManagerSingleton GetInstance()
        {
            return ins;
        }

        /// <summary>
        /// バイナリファイルからデータを読み込みます。
        /// </summary>
        /// <returns>読み込みに成功したら true、それ以外は false</returns>
        public bool ReadData()
        {
            // ファイルから読み込み処理を行う
            var rd = new ProgressRead();
            if (!rd.OpenFile("mainROM.bin")) return false;
            if (ReadAction(rd)) return false;
            rd.Close();

            // バックアップ生成
            BackupData();
            return true;
        }

        /// <summary>
        /// Unity TextAsset からデータを読み込みます。
        /// </summary>
        /// <param name="data">読み込むバイナリデータを持つ TextAsset</param>
        /// <returns>読み込みに成功したら true、それ以外は false</returns>
        public bool ReadData(UnityEngine.TextAsset data)
        {
            var rd = new SlotMaker2022.ProgressRead();
            if (!rd.OpenFile(data.bytes)) return false;
            if (!ReadAction(rd)) return false;
            rd.Close();
            return true;
        }

        /// <summary>
        /// 読み込み処理の共通部分を実行します。
        /// </summary>
        /// <param name="rd">ProgressRead オブジェクト</param>
        /// <returns>読み込みに失敗したら true、それ以外は false</returns>
        private bool ReadAction(ProgressRead rd)
        {
            if (!rd.ReadData(SoftInfo)) return false;
            for (int reelC = 0; reelC < LocalDataSet.REEL_MAX; ++reelC)
            {
                for (int i = 0; i < LocalDataSet.COMA_MAX; ++i)
                {
                    if (!rd.ReadData(ReelArray[reelC][i])) return false;
                }
            }
            if (!rd.ReadData(CastCommonData)) return false;
            if (!rd.ReadData(CastElemData)) return false;
            if (!rd.ReadData(FlagCommonData)) return false;
            if (!rd.ReadData(FlagElemData)) return false;
            if (!rd.ReadData(FlagRandData)) return false;
            if (!rd.ReadData(RTCommonData)) return false;
            if (!rd.ReadData(RTMoveData)) return false;
            if (!rd.ReadData(FreezeControlData)) return false;
            if (!rd.ReadData(FreezeTimeData)) return false;
            if (!rd.ReadData(ComaCombinationData)) return false;

            // ReelSlipDataはデータを一度リセットしてから読み込む
            ReelSlipData.Clear();
            if (!rd.ReadData(ReelSlipData)) return false;

            // ReachDataはデータを一度リセットしてから読み込む
            ReachData.Clear();
            if (!rd.ReadData(ReachData)) return false;

            // CombiPriorityDataはデータを一度リセットしてから読み込む
            CombiPriorityData.Clear();
            if (!rd.ReadData(CombiPriorityData)) return false;

            if (!rd.ReadData(ReelCtrlData)) return false;
            return true;
        }

        /// <summary>
        /// 現在のデータをファイルに保存します。
        /// </summary>
        /// <returns>保存処理を開始したら true</returns>
        public bool SaveData()
        {
            var sw = new ProgressWrite();
            if (sw.OpenFile("mainROM.bin", FILE_VERSION))
            {
                WriteOut(sw);
                sw.Flush();
                sw.Close();
            }
            return true;
        }

        /// <summary>
        /// バックアップファイルを生成します。
        /// </summary>
        /// <returns>バックアップ生成を開始したら true</returns>
        public bool BackupData()
        {
            var sw = new ProgressWrite();
            if (sw.OpenFile("backup.bak", FILE_VERSION))
            {
                WriteOut(sw);
                sw.Flush();
                sw.Close();
            }
            return true;
        }

        /// <summary>
        /// データ書き出しの共通処理を行います。
        /// </summary>
        /// <param name="sw">ProgressWrite オブジェクト</param>
        /// <returns>書き出しに成功したら true</returns>
        private bool WriteOut(ProgressWrite sw)
        {
            // 読み込み順はReadと揃えること
            sw.WriteData(SoftInfo);
            for (int reelC = 0; reelC < LocalDataSet.REEL_MAX; ++reelC)
            {
                for (int i = 0; i < LocalDataSet.COMA_MAX; ++i)
                {
                    sw.WriteData(ReelArray[reelC][i]);
                }
            }
            sw.WriteData(CastCommonData);
            sw.WriteData(CastElemData);
            sw.WriteData(FlagCommonData);
            sw.WriteData(FlagElemData);
            sw.WriteData(FlagRandData);
            sw.WriteData(RTCommonData);
            sw.WriteData(RTMoveData);
            sw.WriteData(FreezeControlData);
            sw.WriteData(FreezeTimeData);
            sw.WriteData(ComaCombinationData);
            sw.WriteData(ReelSlipData);
            sw.WriteData(ReachData);
            sw.WriteData(CombiPriorityData);
            sw.WriteData(ReelCtrlData);

            return true;
        }

        /// <summary>
        /// フラグ名の一覧を取得します。
        /// </summary>
        /// <returns>先頭にハズレを含むフラグ名リスト</returns>
        public List<string> GetFlagName()
        {
            List<string> ans = new List<string>();
            ans.Add("0:ハズレ");
            for (int i = 0; i < FlagElemData.Count; ++i)
                ans.Add((i + 1).ToString() + ":" + FlagElemData[i].UserFlagName);

            return ans;
        }

        /// <summary>
        /// ボーナスフラグ名の一覧を取得します。
        /// </summary>
        /// <returns>すべてのボーナス番号と名称のリスト</returns>
        public List<string> GetBonusName()
        {
            List<string> ans = new List<string>();

            string[] BonusNames = new string[LocalDataSet.BONUSFLAG_MAX];
            const string noneStr = "0:None";
            BonusNames[0] = noneStr;
            for (int i = 1; i < LocalDataSet.BONUSFLAG_MAX; ++i)
                BonusNames[i] = i.ToString() + ":";

            foreach (var item in CastElemData)
            {
                // 全キャストを参照し、ボーナスの場合だけ名称を読み込む。
                if (item.ValidateBonusFlag == 0) continue;
                string name = item.FlagName;
                if (BonusNames[item.ValidateBonusFlag].Length > 2) BonusNames[item.ValidateBonusFlag] += "/";
                BonusNames[item.ValidateBonusFlag] += name;
            }

            ans.AddRange(BonusNames);
            return ans;
        }
    }
}

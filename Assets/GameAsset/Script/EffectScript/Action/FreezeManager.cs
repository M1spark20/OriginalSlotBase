using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SlotMaker2022;
using SlotMaker2022.main_function;

namespace SlotEffectMaker2023.Action
{
    /// <summary>
    /// フリーズアクションのデータを保持するクラス。
    /// 抽選元、発動タイミング、持続時間、繰り越しゲーム数を管理します。
    /// </summary>
    class FreezeActData : ILocalDataInterface
    {
        public LocalDataSet.FreezeControlData.FreezeControlType type { get; set; }  // 抽選元
        public LocalDataSet.FreezeControlData.FreezeTiming timing { get; set; }     // 発動タイミング
        public int durationMS { get; set; } // フリーズ時間[ms]
        public int shiftGame { get; set; }  // 持ち越し残ゲーム数

        /// <summary>
        /// コンストラクタ。デフォルト値を設定します。
        /// </summary>
        public FreezeActData()
        {
            type = LocalDataSet.FreezeControlData.FreezeControlType.Flag;
            timing = LocalDataSet.FreezeControlData.FreezeTiming.Reset;
            durationMS = 0;
            shiftGame = 0;
        }

        /// <summary>
        /// FreezeActData をバイナリ形式で保存します。
        /// </summary>
        /// <param name="fs">BinaryWriter の参照</param>
        /// <param name="version">保存バージョン</param>
        /// <returns>保存処理が成功したか（常に true）</returns>
        public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write((int)type);
            fs.Write((int)timing);
            fs.Write(durationMS);
            fs.Write(shiftGame);
            return true;
        }

        /// <summary>
        /// バイナリ形式から FreezeActData を読み込みます。
        /// </summary>
        /// <param name="fs">BinaryReader の参照</param>
        /// <param name="version">保存バージョン</param>
        /// <returns>読み込み処理が成功したか（常に true）</returns>
        public bool ReadData(ref BinaryReader fs, int version)
        {
            type = (LocalDataSet.FreezeControlData.FreezeControlType)fs.ReadInt32();
            timing = (LocalDataSet.FreezeControlData.FreezeTiming)fs.ReadInt32();
            durationMS = fs.ReadInt32();
            shiftGame = fs.ReadInt32();
            return true;
        }
    }

    /// <summary>
    /// フリーズアクションを管理するクラス。
    /// 抽選条件に基づくデータ生成、保持、消費、読込・保存を担当します。
    /// </summary>
    public class FreezeManager : ILocalDataInterface
    {
        private List<FreezeActData> actData;

        /// <summary>
        /// コンストラクタ。内部リストを初期化します。
        /// </summary>
        public FreezeManager()
        {
            actData = new List<FreezeActData>();
        }

        /// <summary>
        /// 現在のフリーズアクションデータを保存します。
        /// </summary>
        /// <param name="fs">BinaryWriter の参照</param>
        /// <param name="version">保存バージョン</param>
        /// <returns>保存処理が成功したか（常に true）</returns>
        public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write(actData.Count);
            foreach (var item in actData) item.StoreData(ref fs, version);
            return true;
        }

        /// <summary>
        /// バイナリ形式からフリーズアクションデータを読み込みます。
        /// </summary>
        /// <param name="fs">BinaryReader の参照</param>
        /// <param name="version">保存バージョン</param>
        /// <returns>読み込み処理が成功したか（常に true）</returns>
        public bool ReadData(ref BinaryReader fs, int version)
        {
            int size = fs.ReadInt32();
            for (int i = 0; i < size; ++i)
            {
                FreezeActData act = new FreezeActData();
                act.ReadData(ref fs, version);
                actData.Add(act);
            }
            return true;
        }

        /// <summary>
        /// FRGフラグ条件に応じたフリーズ適用を行います。
        /// </summary>
        /// <param name="ctrl">制御条件リスト</param>
        /// <param name="ft">各タイミングの待機時間データ</param>
        /// <param name="flagID">発動フラグID</param>
        /// <param name="isLaunchGame">ボーナスゲーム発動時フラグ</param>
        public void SetFreezeFlag(List<LocalDataSet.FreezeControlData> ctrl, List<LocalDataSet.FreezeTimeData> ft, byte flagID, bool isLaunchGame)
        {
            // 全制御データにチェックをかける
            foreach (var ctrlItem in ctrl)
            {
                if (ctrlItem.ControlType != LocalDataSet.FreezeControlData.FreezeControlType.Flag) continue;
                var cond = new LocalDataSet.FreezeControlData.FreezeCondFlag(ctrlItem.Condition);
                bool condMatch = (flagID == cond.FlagID && (isLaunchGame || !cond.NoBonusFlag));

                // データ適用
                if (condMatch) Apply(ctrlItem, ft);
            }
        }

        /// <summary>
        /// モード遷移条件に応じたフリーズ適用を行います。
        /// </summary>
        /// <param name="ctrl">制御条件リスト</param>
        /// <param name="ft">各タイミングの待機時間データ</param>
        /// <param name="modeSrc">遷移元モード</param>
        /// <param name="modeDst">遷移先モード</param>
        public void SetFreezeMode(List<LocalDataSet.FreezeControlData> ctrl, List<LocalDataSet.FreezeTimeData> ft, byte modeSrc, byte modeDst)
        {
            // 全制御データにチェックをかける
            foreach (var ctrlItem in ctrl)
            {
                if (ctrlItem.ControlType != LocalDataSet.FreezeControlData.FreezeControlType.Mode) continue;
                var cond = new LocalDataSet.FreezeControlData.FreezeCondMode(ctrlItem.Condition);
                // ボーナス成立時は現状未実装
                if (cond.IsBonus) continue;
                bool condMatch = modeSrc == cond.ModeSrc && modeDst == cond.ModeDst;

                // データ適用
                if (condMatch) Apply(ctrlItem, ft);
            }
        }

        /// <summary>
        /// リアルタイム条件に応じたフリーズ適用を行います。
        /// </summary>
        /// <param name="ctrl">制御条件リスト</param>
        /// <param name="ft">各タイミングの待機時間データ</param>
        /// <param name="RTSrc">RT遷移元</param>
        /// <param name="RTDst">RT遷移先</param>
        public void SetFreezeRT(List<LocalDataSet.FreezeControlData> ctrl, List<LocalDataSet.FreezeTimeData> ft, byte RTSrc, byte RTDst)
        {
            // 全制御データにチェックをかける
            foreach (var ctrlItem in ctrl)
            {
                if (ctrlItem.ControlType != LocalDataSet.FreezeControlData.FreezeControlType.RT) continue;
                var cond = new LocalDataSet.FreezeControlData.FreezeCondRT(ctrlItem.Condition);
                bool condMatch = RTSrc == cond.ModeSrc && RTDst == cond.ModeDst;

                // データ適用
                if (condMatch) Apply(ctrlItem, ft);
            }
        }

        /// <summary>
        /// 制御条件に合致した FreezeActData を生成または更新します。
        /// </summary>
        /// <param name="item">制御条件データ</param>
        /// <param name="ft">待機時間データ一覧</param>
        private void Apply(LocalDataSet.FreezeControlData item, List<LocalDataSet.FreezeTimeData> ft)
        {
            // Rand抽選
            int randVal = UnityEngine.Random.Range(0, item.RandVal);
            if (randVal != 0) return;

            if (item.Timing == LocalDataSet.FreezeControlData.FreezeTiming.AddGames)
            {   // 生成条件が同じデータに shiftGame を加算する
                for (int i = 0; i < actData.Count; ++i)
                {
                    if (actData[i].type == item.ControlType) actData[i].shiftGame += item.ShiftGameNum;
                }
            }
            else if (item.Timing == LocalDataSet.FreezeControlData.FreezeTiming.Reset)
            {   // 生成条件が同じデータをすべて削除する
                for (int i = 0; i < actData.Count; ++i)
                {
                    if (actData[i].type == item.ControlType) actData[i] = null;
                }
                actData.RemoveAll(d => d == null);  // null要素をすべて削除
            }
            else
            {   // 新規データ生成
                FreezeActData act = new FreezeActData
                {
                    type = item.ControlType,
                    timing = item.Timing,
                    durationMS = ft[item.WaitID - 1].CalcTime(),
                    shiftGame = item.ShiftGameNum
                };
                actData.Add(act);
            }
        }

        /// <summary>
        /// 指定タイミングの合計フリーズ時間を取得し、使用したデータを削除します。
        /// </summary>
        /// <param name="timing">取得対象のタイミング</param>
        /// <returns>合計フリーズ時間[ms]</returns>
        public int GetFreeze(LocalDataSet.FreezeControlData.FreezeTiming timing)
        {
            int ans = 0;
            for (int i = 0; i < actData.Count; ++i)
            {
                if (actData[i].timing != timing) continue;
                if (actData[i].shiftGame > 0) { actData[i].shiftGame--; continue; }
                ans += actData[i].durationMS;
                actData[i] = null;
            }

            actData.RemoveAll(d => d == null);  // null要素をすべて削除
            return ans;
        }
    }
}

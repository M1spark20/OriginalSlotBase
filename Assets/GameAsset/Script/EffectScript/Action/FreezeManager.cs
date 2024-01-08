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
    class FreezeActData : ILocalDataInterface
    {
        public LocalDataSet.FreezeControlData.FreezeControlType type { get; set; }  // 抽選元
        public LocalDataSet.FreezeControlData.FreezeTiming timing { get; set; }     // 発動タイミング
        public int durationMS { get; set; } // フリーズ時間[ms]
        public int shiftGame { get; set; }  // 持ち越し残ゲーム数

        public FreezeActData()
        {
            type = LocalDataSet.FreezeControlData.FreezeControlType.Flag;
            timing = LocalDataSet.FreezeControlData.FreezeTiming.Reset;
            durationMS = 0;
            shiftGame = 0;
        }
        public bool StoreData(ref BinaryWriter fs, int version) 
        {
            fs.Write((int)type);
            fs.Write((int)timing);
            fs.Write(durationMS);
            fs.Write(shiftGame);
            return true;
        }
        public bool ReadData(ref BinaryReader fs, int version)
        {
            type = (LocalDataSet.FreezeControlData.FreezeControlType)fs.ReadInt32();
            timing = (LocalDataSet.FreezeControlData.FreezeTiming)fs.ReadInt32();
            durationMS = fs.ReadInt32();
            shiftGame = fs.ReadInt32();
            return true;
        }
    }
    public class FreezeManager : ILocalDataInterface
    {
        private List<FreezeActData> actData;

        public FreezeManager()
        {
            actData = new List<FreezeActData>();
        }
        public bool StoreData(ref BinaryWriter fs, int version) {
            fs.Write(actData.Count);
            foreach (var item in actData) item.StoreData(ref fs, version);
            return true;
        }
        public bool ReadData(ref BinaryReader fs, int version)
        {
            int size = fs.ReadInt32();
            for (int i=0; i<size; ++i)
            {
                FreezeActData act = new FreezeActData();
                act.ReadData(ref fs, version);
                actData.Add(act);
            }
            return true;
        }

        // 条件判定を行う
        public void SetFreezeFlag(List<LocalDataSet.FreezeControlData> ctrl, List<LocalDataSet.FreezeTimeData> ft, byte flagID, bool isLaunchGame)
        {
            // 全制御データにチェックをかける
            foreach (var ctrlItem in ctrl)
            {
                if (ctrlItem.ControlType != LocalDataSet.FreezeControlData.FreezeControlType.Flag) continue;
                var cond = new LocalDataSet.FreezeControlData.FreezeCondFlag(ctrlItem.Condition);
                bool condMatch = ( flagID == cond.FlagID && (isLaunchGame || !cond.NoBonusFlag) );
                
                // データ適用
                if (condMatch) Apply(ctrlItem, ft);
            }
        }
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

        // 抽選処理・データ生成を行う
        private void Apply(LocalDataSet.FreezeControlData item, List<LocalDataSet.FreezeTimeData> ft)
        {
            // Rand抽選(Unityではコメントを外す)
            int randVal = UnityEngine.Random.Range(0, item.RandVal);
            if (randVal != 0) return;

            if (item.Timing == LocalDataSet.FreezeControlData.FreezeTiming.AddGames)
            {   // 生成条件が同じデータにshiftGameを加算する
                for (int i = 0; i < actData.Count; ++i)
                {
                    if (actData[i].type == item.ControlType) actData[i].shiftGame += item.ShiftGameNum;
                }
            }
            else if (item.Timing == LocalDataSet.FreezeControlData.FreezeTiming.Reset)
            {   // 生成条件が同じデータをすべて削除する
                for (int i=0; i<actData.Count; ++i)
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
                    durationMS = ft[item.WaitID-1].CalcTime(),
                    shiftGame = item.ShiftGameNum
                };
                actData.Add(act);
            }
        }

        // フリーズ時間[ms]を返す。使用したデータは自動削除する
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

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SlotEffectMaker2023.Action
{
    public class PatternHistoryElem : SlotMaker2022.ILocalDataInterface
    {
        public List<byte> ReelPos { get; set; }     // 停止位置
        public List<byte> SlipCount { get; set; }   // すべりコマ数
        public List<byte> StopOrder { get; set; }   // 押し順
        public byte BetNum { get; set; }            // ベット枚数
        public byte FlagID { get; set; }            // 成立フラグ(非表示)
        public byte BonusID { get; set; }           // 成立ボーナス(非表示)
        public int InEffect { get; set; }           // 演出ID

        public PatternHistoryElem()
        {
            ReelPos = new List<byte>();
            SlipCount = new List<byte>();
            StopOrder = new List<byte>();
            BetNum = 0;
            FlagID = 0;
            BonusID = 0;
            InEffect = 0;
        }
        public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write(ReelPos.Count);
            foreach (var item in ReelPos) fs.Write(item);
            fs.Write(SlipCount.Count);
            foreach (var item in SlipCount) fs.Write(item);
            fs.Write(StopOrder.Count);
            foreach (var item in StopOrder) fs.Write(item);

            fs.Write(BetNum);
            fs.Write(FlagID);
            fs.Write(BonusID);
            fs.Write(InEffect);
            return true;
        }
        public bool ReadData(ref BinaryReader fs, int version)
        {
            int dataCount = fs.ReadInt32();
            for (int i = 0; i < dataCount; ++i) ReelPos.Add(fs.ReadByte());
            dataCount = fs.ReadInt32();
            for (int i = 0; i < dataCount; ++i) SlipCount.Add(fs.ReadByte());
            dataCount = fs.ReadInt32();
            for (int i = 0; i < dataCount; ++i) StopOrder.Add(fs.ReadByte());

            BetNum = fs.ReadByte();
            FlagID = fs.ReadByte();
            BonusID = fs.ReadByte();
            InEffect = fs.ReadInt32();
            return true;
        }
    }
    public class BonusHistoryElem : SlotMaker2022.ILocalDataInterface
    {
        public int InGame { get; set; }         // 入賞G
        public string InDate { get; set; }      // 入賞時刻
        public byte BonusFlag { get; set; }     // 成立ボーナス
        public int MedalBefore { get; set; }    // ボーナス開始時差枚
        public int MedalAfter { get; set; }     // ボーナス終了時差枚
        public ushort LossGame { get; set; }    // 入賞までにかかったG数
        public bool IsActivate { get; set; }    // 当該ボーナスが入賞したか
        public bool IsFinished { get; set; }    // 当該ボーナスが終了したか
        public PatternHistoryElem InPattern { get; set; }   // 成立時出目
        
        public BonusHistoryElem()
        {
            InGame = -1;
            InDate = string.Empty;
            BonusFlag = 0;
            MedalBefore = 0;
            MedalAfter = 0;
            LossGame = 0;
            IsActivate = false;
            IsFinished = false;
            InPattern = new PatternHistoryElem();
        }
        public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write(InGame);
            fs.Write(InDate);
            fs.Write(BonusFlag);
            fs.Write(MedalBefore);
            fs.Write(MedalAfter);
            fs.Write(LossGame);
            fs.Write(IsActivate);
            fs.Write(IsFinished);
            return InPattern.StoreData(ref fs, version);
        }
        public bool ReadData(ref BinaryReader fs, int version)
        {
            InGame = fs.ReadInt32();
            InDate = fs.ReadString();
            BonusFlag = fs.ReadByte();
            MedalBefore = fs.ReadInt32();
            MedalAfter = fs.ReadInt32();
            LossGame = fs.ReadUInt16();
            IsActivate = fs.ReadBoolean();
            IsFinished = fs.ReadBoolean();
            return InPattern.ReadData(ref fs, version);
        }
    }
    public class HistoryManager : SlotMaker2022.ILocalDataInterface
    {	// 各種履歴管理クラス(Sav)
        public const int PATTERN_MAX = 32;
        private const int REEL_MAX = SlotMaker2022.LocalDataSet.REEL_MAX;

        public List<PatternHistoryElem> PatternHist { get; set; }
        public List<BonusHistoryElem> BonusHist { get; set; }

        public HistoryManager()
        {
            PatternHist = new List<PatternHistoryElem>();
            BonusHist = new List<BonusHistoryElem>();
        }
        public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write(PatternHist.Count);
            foreach (var item in PatternHist) item.StoreData(ref fs, version);
            fs.Write(BonusHist.Count);
            foreach (var item in BonusHist) item.StoreData(ref fs, version);
            return true;
        }
        public bool ReadData(ref BinaryReader fs, int version)
        {
            int dataCount = fs.ReadInt32();
            for(int i=0; i<dataCount; ++i)
            {
                PatternHistoryElem ph = new PatternHistoryElem();
                ph.ReadData(ref fs, version);
                PatternHist.Add(ph);
            }
            dataCount = fs.ReadInt32();
            for(int i=0; i<dataCount; ++i)
            {
                BonusHistoryElem bh = new BonusHistoryElem();
                bh.ReadData(ref fs, version);
                BonusHist.Add(bh);
            }
            return true;
        }
        public void Process(SlotValManager vm)
        {   // ボーナス回数を変数に転送する([0]はボーナス総回数)
            List<int> bCount = new List<int>();
            for (int i = 0; i <= Data.HistoryConfig.BONUS_TYPE_MAX; ++i) bCount.Add(0);

            // ボーナス回数をカウント(ただし入賞前データはカウントしない)
            var confBase = Singleton.EffectDataManagerSingleton.GetInstance().HistoryConf;
            foreach(var item in BonusHist)
            {
                if (!item.IsActivate) continue;
                ++bCount[0];
                ++bCount[confBase.GetConfig(item.BonusFlag).BonusType];
            }

            // 転送先を取得してボーナス回数を転送
            for(int i=0; i<=Data.HistoryConfig.BONUS_TYPE_MAX; ++i)
            {
                var setFor = vm.GetVariable(confBase.BonusCountHolder[i]);
                if (setFor == null) continue;
                setFor.val = bCount[i];
            }
        }
        public void LatchHist(SlotBasicData bs, List<ReelBasicData> rd, SlotValManager vm)
        {
            // 出目履歴を記録する(通常時のみ)
            if (bs.gameMode != 0) return;
            var confBase = Singleton.EffectDataManagerSingleton.GetInstance().HistoryConf;
            var nowPtn = new PatternHistoryElem()
            {
                BetNum = bs.betCount,
                FlagID = bs.castFlag,
                BonusID = bs.bonusFlag,
                InEffect = vm.GetVariable(confBase.LaunchEffect)?.val ?? 0,
            };
            for (int i = 0; i < REEL_MAX; ++i)
            {
                nowPtn.ReelPos.Add(rd[i].stopPos);
                nowPtn.SlipCount.Add(rd[i].slipCount);
                nowPtn.StopOrder.Add(rd[i].stopOrder);
            }
            ShiftAdd(PatternHist, nowPtn, PATTERN_MAX);
            Debug.Log("PatHist ReelPos[0]: " + nowPtn.ReelPos[0].ToString() + ", StopOrder[0]: " + nowPtn.StopOrder[0].ToString() + ", InEffect: " + nowPtn.InEffect.ToString() + ", size:" + PatternHist.Count.ToString());

            // ボーナス成立時出目を記録する
            if (bs.bonusFlag == 0) return;
            var conf = confBase.GetConfig(bs.bonusFlag);
            if (conf == null) return;
            if (conf.BonusType <= 0 && conf.BonusType > Data.HistoryConfig.BONUS_TYPE_MAX) return;
            if (BonusHist.Count > 0)
                if (!BonusHist[0].IsFinished) return;

            // ボーナス履歴記録開始(初期段階データのみ代入、残データは[0]を直接編集する)
            BonusHistoryElem inData = new BonusHistoryElem
            {
                BonusFlag = bs.bonusFlag,
                InPattern = nowPtn
            };
            ShiftAdd(BonusHist, inData, -1);
            Debug.Log("BonusHist BonusFlag: " + inData.BonusFlag.ToString() + ", size:" + BonusHist.Count.ToString());
        }
        public void AddLossGame()
        {
            if (BonusHist.Count <= 0) return;
            var mod = BonusHist[0];
            if (mod.IsActivate) return;
            ++mod.LossGame;
            Debug.Log("Add LossGame: " + mod.LossGame.ToString());
        }
        public void StartBonus(SlotBasicData bs, SlotValManager vm)
        {
            if (BonusHist.Count <= 0) return;
            var mod = BonusHist[0];
            if (mod.IsActivate) return;

            // ボーナス入賞時記録
            var confBase = Singleton.EffectDataManagerSingleton.GetInstance().HistoryConf;
            mod.InGame = vm.GetVariable(confBase.InGameHolder)?.val ?? -1;
            mod.InDate = DateTime.Now.ToString("yy-MM-dd HH:mm");
            mod.MedalBefore = (int)bs.outCount - (int)bs.inCount;
            mod.IsActivate = true;
            Debug.Log("BonusHist Latch - InGame: " + mod.InGame.ToString() + ", InDate: " + mod.InDate.ToString() + ", MedalBef: " + mod.MedalBefore.ToString());
        }
        public void FinishBonus(SlotBasicData bs, int nowPayout)
        {
            if (BonusHist.Count <= 0) return;
            var mod = BonusHist[0];
            if (mod.IsFinished) return;

            // ボーナス終了時記録
            mod.MedalAfter = (int)bs.outCount + nowPayout - (int)bs.inCount;
            mod.IsFinished = true;
            Debug.Log("BonusHist Fin - MedalAfter: " + mod.MedalAfter.ToString());
        }

        // シフトしての追加処理
        private void ShiftAdd<T>(List<T> box, T data, int maxSize)
        {
            if (maxSize < 0 || box.Count < maxSize) box.Add(data);          // 仮登録、後で[0]で登録しなおし
            for (int i = box.Count - 1; i > 0; --i) box[i] = box[i - 1];    // データシフト
            box[0] = data;                                                  // 先頭にデータを登録
        }
    }
}

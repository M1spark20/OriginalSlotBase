using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SlotEffectMaker2023.Action
{
    /// <summary>
    /// 停止パターン履歴を保持する要素クラス。
    /// リール停止位置、すべりコマ数、停止順、BET数、成立フラグなどを管理します。
    /// </summary>
    public class PatternHistoryElem : SlotMaker2022.ILocalDataInterface
    {
        public List<byte> ReelPos { get; set; }     // 停止位置
        public List<byte> SlipCount { get; set; }   // すべりコマ数
        public List<byte> StopOrder { get; set; }   // 押し順
        public byte BetNum { get; set; }            // ベット枚数
        public byte FlagID { get; set; }            // 成立フラグ(非表示)
        public byte BonusID { get; set; }           // 成立ボーナス(非表示)
        public int InEffect { get; set; }           // 演出ID

        /// <summary>
        /// コンストラクタ。リストを初期化し、デフォルト値を設定します。
        /// </summary>
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

        /// <summary>
        /// 履歴データをバイナリ形式で保存します。
        /// </summary>
        /// <param name="fs">BinaryWriter の参照</param>
        /// <param name="version">保存バージョン</param>
        /// <returns>保存に成功したか（常に true）</returns>
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

        /// <summary>
        /// バイナリ形式から履歴データを読み込みます。
        /// </summary>
        /// <param name="fs">BinaryReader の参照</param>
        /// <param name="version">保存バージョン</param>
        /// <returns>読み込みに成功したか（常に true）</returns>
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

    /// <summary>
    /// ボーナス履歴を保持する要素クラス。
    /// 入賞G、入賞日時、メダル差枚、損失ゲーム数、状態フラグなどを管理します。
    /// </summary>
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

        /// <summary>
        /// コンストラクタ。デフォルト値を設定します。
        /// </summary>
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

        /// <summary>
        /// 履歴データをバイナリ形式で保存します。
        /// </summary>
        /// <param name="fs">BinaryWriter の参照</param>
        /// <param name="version">保存バージョン</param>
        /// <returns>保存に成功したか</returns>
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

        /// <summary>
        /// バイナリ形式から履歴データを読み込みます。
        /// </summary>
        /// <param name="fs">BinaryReader の参照</param>
        /// <param name="version">保存バージョン</param>
        /// <returns>読み込みに成功したか</returns>
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

    /// <summary>
    /// 残高グラフデータを管理するクラス。
    /// リングバッファを用いて差枚推移を保存し、取得を提供します。
    /// </summary>
    public class BalanceGraph : SlotMaker2022.ILocalDataInterface
    {
        public const int COUNT_INTERVAL = 10;
        public const int BUF_MAX = 10000;

        private int GameCounter;
        private int RingBegin;
        private List<int> GraphData;    // リングバッファ

        /// <summary>
        /// コンストラクタ。内部状態を初期化します。
        /// </summary>
        public BalanceGraph()
        {
            GameCounter = 0;
            RingBegin = 0;
            GraphData = new List<int>();
        }

        /// <summary>
        /// 読み込み後の初期化。データが空の場合は0を追加します。
        /// </summary>
        public void Init()
        {
            // ファイル読込後に要素数がない場合は0を初期値に指定する
            if (GraphData.Count == 0) GraphData.Add(0);
        }

        /// <summary>
        /// グラフデータをバイナリ形式で保存します。
        /// </summary>
        /// <param name="fs">BinaryWriter の参照</param>
        /// <param name="version">保存バージョン</param>
        /// <returns>保存に成功したか（常に true）</returns>
        public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write(GameCounter);
            fs.Write(RingBegin);
            fs.Write(GraphData.Count);
            foreach (var item in GraphData) fs.Write(item);
            return true;
        }

        /// <summary>
        /// バイナリ形式からグラフデータを読み込みます。
        /// </summary>
        /// <param name="fs">BinaryReader の参照</param>
        /// <param name="version">保存バージョン</param>
        /// <returns>読み込みに成功したか（常に true）</returns>
        public bool ReadData(ref BinaryReader fs, int version)
        {
            GameCounter = fs.ReadInt32();
            RingBegin = fs.ReadInt32();
            int dataSize = fs.ReadInt32();
            for (int i = 0; i < dataSize; ++i) GraphData.Add(fs.ReadInt32());
            return true;
        }

        /// <summary>
        /// 一定間隔ごとに現在の差枚をリングバッファに追加します。
        /// </summary>
        /// <param name="bs">スロット基本データ</param>
        public void LatchGame(SlotBasicData bs)
        {
            if (++GameCounter < COUNT_INTERVAL) return;
            GameCounter = 0;
            if (GraphData.Count < BUF_MAX)
            {
                GraphData.Add((int)bs.outCount - (int)bs.inCount);
            }
            else
            {
                GraphData[RingBegin] = (int)bs.outCount - (int)bs.inCount;
                RingBegin = (RingBegin + 1) % BUF_MAX;
            }
        }

        /// <summary>
        /// 指定位置とサイズに基づいてグラフ値を補間取得します。
        /// </summary>
        /// <param name="pos">取得位置インデックス（0～size-1）</param>
        /// <param name="size">出力要素数</param>
        /// <returns>補間後の値、範囲外アクセス時は null</returns>
        public float? GetValue(int pos, int size)
        {
            // pos: [0, (size - 1)]
            // データが範囲外アクセスの場合nullを返す
            if (pos >= GraphData.Count) return null;
            // データ数がsizeを超過していない場合要素をそのまま返す
            if (GraphData.Count <= size) return GraphData[(pos + RingBegin) % GraphData.Count];

            // 超過している場合はデータ数と描画数に応じて計算位置を決定。0とCount-1が出るように調整
            float referenceF = (GraphData.Count - 1) / (float)(size - 1) * pos;
            int ref1 = ((int)referenceF + RingBegin) % GraphData.Count;
            int ref2 = (ref1 + 1) % GraphData.Count;
            float ratio = referenceF % 1f;
            return GraphData[ref1] * (1f - ratio) + GraphData[ref2] * ratio;
        }
    }
    /// <summary>
    /// 履歴管理クラス自体を管理するクラス。
    /// PatternHist, BonusHist, Graph の読み書き・処理を統括します。
    /// </summary>
    public class HistoryManager : SlotMaker2022.ILocalDataInterface
    {
        // 各種履歴管理クラス(Sav)
        public const int PATTERN_MAX = 32;
        private const int REEL_MAX = SlotMaker2022.LocalDataSet.REEL_MAX;

        public List<PatternHistoryElem> PatternHist { get; set; }
        public List<BonusHistoryElem> BonusHist { get; set; }
        public BalanceGraph Graph { get; set; }

        /// <summary>
        /// コンストラクタ。内部リストとグラフを初期化します。
        /// </summary>
        public HistoryManager()
        {
            PatternHist = new List<PatternHistoryElem>();
            BonusHist = new List<BonusHistoryElem>();
            Graph = new BalanceGraph();
        }

        /// <summary>
        /// 読み込み後の初期化処理を行います。
        /// </summary>
        public void Init()
        {
            Graph.Init();
        }

        /// <summary>
        /// 全履歴データをバイナリ形式で保存します。
        /// </summary>
        /// <param name="fs">BinaryWriter の参照</param>
        /// <param name="version">保存バージョン</param>
        /// <returns>保存に成功したか（常に true）</returns>
        public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write(PatternHist.Count);
            foreach (var item in PatternHist) item.StoreData(ref fs, version);
            fs.Write(BonusHist.Count);
            foreach (var item in BonusHist) item.StoreData(ref fs, version);
            Graph.StoreData(ref fs, version);
            return true;
        }

        /// <summary>
        /// バイナリ形式から全履歴データを読み込みます。
        /// </summary>
        /// <param name="fs">BinaryReader の参照</param>
        /// <param name="version">保存バージョン</param>
        /// <returns>読み込みに成功したか</returns>
        public bool ReadData(ref BinaryReader fs, int version)
        {
            int dataCount = fs.ReadInt32();
            for (int i = 0; i < dataCount; ++i)
            {
                PatternHistoryElem ph = new PatternHistoryElem();
                ph.ReadData(ref fs, version);
                PatternHist.Add(ph);
            }
            dataCount = fs.ReadInt32();
            for (int i = 0; i < dataCount; ++i)
            {
                BonusHistoryElem bh = new BonusHistoryElem();
                bh.ReadData(ref fs, version);
                BonusHist.Add(bh);
            }
            if (!Graph.ReadData(ref fs, version)) return false;
            return true;
        }

        /// <summary>
        /// ボーナス回数を変数に転送します。
        /// </summary>
        /// <param name="vm">SlotValManager のインスタンス</param>
        public void Process(SlotValManager vm)
        {
            // ボーナス回数を変数に転送する([0]はボーナス総回数)
            List<int> bCount = new List<int>();
            for (int i = 0; i <= Data.HistoryConfig.BONUS_TYPE_MAX; ++i) bCount.Add(0);

            // ボーナス回数をカウント(ただし入賞前データはカウントしない)
            var confBase = Singleton.EffectDataManagerSingleton.GetInstance().HistoryConf;
            foreach (var item in BonusHist)
            {
                if (!item.IsActivate) continue;
                ++bCount[0];
                ++bCount[confBase.GetConfig(item.BonusFlag).BonusType];
            }

            // 転送先を取得してボーナス回数を転送
            for (int i = 0; i <= Data.HistoryConfig.BONUS_TYPE_MAX; ++i)
            {
                var setFor = vm.GetVariable(confBase.BonusCountHolder[i]);
                if (setFor == null) continue;
                setFor.val = bCount[i];
            }
        }

        /// <summary>
        /// 出目履歴を記録します（通常時のみ）。
        /// </summary>
        /// <param name="bs">SlotBasicData のインスタンス</param>
        /// <param name="rd">リール基本データリスト</param>
        /// <param name="vm">SlotValManager のインスタンス</param>
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

        /// <summary>
        /// ボーナスリール開始時の処理を行います。
        /// </summary>
        public void ReelStart()
        {
            if (BonusHist.Count <= 0) return;
            var mod = BonusHist[0];
            if (mod.IsActivate) return;
            ++mod.LossGame;
            Debug.Log("Add LossGame: " + mod.LossGame.ToString());
        }

        /// <summary>
        /// 払い出し終了時にグラフ用データを追加します。
        /// </summary>
        /// <param name="bs">SlotBasicData のインスタンス</param>
        public void OnPayoutEnd(SlotBasicData bs)
        {
            Graph.LatchGame(bs);
        }

        /// <summary>
        /// ボーナス開始時の履歴記録を行います。
        /// </summary>
        /// <param name="bs">SlotBasicData のインスタンス</param>
        /// <param name="vm">SlotValManager のインスタンス</param>
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

        /// <summary>
        /// ボーナス終了時の履歴記録を行います。
        /// </summary>
        /// <param name="bs">SlotBasicData のインスタンス</param>
        /// <param name="nowPayout">現在の払い出し枚数</param>
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

        /// <summary>
        /// リストにデータをシフト追加します。
        /// </summary>
        /// <typeparam name="U">リスト要素の型</param>
        /// <param name="box">データ格納リスト</param>
        /// <param name="data">追加データ</param>
        /// <param name="maxSize">最大要素数（負値で無制限）</param>
        private void ShiftAdd<U>(List<U> box, U data, int maxSize)
        {
            if (maxSize < 0 || box.Count < maxSize) box.Add(data);          // 仮登録、後で[0]で登録しなおし
            for (int i = box.Count - 1; i > 0; --i) box[i] = box[i - 1];    // データシフト
            box[0] = data;                                                  // 先頭にデータを登録
        }
    }

}

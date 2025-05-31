using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SlotMaker2022;

namespace SlotEffectMaker2023.Action
{
    /// <summary>
    /// 単一のコレクション実績の状態（達成回数・初回日時・最新日時）を管理するクラス。
    /// </summary>
    public class CollectionAchieveElem : ILocalDataInterface
    {
        public ushort CompTimes { get; private set; }   // 達成回数
        public string FirstComp { get; private set; }   // 初回達成日時
        public string RecentComp { get; private set; }  // 最新達成日時

        /// <summary>
        /// コンストラクタ。初期値を設定します。
        /// </summary>
        public CollectionAchieveElem()
        {
            CompTimes = 0;
            FirstComp = "N/A";
            RecentComp = "N/A";
        }

        /// <summary>
        /// 実績データをバイナリ形式で保存します。
        /// </summary>
        /// <param name="fs">BinaryWriter の参照</param>
        /// <param name="version">保存バージョン</param>
        /// <returns>保存処理が成功したか（常に true）</returns>
        public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write(CompTimes);
            fs.Write(FirstComp);
            fs.Write(RecentComp);
            return true;
        }

        /// <summary>
        /// 実績データをバイナリ形式で読み込みます。
        /// </summary>
        /// <param name="fs">BinaryReader の参照</param>
        /// <param name="version">保存バージョン</param>
        /// <returns>読み込み処理が成功したか（常に true）</returns>
        public bool ReadData(ref BinaryReader fs, int version)
        {
            CompTimes = fs.ReadUInt16();
            FirstComp = fs.ReadString();
            RecentComp = fs.ReadString();
            return true;
        }

        /// <summary>
        /// 実績の達成を記録し、日時と回数を更新します。
        /// </summary>
        public void SetAchieved()
        {
            if (CompTimes == 0) FirstComp = DateTime.Now.ToString("yy-MM-dd HH:mm");
            RecentComp = DateTime.Now.ToString("yy-MM-dd HH:mm");
            if (CompTimes < ushort.MaxValue) ++CompTimes;
        }
    }

    /// <summary>
    /// ゲーム全体のコレクション実績の状態を管理するクラス。
    /// 達成状況の記録、判定、保存・読込機能を提供します。
    /// </summary>
    public class CollectionLogger : ILocalDataInterface
    {
        public List<CollectionAchieveElem> Achievements { get; private set; }   // 達成状況
        public List<int> NewGetID { get; private set; }                         // 最近新規達成したID
        public List<int> LatchID { get; private set; }                          // ボーナス入賞までに新規達成したID(Steam Achievement用)

        private List<int> achievedID;                                           // 1ゲーム内に達成判定を行ったID(2重達成マスク用, 保存対象外)
        private const int NewGetMax = 8;

        /// <summary>
        /// コンストラクタ。リストを初期化します。
        /// </summary>
        public CollectionLogger()
        {
            Achievements = new List<CollectionAchieveElem>();
            NewGetID = new List<int>();
            LatchID = new List<int>();
            achievedID = new List<int>();
        }

        /// <summary>
        /// 実績データをバイナリ形式で保存します。
        /// </summary>
        /// <param name="fs">BinaryWriter の参照</param>
        /// <param name="version">保存バージョン</param>
        /// <returns>保存処理が成功したか（常に true）</returns>
        public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write(Achievements.Count);
            foreach (var item in Achievements) item.StoreData(ref fs, version);
            fs.Write(NewGetID.Count);
            foreach (var item in NewGetID) fs.Write(item);
            fs.Write(LatchID.Count);
            foreach (var item in LatchID) fs.Write(item);
            return true;
        }

        /// <summary>
        /// 実績データをバイナリ形式で読み込みます。
        /// </summary>
        /// <param name="fs">BinaryReader の参照</param>
        /// <param name="version">保存バージョン</param>
        /// <returns>読み込み処理が成功したか</returns>
        public bool ReadData(ref BinaryReader fs, int version)
        {
            int dataSize = fs.ReadInt32();
            for (int i = 0; i < dataSize; ++i)
            {
                var item = new CollectionAchieveElem();
                if (!item.ReadData(ref fs, version)) return false;
                Achievements.Add(item);
            }
            dataSize = fs.ReadInt32();
            for (int i = 0; i < dataSize; ++i) NewGetID.Add(fs.ReadInt32());
            dataSize = fs.ReadInt32();
            for (int i = 0; i < dataSize; ++i) LatchID.Add(fs.ReadInt32());
            return true;
        }

        /// <summary>
        /// 読み込み後、コレクション数と実績リスト数を合わせて初期化します。
        /// </summary>
        /// <param name="pColle">コレクションデータ</param>
        public void Init(Data.CollectionData pColle)
        {   // データ読込後、コレクション達成変数がコレクション数より小さければアイテムを追加する
            while (Achievements.Count < pColle.Collections.Count) Achievements.Add(new CollectionAchieveElem());
        }

        /// <summary>
        /// リール停止ごとにコレクション達成判定を行います。
        /// </summary>
        /// <param name="cd">コレクションデータ</param>
        /// <param name="rd">リール基本データリスト</param>
        /// <param name="ra">リールシンボル配列</param>
        /// <param name="vm">変数管理マネージャ</param>
        /// <param name="sb">スロット基本データ</param>
        /// <param name="maskFlag">入賞ゲームのみマスク判定</param>
        public void JudgeCollection(Data.CollectionData cd, List<ReelBasicData> rd, LocalDataSet.ReelArray[][] ra, SlotValManager vm, SlotBasicData sb, bool maskFlag)
        {   // コレクション判定(リール停止毎に判定)
            const int REEL_NUM = LocalDataSet.REEL_MAX;

            // 判定条件(前提としてgameMode = 0であることが必要、ただし入賞Gのみマスク: maskFlag)
            // 判定の中にHazure, Aimingが含まれる場合は入賞Gも判定を行う
            if (sb.gameMode != 0 && !maskFlag) return;
            if ((vm.GetVariable(cd.JudgeCondName)?.val ?? 0) == 0) return;

            // はずれ判定取得
            bool hazureFlag = (vm.GetVariable(cd.JudgeHazure)?.val ?? 0) != 0;
            bool aimingFlag = (vm.GetVariable(cd.JudgeAiming)?.val ?? 0) != 0;
            UnityEngine.Debug.Log("Judge start");

            for (int id = 0; id < cd.Collections.Count; ++id)
            {
                // 探索除外判定を行う
                bool achieveFlag = true;
                // ボーナス入賞時の達成判定。除外しない場合はtrueとする
                bool hasHazure = false;

                foreach (var item in achievedID) achieveFlag &= item != id;
                if (!achieveFlag) continue;

                // 達成判定を行う
                var jd = cd.Collections[id].CollectionElem;
                for (int reelC = 0; reelC < REEL_NUM; ++reelC)
                {
                    // 左回転中以外 かつ 左1st以外なら処理しない
                    if (reelC == 0)
                        achieveFlag &= !(jd[reelC].Pattern != Data.CollectionReelPattern.eRotating && rd[reelC].stopOrder != 1);
                    // 各リール判定処理(REEL_NPOS: 回転中)
                    switch (jd[reelC].Pattern)
                    {
                        case Data.CollectionReelPattern.eAny:
                            if (rd[reelC].stopOrder == REEL_NUM) hasHazure = true;
                            break;
                        case Data.CollectionReelPattern.eRotating:
                            achieveFlag &= rd[reelC].stopPos == ReelBasicData.REEL_NPOS;
                            break;
                        case Data.CollectionReelPattern.eHazure:
                            achieveFlag &= rd[reelC].stopPos != ReelBasicData.REEL_NPOS && hazureFlag;
                            if (rd[reelC].stopOrder == REEL_NUM) hasHazure = true;
                            break;
                        case Data.CollectionReelPattern.eAiming:
                            achieveFlag &= rd[reelC].stopPos != ReelBasicData.REEL_NPOS && aimingFlag;
                            if (rd[reelC].stopOrder == REEL_NUM) hasHazure = true;
                            break;
                        case Data.CollectionReelPattern.eReelPos:
                            achieveFlag &= rd[reelC].stopPos != ReelBasicData.REEL_NPOS && rd[reelC].stopPos == jd[reelC].ReelPos;
                            break;
                        case Data.CollectionReelPattern.eComaItem:
                            achieveFlag &= CheckSymbol(jd[reelC], rd[reelC], ra[reelC]);
                            break;
                        default:
                            achieveFlag = false;
                            break;
                    }
                    if (!achieveFlag) break;
                }
                // 判定の中にHazure, Aimingが含まれる場合、当該リールが最終停止以外なら入賞Gも判定を行う。逆論理でここで判定
                if (!achieveFlag || (!hasHazure && sb.gameMode != 0)) continue;

                // ここまで来ると達成、達成処理を行う
                if (Achievements[id].CompTimes == 0) AddNewAchieve(id);
                achievedID.Add(id);
                Achievements[id].SetAchieved();
                UnityEngine.Debug.Log("Colle Latch: " + (id + 1).ToString());
            }
        }

        /// <summary>
        /// ボーナス入賞に伴うLatchIDのクリアを行います。
        /// </summary>
        public void ClearLatch()
        {   // ボーナス入賞に伴うLatchのクリア
            UnityEngine.Debug.Log("Clear Latch: " + LatchID.Count.ToString());
            LatchID.Clear();
        }

        /// <summary>
        /// 1ゲーム終了時に内部の達成IDリストをクリアします。
        /// </summary>
        public void EndGame()
        {   // 1G終了によるachievedIDのクリア
            UnityEngine.Debug.Log("Clear achieved");
            achievedID.Clear();
        }

        /// <summary>
        /// 全実績の達成数を返します。
        /// </summary>
        /// <returns>達成済みの実績数</returns>
        public int GetAchievedCount()
        {
            int ans = 0;
            foreach (var item in Achievements) if (item.CompTimes > 0) ++ans;
            return ans;
        }

        /// <summary>
        /// 指定レベルの実績達成数を返します。
        /// </summary>
        /// <param name="cd">コレクションデータ</param>
        /// <param name="pLevel">対象レベル</param>
        /// <returns>該当レベルで達成済みの実績数</returns>
        public int GetAchievedCount(Data.CollectionData cd, int pLevel)
        {   // レベル別にカウント
            int ans = 0;
            for (int i = 0; i < cd.Collections.Count; ++i)
                if (cd.Collections[i].Level == pLevel && Achievements[i].CompTimes > 0) ++ans;
            return ans;
        }

        /// <summary>
        /// リールの絵柄がコレクション条件に一致するかチェックします。
        /// </summary>
        /// <param name="cd">コレクションリール要素</param>
        /// <param name="rd">リール基本データ</param>
        /// <param name="ra">リール配列​</param>
        /// <returns>条件に一致すれば true、それ以外は false</returns>
        private bool CheckSymbol(Data.CollectionReelElem cd, ReelBasicData rd, LocalDataSet.ReelArray[] ra)
        {   // リールのシンボルチェックを行う
            if (rd.stopPos == ReelBasicData.REEL_NPOS) return false;

            const int SHOW_NUM = LocalDataSet.SHOW_MAX;
            const int COMA_NUM = LocalDataSet.COMA_MAX;

            for (int showC = 0; showC < SHOW_NUM; ++showC)
            {
                var checkData = Math.Abs(cd.ComaItem[showC]);
                bool InvFlag = cd.ComaItem[showC] < 0;
                if (checkData == 0) continue;

                byte comaPos = (byte)((rd.stopPos + showC) % COMA_NUM);
                // 配列の格納順は反転していることに注意
                short comaMask = (short)(1 << (ra[COMA_NUM - comaPos - 1].Coma));
                bool judge = (checkData & comaMask) != 0 ^ InvFlag;
                if (!judge) return false;
            }
            return true;
        }

        /// <summary>
        /// 新規実績達成を記録し、表示・送信対象リストに登録します。
        /// </summary>
        /// <param name="id">達成ID</param>
        private void AddNewAchieve(int id)
        {
            if (NewGetMax < 0 || NewGetID.Count < NewGetMax) NewGetID.Add(id);          // 仮登録、後で[0]で登録しなおし
            for (int i = NewGetID.Count - 1; i > 0; --i) NewGetID[i] = NewGetID[i - 1]; // データシフト
            NewGetID[0] = id;                                                           // 先頭にデータを登録
            LatchID.Add(id);
        }
    }
}

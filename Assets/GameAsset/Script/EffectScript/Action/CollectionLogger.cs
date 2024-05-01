using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SlotMaker2022;

namespace SlotEffectMaker2023.Action
{
    public class CollectionAchieveElem : ILocalDataInterface
    {
        public ushort CompTimes { get; private set; }   // 達成回数
        public string FirstComp { get; private set; }   // 初回達成日時
        public string RecentComp { get; private set; }  // 最新達成日時

        public CollectionAchieveElem()
        {
            CompTimes = 0;
            FirstComp = "N/A";
            RecentComp = "N/A";
        }

        public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write(CompTimes);
            fs.Write(FirstComp);
            fs.Write(RecentComp);
            return true;
        }
        public bool ReadData(ref BinaryReader fs, int version)
        {
            CompTimes = fs.ReadUInt16();
            FirstComp = fs.ReadString();
            RecentComp = fs.ReadString();
            return true;
        }
        public void SetAchieved()
        {
            if (CompTimes == 0) FirstComp = DateTime.Now.ToString("yy-MM-dd HH:mm");
            RecentComp = DateTime.Now.ToString("yy-MM-dd HH:mm");
            if (CompTimes < ushort.MaxValue) ++CompTimes;
        }
    }
    public class CollectionLogger : ILocalDataInterface
    {
        public List<CollectionAchieveElem> Achievements { get; private set; }   // 達成状況
        public List<int> NewGetID { get; private set; }                         // 最近新規達成したID
        public List<int> LatchID { get; private set; }                          // ボーナス入賞までに新規達成したID(Steam Achevement用)

        private List<int> achievedID;                                           // 1ゲーム内に達成判定を行ったID(2重達成マスク用, 保存対象外)
        private const int NewGetMax = 8;

        public CollectionLogger()
        {
            Achievements = new List<CollectionAchieveElem>();
            NewGetID = new List<int>();
            LatchID = new List<int>();
            achievedID = new List<int>();
        }
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
        public bool ReadData(ref BinaryReader fs, int version)
        {
            int dataSize = fs.ReadInt32();
            for (int i=0; i<dataSize; ++i)
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
        public void Init(Data.CollectionData pColle)
        {   // データ読込後、コレクション達成変数がコレクション数より小さければアイテムを追加する
            while (Achievements.Count < pColle.Collections.Count) Achievements.Add(new CollectionAchieveElem());
        }
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

            for (int id=0; id<cd.Collections.Count; ++id)
            {
                // 探索除外判定を行う
                bool achieveFlag = true;
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
                            break;
                        case Data.CollectionReelPattern.eRotating:
                            achieveFlag &= rd[reelC].stopPos == ReelBasicData.REEL_NPOS;
                            break;
                        case Data.CollectionReelPattern.eHazure:
                            achieveFlag &= rd[reelC].stopPos != ReelBasicData.REEL_NPOS && hazureFlag;
                            hasHazure = true;
                            break;
                        case Data.CollectionReelPattern.eAiming:
                            achieveFlag &= rd[reelC].stopPos != ReelBasicData.REEL_NPOS && aimingFlag;
                            hasHazure = true;
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
                // 判定の中にHazure, Aimingが含まれる場合は入賞Gも判定を行う。逆論理でここで判定
                if (!achieveFlag || (!hasHazure && sb.gameMode != 0)) continue;

                // ここまで来ると達成、達成処理を行う
                if (Achievements[id].CompTimes == 0) AddNewAchieve(id);
                achievedID.Add(id);
                Achievements[id].SetAchieved();
                UnityEngine.Debug.Log("Colle Latch: " + (id+1).ToString());
            }
        }
        public void ClearLatch()
        {   // ボーナス入賞に伴うLatchのクリア
        	UnityEngine.Debug.Log("Clear Latch: " + LatchID.Count.ToString());
            LatchID.Clear();
        }
        public void EndGame()
        {   // 1G終了によるachievedIDのクリア
        	UnityEngine.Debug.Log("Clear achieved");
            achievedID.Clear();
        }
        public int GetAchievedCount()
        {
            int ans = 0;
            foreach (var item in Achievements) if (item.CompTimes > 0) ++ans;
            return ans;
        }

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

        private void AddNewAchieve(int id)
        {
            if (NewGetMax < 0 || NewGetID.Count < NewGetMax) NewGetID.Add(id);          // 仮登録、後で[0]で登録しなおし
            for (int i = NewGetID.Count - 1; i > 0; --i) NewGetID[i] = NewGetID[i - 1]; // データシフト
            NewGetID[0] = id;                                                           // 先頭にデータを登録
            
            LatchID.Add(id);
        }
    }
}

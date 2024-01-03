using System;
using System.Collections.Generic;
using System.Text;

namespace SlotMaker2022.main_function
{
    public partial class MainReelManager
    {
        // GetCastResultを別ファイルで定義する
        // pStopPos: index範囲外/負数で回転中を表現可能。全コマAnyで判定する。下段基準の停止位置を指定
        // 戻り値はAnyの場合を含む全停止形で止まる配当を重複ありで出力する
        public GetCastResult GetCast(int[] pStopPos, int pBetNum, uint pGameMode, int pFlagID, int pBonusID)
        {
            /* 【制御用データ作成メモ】
             *  1. 単体の払い出し枚数が最大となる配当を抽出し、払い出し枚数をメモする
             *  2. 回転中リールがANYの時に取りえる配当組み合わせ数を抽出する
             */

            var castCommon = MainROMDataManagerSingleton.GetInstance().CastCommonData;
            var castElemData = MainROMDataManagerSingleton.GetInstance().CastElemData;
            var flagElemData = MainROMDataManagerSingleton.GetInstance().FlagElemData;

            // 出力データ作成
            GetCastResult res = new GetCastResult();

            /* 引数条件確認 */
            {
                if (pStopPos.Length != LocalDataSet.REEL_MAX) return null;
                if (pBetNum < 0 || pBetNum >= LocalDataSet.BET_MAX) return null;
            }

            List<int> stopReel = new List<int>();       // 回転中リールindex
            const int comaMax = LocalDataSet.COMA_MAX;
            int nonStopMask = 0;                        // 回転中リールマスク
            int launchFlagMask = 0;                     // 成立フラグマスク(全停止時は0のままとする)
            List<int> lineDataOpen = new List<int>();   // 解析を行う有効ラインの並び
            List<int> lineDataID = new List<int>();     // 解析を行う有効ラインID
            List<int> lineDataClose = new List<int>();  // 配当を得た有効ラインの並び

            /* 停止済みor停止リールを抽出する。併せて回転中リールマスクを作成する */
            for (int reelC = 0; reelC < pStopPos.Length; ++reelC)
            {
                if (pStopPos[reelC] < 0 || pStopPos[reelC] >= comaMax)
                {
                    int allStopMask = (1 << (LocalDataSet.SYMBOL_MAX + 1)) - 1;
                    nonStopMask |= allStopMask << LocalDataSet.SYMBOL_MAX * reelC;
                }
                else stopReel.Add(reelC);
            }

            // 回転中リールがなければpayoutNumを有効にする
            if (nonStopMask == 0) res.payoutNum = 0;

            /* 有効ラインのBitDataを生成する */
            for (int lineC = 0; lineC < LocalDataSet.PAYLINE_MAX; ++lineC)
            {
                /* 有効ライン情報が無効なら処理を行わない */
                {
                    // payLineData自体が無効である
                    if (castCommon.PayLineData.GetData((uint)lineC) >= castCommon.PayLineData.BaseNum - 1) continue;

                    // 指定のBET数で無効である
                    uint availPosRef = (uint)(pBetNum * LocalDataSet.PAYLINE_MAX + lineC);
                    if (castCommon.AvailableLineData.GetData(availPosRef) == 0) continue;
                }

                // 停止済みリール位置BitData生成
                int reelBitData = 0;
                for (int reelC = 0; reelC < stopReel.Count; ++reelC)
                {
                    int comaID = GetComaIDFromLine(stopReel[reelC], pStopPos[stopReel[reelC]], lineC);
                    reelBitData |= 1 << (LocalDataSet.SYMBOL_MAX * stopReel[reelC]) + comaID;
                }
                lineDataOpen.Add(reelBitData);
                lineDataID.Add(lineC);
            }

            /* 全配当に対してチェックを入れる */
            for (int castC = 0; castC < castElemData.Count; ++castC)
            {
                var elem = castElemData[castC];

                // 配当が指定gamemodeで無効なら何もしない
                if (elem.AvailGameMode.GetData(pGameMode) == 0) continue;
                int castMask = (int)elem.PaySymbol.Export();

                // フラグが成立していない場合、回転中リールがあれば処理をしない
                // 回転中リールがない場合停止位置の棄却を行うため処理を行う
                bool isLaunch = pBonusID < 0 && pFlagID < 0;        // 成立フラグがなければ必ず処理する
                {
                    // ボーナスフラグ判定
                    if (pBonusID >= 0 && elem.ValidateBonusFlag > 0)
                        isLaunch |= pBonusID == elem.ValidateBonusFlag;

                    // 小役フラグ判定(ハズレの場合isLaunch=false、ここに入らずfalseから変わらない)
                    if (pFlagID > 0 && elem.ValidateBonusFlag == 0)
                    {
                        var judgeData = flagElemData[pFlagID - 1];
                        var judgeRef = castC + 1;
                        isLaunch |= judgeRef >= judgeData.FlagBegin && judgeRef <= judgeData.FlagEnd;
                    }
                }
                // スキップ判定
                if (!isLaunch && stopReel.Count < LocalDataSet.REEL_MAX) continue;

                /* 有効ライン数分繰り返す：全ラインのデータが見たい */
                for (int lineC = 0; lineC < lineDataOpen.Count; ++lineC)
                {
                    // reelBitDataがマスクで変化する場合不適合→continue
                    if ((lineDataOpen[lineC] & castMask) != lineDataOpen[lineC]) continue;

                    // データが成立フラグでなければ停止位置を棄却する(全リール停止時のみ)
                    if (!isLaunch) return new GetCastResult();

                    /* closeリストにデータがない場合、回転中リールを含めた入賞可能配当組み合わせ数を計算する */
                    if(!lineDataClose.Contains(lineDataOpen[lineC])) {
                        // 回転中リールをANYにしたマスク結果を作成
                        int reelBitDataRotating = (lineDataOpen[lineC] | nonStopMask) & castMask;

                        // リールごとにビット数を数える(回転中リールがある場合のみ)
                        uint patternNum = 1;
                        if (nonStopMask != 0)
                        {
                            // 各リールにいくつbitが立っているか数える
                            uint[] bitCount = new uint[LocalDataSet.REEL_MAX];
                            for (int i = 0; reelBitDataRotating != 0; ++i, reelBitDataRotating >>= 1)
                            {   // bitが立っていれば該当リールの位置にビット数を加算する
                                if ((reelBitDataRotating & 1) != 0) ++bitCount[i / LocalDataSet.SYMBOL_MAX];
                            }

                            // ビット数から組み合わせ数を割り出して保存する
                            for (int i = 0; i < bitCount.Length; ++i) patternNum *= bitCount[i];
                        }
                        res.castPattern += patternNum;
                    }

                    // 回転中リールがある場合成立フラグマスク更新
                    if (isLaunch && nonStopMask != 0) launchFlagMask |= castMask;

                    // closeリストに追加
                    lineDataClose.Add(lineDataOpen[lineC]);
                    // 入賞ライン追加
                    res.payLine.Add(lineDataID[lineC]);
                    // 入賞役データ追加
                    res.matchCast.Add(elem);

                    // 配当枚数を確認する
                    uint payoutNumID = elem.PayoutNumID.GetData((uint)pBetNum);
                    if (payoutNumID > LocalDataSet.CastCommonData.PAYOUT_DATA_MAX)
                    {
                        // リプレイ。payPriorityを最大にする
                        res.payPriority = ushort.MaxValue;
                    }
                    else if (payoutNumID > 0)
                    {
                        // 小役払い出し。払い出し枚数が最大か確認する。indexは定義数-1とする
                        // 全停止時のみ払い出し枚数を合算する
                        uint payNum = castCommon.PayoutData.GetData(payoutNumID - 1);
                        res.maximumPayCast = Math.Max(res.maximumPayCast, payNum);
                        if (res.payoutNum >= 0)
                        {
                            uint payMax = MainROMDataManagerSingleton.GetInstance().CastCommonData.MaxPayout.GetData((uint)pBetNum);
                            res.payoutNum += (int)payNum;
                            res.payoutNum = Math.Min(res.payoutNum, (int)payMax);
                        }
                    }

                    // 配当がボーナスの場合フラグを立てる
                    res.getBonus |= elem.ValidateBonusFlag > 0;
                }
            }

            /* 取りこぼしのない小役か判定する。*/
            if (launchFlagMask != 0)
            {
                bool noLostElem = true;
                for (int reelC = 0; reelC < LocalDataSet.REEL_MAX; ++reelC)
                {
                    // 指定リールが停止中なら処理をしない
                    if (stopReel.Contains(reelC)) continue;
                    // 取りこぼし有無を判定する
                    noLostElem &= CheckNoLost(reelC, launchFlagMask);
                }
                res.canLose &= !noLostElem;
            }

            /* リプレイが揃わない場合、payPriority生成 */
            if (res.payPriority < ushort.MaxValue)
            {
                // 最大配当により優先度決定
                if (res.maximumPayCast > 0) res.payPriority |= (ushort)(1u << (int)res.maximumPayCast - 1);

                // 子役優先制御の場合、payPriorityを1bitシフト。0bit目にはボーナス状態が入る
                if (!castCommon.IsPriorBonus) res.payPriority <<= 1;

                // ボーナスが揃えられる場合 or 取りこぼしのない場合、ボーナスに相当するフラグを立てる
                if (castCommon.IsPriorBonus && res.getBonus) res.payPriority |= 0x8000;                     // ボーナス優先制御(取りこぼし有無は見ない)
                if (!castCommon.IsPriorBonus && (res.getBonus || !res.canLose)) res.payPriority |= 0x0001;  // 小役優先制御　　(取りこぼし有無を見る)
            }

            // 入力位置が停止可能と判断して結果を返す
            res.stopAvailable = true;
            return res;
        }
        public string GetCastName(int[] pStopPos, int pBetNum, uint pGameMode)
        {
            var matchData = GetCast(pStopPos, pBetNum, pGameMode, -1, -1);
            if (matchData == null) return string.Empty;

            string ans = string.Empty;
            foreach (var item in matchData.matchCast) ans += item.FlagName;
            return ans;
        }

        // 指定リールのコマ番号を取得(-1:回転中orエラー)
        private int GetComaIDFromLine(int pReelID, int pStopPos, int pLineID)
        {
            if (pReelID < 0 || pReelID >= LocalDataSet.REEL_MAX) return -1;
            if (pLineID < 0 || pLineID >= LocalDataSet.PAYLINE_MAX) return -1;

            // indexが範囲内なら何もしない -> 回転中の扱い
            if (pStopPos < 0 || pStopPos >= LocalDataSet.COMA_MAX) return -1;

            /* 参照コマ導出 */
            var castCommon = MainROMDataManagerSingleton.GetInstance().CastCommonData;

            int fracVal = (int)Math.Pow(LocalDataSet.SHOW_MAX, pReelID);
            // 指定リールにて確認するコマの下段からの位置を取得
            int checkHeight = (int)castCommon.PayLineData.GetData((uint)pLineID) / fracVal % LocalDataSet.SHOW_MAX;

            // 高さを指定してコマ番号を返す
            return GetComaID(pReelID, pStopPos, checkHeight);
        }
        private int GetComaID(int pReelID, int pStopPos, int offset)
        {
            var reelData = MainROMDataManagerSingleton.GetInstance().ReelArray;
            int comaPos = GetPosFromOffset(pStopPos, offset);

            // 図柄番号を返す
            return reelData[pReelID][comaPos].Coma;
        }
        private int GetPosFromOffset(int basePos, int offset)
        {
            // 参照リールコマ番号を取得(実配列データ型との整合を取る)
            var reelData = MainROMDataManagerSingleton.GetInstance().ReelArray;
            int comaPos = (basePos + offset) % LocalDataSet.COMA_MAX;
            comaPos = LocalDataSet.COMA_MAX - 1 - comaPos;

            return comaPos;
        }
        
        // 小役の取りこぼしがあるか調べる
        private bool CheckNoLost(int pReelID, int pCastMask)
        {
            if (pReelID < 0 || pReelID >= LocalDataSet.REEL_MAX) return false;
            int slipCount = 0;

            for (int comaC = 0; comaC < LocalDataSet.COMA_MAX || slipCount > 0; ++comaC)
            {
                // 図柄番号をマスク形式で取得
                int comaMask = 1 << pReelID * LocalDataSet.SYMBOL_MAX + GetComaID(pReelID, comaC, 0);
                // マスク通過判定
                slipCount = (comaMask & pCastMask) == comaMask ? 0 : slipCount + 1;
                if (slipCount >= LocalDataSet.SLIP_MAX) return false;
            }
            return true;
        }

        // 指定箇所のリーチ目レベルを判定する
        public int GetReachLevel(int pReachDataID, int[] pStopPos)
        {
            // 回転中を許容しない。発見した場合リーチ目として扱わない
            foreach (var item in pStopPos)
                if (item < 0 || item >= LocalDataSet.COMA_MAX) return 0;
            // pReachDataIDが範囲外なら探索しない
            if (pReachDataID < 0 || pReachDataID >= MainROMDataManagerSingleton.GetInstance().ReachData.Count) return 0;

            // 停止位置の配当を確認
            const int betNum = LocalDataSet.BET_MAX - 1;
            const int gameMode = 0;
            var stopCast = GetCast(pStopPos, betNum, gameMode, -1, -1);
            if (stopCast == null) return 0;
            bool haveCast = stopCast.matchCast.Count > 0;

            /* 探索対象とするリールを選定する */
            int baseReel = pReachDataID / LocalDataSet.COMA_MAX;

            // リーチ目データを探索する
            var mainData = MainROMDataManagerSingleton.GetInstance();
            var reachData = mainData.ReachData[pReachDataID];
            int dataIndex = -1;

            foreach (var item in reachData.PosData)
            {
                bool PassFlag = true;
                bool noCastFlag = false;
                ++dataIndex;

                // 各リールの状況を調べ、リーチ目データとの照合性を確認する
                // 結果はPassFlagに格納し、trueなら子役ハズレ判定を行って「リーチ目」の判定結果を返す
                for (int reelC = 0; reelC < LocalDataSet.ReelPosData.DefReelPosNum; ++reelC)
                {
                    // baseReelに対しては処理を行わない
                    if (reelC == baseReel) continue;

                    var localPosData = item.PosData[reelC];

                    // JudgeComaPosにデータがない場合子役ハズレ目として取り扱う
                    if (localPosData.JudgeComaPos.Export() == 0)
                    {
                        noCastFlag = true;
                        continue;
                    }

                    // ReelPosElemDataとの合致を判定し、合致しない場合はPassFlagをfalseとする
                    if (!GetMatchStopPos(localPosData, pStopPos[reelC], reelC)) { PassFlag = false; break; }
                }

                // 停止位置データが棄却された場合当該データの処理を終了する
                if (!PassFlag) continue;

                // 子役ハズレ判定
                if (noCastFlag && haveCast) continue;

                // リーチ目レベルを取得してレベルを書き出し
                int patTotal = 0;
                for (int i = reachData.ReachPatNum.Length - 1; i >= 0; --i)
                {
                    patTotal += reachData.ReachPatNum[i];
                    if (patTotal > dataIndex) return i + 1;
                }
            }
            return 0;
        }

        // 停止位置がStopPosDataに合致するかどうかを返す
        // pCompData: 検索対象リールの比較対象データ
        // pStopPos : 比較対象データの停止位置
        private bool GetMatchStopPos(LocalDataSet.ReelPosElemData pCompData, int pStopPos, int pStopReelID)
        {
            // JudgeComaPosが有効な高さのコマ番号を取得し、コマ番号を比較する。
            // 判定結果はJudgeReelに格納し、判定結果を戻り値として返す
            bool JudgeReel = false;
            var offset = LocalDataSet.SYMBOL_MAX;
            var mainData = MainROMDataManagerSingleton.GetInstance();
            int stopReel = pCompData.JudgeReel;
            if (stopReel >= LocalDataSet.REEL_MAX) stopReel = pStopReelID;
            if (stopReel < 0 || stopReel >= LocalDataSet.REEL_MAX) return false;

            // 表示高さごとに比較
            for (uint showC = 0; showC < pCompData.JudgeComaPos.ElemSize; ++showC)
            {
                // コマ高さが判定対象外の場合判定を行わない
                if (!(pCompData.JudgeComaPos.GetData(showC) > 0)) continue;
                // 確認位置の図柄番号取得
                uint comaNo = (uint)GetComaID(stopReel, pStopPos, (int)showC);

                // データ比較
                if (pCompData.CombinationID < offset)
                {
                    // 図柄IDを直接指定する場合
                    JudgeReel |= comaNo == pCompData.CombinationID;
                }
                else
                {
                    // Combinationを指定する場合
                    var comb = mainData.ComaCombinationData[pCompData.CombinationID - offset];

                    /* 図柄No.とCombinationの比較 */
                    {
                        // 参照位置の図柄Noを比較する
                        // CombinationDataを呼び出して図柄Noと比較する
                        JudgeReel |= comb.Combination.GetData(comaNo) > 0;
                    }

                    /* ExDataと停止位置の比較 */
                    {
                        int offsetComa = GetPosFromOffset(pStopPos, (int)showC);
                        var ReelData = mainData.ReelArray[stopReel][offsetComa];
                        bool[] compData = new bool[] { ReelData.Ex10, ReelData.Ex11, ReelData.Ex12, ReelData.Ex13 };

                        const uint combOffset = LocalDataSet.SYMBOL_MAX;
                        for (uint i = 0; i < compData.Length; ++i)
                            JudgeReel |= comb.Combination.GetData(i + combOffset) > 0 && compData[i];
                    }
                }
            }
            
            // 論理反転処理
            if (pCompData.IsInvert) JudgeReel = !JudgeReel;
            return JudgeReel;
        }

        // 指定箇所のリール制御結果をすべりコマで返す(エラー：-1)
        public int GetReelControl3R(int pStopReelID, int[] pStopPos, int p1stStopID, int p1stSlipNum, int pBetNum, int pMode, int pBonusFlag, int pCastFlag)
        {
            // 停止リールのpStopPosがデータ範囲外の場合は判定を行わない
            if (pStopReelID < 0 || pStopReelID >= LocalDataSet.REEL_MAX) return -1;
            if (pStopPos[pStopReelID] < 0 || pStopPos[pStopReelID] >= LocalDataSet.COMA_MAX) return -1;

            // MainROM取得
            var mainROM = MainROMDataManagerSingleton.GetInstance();

            // ctrlデータ分ループさせる
            foreach (var ctrl in mainROM.ReelCtrlData)
            {
                // 条件判定
                if (ctrl.BetNum != pBetNum) continue;
                if (ctrl.GameMode != pMode) continue;
                if (ctrl.BonusFlag != pBonusFlag) continue;
                if (ctrl.CastFlag != pCastFlag) continue;

                // 制御データの抽出を行う(データがなければデフォルト値処理を行う)
                LocalDataSet.ReelControlElem3Reels useData = null;
                foreach (var useCheck in ctrl.ElemData)
                {
                    if (useCheck.DefinePos == p1stStopID) { useData = useCheck; break; }
                }
                if (useData == null) useData = new LocalDataSet.ReelControlElem3Reels();

                // 各種テーブルを取得すべきデータを算出する。
                int stopReelNum = -1;   // 停止中リール数(priority用)
                int priIndex = 0;       // priority停止順index
                int slipIndex = 0;      // slipData

                foreach (var data in pStopPos) {
                    // リールが回転中なら処理を行わない
                    if (data < 0 || data >= LocalDataSet.COMA_MAX) continue;
                    ++stopReelNum;  // 停止中リール数加算
                }

                /* slipTableによるindex設定 */
                {
                    int slipLoopNum = 0;
                    if (stopReelNum == 1)
                    {   // 2nd停止時の処理
                        if (useData.Exist2ndSlip.Export() != 0)
                        {   // 個別定義の場合に調整を行う
                            // 1stスベリが無効な場合はエラーを返す
                            if (p1stSlipNum < 0 || p1stSlipNum >= LocalDataSet.SLIP_MAX) return -1;
                            slipLoopNum = p1stSlipNum;
                        }
                    }
                    else if (stopReelNum == 2)
                    {   // 3rd停止時、すべりコマを個別設定する場合の処置
                        if (useData.Exist3rdSlip)
                        {   // 定義あり
                            if (useData.Exist2ndSlip.Export() == 0) slipIndex += LocalDataSet.REEL_MAX - 1;
                            else slipLoopNum = LocalDataSet.SLIP_MAX;
                        }
                        else slipIndex = -1;
                    }
                    for(uint i=0; i<slipLoopNum && slipIndex >= 0; ++i)
                    {
                        if (useData.Exist2ndSlip.GetData(i) == 0) continue;
                        slipIndex += LocalDataSet.REEL_MAX - 1;
                    }
                }

                /* 停止リールによるindex設定 */
                {
                    int stop1st = p1stStopID / LocalDataSet.COMA_MAX;
                    // 停止リール検索によるindexオフセット
                    for (int reelC = 0; reelC < pStopReelID; ++reelC)
                    {
                        if (reelC == stop1st) continue;     // 第一停止リールなら処理スキップ
                        ++priIndex;
                        if (slipIndex >= 0) ++slipIndex;    // すべり定義使用なし時は処理スキップ
                    }
                    // 3rdのみpriIndexを反転させる
                    if (stopReelNum == 2) priIndex = 1 - priIndex;

                    // 停止リール数によるindex操作
                    if (stopReelNum < 0) return -1; // 停止リールが存在しない(エラー処理)
                    if (stopReelNum > 0) priIndex += (LocalDataSet.REEL_MAX - 1) * stop1st;
                }

                // テーブル取得(index範囲外の場合は初期値を入れる)
                var slipElemData = (slipIndex < 0 || slipIndex >= useData.SlipElem.Count) ? 
                    new LocalDataSet.ControlSlipElem() : useData.SlipElem[slipIndex];
                var priorityData = mainROM.CombiPriorityData[ctrl.CombiPriority];
                bool isPriority = priorityData.PriData[stopReelNum].GetData((uint)priIndex) > 0;

                // すべりコマ数分の停止可否判定データを取得する
                GetCastResult[] posJudge = new GetCastResult[LocalDataSet.SLIP_MAX];
                for (int slipC=0; slipC<posJudge.Length; ++slipC)
                {
                    // CT中の規定すべり数を超える場合処理を行わない
                    if (pCastFlag > 0)
                    {
                        if (mainROM.FlagElemData[pCastFlag - 1].ControlCT == pStopReelID + 1 && slipC >= LocalDataSet.SLIP_CT) {
                            posJudge[slipC] = null; continue; 
                        }
                    }

                    int[] judgeReelPos = new int[pStopPos.Length];
                    for (int reelC=0; reelC<pStopPos.Length; ++reelC)
                    {
                        judgeReelPos[reelC] = pStopPos[reelC];
                        if (reelC == pStopReelID) judgeReelPos[reelC] = (judgeReelPos[reelC] + slipC) % LocalDataSet.COMA_MAX;
                    }
                    posJudge[slipC] = GetCast(judgeReelPos, pBetNum, (uint)pMode, pCastFlag, pBonusFlag);
                }

                // step1: 停止可否のみ参照しすべりコマに優先順位をつける。
                // 併せて優先レベルと組み合わせ数の最大値を取得する
                List<int> slipPri_Step1 = new List<int>();
                ushort payPriMax = 0;
                int payNumMax = int.MinValue;
                uint combiMax = 0;              // 全停止形の最大組合せ
                int combiPayMax = int.MinValue; // 最大組合せ時の最大配当

                for (int slipC = 0; slipC < LocalDataSet.SLIP_MAX; ++slipC)
                {
                    // 判定すべり数制定
                    int checkSlip = (int)mainROM.ReelSlipData[slipElemData.TableID].Table.GetData((uint)pStopPos[pStopReelID]);
                    checkSlip = (checkSlip + slipC * slipElemData.SlipIncrement) % LocalDataSet.SLIP_MAX;

                    // CTなどで判定結果がnullの場合は処理を行わない
                    if (posJudge[checkSlip] == null) continue;

                    // 停止可能な判断であればslipPriにデータを追加してレベルの最大値を更新する
                    if (!posJudge[checkSlip].stopAvailable) continue;
                    slipPri_Step1.Add(checkSlip);
                    payPriMax = Math.Max(payPriMax, posJudge[checkSlip].payPriority);
                    payNumMax = Math.Max(payNumMax, posJudge[checkSlip].payoutNum);
                    combiMax = Math.Max(combiMax, posJudge[checkSlip].castPattern);
                }
                // step1の段階で停止可能な位置がない場合は-1を返す
                if (slipPri_Step1.Count == 0) return -1;

                // combiPriMax抽出
                foreach (var item in slipPri_Step1)
                {
                    if (posJudge[item].castPattern == combiMax)
                        combiPayMax = Math.Max(combiPayMax, posJudge[item].payoutNum);
                }

                // step2: 停止優先度が最も高いデータのみを抽出する
                List<int> slipPri_Step2 = new List<int>();
                foreach (var item in slipPri_Step1)
                {
                    // 回転中リールがない場合(payoutNum >= 0)、最大払出配当が揃えられ、配当が最大となるものを優先
                    bool addFlag = posJudge[item].payoutNum >= 0 && posJudge[item].payPriority == payPriMax && posJudge[item].payoutNum == payNumMax;
                    // 回転中リールがある場合、最大払出配当が揃えられるものを優先
                    addFlag |= posJudge[item].payoutNum < 0 && posJudge[item].payPriority == payPriMax;
                    // 組合せ優先許可が出ている場合、組合せ数が最大かつその中で配当が最大となる物も許容
                    addFlag |= posJudge[item].castPattern == combiMax && posJudge[item].payoutNum == combiPayMax && isPriority;
                    
                    if (addFlag) slipPri_Step2.Add(item);
                }

                // step3: 回避データに合致しないものを抽出する。ただし「引込」に合致した場合データを頭に追加する
                List<int> slipPri_Step3 = new List<int>();
                {
                    var avoidNum = useData.AvoidPosDataCount;
                    int priPos = 0;         // 優先格納先

                    foreach (var item in slipPri_Step2)
                    {
                        bool passFlag = true;
                        bool priFlag = false;   // 優先格納フラグ
                        // データ数分ループする
                        for (int avoidC=0; avoidC<useData.AvoidPos.Count; ++avoidC)
                        {
                            // 押し順によってスキップ処理を行う(ゴリゴリのマジックナンバーだがとりあえず実装…)
                            if (stopReelNum >= 1 && avoidC >= avoidNum[0] + avoidNum[1] + avoidNum[2]) break;
                            if (stopReelNum == 1 && avoidC >= avoidNum[0] + avoidNum[1]) break;
                            if (stopReelNum == 2 && avoidC >= avoidNum[0] && avoidC < avoidNum[0] + avoidNum[1]) continue;

                            // 判定を行う
                            var avoidData = useData.AvoidPos[avoidC].PosData[pStopReelID];
                            if (GetMatchStopPos(avoidData, pStopPos[pStopReelID] + item, pStopReelID))
                            {
                                priFlag = avoidData.JudgeReel >= LocalDataSet.REEL_MAX;
                                passFlag = priFlag;
                                break;
                            }
                        }
                        if (passFlag)
                        {
                            if (priFlag) { slipPri_Step3.Insert(priPos, item); ++priPos; }
                            else slipPri_Step3.Add(item);
                        }
                    }
                }
                if (slipPri_Step3.Count == 0) return slipPri_Step2[0];

                // step4: リーチ目に合致するものを抽選して返す
                int ansMemo = -1;
                int[] reachPos = new int[pStopPos.Length];
                foreach(var item in slipPri_Step3)
                {
                    for (int reelC = 0; reelC < pStopPos.Length; ++reelC)
                    {
                        reachPos[reelC] = pStopPos[reelC];
                        if (reelC == pStopReelID) reachPos[reelC] = (reachPos[reelC] + item) % LocalDataSet.COMA_MAX;
                    }
                    int reachLevel = GetReachLevel(p1stStopID, reachPos);

                    if (reachLevel > 0)
                    {
                        uint checkIndex = (uint)reachLevel - 1;
                        if (ctrl.ReachAvail.GetData(checkIndex) == 0) continue;
                        if (ctrl.ReachPri.GetData(checkIndex) > 0) return item;
                        if (ctrl.ReachAvoid.GetData(checkIndex) > 0 || ansMemo == -1) ansMemo = item;
                    }
                    else if (ansMemo == -1) ansMemo = item;
                }

                if (ansMemo == -1) ansMemo = slipPri_Step3[0];
                return ansMemo;
            }

            return -1;
        }
    }
}

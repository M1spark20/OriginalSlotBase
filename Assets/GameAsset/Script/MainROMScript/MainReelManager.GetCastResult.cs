using System.Collections.Generic;

namespace SlotMaker2022.main_function
{
    public partial class MainReelManager
    {
        public class GetCastResult
        {
            public uint castPattern;    // 組み合わせ可能フラグ数
            public int  payoutNum;      // 全停止時は総獲得枚数, それ以外は0
            public uint maximumPayCast; // 1配当の最大払い出し枚数
            public bool getBonus;       // ボーナスを揃えられるか
            public bool canLose;        // 取りこぼしがあるか(全停止時はtrueとする)
            public ushort payPriority;  // 停止位置優先度
            public bool stopAvailable;  // 当該位置が停止可能か

            /* PayPriorityの値
             * [ボーナス優先制御の場合]
             * ushort.MaxValue: リプレイ
             * 15    : ボーナス
             * 14～00: 小役(15枚～1枚、最大払い出し枚数のみ)
             * [小役優先制御の場合]
             * ushort.MaxValue: リプレイ
             * 15～01: 小役(15枚～1枚、最大払い出し枚数のみ)
             * 00    : ボーナス
             */
            public List<LocalDataSet.CastElemData> matchCast;  // 入賞可能配当データ一覧
            public List<int>                       payLine;    // 入賞ライン

            public GetCastResult()
            {
                castPattern = 0;
                payoutNum = -1;
                maximumPayCast = 0;
                getBonus = false;
                canLose = true;
                payPriority = 0;
                stopAvailable = false;
                matchCast = new List<LocalDataSet.CastElemData>();
                payLine = new List<int>();
            }
        }
    }
}

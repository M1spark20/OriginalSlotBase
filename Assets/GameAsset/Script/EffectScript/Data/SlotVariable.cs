using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SlotEffectMaker2023.Data
{
	// 計算式オペランド
	public enum ECalcOperand { eAdd, eSub, eMul, eDiv, eMod, eNone }

	public class SlotVariable : IEffectNameInterface
	{	// 変数要素データ(Sys/Sav)(valは直接アクセスすることが可能)
		public string name { get; set; } // 変数名
		public int val { get; set; }     // 変数値(Sysでは初期値)
		public string usage { get; set; }// 用途(メモのみ)

		// 読み込み関連処理
		// データを外部から読み込んだ時のコンストラクタ: ReadData関数を直接動かしてデータを読む必要あり
		public SlotVariable() { name = string.Empty; val = 0; usage = string.Empty; }
		public SlotVariable(string pValName)
		{
			// 変数を新規に作成するときのコンストラクタ: 変数を指定して新規作成する。
			// 呼び出し前に変数名が重複しないことを確認すること
			name = pValName;
			val = 0;
			usage = string.Empty;
		}
		public SlotVariable(string pValName, int pInitVal)
		{
			// 変数を新規に作成するときのコンストラクタ: 変数を指定して新規作成する。初期値を入力できる。
			// 呼び出し前に変数名が重複しないことを確認すること
			name = pValName;
			val = pInitVal;
			usage = string.Empty;
		}
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			fs.Write(name);
			fs.Write(val);
			fs.Write(usage);
			return true;
		}
		public bool ReadData(ref BinaryReader fs, int version)
		{
			name = fs.ReadString();
			val = fs.ReadInt32();
			usage = fs.ReadString();
			return true;
		}
		public void Rename(EChangeNameType type, string src, string dst) { }

		// valの高度計算あれこれ
		// bool値設定
		public void SetBool(bool flag) { val = flag ? 1 : 0; }
		public bool GetBool() { return val != 0; }
		// valが指定された範囲にいるか確認。inclBorder=trueの時は=も許容する
		public bool CheckRange(int min, int max, bool inclBorder)
		{
			if (inclBorder && (val == min || val == max)) return true;
			return val > min && val < max;
		}
	}

	// 変数一覧管理クラス(Sys)
	public class VarList : IEffectNameInterface
	{
		// 生成変数一覧
		public List<SlotVariable> VData { get; set; }
		public VarList()
        {
            VData = new List<SlotVariable>
            {
                // システムデータ入力
				new SlotVariable("_slotSetting", 0),
				new SlotVariable("_inCount", 0),
				new SlotVariable("_outCount", 0),
                new SlotVariable("_betCount", 0),
                new SlotVariable("_creditCount", 0),
                new SlotVariable("_payoutCount", 0),
                new SlotVariable("_isBetLatched", 0),
                new SlotVariable("_isReplay", 0),
                new SlotVariable("_gameMode", 0),
                new SlotVariable("_modeGameCount", 0),
                new SlotVariable("_modeJacCount", 0),
                new SlotVariable("_modeMedalCount", 0),
                new SlotVariable("_RTMode", 0),
                new SlotVariable("_RTOverride", 0),
                new SlotVariable("_RTGameCount", 0),
				new SlotVariable("_flagID", 0),
				new SlotVariable("_bonusID", 0),
				new SlotVariable("_castBonusID", 0),
				new SlotVariable("_payLine", 0),
            };
			// リール関係システムデータ
			for (int i=0; i<SlotMaker2022.LocalDataSet.REEL_MAX; ++i)
            {
				VData.Add(new SlotVariable("_reelPushPos[" + i + "]", -1));
				VData.Add(new SlotVariable("_reelStopPos[" + i + "]", -1));
				VData.Add(new SlotVariable("_reelStopOrder[" + i + "]", -1));
            }
        }
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			// ユーザタイマ(_から始まらないデータ)のみ保存
			int dataCount = 0;
			foreach (var item in VData)
				if (!item.name.StartsWith("_")) ++dataCount;
			// 保存処理
			fs.Write(dataCount);
			foreach (var item in VData)
			{
				if (item.name.StartsWith("_")) continue;
				item.StoreData(ref fs, version);
			}
			return true;
		}
		public bool ReadData(ref BinaryReader fs, int version)
		{
			int dataCount = fs.ReadInt32();
			for (int i = 0; i < dataCount; ++i)
			{
				SlotVariable sv = new SlotVariable();
				sv.ReadData(ref fs, version);
				VData.Add(sv);
			}
			return true;
		}
		public void Rename(EChangeNameType type, string src, string dst)
        {
			foreach (var item in VData) item.Rename(type, src, dst);
        }

		public SlotVariable GetData(string pName)
        {
			foreach(var item in VData)
            {
				if (item.name.Equals(pName)) return item;
			}
			return null;
        }
		public string[] GetVariableNameList()
        {
			string[] ans = new string[VData.Count];
			for (int i = 0; i < ans.Length; ++i) ans[i] = VData[i].name;
			return ans;
        }
		public string[] GetUserVariableNameList()
        {
			List<string> ans = new List<string>();
			for (int i = 0; i < VData.Count; ++i)
			{
				if (VData[i].name.StartsWith("_")) continue;
				ans.Add(VData[i].name);
			}
			return ans.ToArray();

        }
	}
}
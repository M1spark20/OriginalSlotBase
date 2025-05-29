using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SlotEffectMaker2023.Data
{
	/// <summary>
	/// 算術演算子の種類を表す列挙型です。
	/// </summary>
	public enum ECalcOperand
	{
		/// <summary>加算</summary>
		eAdd,
		/// <summary>減算</summary>
		eSub,
		/// <summary>乗算</summary>
		eMul,
		/// <summary>除算</summary>
		eDiv,
		/// <summary>剰余</summary>
		eMod,
		/// <summary>演算なし／初期値</summary>
		eNone
	}

	/// <summary>
	/// 変数要素データを管理するクラスです。（Sys／Sav）
	/// <para>変数値は直接アクセス可能です。</para>
	/// </summary>
	public class SlotVariable : IEffectNameInterface
	{
		/// <summary>変数名</summary>
		public string name { get; set; } // 変数名

		/// <summary>変数値（Sysでは初期値）</summary>
		public int val { get; set; }     // 変数値(Sysでは初期値)

		/// <summary>用途（メモのみ）</summary>
		public string usage { get; set; }// 用途(メモのみ)

		/// <summary>
		/// デフォルトコンストラクタ。プロパティを初期化します。
		/// </summary>
		public SlotVariable()
		{
			name = string.Empty;
			val = 0;
			usage = string.Empty;
		}

		/// <summary>
		/// 変数名を指定して新規作成するコンストラクタ。
		/// </summary>
		/// <param name="pValName">新規変数の名前（重複しないこと）</param>
		public SlotVariable(string pValName)
		{
			// 変数を新規に作成するときのコンストラクタ: 変数を指定して新規作成する。
			// 呼び出し前に変数名が重複しないことを確認すること
			name = pValName;
			val = 0;
			usage = string.Empty;
		}

		/// <summary>
		/// 変数名と初期値を指定して新規作成するコンストラクタ。
		/// </summary>
		/// <param name="pValName">新規変数の名前（重複しないこと）</param>
		/// <param name="pInitVal">初期値</param>
		public SlotVariable(string pValName, int pInitVal)
		{
			// 変数を新規に作成するときのコンストラクタ: 変数を指定して新規作成する。初期値を入力できる。
			// 呼び出し前に変数名が重複しないことを確認すること
			name = pValName;
			val = pInitVal;
			usage = string.Empty;
		}

		/// <summary>
		/// コピーコンストラクタ。
		/// </summary>
		/// <param name="ins">コピー元の SlotVariable</param>
		public SlotVariable(SlotVariable ins)
		{
			name = ins.name;
			val = ins.val;
			usage = ins.usage;
		}

		/// <summary>
		/// このインスタンスをバイナリに書き込みます。
		/// </summary>
		/// <param name="fs">書き込み先の BinaryWriter（ref）</param>
		/// <param name="version">データのバージョン</param>
		/// <returns>書き込み成功時に true を返します。</returns>
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			fs.Write(name);
			fs.Write(val);
			fs.Write(usage);
			return true;
		}

		/// <summary>
		/// バイナリからこのインスタンスを読み込みます。
		/// </summary>
		/// <param name="fs">読み込み元の BinaryReader（ref）</param>
		/// <param name="version">データのバージョン</param>
		/// <returns>読み込み成功時に true を返します。</returns>
		public bool ReadData(ref BinaryReader fs, int version)
		{
			name = fs.ReadString();
			val = fs.ReadInt32();
			usage = fs.ReadString();
			return true;
		}

		/// <summary>
		/// 名前変更時に呼び出されますが、本クラスでは特に処理を行いません。
		/// </summary>
		public void Rename(EChangeNameType type, string src, string dst)
		{
			// 処理なし
		}

		/// <summary>
		/// bool 値を設定します。true → 1、false → 0。
		/// </summary>
		/// <param name="flag">設定する bool 値</param>
		public void SetBool(bool flag) { val = flag ? 1 : 0; }

		/// <summary>
		/// 現在の val を bool として取得します。
		/// </summary>
		/// <returns>0 以外なら true、それ以外は false</returns>
		public bool GetBool() { return val != 0; }

		/// <summary>
		/// val が指定範囲内かを判定します。
		/// </summary>
		/// <param name="min">範囲の最小値</param>
		/// <param name="max">範囲の最大値</param>
		/// <param name="inclBorder">境界値を含む場合は true</param>
		/// <returns>範囲内なら true</returns>
		public bool CheckRange(int min, int max, bool inclBorder)
		{
			// valが指定された範囲にいるか確認。inclBorder=trueの時は=も許容する
			if (inclBorder && (val == min || val == max)) return true;
			return val > min && val < max;
		}

		/// <summary>
		/// offset を加味して val が指定範囲内かを判定します。
		/// </summary>
		/// <param name="min">範囲の最小値</param>
		/// <param name="max">範囲の最大値</param>
		/// <param name="inclBorder">境界値を含む場合は true</param>
		/// <param name="offset">val に加算するオフセット値</param>
		/// <returns>範囲内なら true</returns>
		public bool CheckRange(int min, int max, bool inclBorder, int offset)
		{
			// 20241018追加：offsetつき判定
			int ev = val + offset;
			if (inclBorder && (ev == min || ev == max)) return true;
			return ev > min && ev < max;
		}
	}

	/// <summary>
	/// 変数一覧を管理するクラスです。（Sys）
	/// </summary>
	public class VarList : IEffectNameInterface
	{
		/// <summary>生成された変数のリスト</summary>
		public List<SlotVariable> VData { get; set; }

		/// <summary>
		/// デフォルトコンストラクタ。初期のシステム変数を生成します。
		/// </summary>
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
				new SlotVariable("_unlockColleNum", 0),
			};
			// リール関係システムデータ
			for (int i = 0; i < SlotMaker2022.LocalDataSet.REEL_MAX; ++i)
			{
				VData.Add(new SlotVariable("_reelPushPos[" + i + "]", -1));
				VData.Add(new SlotVariable("_reelStopPos[" + i + "]", -1));
				VData.Add(new SlotVariable("_reelStopOrder[" + i + "]", -1));
			}
			// 出目コレ達成数データ(ID:0-4, Lv:1-5)
			for (int i = 0; i < CollectionDataElem.COLLECTION_LEVEL_MAX; ++i)
				VData.Add(new SlotVariable("_unlockColleNumLv[" + i + "]", 0));
		}

		/// <summary>
		/// ユーザ変数のみをバイナリに書き込みます（名前が '_' で始まらないもの）。
		/// </summary>
		/// <param name="fs">書き込み先の BinaryWriter（ref）</param>
		/// <param name="version">データのバージョン</param>
		/// <returns>書き込み成功時に true を返します。</returns>
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			// ユーザタイマ(_から始まらないデータ)のみ保存
			int dataCount = 0;
			foreach (var item in VData)
				if (!item.name.StartsWith("_")) ++dataCount;

			fs.Write(dataCount);
			foreach (var item in VData)
			{
				if (item.name.StartsWith("_")) continue;
				item.StoreData(ref fs, version);
			}
			return true;
		}

		/// <summary>
		/// バイナリからユーザ変数を読み込み、リストに追加します。
		/// </summary>
		/// <param name="fs">読み込み元の BinaryReader（ref）</param>
		/// <param name="version">データのバージョン</param>
		/// <returns>読み込み成功時に true を返します。</returns>
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

		/// <summary>
		/// 名前変更時にリスト内すべての Rename を呼び出します。
		/// </summary>
		/// <param name="type">変更の種類</param>
		/// <param name="src">元の名前</param>
		/// <param name="dst">新しい名前</param>
		public void Rename(EChangeNameType type, string src, string dst)
		{
			foreach (var item in VData) item.Rename(type, src, dst);
		}

		/// <summary>
		/// 名前で変数を検索して取得します。
		/// </summary>
		/// <param name="pName">検索する変数名</param>
		/// <returns>見つかった場合は SlotVariable、存在しない場合は null を返します。</returns>
		public SlotVariable GetData(string pName)
		{
			foreach (var item in VData)
			{
				if (item.name.Equals(pName)) return item;
			}
			return null;
		}

		/// <summary>
		/// すべての変数名を配列で取得します。
		/// </summary>
		/// <returns>変数名の配列</returns>
		public string[] GetVariableNameList()
		{
			string[] ans = new string[VData.Count];
			for (int i = 0; i < ans.Length; ++i) ans[i] = VData[i].name;
			return ans;
		}

		/// <summary>
		/// ユーザ変数（名前が '_' で始まらないもの）のみ変数名を配列で取得します。
		/// </summary>
		/// <returns>ユーザ変数名の配列</returns>
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
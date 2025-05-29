using System.Collections;
using System.Collections.Generic;
using SlotEffectMaker2023.Data;
using System.IO;

namespace SlotEffectMaker2023.Action
{
	/// <summary>
	/// ゲーム内で使用される変数を管理し、セーブ/ロード機能を提供するクラス。
	/// </summary>
	public class SlotValManager : SlotMaker2022.ILocalDataInterface
	{
		// ゲームで使用される変数の管理を行う(Sav)
		// 変数
		private List<SlotVariable> valData;
		// Resume用変数
		private List<string> resVarName;
		private List<int> resVarValue;

		/// <summary>
		/// コンストラクタ。内部リストを初期化します。
		/// </summary>
		public SlotValManager()
		{
			valData = new List<SlotVariable>();
			resVarName = new List<string>();
			resVarValue = new List<int>();
		}

		/// <summary>
		/// 変数リストを初期化し、保存データからの復帰値を適用します。
		/// </summary>
		/// <param name="vl">初期変数定義が格納された VarList</param>
		public void Init(VarList vl)
		{
			// データを作成する
			foreach (var item in vl.VData)
			{
				CreateVariable(item);
			}
			// 読み込んだ初期値を設定する
			int dataNum = resVarName.Count;
			for (int i = 0; i < dataNum; ++i)
			{
				var data = GetVariable(resVarName[i]);
				if (data == null) continue;
				data.val = resVarValue[i];
			}
		}

		/// <summary>
		/// 現在の変数値をバイナリ形式で保存します。
		/// </summary>
		/// <param name="fs">BinaryWriter の参照</param>
		/// <param name="version">保存バージョン</param>
		/// <returns>保存に成功したか（常に true）</returns>
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			int dataNum = valData.Count;
			fs.Write(dataNum);
			for (int i = 0; i < dataNum; ++i)
			{
				fs.Write(valData[i].name);
				fs.Write(valData[i].val);
			}
			return true;
		}

		/// <summary>
		/// バイナリ形式から変数値を読み込み、復帰リストに保存します。
		/// </summary>
		/// <param name="fs">BinaryReader の参照</param>
		/// <param name="version">保存バージョン</param>
		/// <returns>読み込みに成功したか（常に true）</returns>
		public bool ReadData(ref BinaryReader fs, int version)
		{
			// 読み込み処理(あとで実装)
			int dataNum = fs.ReadInt32();
			for (int i = 0; i < dataNum; ++i)
			{
				resVarName.Add(fs.ReadString());
				resVarValue.Add(fs.ReadInt32());
			}
			return true;
		}

		/// <summary>
		/// 新しい変数を作成します。名前の重複がある場合は追加されません。
		/// </summary>
		/// <param name="sv">作成する変数情報が格納された SlotVariable</param>
		/// <returns>変数を新規追加したか</returns>
		public bool CreateVariable(SlotVariable sv)
		{
			for (int i = 0; i < valData.Count; ++i)
			{
				if (valData[i].name.Equals(sv.name)) return false;
			}
			valData.Add(new SlotVariable(sv));
			return true;
		}

		/// <summary>
		/// 指定した名前の変数を取得します。
		/// </summary>
		/// <param name="pValName">取得する変数名</param>
		/// <returns>該当する SlotVariable、存在しない場合は null</returns>
		public SlotVariable GetVariable(string pValName)
		{
			for (int i = 0; i < valData.Count; ++i)
			{
				if (valData[i].name == pValName) return valData[i];
			}
			return null;
		}
	}
}

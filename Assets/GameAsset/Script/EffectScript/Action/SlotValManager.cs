using System.Collections;
using System.Collections.Generic;
using SlotEffectMaker2023.Data;

namespace SlotEffectMaker2023.Action
{
	public class SlotValManager
	{	// ゲームで使用される変数の管理を行う(Sav)
		// 変数
		List<SlotVariable> valData;

		/// <summary>
		/// インスタンスの初期化を行います。
		/// </summary>
		public SlotValManager()
		{
			valData = new List<SlotVariable>();
		}
		// valDataの初期化(変数データ作成)を行う
		public void Init(VarList vl)
		{
			foreach (var item in vl.VData) CreateVariable(item);
		}
		// valDataの読み込みを行う(セーブデータ)
		public bool ReadData()
		{
			// 読み込み処理(あとで実装)

			return true;
		}

		// 名前に重複がないことを確認して変数を新規作成する。
		// [ret]変数を追加したか
		public bool CreateVariable(SlotVariable sv)
		{
			for (int i = 0; i < valData.Count; ++i)
			{
				if (valData[i].name.Equals(sv.name)) return false;
			}
			valData.Add(sv);
			return true;
		}
		// 名前に一致した変数を取得する
		// [ret]タイマのインスタンス, 見つからない場合はnull
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
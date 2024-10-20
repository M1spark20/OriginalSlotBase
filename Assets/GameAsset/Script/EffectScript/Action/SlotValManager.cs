using System.Collections;
using System.Collections.Generic;
using SlotEffectMaker2023.Data;
using System.IO;

namespace SlotEffectMaker2023.Action
{
	public class SlotValManager : SlotMaker2022.ILocalDataInterface
	{	// ゲームで使用される変数の管理を行う(Sav)
		// 変数
		List<SlotVariable> valData;
		// Resume用変数
		private List<string> resVarName;
		private List<int> resVarValue;

		/// <summary>
		/// インスタンスの初期化を行います。
		/// </summary>
		public SlotValManager()
		{
			valData = new List<SlotVariable>();
			resVarName = new List<string>();
			resVarValue = new List<int>();
		}
		// valDataの初期化(変数データ作成)を行い、初期値を設定する
		public void Init(VarList vl)
		{
			// データを作成する
			foreach (var item in vl.VData) CreateVariable(item);
			// 読み込んだ初期値を設定する
			int dataNum = resVarName.Count;
			for (int i = 0; i < dataNum; ++i)
            {
				var data = GetVariable(resVarName[i]);
				if (data == null) continue;
				data.val = resVarValue[i];
            }
		}
		// 変数の書き出しを行う(Systemは重複となるが初期化タイミングを考えたくないので登録する)
		public bool StoreData(ref BinaryWriter fs, int version)
        {
			int dataNum = valData.Count;
			fs.Write(dataNum);
			for (int i=0; i<dataNum; ++i)
            {
				fs.Write(valData[i].name);
				fs.Write(valData[i].val);
            }
			return true;
        }
		// valDataの読み込みを行う(セーブデータ)
		public bool ReadData(ref BinaryReader fs, int version)
		{
			// 読み込み処理(あとで実装)
			int dataNum = fs.ReadInt32();
			for (int i=0; i<dataNum; ++i)
            {
				resVarName.Add(fs.ReadString());
				resVarValue.Add(fs.ReadInt32());
            }
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
			valData.Add(new SlotVariable(sv));
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
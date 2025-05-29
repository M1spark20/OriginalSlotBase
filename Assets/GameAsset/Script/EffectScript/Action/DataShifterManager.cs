using System.Collections.Generic;
using System.IO;
using SlotEffectMaker2023.Data;

namespace SlotEffectMaker2023.Action
{
	/// <summary>
	/// 汎用データシフターマネージャ。
	/// DataShifterBase を継承したデータの読み書きおよびエレメント変更を管理します。
	/// </summary>
	/// <typeparam name="T">DataShifterBase を継承したデータ型</typeparam>
	public class DataShifterManager<T> : SlotMaker2022.ILocalDataInterface where T : DataShifterBase
	{
		/// <summary>
		/// シフター名と要素名のタプルリスト
		/// </summary>
		private List<(string shifterName, string elemName)> SoundData { get; set; }

		/// <summary>
		/// コンストラクタ。内部リストを初期化します。
		/// </summary>
		public DataShifterManager()
		{
			SoundData = new List<(string, string)>();
		}

		/// <summary>
		/// 初期データを受け取り、シフターデータを構築します。
		/// </summary>
		/// <param name="pShiftData">初期化用のデータリスト</param>
		public void Init(List<T> pShiftData)
		{
			// 最初に鳴り分け要素を作成しておく
			foreach (var item in pShiftData)
			{
				SoundData.Add((item.ShifterName, item.DefaultElemID));
			}
		}

		/// <summary>
		/// 現在のシフター状態をバイナリ形式で保存します。
		/// </summary>
		/// <param name="fs">BinaryWriter の参照</param>
		/// <param name="version">保存バージョン</param>
		/// <returns>保存に成功したか（常に true）</returns>
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			fs.Write(SoundData.Count);
			for (int i = 0; i < SoundData.Count; ++i)
			{
				fs.Write(SoundData[i].shifterName);
				fs.Write(SoundData[i].elemName);
			}
			return true;
		}

		/// <summary>
		/// バイナリ形式のシフター状態を読み込みます。
		/// </summary>
		/// <param name="fs">BinaryReader の参照</param>
		/// <param name="version">保存バージョン</param>
		/// <returns>読み込みに成功したか（常に true）</returns>
		public bool ReadData(ref BinaryReader fs, int version)
		{
			int dataSize = fs.ReadInt32();
			for (int i = 0; i < dataSize; ++i)
			{
				(string shifterName, string elemName) newData;
				newData.shifterName = fs.ReadString();
				newData.elemName = fs.ReadString();
				SoundData.Add(newData);
			}
			return true;
		}

		/// <summary>
		/// 指定シフターの要素を変更します。
		/// </summary>
		/// <param name="pPlayerID">シフターの識別子</param>
		/// <param name="pSoundID">変更後の要素名</param>
		public void ChangeElem(string pPlayerID, string pSoundID)
		{
			// データ編集用関数
			for (int i = 0; i < SoundData.Count; ++i)
			{
				if (SoundData[i].shifterName == pPlayerID)
				{
					SoundData[i] = (pPlayerID, pSoundID);
					return;
				}
			}
			// データがない場合は新規追加
			SoundData.Add((pPlayerID, pSoundID));
		}

		/// <summary>
		/// 指定シフターの要素名をエクスポートします。
		/// </summary>
		/// <param name="pPlayerID">シフターの識別子</param>
		/// <returns>要素名。存在しない場合は null。</returns>
		public string ExportElemName(string pPlayerID)
		{
			// Unityへのデータ出力
			for (int i = 0; i < SoundData.Count; ++i)
			{
				if (SoundData[i].shifterName == pPlayerID)
				{
					return SoundData[i].elemName;
				}
			}
			return null;
		}
	}
}

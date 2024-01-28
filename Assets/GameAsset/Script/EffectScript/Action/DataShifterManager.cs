using System.Collections.Generic;
using System.IO;
using SlotEffectMaker2023.Data;

namespace SlotEffectMaker2023.Action
{
    // データシフト管理クラス(Sav)
    public class DataShifterManager<T> : SlotMaker2022.ILocalDataInterface where T : DataShifterBase
	{
		// 変数
		List<(string shifterName, string elemName)> SoundData { get; set; }	// 鳴り分けデータ(PlayList, IDList)

		public DataShifterManager()
		{
			SoundData = new List<(string, string)>();
		}
		// 最初に鳴り分け要素を作成しておく
		public void Init(List<T> pShiftData)
        {
			foreach (var item in pShiftData)
				SoundData.Add( (item.ShifterName, item.DefaultElemID) );
		}
		// 現在の鳴り分け状況を保存する
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

		// データ編集用関数
		public void ChangeElem(string pPlayerID, string pSoundID)
		{
			for (int i = 0; i < SoundData.Count; ++i)
			{
                if (SoundData[i].shifterName == pPlayerID) { SoundData[i] = (pPlayerID, pSoundID); return; }
			}
			// データがない場合の追加
			SoundData.Add( (pPlayerID, pSoundID) );
		}

		// Unityへのデータ出力
		public string ExportElemName(string pPlayerID)
        {
			for (int i = 0; i < SoundData.Count; ++i)
			{
                if (SoundData[i].shifterName == pPlayerID) { return SoundData[i].elemName; }
			}
			return null;
        }
	}
}
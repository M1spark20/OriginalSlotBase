using System.Collections.Generic;
using System.IO;
using SlotEffectMaker2023.Data;

namespace SlotEffectMaker2023.Action
{
    // サウンドデータ管理クラス(Sav)
    public class SoundDataManager : SlotMaker2022.ILocalDataInterface
	{
		// 変数
		List<(string playerName, string soundName)> SoundData { get; set; }	// 鳴り分けデータ(PlayList, IDList)

		public SoundDataManager()
		{
			SoundData = new List<(string, string)>();
		}
		// 最初に鳴り分け要素を作成しておく
		public void Init(List<Data.SoundPlayData> pPlayData)
        {
			foreach (var item in pPlayData)
				SoundData.Add( (item.PlayerName, item.DefaultSoundID) );
		}
		// 現在の鳴り分け状況を保存する
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			fs.Write(SoundData.Count);
			for (int i = 0; i < SoundData.Count; ++i)
            {
				fs.Write(SoundData[i].playerName);
				fs.Write(SoundData[i].soundName);
            }
			return true;
		}
		public bool ReadData(ref BinaryReader fs, int version)
		{
			int dataSize = fs.ReadInt32();
			for (int i = 0; i < dataSize; ++i)
			{
				(string playerName, string soundName) newData;
				newData.playerName = fs.ReadString();
				newData.soundName = fs.ReadString();
				SoundData.Add(newData);
			}
			return true;
		}

		// データ編集用関数
		public void ChangeSoundID(string pPlayerID, string pSoundID)
		{
			for (int i = 0; i < SoundData.Count; ++i)
			{
                if (SoundData[i].playerName == pPlayerID) { SoundData[i] = (pPlayerID, pSoundID); return; }
			}
			// データがない場合の追加
			SoundData.Add( (pPlayerID, pSoundID) );
		}

		// Unityへの音源データ出力
		public string ExportSoundIDName(string pPlayerID)
        {
			for (int i = 0; i < SoundData.Count; ++i)
			{
                if (SoundData[i].playerName == pPlayerID) { return SoundData[i].soundName; }
			}
			return null;
        }
	}
}
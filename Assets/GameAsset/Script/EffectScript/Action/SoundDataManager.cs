using System.Collections.Generic;
using System.IO;
using SlotEffectMaker2023.Data;

namespace SlotEffectMaker2023.Action
{
	/// <summary>
	/// サウンドの再生設定を管理し、セーブ/ロード機能を提供するクラス。
	/// プレイヤーIDとサウンドIDの対応を保存・復元します。
	/// </summary>
	// サウンドデータ管理クラス(Sav)
	public class SoundDataManager : SlotMaker2022.ILocalDataInterface
	{
		// 変数
		List<(string playerName, string soundName)> SoundData { get; set; }    // 鳴り分けデータ(PlayList, IDList)

		/// <summary>
		/// コンストラクタ。内部リストを初期化します。
		/// </summary>
		public SoundDataManager()
		{
			SoundData = new List<(string, string)>();
		}

		/// <summary>
		/// 初期の鳴り分けデータを設定します。
		/// </summary>
		/// <param name="pPlayData">SoundPlayData のリスト</param>
		public void Init(List<SoundPlayData> pPlayData)
		{
			// 最初に鳴り分け要素を作成しておく
			foreach (var item in pPlayData)
				SoundData.Add((item.ShifterName, item.DefaultElemID));
		}

		/// <summary>
		/// 現在のサウンド設定をバイナリ形式で保存します。
		/// </summary>
		/// <param name="fs">BinaryWriter の参照</param>
		/// <param name="version">保存バージョン</param>
		/// <returns>保存処理が成功したか（常に true）</returns>
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

		/// <summary>
		/// バイナリ形式からサウンド設定を読み込みます。
		/// </summary>
		/// <param name="fs">BinaryReader の参照</param>
		/// <param name="version">保存バージョン</param>
		/// <returns>読み込み処理が成功したか（常に true）</returns>
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

		/// <summary>
		/// 指定したプレイヤーのサウンドIDを更新または追加します。
		/// </summary>
		/// <param name="pPlayerID">プレイヤーID</param>
		/// <param name="pSoundID">新しいサウンドID</param>
		public void ChangeSoundID(string pPlayerID, string pSoundID)
		{
			// データ編集用関数
			for (int i = 0; i < SoundData.Count; ++i)
			{
				if (SoundData[i].playerName == pPlayerID)
				{
					SoundData[i] = (pPlayerID, pSoundID);
					return;
				}
			}
			// データがない場合の追加
			SoundData.Add((pPlayerID, pSoundID));
		}

		/// <summary>
		/// 指定したプレイヤーのサウンドIDを取得します。
		/// </summary>
		/// <param name="pPlayerID">プレイヤーID</param>
		/// <returns>サウンドID、存在しない場合は null</returns>
		public string ExportSoundIDName(string pPlayerID)
		{
			// Unityへの音源データ出力
			for (int i = 0; i < SoundData.Count; ++i)
			{
				if (SoundData[i].playerName == pPlayerID)
					return SoundData[i].soundName;
			}
			return null;
		}
	}
}

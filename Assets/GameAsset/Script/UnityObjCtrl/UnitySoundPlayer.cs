using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// サウンド再生管理コンポーネント。
/// 複数のSoundPlayerDataを生成し、音量設定と再生制御を行います。
/// </summary>
public class UnitySoundPlayer : MonoBehaviour
{
	/// <summary>
	/// 使用する音源再生プレハブ。
	/// </summary>
	[SerializeField] private GameObject PrehabSoundPlayer;    // 使用する音源再生機(Prehab)
	/// <summary>
	/// 登録されたAudioClip一覧。
	/// </summary>
	[SerializeField] private AudioClip[] SoundData;           // 使用する音源一覧をSerializableで登録する
	/// <summary>
	/// マスターボリューム調整用スライダー。
	/// </summary>
	[SerializeField] private UIVolSlider VolMaster;           // 音量調整用データ
	/// <summary>
	/// BGMボリューム調整用スライダー。
	/// </summary>
	[SerializeField] private UIVolSlider VolBGM;
	/// <summary>
	/// SEボリューム調整用スライダー。
	/// </summary>
	[SerializeField] private UIVolSlider VolSE;

	private List<SoundPlayerData> player;
	private SlotEffectMaker2023.Action.DataShifterManager<SlotEffectMaker2023.Data.SoundPlayData> SndManager;    // 音制御データ
	private List<SlotEffectMaker2023.Data.SoundPlayData> SoundPlayData;   // 音再生データ

	private SlotEffectMaker2023.Singleton.EffectDataManagerSingleton effectData;
	private SlotEffectMaker2023.Action.SystemData sys;

	/// <summary>
	/// Start は初期化処理を行い、SoundPlayerDataリストの作成、初期クリップと音量設定を行います。
	/// </summary>
	private void Start()
	{
		player = new List<SoundPlayerData>();
		SndManager = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().soundData;
		effectData = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance();
		SoundPlayData = effectData.SoundPlayList;

		// playerデータを作成する
		Transform parent = this.transform;
		foreach (var data in SoundPlayData)
		{
			GameObject shot = Instantiate(PrehabSoundPlayer, parent);
			GameObject loop = Instantiate(PrehabSoundPlayer, parent);
			player.Add(new SoundPlayerData(data, shot.GetComponent<AudioSource>(), loop.GetComponent<AudioSource>()));
		}
		// 初期音源を設定する
		for (int i = 0; i < player.Count; ++i) SetClip(i);

		// 初期音量を設定する
		sys = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().sysData;
		VolMaster.SetVolume(sys.MasterVol);
		VolBGM.SetVolume(sys.BGMVol);
		VolSE.SetVolume(sys.SEVol);
	}

	/// <summary>
	/// Update は毎フレーム呼び出され、音量記録、音源更新、音量調整、再生制御を行います。
	/// </summary>
	private void Update()
	{
		// 音量を記録する
		sys.MasterVol = VolMaster.Volume;
		sys.BGMVol = VolBGM.Volume;
		sys.SEVol = VolSE.Volume;

		// すべての音源データに対して処理を行う
		for (int i = 0; i < player.Count; ++i)
		{
			var data = player[i];
			// 音源の更新を行う
			if (data.LastSoundID != SndManager.ExportElemName(SoundPlayData[i].ShifterName)) SetClip(i);
			// 音量調整を行う
			data.SetVolume(VolMaster.Volume, VolSE.Volume, VolBGM.Volume);
			// 音の制御を行う
			data.Process();
		}
	}

	/// <summary>
	/// 指定プレイヤーIDのサウンドクリップを設定します。
	/// </summary>
	/// <param name="pPlayerID">SoundPlayerDataリストのインデックス。</param>
	private void SetClip(int pPlayerID)
	{
		var playData = player[pPlayerID];
		string soundIDName = SndManager.ExportElemName(SoundPlayData[pPlayerID].ShifterName);
		var soundData = effectData.GetSoundID(soundIDName);

		// 音源を読み込む
		AudioClip shot = GetClipByName(soundData.ShotResName);
		AudioClip loop = GetClipByName(soundData.LoopResName);
		// 音源を設定する
		playData.ChangeSoundID(shot, loop, soundData.LoopBegin, soundIDName);
	}

	/// <summary>
	/// 登録されたSoundDataから指定名のAudioClipを取得します。
	/// </summary>
	/// <param name="name">取得するAudioClipの名前。</param>
	/// <returns>一致するAudioClip。見つからない場合はnull。</returns>
	private AudioClip GetClipByName(string name)
	{
		if (name == string.Empty) return null;
		foreach (var item in SoundData)
		{
			if (item == null) continue;
			if (item.name.Equals(name)) return item;
		}
		return null;
	}
}

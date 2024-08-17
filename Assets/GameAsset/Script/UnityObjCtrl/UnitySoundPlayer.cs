using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitySoundPlayer : MonoBehaviour
{
	// 使用する音源再生機(Prehab)
	[SerializeField] private GameObject PrehabSoundPlayer;
	// 使用する音源一覧をSerializableで登録する
	[SerializeField] AudioClip[] SoundData;
	// 音量調整用データ
	[SerializeField] UIVolSlider VolMaster;
	[SerializeField] UIVolSlider VolBGM;
	[SerializeField] UIVolSlider VolSE;
	
	List<SoundPlayerData>	player;
	SlotEffectMaker2023.Action.DataShifterManager<SlotEffectMaker2023.Data.SoundPlayData>	SndManager;		// 音制御データ
	List<SlotEffectMaker2023.Data.SoundPlayData>											SoundPlayData;	// 音再生データ
	
	SlotEffectMaker2023.Singleton.EffectDataManagerSingleton effectData;
	SlotEffectMaker2023.Action.SystemData sys;
	
    // Start is called before the first frame update
    void Start()
    {
        player = new List<SoundPlayerData>();
        SndManager = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().soundData;
        effectData = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance();
        SoundPlayData = effectData.SoundPlayList;
        
        // playerデータを作成する
        Transform parent = this.transform;
        foreach(var data in SoundPlayData){
        	GameObject shot = Instantiate(PrehabSoundPlayer, parent);
        	GameObject loop = Instantiate(PrehabSoundPlayer, parent);
        	player.Add(new SoundPlayerData(data, shot.GetComponent<AudioSource>(), loop.GetComponent<AudioSource>()));
        }
        // 初期音源を設定する
        for(int i=0; i<player.Count; ++i) SetClip(i);
        
        // 初期音量を設定する
        sys = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().sysData;
        VolMaster.SetVolume(sys.MasterVol);
        VolBGM.SetVolume(sys.BGMVol);
        VolSE.SetVolume(sys.SEVol);
    }

    // Update is called once per frame
    void Update()
    {
    	// 音量を記録する
    	sys.MasterVol = VolMaster.Volume;
    	sys.BGMVol = VolBGM.Volume;
    	sys.SEVol = VolSE.Volume;
    	
    	// すべての音源データに対して処理を行う
    	for(int i=0; i<player.Count; ++i){
    		var data = player[i];
    		// 音源の更新を行う
    		if (data.LastSoundID != SndManager.ExportElemName(SoundPlayData[i].ShifterName)) SetClip(i);
    		// 音量調整を行う
    		data.SetVolume(VolMaster.Volume, VolSE.Volume, VolBGM.Volume);
    		// 音の制御を行う
    		data.Process();
    	}
    }
    
    // 音源データを設定する
    void SetClip(int pPlayerID){
    	var playData = player[pPlayerID];
    	string soundIDName = SndManager.ExportElemName(SoundPlayData[pPlayerID].ShifterName);
    	var soundData = effectData.GetSoundID(soundIDName);
    	
    	// 音源を読み込む
    	AudioClip shot = GetClipByName(soundData.ShotResName);
    	AudioClip loop = GetClipByName(soundData.LoopResName);
    	// 音源を設定する
    	playData.ChangeSoundID(shot, loop, soundData.LoopBegin, soundIDName);
    }
    
    // 読み込まれた音源データの名前から音源を取り出す
    AudioClip GetClipByName(string name){
    	if (name == string.Empty) return null;
    	foreach (var item in SoundData){
    		if (item == null) continue;
    		if (item.name.Equals(name)) return item;
    	}
    	return null;
    }
}

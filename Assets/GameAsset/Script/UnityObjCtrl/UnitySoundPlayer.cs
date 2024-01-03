using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitySoundPlayer : MonoBehaviour
{
	List<SoundPlayerData>	player;
	SlotEffectMaker2023.Action.SoundDataManager		SndManager;		// 音制御データ
	List<SlotEffectMaker2023.Data.SoundPlayData>	SoundPlayData;	// 音再生データ
	
	SlotEffectMaker2023.Singleton.EffectDataManagerSingleton effectData;
	
    // Start is called before the first frame update
    void Start()
    {
        player = new List<SoundPlayerData>();
        SndManager = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().soundData;
        effectData = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance();
        SoundPlayData = effectData.SoundPlayList;
        
        // playerデータを作成する
        GameObject prehab = Resources.Load<GameObject>("PrehabSound");
        Transform parent = this.transform;
        foreach(var data in SoundPlayData){
        	GameObject shot = Instantiate(prehab, parent);
        	GameObject loop = Instantiate(prehab, parent);
        	player.Add(new SoundPlayerData(data, shot.GetComponent<AudioSource>(), loop.GetComponent<AudioSource>()));
        }
        // 初期音源を設定する
        for(int i=0; i<player.Count; ++i) SetClip(i);
    }

    // Update is called once per frame
    void Update()
    {
    	// すべての音源データに対して処理を行う
    	for(int i=0; i<player.Count; ++i){
    		var data = player[i];
    		// 音源の更新を行う
    		if (data.LastSoundID != SndManager.ExportSoundIDName(SoundPlayData[i].PlayerName)) SetClip(i);
    		// 音の制御を行う
    		data.Process();
    	}
    }
    
    // 音源データを設定する
    void SetClip(int pPlayerID){
    	var playData = player[pPlayerID];
    	string soundIDName = SndManager.ExportSoundIDName(SoundPlayData[pPlayerID].PlayerName);
    	var soundData = effectData.GetSoundID(soundIDName);
    	AudioClip shot = null;
    	AudioClip loop = null;
    	
    	// 音源を読み込む
    	if (soundData.ShotResName != string.Empty) shot = Resources.Load<AudioClip>(soundData.ShotResName);
    	if (soundData.LoopResName != string.Empty) loop = Resources.Load<AudioClip>(soundData.LoopResName);
    	// 音源を設定する
    	playData.ChangeSoundID(shot, loop, soundData.LoopBegin, soundIDName);
    }
}

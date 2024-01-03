using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
	List<SoundPlayerData>	player;
	SlotDataSingleton		slotData;	// スロット基本情報
	
    // Start is called before the first frame update
    void Start()
    {
        player = new List<SoundPlayerData>();
        slotData = SlotDataSingleton.GetInstance();
        
        // playerデータを作成する
        GameObject prehab = Resources.Load<GameObject>("PrehabSound");
        Transform parent = this.transform;
        foreach(SoundPlayData data in slotData.soundData.PlayList){
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
    		if (data.LastSoundID != slotData.soundData.SoundID[i]) SetClip(i);
    		// 音の制御を行う
    		data.Process();
    	}
    }
    
    // 音源データを設定する
    void SetClip(int pPlayerID){
    	var playData  = player[pPlayerID];
    	int soundID = slotData.soundData.SoundID[pPlayerID];
    	var soundData = slotData.soundData.IDList[soundID];
    	AudioClip shot = null;
    	AudioClip loop = null;
    	
    	// 音源を読み込む
    	if (soundData.ShotResName != string.Empty) shot = Resources.Load<AudioClip>(soundData.ShotResName);
    	if (soundData.LoopResName != string.Empty) loop = Resources.Load<AudioClip>(soundData.LoopResName);
    	// 音源を設定する
    	playData.ChangeSoundID(shot, loop, soundData.LoopBegin, soundID);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// サウンド再生データ
public class SoundPlayerData {
	// 変数
	SlotEffectMaker2023.Data.SoundPlayData PlayData;			// 参照する再生データ
	AudioSource   SManagerShot;		// Shot音源を再生するGameObject
	AudioSource   SManagerLoop;		// Loop音源を再生するGameObject
	AudioClip     SClipShot;		// Shot音源
	AudioClip     SClipLoop;		// Loop音源
	
	bool          SoundPlayFlag;	// 音を流しているとtrue, 音を止めているとfalse
	bool          SoundStopFlag;	// 音を中断する場合trueにする
	
	SlotEffectMaker2023.Action.SlotTimer     ConditionTimer;	// 条件判定に使用するタイマ
	SlotEffectMaker2023.Action.SlotTimer     ShotTimer;		// Shot音源に使用するタイマ
	SlotEffectMaker2023.Action.SlotTimer     LoopTimer;		// Loop音源に使用するタイマ
	int           LoopBegin;		// ループ音源鳴動開始時間
	float?        LastCondTime;		// 前回コンディションタイマ値
	
	const float TIME_DIV = SlotEffectMaker2023.Data.SoundPlayData.TIME_DIV;
	
	public string LastSoundID { get; private set; } // 前回サウンドID値
	
	// timerの初期化後にこのコンストラクタを呼ぶこと
	// また、音源はChangeSoundIDより初期化すること
	public SoundPlayerData(SlotEffectMaker2023.Data.SoundPlayData pPlayData, AudioSource pSoundObjectShot, AudioSource pSoundObjectLoop){
		PlayData      = pPlayData;
		SManagerShot  = pSoundObjectShot;
		SManagerLoop  = pSoundObjectLoop;
		SClipShot     = null;
		SClipLoop     = null;
		LoopBegin     = -1;
		
		SoundPlayFlag = false;
		SoundStopFlag = false;
		
		var timer = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().timerData;
		ConditionTimer = timer.GetTimer(PlayData.UseTimerName);
		ShotTimer = timer.GetTimer(PlayData.GetShotTimerName());
		LoopTimer = timer.GetTimer(PlayData.GetLoopTimerName());
		LastCondTime = null;
		LastSoundID = PlayData.DefaultElemID;
	}
	
	// 鳴らす音を変える
	public void ChangeSoundID(AudioClip pShotClip, AudioClip pLoopClip, int pLoopBegin, string pSoundID){
		// 音源切り替え時の前処理を行う
		SoundStopFlag = true;
		StopObject();
		
		// 音源を設定する
		SClipShot = pShotClip;
		SClipLoop = pLoopClip;
		LoopBegin = pLoopBegin;
		LastSoundID = pSoundID;
	}
	// 音を止める
	public void StopSound() { SoundStopFlag = true; }
	private void StopObject() {
		if(LoopBegin >= 0) SManagerShot.Stop();
		SManagerLoop.Stop();
		if(LoopBegin >= 0) ShotTimer?.SetDisabled();
		LoopTimer?.SetDisabled();
		SoundPlayFlag = false;
	}
	
	// 音を再生する
	private void PlayObject() {
		// Oneshotの再生
		if (SClipShot != null) {
			SManagerShot.PlayOneShot(SClipShot);
			ShotTimer?.Activate();
		}
		// Loopの再生
		if (LoopBegin >= 0 && SClipLoop != null) {
			float delay = LoopBegin / TIME_DIV;
			SManagerLoop.clip = SClipLoop;
			SManagerLoop.loop = true;
			SManagerLoop.PlayDelayed(delay);
			LoopTimer?.Activate(-delay);
		}
		// フラグ登録
		SoundPlayFlag = true;
	}
	
	// 常時処理を行う。この中でサウンドを再生/停止を制御する
	public void Process() {
		// コンディションタイマのインスタンスがない場合処理しない
		if (ConditionTimer == null) return;
		
		// shotタイマの停止処理を行う
		if (!SManagerShot.isPlaying) ShotTimer?.SetDisabled();
		
		// 音を止める処理
		if (SoundPlayFlag) {
			// タイマが無効になった場合
			if (!ConditionTimer.isActivate) SoundStopFlag = true;
			else {
				// 停止時間を超過した場合
				SoundStopFlag |= (float)ConditionTimer.elapsedTime >= PlayData.StopTime / TIME_DIV && PlayData.StopTime >= 0;
				// 経過時間が巻き戻った場合
				if (LastCondTime.HasValue) SoundStopFlag |= (float)ConditionTimer.elapsedTime < (float)LastCondTime;
			}
			
			// 音を止める処理本体
			if (SoundStopFlag){ StopObject(); }
		}
		
		// ストップフラグクリア
		SoundStopFlag = false;
		
		// タイマが再生時間を今回のProcessで通過した場合に音を再生する
		if (!SoundPlayFlag){
			bool activate = false;
			// 条件タイマが無効なら処理しない
			if (!ConditionTimer.isActivate) activate = false;
			// 条件タイマが鳴動条件未達なら処理終了
			else if ((float)ConditionTimer.elapsedTime < PlayData.BeginTime / TIME_DIV) activate = false;
			// 前回タイマ値が無効なら鳴動させる。
			else if (!LastCondTime.HasValue) activate = true;
			// 経過時間が巻き戻った場合、鳴動させる。
			else if ((float)ConditionTimer.elapsedTime < (float)LastCondTime) activate = true;
			// 前回タイマ値が無効なら、前回経過時間が鳴動条件未達であれば鳴動させる。
			else if ((float)LastCondTime < PlayData.BeginTime / TIME_DIV) activate = true;
			
			// 音を鳴らす処理
			if (activate) PlayObject();
		}
		
		// 前回タイマ値更新
		LastCondTime = ConditionTimer.elapsedTime;
	}
	
	// 音量を調整する
	public void SetVolume(float volMaster, float volSE, float volBGM) {
		float volSet = PlayData.ShifterName.Contains("BGM") ? volBGM : volSE;
		volSet *= volMaster;
		SManagerShot.volume = volSet;
		SManagerLoop.volume = volSet;
	}
}

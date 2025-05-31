using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// サウンド再生を管理するデータクラス。
/// 条件タイマに基づき音声の再生/停止およびループ制御を行います。
/// </summary>
public class SoundPlayerData
{
	/// <summary>
	/// 再生設定データ。
	/// </summary>
	private SlotEffectMaker2023.Data.SoundPlayData PlayData; // 参照する再生データ
	/// <summary>
	/// Shot音源再生用AudioSource。
	/// </summary>
	private AudioSource SManagerShot;       // Shot音源を再生するGameObject
	/// <summary>
	/// Loop音源再生用AudioSource。
	/// </summary>
	private AudioSource SManagerLoop;       // Loop音源を再生するGameObject
	/// <summary>
	/// Shot用AudioClip。
	/// </summary>
	private AudioClip SClipShot;            // Shot音源
	/// <summary>
	/// Loop用AudioClip。
	/// </summary>
	private AudioClip SClipLoop;            // Loop音源

	/// <summary>
	/// 再生中フラグ。
	/// </summary>
	private bool SoundPlayFlag;             // 音を流しているとtrue, 音を止めているとfalse
	/// <summary>
	/// 中断要求フラグ。
	/// </summary>
	private bool SoundStopFlag;             // 音を中断する場合trueにする

	/// <summary>
	/// 条件判定に使用するタイマ。
	/// </summary>
	private SlotEffectMaker2023.Action.SlotTimer ConditionTimer;
	/// <summary>
	/// Shot再生制御用タイマ。
	/// </summary>
	private SlotEffectMaker2023.Action.SlotTimer ShotTimer;
	/// <summary>
	/// Loop再生制御用タイマ。
	/// </summary>
	private SlotEffectMaker2023.Action.SlotTimer LoopTimer;
	/// <summary>
	/// ループ再生開始タイミング（TIME_DIV単位）。
	/// </summary>
	private int LoopBegin;
	/// <summary>
	/// 前回の条件タイマ値。
	/// </summary>
	private float? LastCondTime;

	/// <summary>
	/// 時間単位変換定数。
	/// </summary>
	private const float TIME_DIV = SlotEffectMaker2023.Data.SoundPlayData.TIME_DIV;

	/// <summary>
	/// 最後に設定されたサウンドID。
	/// </summary>
	public string LastSoundID { get; private set; }

	/// <summary>
	/// コンストラクタ。タイマ初期化後に呼び出します。
	/// </summary>
	/// <param name="pPlayData">再生設定データ。</param>
	/// <param name="pSoundObjectShot">Shot再生用AudioSource。</param>
	/// <param name="pSoundObjectLoop">Loop再生用AudioSource。</param>
	public SoundPlayerData(SlotEffectMaker2023.Data.SoundPlayData pPlayData, AudioSource pSoundObjectShot, AudioSource pSoundObjectLoop)
	{
		PlayData = pPlayData;
		SManagerShot = pSoundObjectShot;
		SManagerLoop = pSoundObjectLoop;
		SClipShot = null;
		SClipLoop = null;
		LoopBegin = -1;

		SoundPlayFlag = false;
		SoundStopFlag = false;

		var timer = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().timerData;
		ConditionTimer = timer.GetTimer(PlayData.UseTimerName);
		ShotTimer = timer.GetTimer(PlayData.GetShotTimerName());
		LoopTimer = timer.GetTimer(PlayData.GetLoopTimerName());
		LastCondTime = null;
		LastSoundID = PlayData.DefaultElemID;
	}

	/// <summary>
	/// 再生するサウンドを切り替えます。
	/// </summary>
	/// <param name="pShotClip">新しいShot用AudioClip。</param>
	/// <param name="pLoopClip">新しいLoop用AudioClip。</param>
	/// <param name="pLoopBegin">ループ開始タイミング（TIME_DIV単位）。</param>
	/// <param name="pSoundID">サウンド識別ID。</param>
	public void ChangeSoundID(AudioClip pShotClip, AudioClip pLoopClip, int pLoopBegin, string pSoundID)
	{
		SoundStopFlag = true;
		StopObject();

		SClipShot = pShotClip;
		SClipLoop = pLoopClip;
		LoopBegin = pLoopBegin;
		LastSoundID = pSoundID;
	}

	/// <summary>
	/// 再生中のサウンドを停止要求します。
	/// </summary>
	public void StopSound()
	{
		SoundStopFlag = true;
	}

	/// <summary>
	/// 再生AudioSourceとタイマを停止し、フラグをリセットします。
	/// </summary>
	private void StopObject()
	{
		if (LoopBegin >= 0) SManagerShot.Stop();
		SManagerLoop.Stop();
		if (LoopBegin >= 0) ShotTimer?.SetDisabled();
		LoopTimer?.SetDisabled();
		SoundPlayFlag = false;
	}

	/// <summary>
	/// SoundPlayFlagがfalseの場合に音声を再生し、タイマを起動します。
	/// </summary>
	private void PlayObject()
	{
		if (SClipShot != null)
		{
			SManagerShot.PlayOneShot(SClipShot);
			ShotTimer?.Activate();
		}
		if (LoopBegin >= 0 && SClipLoop != null)
		{
			float delay = LoopBegin / TIME_DIV;
			SManagerLoop.clip = SClipLoop;
			SManagerLoop.loop = true;
			SManagerLoop.PlayDelayed(delay);
			LoopTimer?.Activate(-delay);
		}
		SoundPlayFlag = true;
	}

	/// <summary>
	/// 毎フレーム呼び出し、条件タイマに応じて再生/停止および点滅制御を行います。
	/// </summary>
	public void Process()
	{
		if (ConditionTimer == null) return;

		if (!SManagerShot.isPlaying) ShotTimer?.SetDisabled();

		if (SoundPlayFlag)
		{
			if (!ConditionTimer.isActivate) SoundStopFlag = true;
			else
			{
				SoundStopFlag |= (float)ConditionTimer.elapsedTime >= PlayData.StopTime / TIME_DIV && PlayData.StopTime >= 0;
				if (LastCondTime.HasValue)
					SoundStopFlag |= (float)ConditionTimer.elapsedTime < (float)LastCondTime;
			}
			if (SoundStopFlag) { StopObject(); }
		}

		SoundStopFlag = false;

		if (!SoundPlayFlag)
		{
			bool activate = false;
			if (!ConditionTimer.isActivate) activate = false;
			else if ((float)ConditionTimer.elapsedTime < PlayData.BeginTime / TIME_DIV) activate = false;
			else if (!LastCondTime.HasValue) activate = true;
			else if ((float)ConditionTimer.elapsedTime < (float)LastCondTime) activate = true;
			else if ((float)LastCondTime < PlayData.BeginTime / TIME_DIV) activate = true;

			if (activate) PlayObject();
		}

		LastCondTime = ConditionTimer.elapsedTime;
	}

	/// <summary>
	/// 音量設定を行います。
	/// </summary>
	/// <param name="volMaster">マスターボリューム(0.0～1.0)。</param>
	/// <param name="volSE">効果音ボリューム(0.0～1.0)。</param>
	/// <param name="volBGM">BGMボリューム(0.0～1.0)。</param>
	public void SetVolume(float volMaster, float volSE, float volBGM)
	{
		float volSet = PlayData.ShifterName.Contains("BGM") ? volBGM : volSE;
		volSet *= volMaster;
		SManagerShot.volume = volSet;
		SManagerLoop.volume = volSet;
	}
}
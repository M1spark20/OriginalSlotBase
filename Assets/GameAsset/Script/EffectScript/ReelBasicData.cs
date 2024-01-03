using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class ReelBasicData : SlotMaker2022.ILocalDataInterface
{
	// 定数定義
	public const byte REEL_NPOS = byte.MaxValue;
	const float acc 		= 160.0f;	// リール加速度[rpm]
	const float maxSpeed	=  79.5f;	// リール最高速度[rpm]
	
	// 定義変数
	public float reelPos	{ get; private set; }	// 現在のリール座標[0, COMA_MAX)
	public float reelSpeed	{ get; private set; }	// 現在のリール速度[rpm](+:下向き)
	public byte  stopPos	{ get; private set; }	// 停止目標
	public byte  pushPos	{ get; private set; }	// リール押下位置
	public byte  slipCount	{ get; private set; }	// 停止時すべりコマ数
	public bool  isRotate	{ get; private set; }	// リールが回転中か
	public bool  accEnd		{ get; private set; }	// リールが加速を終えたか
	
	public ReelBasicData(){
		reelPos		= 0.0f;
		reelSpeed	= 0.0f;
		stopPos		= 0;
		pushPos		= 0;
		slipCount	= 0;
		isRotate	= false;
		accEnd		= false;
	}
	public ReelBasicData(byte defaultPos){
		reelPos		= (float)defaultPos;
		reelSpeed	= 0.0f;
		stopPos		= defaultPos;
		pushPos		= defaultPos;
		slipCount	= 0;
		isRotate	= false;
		accEnd		= false;
	}
	public bool StoreData(ref BinaryWriter fs, int version){
		fs.Write(reelPos);
		fs.Write(reelSpeed);
		fs.Write(stopPos);
		fs.Write(pushPos);
		fs.Write(slipCount);
		fs.Write(isRotate);
		fs.Write(accEnd);
		return true;
	}
	public bool ReadData(ref BinaryReader fs, int version){
		reelPos		= fs.ReadSingle();
		reelSpeed	= fs.ReadSingle();
		stopPos		= fs.ReadByte();
		pushPos		= fs.ReadByte();
		slipCount	= fs.ReadByte();
		isRotate	= fs.ReadBoolean();
		accEnd		= fs.ReadBoolean();
		return true;
	}
	
	// 制御系変数 //
	
	// リールを始動させる
	public void Start() {
		isRotate = true;
		accEnd = false;
		stopPos = REEL_NPOS;
		pushPos = REEL_NPOS;
		slipCount = REEL_NPOS;
	}
	// リールが参照するコマを取得する(位置補正なし: リール回転用)
	public byte GetReelComaID() {
		return reelSpeed >= 0 ? (byte)Math.Ceiling(reelPos) : (byte)Math.Floor(reelPos);
	}
	// リールが参照するコマを取得する(位置補正あり: リール制御用)
	public byte GetReelComaIDFixed() {
		int ans = GetReelComaID();
		
		const int comaNum = SlotMaker2022.LocalDataSet.COMA_MAX;
		while(ans >= comaNum) ans -= comaNum;
		while(ans < 0)        ans += comaNum;
		return (byte)ans;
	}
	
	// リールが停止処理可能か返す
	// リール回転中 and 速度一定 and 停止制御未実施なら、停止処理可能
	public bool CanStop() { return (isRotate && accEnd && pushPos == REEL_NPOS); }
	
	// リールの停目を設定し、停止制御を行う
	public void SetStopPos(int pSlipCount){
		// リールが回転していない or 速度が一定でない or 停止制御済みの場合、処理を行わない
		if (!CanStop()) return;
		
		const int comaNum = SlotMaker2022.LocalDataSet.COMA_MAX;
		slipCount = (byte)pSlipCount;
		pushPos = GetReelComaIDFixed();
		stopPos = (byte)((pushPos + slipCount) % comaNum);
	}
	// リールの回転処理を行う
	public void Process(){
		if(!isRotate) return;	// 回転していない場合は処理を行わない
		const int comaNum = SlotMaker2022.LocalDataSet.COMA_MAX;
		
		// 前回フレームからの経過時間と座標更新前の位置を取得する
		float y0 = reelPos;	// 座標更新前のリール位置
		float dt = Time.deltaTime;
		
		/* リール速度から座標を変化させる */{
			// 等加速度運動成分を計算する
			float accTime = 0.0f;	// 等加速度運動時間
			float v0 = reelSpeed;	// 等加速度運動における初速度[rpm]
			if (Math.Abs(reelSpeed) < Math.Abs(maxSpeed)){
				// 最高速度到達までの時間を計算し、等加速度運動の時間を計算する
				float accDuration = (maxSpeed - reelSpeed) / acc;
				if (accDuration < dt) { accTime = accDuration; reelSpeed = maxSpeed; }
				else { accTime = dt; reelSpeed += acc * accTime; }
				//Debug.Log(reelSpeed.ToString());
			}
			// 位置の増分を計算(rpmから換算して計算)
			const float speedBase = (float)comaNum / 60.0f;	// rpm換算式
			reelPos += (v0*speedBase * accTime + acc*speedBase * accTime*accTime/2.0f);
			
			// 等速直線運動成分を計算(rpmから換算)
			float slipTime = dt - accTime;	// 等速直線運動時間
			reelPos += slipTime * reelSpeed * speedBase;
			// 等速直線運動要素があった場合、accEndをtrueにする
			accEnd = slipTime > 0f;
		}
		
		// リール停止制御がある場合、停止判定を行う
		if (stopPos != REEL_NPOS){
			float targetPos = (float)stopPos;
			// 前回リール位置y0に対し、targetPosをリール回転方向の前方にあるように補正する
			while (reelSpeed > 0 && targetPos < y0) targetPos += (float)comaNum;	// 速度が正で、targetPosが負の方向にある場合
			while (reelSpeed < 0 && targetPos > y0) targetPos -= (float)comaNum;	// 速度が負で、targetPosが正の方向にある場合
			
			// 「現在速度によるリール移動距離」が「前回リール位置から停目までの距離」を上回った場合にリールを停止させる(absを用いて正負とも判定する)
			if (Math.Abs(reelPos - y0) >= Math.Abs(targetPos - y0)){
				reelPos = targetPos;
				reelSpeed = 0.0f;
				isRotate = false;
			}
		}
		
		// リール位置を補正する
		while (reelPos >= (float)comaNum)	reelPos -= (float)comaNum;
		while (reelPos <  0.0f)				reelPos += (float)comaNum;
	}
}

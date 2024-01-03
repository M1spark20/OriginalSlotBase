using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// 計算式オペランド
public enum ECalcOperand { eAdd, eSub, eMul, eDiv, eMod }

// 変数要素データ(valは直接アクセスすることが可能)
public class SlotVariable : SlotMaker2022.ILocalDataInterface
{
	public string name { get; private set; }	// 変数名(読み取り専用)
	public int    val  { get; set; }			// 変数値
	
	// 読み込み関連処理
	// データを外部から読み込んだ時のコンストラクタ: ReadData関数を直接動かしてデータを読む必要あり
	public SlotVariable(){ /* None */ }
	public SlotVariable(string pValName){
		// 変数を新規に作成するときのコンストラクタ: 変数を指定して新規作成する。
		// 呼び出し前にタイマ名が重複しないことを確認すること
		name = pValName;
		val  = 0;
	}
	public SlotVariable(string pValName, int pInitVal){
		// 変数を新規に作成するときのコンストラクタ: 変数を指定して新規作成する。初期値を入力できる。
		// 呼び出し前にタイマ名が重複しないことを確認すること
		name = pValName;
		val  = pInitVal;
	}
	public bool StoreData(ref BinaryWriter fs, int version){
		fs.Write(name);
		fs.Write(val);
		return true;
	}
	public bool ReadData(ref BinaryReader fs, int version){
		name = fs.ReadString();
		val  = fs.ReadInt32();
		return true;
	}
		
	// valの高度計算あれこれ
	// bool値設定
	public void SetBool(bool flag) { val = flag ? 1 : 0; }
	public bool GetBool() { return val != 0; }
	// valが指定された範囲にいるか確認。inclBorder=trueの時は=も許容する
	public bool CheckRange(int min, int max, bool inclBorder) {
		if (inclBorder && (val == min || val == max)) return true;
		return val > min && val < max;
	}
	// 引数で指定した数値に対し計算した結果を返す。自身は計算式の右辺となる
	// 除数が0で計算できない場合、計算結果は0を返す
	public int Operate(ECalcOperand operand, int opLeft) {
		if (operand == ECalcOperand.eAdd) return opLeft + val;
		if (operand == ECalcOperand.eSub) return opLeft - val;
		if (operand == ECalcOperand.eMul) return opLeft * val;
		// 以降は除数のため、valを確認
		if (val == 0) return 0;
		if (operand == ECalcOperand.eDiv) return opLeft / val;
		if (operand == ECalcOperand.eMod) return opLeft % val;
		// 何も引っかからなかった時
		return 0;
	}
}

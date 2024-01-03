using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotValManager
{
	// 変数
	List<SlotVariable> valData;
	
	/// <summary>
	/// インスタンスの初期化を行います。
	/// </summary>
	public SlotValManager()
	{
		valData = new List<SlotVariable>();
	}
	
	// valDataの読み込みを行う
	public bool ReadData(){
		// 読み込み処理(あとで実装)
		
		// システム変数を新規作成する。読み込み処理で作成済みの場合は重複定義しない
		AddSystemVal();
		return true;
	}
	
	// 名前に重複がないことを確認して変数を新規作成する。
	// [ret]変数を追加したか
	public bool CreateVariable(string pValName){
		for(int i=0; i<valData.Count; ++i){
			if (valData[i].name == pValName) return false;
		}
		valData.Add(new SlotVariable(pValName));
		return true;
	}
	// 名前に一致したタイマを取得する
	// [ret]タイマのインスタンス, 見つからない場合はnull
	public SlotVariable GetVariable(string pValName){
		for(int i=0; i<valData.Count; ++i){
			if (valData[i].name == pValName) return valData[i];
		}
		return null;
	}
	
	// システム変数作成
	private void AddSystemVal(){
		CreateVariable("_betCount");
		CreateVariable("_creditCount");
		CreateVariable("_payoutCount");
		CreateVariable("_isReplay");
	}
}

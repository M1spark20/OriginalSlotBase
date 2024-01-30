using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowNumBasic22 : ShowNumBase
{
	// 実装すべき関数の未実装
	protected override void InitDivision() {
		DivX = 11;
		DivY =  2;
		CutWayX = true;
		ShowX = -ShowDigit;
		ShowY = 1;
	}
	
	protected override int? GetTextureIndex(int val, uint getDigit) {
		int digit = Mathf.Abs(val);
		int lastVal = digit;
		int baseDig = val < 0 ? 11 : 0;
		
		for(int i=0; i<getDigit; ++i) { lastVal = digit; digit /= 10; }
		// 最上位ケタは数値がマイナスならマイナスを出す
		if ( ((digit == 0 && getDigit > 0) || getDigit+1 == ShowDigit) && lastVal > 0) return val < 0 ? 21 : 10;
		// 表示しないケタ
		if (digit == 0 && getDigit > 0 && lastVal == 0) return null;
		return (digit % 10) + baseDig;
	}
}

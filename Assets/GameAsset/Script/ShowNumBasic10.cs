using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowNumBasic10 : IMultiImageWriter
{
	// 実装すべき関数の未実装
	protected override void InitDivision() {
		DivX = 10;
		DivY =  1;
		CutWayX = true;
	}
	
	protected override int? GetTextureIndex(int val, uint getDigit) {
		int digit = val;
		for(int i=0; i<getDigit; ++i) digit /= 10;
		if (digit == 0 && getDigit > 0) return null;
		return digit % 10;
	}
}

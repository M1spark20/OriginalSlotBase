using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum EGameButtonID {
	eBetStart, e1Bet, e2Bet, e3Bet, e1Reel, e2Reel, e3Reel, eButtonMax
}

interface ISlotControllerBase
{
	public void Process();
	public void ButtonClick(EGameButtonID pButtonID);
}

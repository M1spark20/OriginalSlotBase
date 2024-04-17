using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollFeederBonusHist : ScrollFeederBase
{
	private SlotEffectMaker2023.Action.HistoryManager hm;

	protected override void Start(){
        hm = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().historyManager;
        
        // あとから実行する
		base.Start();
	}

	protected override int FeedContentSize() { return hm.BonusHist.Count; }
	protected override int FeedOffsetSize() {
		if (hm.BonusHist.Count <= 0) return 0;
		return hm.BonusHist[0].IsActivate ? 0 : 1;
	}
}

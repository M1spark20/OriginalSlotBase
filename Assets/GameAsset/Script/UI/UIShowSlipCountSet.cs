using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIShowSlipCountSet : MonoBehaviour
{
	[SerializeField] private Button[] ChangerUI;
	
	private Image[] im;
	private TextMeshProUGUI[] Text;
	private SlotEffectMaker2023.Action.SystemData sys;

    // Start is called before the first frame update
    void Start()
    {
    	im = new Image[ChangerUI.Length];
    	Text = new TextMeshProUGUI[ChangerUI.Length];
    	for(int i=0; i<ChangerUI.Length; ++i){
    		im[i] = ChangerUI[i].GetComponent<Image>();
    		Text[i] = ChangerUI[i].transform.Find("IDText").GetComponent<TextMeshProUGUI>();
    	}
    	
    	sys = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().sysData;
    	ChangeShow(sys.ShowSlipCount);
    }

	public void ChangeShow(bool enabled){
		sys.ShowSlipCount = enabled;
		for(int i=0; i<ChangerUI.Length; ++i){
    		Color itemCol = sys.ShowSlipCount ^ (i == 0) ? Color.yellow : Color.white;
    		im[i].color = itemCol;
    		Text[i].color = itemCol;
		}
	}
}

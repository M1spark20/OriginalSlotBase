using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlipCountManagerUI : MonoBehaviour
{
	[SerializeField] private Canvas CanvasEnable;
	[SerializeField] private TextMeshProUGUI[] ShowText;
	
	private SlotEffectMaker2023.Action.SystemData sys;
	private List<SlotEffectMaker2023.Action.ReelBasicData> rb;
	
    // Start is called before the first frame update
    void Start()
    {
    	var sg = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance();
    	sys = sg.sysData;
    	rb = sg.reelData;
    }

    // Update is called once per frame
    void Update()
    {
    	CanvasEnable.enabled = sys.ShowSlipCount;
        if (!sys.ShowSlipCount) return;
        for (int i=0; i<ShowText.Length; ++i){
        	byte slipCount = rb[i].slipCount;
        	ShowText[i].text = rb[i].isRotate ? "[X]" : "[" + slipCount.ToString() + "]";
        }
    }
}

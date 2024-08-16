using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SlotEffectMaker2023.Action;

public class ProbabilityShow : MonoBehaviour
{
	[SerializeField] private string CheckVar;
	[SerializeField] private string TotalGameVar;
	[SerializeField] private TextMeshProUGUI Count;
	[SerializeField] private TextMeshProUGUI Probability;
	
	private SlotValManager vm;
	
    // Start is called before the first frame update
    void Start()
    {
        vm = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().valManager;
    }

    // Update is called once per frame
    void Update()
    {
        int cnt = vm.GetVariable(CheckVar).val;
        int total = vm.GetVariable(TotalGameVar).val;
        
        Count.text = cnt.ToString();
        Probability.text = "(1/-----)";
        if (cnt > 0){
        	float prob = total / (float)cnt;
        	Probability.text = "(1/" + prob.ToString("F2") + ")";
        }
    }
}

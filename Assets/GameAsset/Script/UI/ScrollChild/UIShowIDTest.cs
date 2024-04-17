using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIShowIDTest : ScrollPrehabBase
{
	protected override void RefreshData(int pID){
		transform.Find("Title").GetComponent<TextMeshProUGUI>().text = "[" + pID.ToString() + "] Prehab: " + name;
	}
}

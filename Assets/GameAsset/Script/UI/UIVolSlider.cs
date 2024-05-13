using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIVolSlider : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI Text;
	[SerializeField] private Slider Slider;
	
	public float Volume { get; private set; }
	
    // Update is called once per frame
    private void Update()
    {
    	// 表記上数値は2倍しておく
        Text.text = Mathf.Floor(Slider.value * 200f).ToString("F0") + "%";
        Volume = Slider.value;
    }
    
    public void SetVolume(float valStd) {
    	Slider.value = valStd;
    	Volume = valStd;
    }
    
    public void Reset(){
    	SetVolume(0.5f);
    }
}

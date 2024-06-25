using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using TMPro;
using System.Threading.Tasks;

public class UILanguageChanger : MonoBehaviour
{
	[SerializeField] private Button[] ChangerUI;
	
	private Image[] im;
	private TextMeshProUGUI[] Text;
	
	// quoted from: https://anogame.net/unitypackage_localization/
	
    private void Awake()
    {
    	im = new Image[ChangerUI.Length];
    	Text = new TextMeshProUGUI[ChangerUI.Length];
    	for(int i=0; i<ChangerUI.Length; ++i){
    		im[i] = ChangerUI[i].GetComponent<Image>();
    		Text[i] = ChangerUI[i].transform.Find("IDText").GetComponent<TextMeshProUGUI>();
    	}
    }

    // Update is called once per frame
    void Update()
    {
    	string nowLocale = LocalizationSettings.SelectedLocale.Identifier.Code;
    	for(int i=0; i<ChangerUI.Length; ++i){
    		Color itemCol = ChangerUI[i].name == nowLocale ? Color.yellow : Color.white;
    		im[i].color = itemCol;
    		Text[i].color = itemCol;
    	}
    }
    
    public void OnLocaleChange(string localeStr)
    {
		var _ = ChangeSelectedLocale(localeStr);
    }

    private async Task ChangeSelectedLocale(string locale)
    {
        LocalizationSettings.SelectedLocale = Locale.CreateLocale(locale);
        await LocalizationSettings.InitializationOperation.Task;
    }
}

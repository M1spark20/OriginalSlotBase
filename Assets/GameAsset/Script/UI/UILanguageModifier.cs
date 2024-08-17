using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;


public class UILanguageModifier : MonoBehaviour
{
	private SlotEffectMaker2023.Action.SystemData sys;
	
	private async void Start(){
		sys = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().sysData;
        await LocalizationSettings.InitializationOperation.Task;
		OnLocaleChange(sys.Locale.ToString());
	}
	
    public void OnLocaleChange(string localeStr)
    {
    	if (localeStr == "en") sys.Locale = SlotEffectMaker2023.Action.LangLocale.en;
    	if (localeStr == "ja") sys.Locale = SlotEffectMaker2023.Action.LangLocale.ja;
		var _ = ChangeSelectedLocale(localeStr);
    }

    private async Task ChangeSelectedLocale(string locale)
    {
        LocalizationSettings.SelectedLocale = Locale.CreateLocale(locale);
        await LocalizationSettings.InitializationOperation.Task;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

/// <summary>
/// 言語設定を内部システムとUnity Localizationに反映させる制御クラス
/// </summary>
public class UILanguageModifier : MonoBehaviour
{
    /// <summary>
    /// システムデータ（内部のロケール情報などを保持）
    /// </summary>
    private SlotEffectMaker2023.Action.SystemData sys;

    /// <summary>
    /// 初期化時にLocalizationの準備を待機し、保存されているロケールを適用します
    /// </summary>
    private async void Start()
    {
        sys = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().sysData;
        await LocalizationSettings.InitializationOperation.Task;
        OnLocaleChange(sys.Locale.ToString());
    }

    /// <summary>
    /// 指定されたロケール文字列に応じて内部ロケールを更新し、Unity Localization に反映します
    /// </summary>
    /// <param name="localeStr">"ja" や "en" などのロケールコード</param>
    public void OnLocaleChange(string localeStr)
    {
        if (localeStr == "en") sys.Locale = SlotEffectMaker2023.Action.LangLocale.en;
        if (localeStr == "ja") sys.Locale = SlotEffectMaker2023.Action.LangLocale.ja;
        var _ = ChangeSelectedLocale(localeStr);
    }

    /// <summary>
    /// Unity Localization に選択されたロケールを非同期で適用します
    /// </summary>
    /// <param name="locale">適用するロケールコード（例："ja", "en"）</param>
    /// <returns>非同期処理を示す Task</returns>
    private async Task ChangeSelectedLocale(string locale)
    {
        LocalizationSettings.SelectedLocale = Locale.CreateLocale(locale);
        await LocalizationSettings.InitializationOperation.Task;
    }
}

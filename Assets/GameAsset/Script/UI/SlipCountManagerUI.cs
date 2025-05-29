using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// リールごとのスリップ回数を UI 上に表示するクラス
/// </summary>
public class SlipCountManagerUI : MonoBehaviour
{
    /// <summary>
    /// スリップ回数表示用のキャンバス
    /// </summary>
    [SerializeField] private Canvas CanvasEnable;

    /// <summary>
    /// 各リールのスリップ回数表示用テキスト配列
    /// </summary>
    [SerializeField] private TextMeshProUGUI[] ShowText;

    private SlotEffectMaker2023.Action.SystemData sys;
    private List<SlotEffectMaker2023.Action.ReelBasicData> rb;

    /// <summary>
    /// 初期化処理：シングルトンからシステムデータとリールデータを取得
    /// </summary>
    void Start()
    {
        var sg = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance();
        sys = sg.sysData;
        rb = sg.reelData;
    }

    /// <summary>
    /// 毎フレームの更新処理：スリップ回数の表示を更新
    /// </summary>
    void Update()
    {
        CanvasEnable.enabled = sys.ShowSlipCount;

        if (!sys.ShowSlipCount) return;

        for (int i = 0; i < ShowText.Length; ++i)
        {
            byte slipCount = rb[i].slipCount;
            ShowText[i].text = rb[i].isRotate ? "[X]" : "[" + slipCount.ToString() + "]";
        }
    }
}

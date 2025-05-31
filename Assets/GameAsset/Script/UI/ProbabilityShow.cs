using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SlotEffectMaker2023.Action;

/// <summary>
/// 変数のカウント数および確率を表示するためのコンポーネント
/// </summary>
public class ProbabilityShow : MonoBehaviour
{
    /// <summary>
    /// カウント対象となる変数名
    /// </summary>
    [SerializeField] private string CheckVar;

    /// <summary>
    /// 分母となる総ゲーム数の変数名
    /// </summary>
    [SerializeField] private string TotalGameVar;

    /// <summary>
    /// カウント値を表示する TextMeshProUGUI
    /// </summary>
    [SerializeField] private TextMeshProUGUI Count;

    /// <summary>
    /// 確率を表示する TextMeshProUGUI
    /// </summary>
    [SerializeField] private TextMeshProUGUI Probability;

    /// <summary>
    /// スロット変数マネージャー
    /// </summary>
    private SlotValManager vm;

    /// <summary>
    /// 初期化処理：SlotDataSingleton から変数マネージャーを取得します
    /// </summary>
    void Start()
    {
        vm = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().valManager;
    }

    /// <summary>
    /// 毎フレーム更新処理：変数から値を取得し、表示用テキストを更新します
    /// </summary>
    void Update()
    {
        int cnt = vm.GetVariable(CheckVar).val;
        int total = vm.GetVariable(TotalGameVar).val;

        Count.text = cnt.ToString();
        Probability.text = "(1/-----)";
        if (cnt > 0)
        {
            float prob = total / (float)cnt;
            Probability.text = "(1/" + prob.ToString("F2") + ")";
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 音量スライダーのUI表示と値取得を行うコンポーネント。
/// </summary>
public class UIVolSlider : MonoBehaviour
{
    /// <summary>
    /// 音量表示用のTextMeshProUGUIコンポーネント。
    /// </summary>
    [SerializeField] private TextMeshProUGUI Text;
    /// <summary>
    /// 音量調整用のSliderコンポーネント。
    /// </summary>
    [SerializeField] private Slider Slider;

    /// <summary>
    /// 現在の音量値（0～1）。
    /// </summary>
    public float Volume { get; private set; }

    /// <summary>
    /// Update は毎フレーム呼び出され、スライダーの値を元に表示テキストを更新し、音量を反映します。
    /// </summary>
    private void Update()
    {
        // 表記上数値は2倍しておく
        Text.text = Mathf.Floor(Slider.value * 200f).ToString("F0") + "%";
        Volume = Slider.value;
    }

    /// <summary>
    /// スライダーと音量を指定値に設定します。
    /// </summary>
    /// <param name="valStd">設定する音量の標準値（0.0～1.0）。</param>
    public void SetVolume(float valStd)
    {
        Slider.value = valStd;
        Volume = valStd;
    }

    /// <summary>
    /// 音量をデフォルト値（50%）にリセットします。
    /// </summary>
    public void Reset()
    {
        SetVolume(0.5f);
    }
}

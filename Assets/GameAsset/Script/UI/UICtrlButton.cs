using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゲーム内のボタン操作を管理する UI ボタンコントローラークラス
/// </summary>
public class UICtrlButton : MonoBehaviour
{
    /// <summary>
    /// 割り当てられた機能ボタンID
    /// </summary>
    [SerializeField] private EGameButtonID Function;

    /// <summary>
    /// 入力通知先のデータマネージャー
    /// </summary>
    [SerializeField] private SlotDataManager DataManager;

    /// <summary>
    /// スライド操作（ドラッグによる入力）を許可するかどうか
    /// </summary>
    [SerializeField] private bool Slidable;

    /// <summary>
    /// ボタンが押されている状態かどうかのフラグ
    /// </summary>
    private bool buttonDownFlag;

    /// <summary>
    /// 初期化処理：押下状態フラグを初期化
    /// </summary>
    private void Awake()
    {
        buttonDownFlag = false;
    }

    /// <summary>
    /// 毎フレームの更新処理：スライド操作やホバー時の処理を実行
    /// </summary>
    private void Update()
    {
        if (Slidable) buttonDownFlag &= Input.GetMouseButton(0);
        if (buttonDownFlag) DataManager?.OnScreenHover(Function);
    }

    /// <summary>
    /// ボタンが離されたときに呼び出される処理
    /// </summary>
    public void OnButtonUp()
    {
        buttonDownFlag = false;
    }

    /// <summary>
    /// ボタンが押されたときに呼び出される処理（初回のみタッチ処理を実行）
    /// </summary>
    public void OnButtonDown()
    {
        if (!buttonDownFlag) DataManager?.OnScreenTouch(Function); // 初回のみ
        buttonDownFlag = true;
    }

    /// <summary>
    /// ボタンからカーソルが外れたときに呼び出される処理（ドラッグ解除）
    /// </summary>
    public void OnPointerExit()
    {
        buttonDownFlag = false;
    }

    /// <summary>
    /// スライドストップ機能:
    /// タップまたはクリック中にカーソルがこのボタンに入った場合、初回のみタッチ処理を実行
    /// </summary>
    public void OnPointerEnter()
    {
        if (!Slidable) return;
        if (!Input.GetMouseButton(0)) return;
        if (buttonDownFlag) return;

        DataManager?.OnScreenTouch(Function); // 初回のみ
        buttonDownFlag = true;
    }
}

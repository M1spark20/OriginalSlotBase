using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// メインメニュー画面上の共通要素の基底クラスです。
/// 各要素の初期化、データ更新、入力処理を定義します。
/// </summary>
public abstract class MainMenuElemBase : MonoBehaviour
{
    [SerializeField]
    private string ElemName;

    /// <summary>
    /// オブジェクト生成時に呼び出される初期化メソッドです。
    /// 派生クラスで必要に応じてオーバーライドできます。
    /// </summary>
    virtual protected void Awake()
    {
    }

    /// <summary>
    /// ゲーム開始時に1度だけ呼び出されます。
    /// 特にデータ更新のみ必要な場合はオーバーライド不要で、<see cref="RefreshData"/> が呼ばれます。
    /// </summary>
    virtual protected void Start()
    {
        // 特段データを更新するだけでいい場合はオーバーライドしない
        RefreshData();
    }

    /// <summary>
    /// 要素の表示データを最新状態に更新する処理を実装します。
    /// </summary>
    public abstract void RefreshData();

    /// <summary>
    /// メニュー操作キー入力時の処理を実装します。
    /// </summary>
    /// <param name="eKeyID">押下されたメニュー操作ボタンの識別子</param>
    public abstract void OnGetKeyDown(EMenuButtonID eKeyID);

    /// <summary>
    /// この要素の名称を取得します。
    /// </summary>
    /// <returns>要素名文字列</returns>
    public string GetElemName()
    {
        return ElemName;
    }
}

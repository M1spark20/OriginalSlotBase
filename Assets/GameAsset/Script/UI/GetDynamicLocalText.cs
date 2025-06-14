using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// LocalizedStringTable から文字列を取得して使用するためのコンポーネント
/// </summary>
public class GetDynamicLocalText : MonoBehaviour
{
  // [SerializeField] TMPro.TMP_Text m_text;
  // テキスト表示用のローカライズされた文字列テーブル
  [SerializeField] UnityEngine.Localization.LocalizedStringTable m_table;

  /// <summary>
  /// 初期化時に文字列テーブルが空かどうかをデバッグ出力します
  /// </summary>
  void Start()
  {
    if (m_table.IsEmpty) Debug.Log("Opus");
  }

  /// <summary>
  /// 指定されたIDのローカライズ文字列を取得します
  /// </summary>
  /// <param name="ID">取得する文字列の識別子</param>
  /// <returns>対応するローカライズ文字列を返します</returns>
  public string GetText(string ID)
  {
    UnityEngine.Localization.Tables.StringTable keyValuePairs = m_table.GetTable();
    return keyValuePairs[ID].Value;
  }
}

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// スクリーンショット付きのツイート機能を持つボタン制御クラス
/// </summary>
public class PostXButton : MonoBehaviour
{
  /// <summary>
  /// Imgur クライアントID（スクリーンショットアップロード用）
  /// </summary>
  [SerializeField] private string _imgurClientId;

  /// <summary>
  /// デフォルトのツイート本文
  /// </summary>
  [SerializeField, Multiline] private string DefaultText;

  // 透明度関連
  private Button bt;

  /// <summary>
  /// ボタンの初期化処理（透明度による有効化に備える）
  /// </summary>
  private void Start()
  {
    bt = this.GetComponent<Button>();
  }

  /// <summary>
  /// ツイート処理を開始する関数（スクリーンショット付き）
  /// </summary>
  public void StartTweet()
  {
    StartCoroutine(TweetWithScreenShot.TweetManager.TweetWithScreenShot(DefaultText));
    Debug.Log("Tweet Done");
  }
}

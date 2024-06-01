using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PostXButton : MonoBehaviour
{
    [SerializeField] private string _imgurClientId;
    
    // 透明度関連
    private Button bt;
    
    // 透明度によるボタン有効化
    private void Start() {
    	bt        = this.GetComponent<Button>();
    }
    
    public void StartTweet() {
    	string postStr = "投稿テスト";
		StartCoroutine(TweetWithScreenShot.TweetManager.TweetWithScreenShot(postStr));
		Debug.Log("Tweet Done");
    }
}

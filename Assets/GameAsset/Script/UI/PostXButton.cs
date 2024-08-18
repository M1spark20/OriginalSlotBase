using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PostXButton : MonoBehaviour
{
    [SerializeField] private string _imgurClientId;
    [SerializeField,Multiline] private string DefaultText;
    
    // 透明度関連
    private Button bt;
    
    // 透明度によるボタン有効化
    private void Start() {
    	bt        = this.GetComponent<Button>();
    }
    
    public void StartTweet() {
		StartCoroutine(TweetWithScreenShot.TweetManager.TweetWithScreenShot(DefaultText));
		Debug.Log("Tweet Done");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// スロットが描画されない幅が広い場合、スロットの端にデータを合わせるスクリプト
public class UIPlotter : MonoBehaviour
{
    [SerializeField, Min(0)] private float GameWidth;
    [SerializeField, Min(0)] private float TargetWidth;
    [SerializeField, Min(0)] private float TargetHeight;
    [SerializeField, Min(0)] private bool IsLeftPanel;
    
	private RectTransform rect;
	
	// 表示位置設定
	[SerializeField] bool[] visible;
	[SerializeField] float[] anchorY;
	private int posID;
	
	void Start() {
		rect = GetComponent<RectTransform>();
		posID = 0;
    }

    // Update is called once per frame
    void Update()
    {
    	if (posID >= anchorY.Length || posID >= visible.Length || !visible[posID]) return;
		// 拡大後の自身の幅を計算
		float UIext = Screen.width / TargetWidth;
		float myWidth = rect.sizeDelta.x * UIext;
		// 拡大後のゲーム描画幅を計算
		float Gameext = Screen.height / TargetHeight;
		float bgWidth = (Screen.width - GameWidth * Gameext) / 2f;
		// 移動幅を計算、移動量はUIに関係するためUIextで割る
		float ansX = Mathf.Max(0f, (bgWidth - myWidth) / UIext);
		if (!IsLeftPanel) ansX *= -1f;
		rect.pivot = new Vector2(rect.pivot.x, anchorY[posID]);
		rect.anchoredPosition = new Vector2(ansX, 0);
    }
}

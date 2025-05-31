using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゲーム終了処理を行う UI 用クラス。
/// Unity エディタ上ではプレイモードを停止し、ビルド環境ではアプリケーションを終了します。
/// </summary>
public class UIQuitGame : MonoBehaviour
{
	/// <summary>
	/// ゲームを終了する処理。
	/// </summary>
	public void Quit()
	{
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // ゲームプレイ終了
#else
		Application.Quit();
#endif
	}
}

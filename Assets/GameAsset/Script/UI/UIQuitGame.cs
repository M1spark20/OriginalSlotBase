using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIQuitGame : MonoBehaviour
{
	public void Quit() {
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
		#else
			Application.Quit();
		#endif
	}
}

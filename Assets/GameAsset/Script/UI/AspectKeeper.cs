// quoted: https://3dunity.org/game-create-lesson/clicker-game/mobile-adjustment/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
/// <summary>
/// 画面のアスペクト比を指定の比率に合わせてViewportを調整するコンポーネントです。
/// </summary>
public class AspectKeeper : MonoBehaviour
{
    [SerializeField]
    private Camera targetCamera; // 対象とするカメラ

    [SerializeField]
    private Vector2 aspectVec;   // 目的解像度

    /// <summary>
    /// 毎フレーム呼び出され、現在のスクリーンサイズに基づいてカメラのViewportを調整します。
    /// </summary>
    private void Update()
    {
        var screenAspect = Screen.width / (float)Screen.height; // 画面のアスペクト比
        var targetAspect = aspectVec.x / aspectVec.y;          // 目的のアスペクト比

        var magRate = targetAspect / screenAspect;             // 目的アスペクト比にするための倍率

        var viewportRect = new Rect(0, 0, 1, 1);               // Viewport 初期値で Rect を作成

        if (magRate < 1)
        {
            viewportRect.width = magRate;                      // 使用する横幅を変更
            viewportRect.x = 0.5f - viewportRect.width * 0.5f; // 中央寄せ
        }
        else
        {
            viewportRect.height = 1 / magRate;                 // 使用する縦幅を変更
            viewportRect.y = 0.5f - viewportRect.height * 0.5f;// 中央寄せ
        }

        targetCamera.rect = viewportRect;                      // カメラのViewportに適用
    }
}

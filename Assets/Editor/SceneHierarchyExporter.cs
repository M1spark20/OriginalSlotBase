using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class SceneHierarchyExporter : EditorWindow
{
    private string outputFile = "Assets/SceneHierarchy.md";

    [MenuItem("Tools/Export Scene Hierarchy")]
    public static void ShowWindow()
    {
        GetWindow<SceneHierarchyExporter>("Scene Exporter");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Scene Hierarchy Exporter", EditorStyles.boldLabel);
        outputFile = EditorGUILayout.TextField("Output File", outputFile);
        if (GUILayout.Button("Export Current Scene"))
            ExportHierarchy();
    }

    public static void ExportHierarchy()
    {
        var window = GetWindow<SceneHierarchyExporter>();
        var path = window.outputFile;
        using (var writer = new StreamWriter(path))
        {
            var scene = EditorSceneManager.GetActiveScene();
            writer.WriteLine($"# Scene: {scene.name}  ");
            foreach (var root in scene.GetRootGameObjects())
                WriteObject(writer, root, 0);
        }
        AssetDatabase.Refresh();
        Debug.Log($"Scene hierarchy exported to {path}");
    }

    static void WriteObject(StreamWriter writer, GameObject obj, int indent)
    {
        if (obj == null || obj.name == "__default__") return;

        writer.WriteLine();
        string heading = new string('#', Mathf.Min(indent + 2, 6));
        string objDesc = GetObjectDescription(obj.name);
        writer.WriteLine($"{heading} {obj.name}  // {objDesc}  ");

        foreach (var comp in obj.GetComponents<Component>())
        {
            if (comp == null) continue;

            string compName = comp.GetType().Name;
            bool isCustom = IsCustomScript(comp);
            string compDesc = isCustom ? "自作スクリプト" : GetComponentDescription(compName);

            // UnityEvent fields via reflection
            var rows = new List<string>();
            foreach (var field in comp.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (typeof(UnityEventBase).IsAssignableFrom(field.FieldType))
                {
                    var ue = field.GetValue(comp) as UnityEventBase;
                    ExtractUnityEvent(ue, field.Name, rows);
                }
            }
            // Common UnityEvent components
            if (comp is Button b) ExtractUnityEvent(b.onClick, "onClick", rows);
            else if (comp is Toggle t) ExtractUnityEvent(t.onValueChanged, "onValueChanged", rows);
            else if (comp is Slider s) ExtractUnityEvent(s.onValueChanged, "onValueChanged", rows);
            else if (comp is Dropdown d) ExtractUnityEvent(d.onValueChanged, "onValueChanged", rows);
            else if (comp is EventTrigger et)
            {
                foreach (var entry in et.triggers)
                {
                    string eventType = entry.eventID.ToString();
                    for (int i = 0; i < entry.callback.GetPersistentEventCount(); i++)
                    {
                        var targetObj = entry.callback.GetPersistentTarget(i);
                        string targetType = targetObj != null ? targetObj.GetType().Name : "None";
                        string methodName = entry.callback.GetPersistentMethodName(i) ?? "";
                        bool isCustomEvent = targetObj is Component tComp && IsCustomScript(tComp);
                        string customTag = isCustomEvent ? " (自作)" : "";
                        rows.Add($"| {eventType} | persistent | `{targetType}.{methodName}(){customTag}` |");
                    }
                }
            }

            if (rows.Count > 0)
            {
                writer.WriteLine("<details>");
                writer.WriteLine($"  <summary>{compName} // {compDesc}</summary>");
                writer.WriteLine();
                writer.WriteLine("  | イベント | 種類 | 呼び出し先 |");
                writer.WriteLine("  |--------|------|------------|");
                foreach (var row in rows)
                    writer.WriteLine("  " + row);
                writer.WriteLine("</details>  ");
                continue;
            }

            if (isCustom)
            {
                writer.WriteLine(); // 空行
                // 関数名をアコーディオンで出力
                writer.WriteLine("<details>");
                writer.WriteLine($"  <summary>{compName} // GameObject \"{obj.name}\" にアタッチされた自作スクリプト</summary>");
                writer.WriteLine();
                writer.WriteLine("  | 関数名 |");
                writer.WriteLine("  |--------|");
                foreach (var mi in comp.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (mi.IsSpecialName) continue;
                    writer.WriteLine($"  | {mi.Name}() |");
                }
                writer.WriteLine("</details>");
                continue;
            }

            // Fallback: serialized properties
            var so = new SerializedObject(comp);
            so.Update();
            var iter = so.GetIterator();
            SerializedObject defSo = null;
            GameObject temp = null;
            if (!(comp is Transform))
            {
                try
                {
                    temp = new GameObject("__default__");
                    var defComp = temp.AddComponent(comp.GetType());
                    if (defComp != null)
                    {
                        defSo = new SerializedObject(defComp);
                        defSo.Update();
                    }
                }
                catch { }
            }

            var props = new List<string>();
            if (iter.NextVisible(true))
            {
                do
                {
                    string name = SanitizeCell(iter.name);
                    string desc = SanitizeCell(GetDescription(iter));
                    string val = SanitizeCell(GetPropertyString(iter));
                    string defVal = string.Empty;
                    if (defSo != null)
                    {
                        var dp = defSo.FindProperty(iter.propertyPath);
                        defVal = dp != null ? SanitizeCell(GetPropertyString(dp)) : string.Empty;
                    }
                    if (val != defVal)
                        props.Add($"| {name} | {desc} | {val} | {defVal} |");
                } while (iter.NextVisible(false));

                if (props.Count > 0)
                {
                    writer.WriteLine("<details>");
                    writer.WriteLine($"  <summary>{compName} // {compDesc}</summary>");
                    writer.WriteLine();
                    writer.WriteLine("  | オプション（英名） | 説明（日本語） | 現在値 | デフォルト |");
                    writer.WriteLine("  |---------------|--------------|--------|----------|");
                    foreach (var p in props)
                        writer.WriteLine("  " + p);
                    writer.WriteLine("</details>  ");
                }
            }

            if (temp != null)
                GameObject.DestroyImmediate(temp);
        }

        // 子階層は最大孫( indent < 2 )まで
        if (indent < 2)
        {
            foreach (Transform child in obj.transform)
                WriteObject(writer, child.gameObject, indent + 1);
        }
    }

    static void ExtractUnityEvent(UnityEventBase ue, string label, List<string> rows)
    {
        if (ue == null) return;

        for (int i = 0; i < ue.GetPersistentEventCount(); i++)
        {
            UnityEngine.Object rawTarget = null;
            try { rawTarget = ue.GetPersistentTarget(i); } catch { rawTarget = null; }
            string targetName = "None";
            if (rawTarget != null)
            {
                if (rawTarget is Component compTarget)
                    targetName = compTarget.gameObject != null ? compTarget.gameObject.name : rawTarget.name;
                else
                    targetName = rawTarget.name;
            }
            string method = "";
            try { method = ue.GetPersistentMethodName(i) ?? ""; } catch { method = ""; }
            rows.Add($"| {label} | persistent | {targetName}.{method} |");
        }

        try
        {
            var callsField = typeof(UnityEventBase).GetField("m_Calls", BindingFlags.NonPublic | BindingFlags.Instance);
            var calls = callsField.GetValue(ue);
            var runtimeField = calls.GetType().GetField("m_RuntimeCalls", BindingFlags.NonPublic | BindingFlags.Instance);
            var rCalls = runtimeField.GetValue(calls) as IList;
            if (rCalls != null)
            {
                foreach (var c in rCalls)
                {
                    var delField = c.GetType().GetField("Delegate", BindingFlags.NonPublic | BindingFlags.Instance);
                    var action = delField.GetValue(c) as UnityAction;
                    if (action != null)
                    {
                        var mi = action.Method;
                        rows.Add($"| {label} | runtime | {mi.DeclaringType.Name}.{mi.Name} |");
                    }
                }
            }
        }
        catch { }
    }

    static string SanitizeCell(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        return s.Replace("\r\n", " ").Replace("\n", " ").Replace("|", "\\|");
    }
    // --- 以下 ヘルパーメソッド ---

    static string GetPropertyString(SerializedProperty prop)
    {
        switch (prop.propertyType)
        {
            case SerializedPropertyType.Boolean: return prop.boolValue.ToString();
            case SerializedPropertyType.Integer: return prop.intValue.ToString();
            case SerializedPropertyType.Float: return prop.floatValue.ToString();
            case SerializedPropertyType.String: return prop.stringValue;
            case SerializedPropertyType.ObjectReference: return prop.objectReferenceValue != null ? prop.objectReferenceValue.name : "None";
            case SerializedPropertyType.Enum:
                var names = prop.enumNames;
                int idx = prop.enumValueIndex;
                return (idx >= 0 && idx < names.Length) ? names[idx] : $"<UnknownEnum:{idx}>";
            case SerializedPropertyType.Vector2: return prop.vector2Value.ToString();
            case SerializedPropertyType.Vector3: return prop.vector3Value.ToString();
            case SerializedPropertyType.Color: return prop.colorValue.ToString();
            case SerializedPropertyType.Rect: return prop.rectValue.ToString();
            default: return prop.propertyType.ToString();
        }
    }

    static string GetDescription(SerializedProperty prop)
    {
        switch (prop.name)
        {
            case "m_LocalPosition": return "ワールド座標の位置";
            case "m_LocalRotation": return "ワールド空間の回転（Quaternion型）";
            case "m_LocalScale": return "ローカルスケール（拡大率）";
            case "m_AnchoredPosition": return "アンカー基準の位置";
            case "m_SizeDelta": return "サイズの変化量";
            case "m_AnchorMin": return "アンカーの最小値";
            case "m_AnchorMax": return "アンカーの最大値";
            case "m_Pivot": return "ピボット位置";
            case "m_ClearFlags": return "クリアモード";
            case "m_BackGroundColor": return "背景色";
            case "m_NearClipPlane": return "ニアクリップ平面";
            case "m_FarClipPlane": return "ファークリップ平面";
            case "m_FieldOfView": return "視野角（FOV）";
            case "m_Orthographic": return "正射影モード";
            case "m_OrthographicSize": return "正射影サイズ";
            case "m_Interactable": return "操作可能か";
            case "m_TargetGraphic": return "対象グラフィック";
            case "m_OnClick": return "クリックイベント";
            case "m_Sprite": return "表示スプライト";
            case "m_Color": return "描画カラー";
            case "m_Type": return "画像タイプ";
            case "m_MinValue": return "最小値";
            case "m_MaxValue": return "最大値";
            case "m_Value": return "現在値";
            case "m_OnValueChanged": return "変更イベント";
            case "m_Content": return "コンテンツ（RectTransform）";
            case "m_Horizontal": return "横スクロール可否";
            case "m_Vertical": return "縦スクロール可否";
            case "m_Size": return "表示領域の割合";
            case "m_NumberOfSteps": return "ステップ数";
            case "m_Text": return "表示テキスト";
            case "m_FontAsset": return "フォントアセット";
            case "m_FontSize": return "フォントサイズ";
            case "m_EnableAutoSizing": return "自動サイズ調整";
            case "m_RenderMode": return "レンダーモード";
            case "m_ScaleFactor": return "スケール係数";
            case "m_PixelPerfect": return "ピクセルパーフェクト";
            case "m_UiScaleMode": return "UI スケールモード";
            case "m_ReferenceResolution": return "参照解像度";
            case "m_ScreenMatchMode": return "解像度マッチモード";
            case "horizontalAxis": return "横入力軸";
            case "verticalAxis": return "縦入力軸";
            case "submitButton": return "決定ボタン";
            case "cancelButton": return "キャンセルボタン";
            default: return prop.name;
        }
    }

    static string GetObjectDescription(string objectName)
    {
        switch (objectName)
        {
            case "Main Camera": return "ゲームのメインカメラ";
            case "CLS": return "クリア画面用カメラ";
            case "ScrSlotDataManager": return "スロットデータ管理";
            default: return "オブジェクト説明未設定";
        }
    }

    static string GetComponentDescription(string compName)
    {
        switch (compName)
        {
            case "Transform": return "位置・回転・拡大率を管理";
            case "RectTransform": return "UI配置のためのTransform";
            case "Camera": return "シーンを描画するカメラ";
            case "AudioListener": return "オーディオリスナー";
            case "AspectKeeper": return "アスペクト比維持";
            default: return compName;
        }
    }

    static bool IsCustomScript(Component comp)
    {
        var ns = comp.GetType().Namespace;
        return string.IsNullOrEmpty(ns) || ns.StartsWith("Assembly-CSharp");
    }
}

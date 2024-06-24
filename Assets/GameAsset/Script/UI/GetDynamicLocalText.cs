using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GetDynamicLocalText : MonoBehaviour
{
    //[SerializeField] TMPro.TMP_Text m_text;
    [SerializeField] UnityEngine.Localization.LocalizedStringTable m_table;

    // Start is called before the first frame update
    void Start()
    {
        if (m_table.IsEmpty) Debug.Log("Opus");
    }
    
    public string GetText(string ID){
		UnityEngine.Localization.Tables.StringTable keyValuePairs = m_table.GetTable();
		return keyValuePairs[ID].Value;
    }
}

using TMPro;
using UnityEngine;

public class CopyCode : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _text;

    public void CopyCodeText()
    {
        GUIUtility.systemCopyBuffer = _text.text;
    }
}

using UnityEngine;
using UnityEngine.UI;

public class ButtonSelector : MonoBehaviour
{
    [SerializeField]
    private Button _button;
    private void OnEnable()
    {
        _button.Select();
    }

    public void SelectButton()
    {
        _button.Select();
    }
}

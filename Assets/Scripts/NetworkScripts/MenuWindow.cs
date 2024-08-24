using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuWindow : MonoBehaviour
{
    [SerializeField]
    private Transform _nextWindow;
    [SerializeField]
    private Button _button;

    public Transform Get() => _nextWindow;

   

    public void GoToNextWindow()
    {
        if (Get() == null)
            return;

        SelectButton();

        MenuHandler.Instance.AddMenu(Get());
    }

    public void SelectButton()
    {
        if (_button != null)
            _button.Select();
    }

    public void GoToPrevWindow() => MenuHandler.Instance.RemoveLastMenu();
}

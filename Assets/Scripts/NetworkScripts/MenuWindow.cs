using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuWindow : MonoBehaviour
{
    [SerializeField]
    private Transform _nextWindow;

    public Transform Get() => _nextWindow;

    public void GoToNextWindow()
    {
        if (Get() == null)
            return;

        MenuHandler.Instance.AddMenu(Get());
    }

    public void GoToPrevWindow() => MenuHandler.Instance.RemoveLastMenu();
}

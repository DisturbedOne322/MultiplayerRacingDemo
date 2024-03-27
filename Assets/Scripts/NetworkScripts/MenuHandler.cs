using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuHandler : MonoBehaviour
{
    public static MenuHandler Instance { get; private set; }

    [SerializeField]
    private List<Transform> _menuHierarchyList;
    [SerializeField]
    private Transform _rootMenu;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            RemoveLastMenu();
            Lobby.Instance.LeaveLobby();
        }
    }

    public void GoBack()
    {
        RemoveLastMenu();
    }

    public void ResetMenuList()
    {
        RemoveLastMenu();
        _menuHierarchyList.Clear();
        AddMenu(_rootMenu);
    }

    public void AddMenu(Transform newMenu)
    {
        if(_menuHierarchyList.Count > 0)
        {
            _menuHierarchyList[_menuHierarchyList.Count - 1].gameObject.SetActive(false);
        }

        _menuHierarchyList.Add(newMenu);
        _menuHierarchyList[_menuHierarchyList.Count - 1].gameObject.SetActive(true);    
    }

    public void RemoveLastMenu()
    {
        //don't go back to auth window
        if (_menuHierarchyList.Count <= 1)
        {
            return;
        }

        _menuHierarchyList[_menuHierarchyList.Count - 1].gameObject.SetActive(false);
        _menuHierarchyList.RemoveAt(_menuHierarchyList.Count - 1);
        _menuHierarchyList[_menuHierarchyList.Count - 1].gameObject.SetActive(true);
    }
}

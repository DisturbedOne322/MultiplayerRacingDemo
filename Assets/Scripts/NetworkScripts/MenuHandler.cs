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
        for (int i = 0; i < _menuHierarchyList.Count; i++)
            _menuHierarchyList[i].gameObject.SetActive(false);

        _menuHierarchyList.Clear();
        _menuHierarchyList.Add(_rootMenu);
        _menuHierarchyList[0].gameObject.SetActive(true);
    }

    public void AddMenu(Transform newMenu)
    {
        if (_menuHierarchyList.Count > 0)
            _menuHierarchyList[_menuHierarchyList.Count - 1].gameObject.SetActive(false);

        _menuHierarchyList.Add(newMenu);
        _menuHierarchyList[_menuHierarchyList.Count - 1].gameObject.SetActive(true);    
    }

    public void SetFirstMenu(Transform newFirstMenu)
    {
        _menuHierarchyList[0].gameObject.SetActive(false);
        _menuHierarchyList.RemoveAt(0);

        AddMenu(newFirstMenu);
    }

    public void RemoveLastMenu()
    {
        if (_menuHierarchyList.Count <= 1)
            return;

        _menuHierarchyList[_menuHierarchyList.Count - 1].gameObject.SetActive(false);
        _menuHierarchyList.RemoveAt(_menuHierarchyList.Count - 1);
        _menuHierarchyList[_menuHierarchyList.Count - 1].gameObject.SetActive(true);
    }
}

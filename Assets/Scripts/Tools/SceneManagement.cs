using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    private  void Awake()
    {
        SceneManager.LoadScene("MainMenuScene");    
    }
}

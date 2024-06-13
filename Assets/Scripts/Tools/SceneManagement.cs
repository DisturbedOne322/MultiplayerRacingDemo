using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    private async void Awake()
    {
        await SceneManager.LoadSceneAsync("MainMenuScene");    
    }
}

using Assets.VehicleController;
using UnityEngine;

public class NitroSoundEffec : MonoBehaviour
{
    public AudioSource _sourceInit;
    public AudioSource _sourceCont;
    public AudioClip _initClip;
    public AudioClip _contClip;
    public CurrentCarStats _currentCarStats;
    public AudioReverbZone _reverbZone;
    private float boostingTime = 0;

    private void Awake()
    {
        _sourceCont.loop = true;
        _sourceInit.Stop();
        _sourceCont.Stop();
    }

    private void Update()
    {
        
    }
}

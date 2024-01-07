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
        if(_currentCarStats.NitroBoosting)
        {
            if(boostingTime == 0)
            {
                _sourceInit.volume = 0.3f;
                _sourceInit.Play();
            }


            _sourceCont.volume = 1;

            if (boostingTime > 0.4f)
            {
                _reverbZone.reverbPreset = AudioReverbPreset.Psychotic;
                if (!_sourceCont.isPlaying)
                    _sourceCont.Play();
            }


            boostingTime += Time.deltaTime;
        }
        else
        {
            _reverbZone.reverbPreset = AudioReverbPreset.Off;
            boostingTime = 0;
            _sourceCont.volume -= Time.deltaTime * 4;
            _sourceInit.volume -= Time.deltaTime * 4;

            if (_sourceCont.volume == 0)
                if(_sourceCont.isPlaying)
                    _sourceCont.Stop();

            if(_sourceInit.volume == 0)
                if(_sourceInit.isPlaying)
                    _sourceInit.Stop();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    [SerializeField]
    private TrackDataSO[] _mainMenuTracksArray;
    [SerializeField]
    private TrackDataSO[] _musicTracksArray;

    [SerializeField]
    private AudioSource _audioSource;

    [SerializeField]
    private MusicTrackUI _musicTrackUI;

    private bool _changingSong = false;
    private float _songFadeOutTime = 0.75f;
    private float _songFadeIn = 2f;

    private float _nextSongTime;

    private uint _songID = 0;
    [SerializeField]
    private AudioMixer _audioMixer;
    private float _currentVolume = 0.7f;

    private void Awake()
    {
        _songID = (uint)Random.Range(0, _musicTracksArray.Length);
        StartCoroutine(ChangeSong());
        _audioMixer.SetFloat("AudioVolume", Mathf.Log(_currentVolume) * 20);
    }
    
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        { 
            if(!_changingSong && _musicTrackUI.Hidden)
                StartCoroutine(ChangeSong());
        }

        if(!_changingSong && _musicTrackUI.Hidden && Time.realtimeSinceStartup >= _nextSongTime)
            StartCoroutine(ChangeSong());

        HandleAudio();
    }

    private void HandleAudio()
    {
        if (Input.GetKeyDown(KeyCode.Alpha9))
            _currentVolume -= 0.1f;

        if (Input.GetKeyDown(KeyCode.Alpha0))
            _currentVolume += 0.1f;

        _currentVolume = Mathf.Clamp(_currentVolume, 0.001f, 1);
        _audioMixer.SetFloat("AudioVolume", Mathf.Log(_currentVolume) * 20);
    }

    private IEnumerator ChangeSong()
    {
        _changingSong = true;

        _songID++;
        _musicTrackUI.UpdateAndShowData(_musicTracksArray[_songID % _musicTracksArray.Length]);

        while (_audioSource.volume > 0)
        {
            _audioSource.volume -= Time.deltaTime / _songFadeOutTime;
            yield return null;
        }
        _audioSource.clip = _musicTracksArray[_songID % _musicTracksArray.Length].MusicTrack;
        _audioSource.Play();

        UpdateNextSongTime();

        while (_audioSource.volume < 1)
        {
            _audioSource.volume += Time.deltaTime / _songFadeIn;
            yield return null;
        }
        _changingSong = false;
    }

    private void UpdateNextSongTime()
    {
        _nextSongTime = Time.realtimeSinceStartup + _audioSource.clip.length - _songFadeOutTime;
    }
}

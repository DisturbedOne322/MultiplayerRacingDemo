using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] _musicArray;

    [SerializeField]
    private AudioSource _audioSource;

    private bool _changingSong = false;
    private float _songFadeOutTime = 1f;
    private float _songFadeIn = 1f;

    private float _nextSongTime;

    private uint _songID = 0;

    private void Awake()
    {
        StartCoroutine(ChangeSong());
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        { 
            if(!_changingSong)
                StartCoroutine(ChangeSong());
        }

        if(!_changingSong && Time.realtimeSinceStartup >= _nextSongTime)
            StartCoroutine(ChangeSong());
    }

    private IEnumerator ChangeSong()
    {
        _changingSong = true;

        while(_audioSource.volume > 0)
        {
            _audioSource.volume -= Time.deltaTime / _songFadeOutTime;
            yield return null;
        }

        _songID++;

        _audioSource.clip = _musicArray[_songID % _musicArray.Length];
        UpdateNextSongTime();
        _audioSource.Play();
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

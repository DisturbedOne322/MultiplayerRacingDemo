using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
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

    private void Awake()
    {
        _songID = (uint)Random.Range(0, _musicTracksArray.Length);
        StartCoroutine(ChangeSong());
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

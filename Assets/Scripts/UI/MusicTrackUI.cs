using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MusicTrackUI : MonoBehaviour
{
    [SerializeField]
    private RectMask2D _rectMask;
    [SerializeField]
    private TextMeshProUGUI _authorName;
    [SerializeField] 
    private TextMeshProUGUI _musicName;

    private float _maskWidth = 500;
    private float _widthOffsetMax = 30;

    private float _smDampTime = 0.8f;
    private float _holdTime = 0.5f;
    private float _smDampVelocity;

    public bool Hidden = true;

    public void UpdateAndShowData(TrackDataSO trackDataSO)
    {
        StopAllCoroutines();
        _authorName.text = trackDataSO.AuthorName;
        _musicName.text = trackDataSO.MusicName;
        StartCoroutine(ShowTrack());
    }

    public IEnumerator ShowTrack()
    {
        Hidden = false;
        Vector4 padding = _rectMask.padding;

        while (padding.z > _widthOffsetMax)
        {
            padding.z = Mathf.SmoothDamp(padding.z, 0, ref _smDampVelocity, _smDampTime);
            _rectMask.padding = padding;
            yield return null;
        }

        yield return new WaitForSeconds(_holdTime);

        StartCoroutine(HideTrack());
    }

    private IEnumerator HideTrack()
    {
        Vector4 padding = _rectMask.padding;
        float target = _maskWidth - _widthOffsetMax;

        while (padding.z < target)
        {
            padding.z = Mathf.SmoothDamp(padding.z, _maskWidth, ref _smDampVelocity, _smDampTime / 2);
            _rectMask.padding = padding;
            yield return null;
        }
        Hidden = true;
    }
}

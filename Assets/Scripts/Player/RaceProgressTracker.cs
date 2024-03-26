using Assets.VehicleController;
using System.Collections;
using System.Text;
using TMPro;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class RaceProgressTracker : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _progressText;

    [SerializeField]
    private TextMeshProUGUI _wrongWayText;

    private Transform _playerTransform;

    private SplineContainer _raceLayoutSpline;

    private const float TIMER_UPDATE_DELAY = 0.25f;
    private WaitForSeconds _updateProgressTimer = new WaitForSeconds(TIMER_UPDATE_DELAY);
    private float _time = 0;

    private float _distanceToDisplayWrongWay = 10;

    private float _progress = 0;
    private float _lastProgress = 0;

    private float _displayProgress = 0;

    private bool _wrongWay = false;

    private bool _grow;

    private float _minFontSize = 80;
    private float _maxFontSize = 100;

    private float _fontSizeChangePerSec = 20;
    private float _animSpeed = 5;

    private enum AnimState
    {
        Start,
        Anim,
        End,
        Idle
    }

    private AnimState _state;

    // Start is called before the first frame update
    void Start()
    {
        _state = AnimState.Idle;
        _playerTransform = transform.root;
        RaceStartHandler.OnRaceStart += RaceStartHandler_OnRaceStart;
        _progressText.text = "0.0";
    }

    private void RaceStartHandler_OnRaceStart(SplineContainer raceLayout)
    {
        _raceLayoutSpline = raceLayout;
        _distanceToDisplayWrongWay /= _raceLayoutSpline.CalculateLength();
        StartCoroutine(UpdateProgress());
    }

    private IEnumerator UpdateProgress()
    {
        while (true)
        {
            float3 pos = _playerTransform.position;


            SplineUtility.GetNearestPoint(_raceLayoutSpline.Spline, _raceLayoutSpline.transform.InverseTransformPoint(pos), out float3 temp, out float t);
            _wrongWay = _lastProgress - _distanceToDisplayWrongWay > t;
            _lastProgress = t;

            _displayProgress = _progress;
            if (t > _progress)
            {
               _progress = t;
                if (_progress > 1)
                    _progress = 1;
            }
            _time = 0;
            yield return _updateProgressTimer;
        }
    }

    private void Update()
    {
        HandleWrongWay();
        DisplayProgress();
    }

    private void DisplayProgress()
    {
        _progressText.SetText(string.Format("{0:0.0}", Mathf.Lerp(_displayProgress, _progress, _time) * 100));
        _time += Time.deltaTime / TIMER_UPDATE_DELAY;
    }

    private void HandleWrongWay()
    {
        UpdateState();
        UpdateFontSize();
    }

    private void UpdateState()
    {
        if (_wrongWay)
        {
            if (_state == AnimState.Idle || _state == AnimState.End)
                _state = AnimState.Start;

            if (_state == AnimState.Start && _wrongWayText.fontSize > _minFontSize)
            {
                _grow = true;
                _state = AnimState.Anim;
            }
        }
        else
        {
            if(_state == AnimState.Anim || _state == AnimState.Start)
                _state = AnimState.End;

            if (_state == AnimState.End && _wrongWayText.fontSize <= 0)
            {
                _state = AnimState.Idle;
            }
        }
    }

    private void UpdateFontSize()
    {
        switch(_state)
        {
            case AnimState.Start:
                _wrongWayText.fontSize += Time.deltaTime * _fontSizeChangePerSec * _animSpeed;
                break;
            case AnimState.End:
                _wrongWayText.fontSize -= Time.deltaTime * _fontSizeChangePerSec * _animSpeed;
                break;
            case AnimState.Anim:
                if(_grow)
                {
                    _wrongWayText.fontSize += Time.deltaTime * _fontSizeChangePerSec * _animSpeed;
                    if(_wrongWayText.fontSize >= _maxFontSize)
                        _grow = false;
                }
                else
                {
                    _wrongWayText.fontSize -= Time.deltaTime * _fontSizeChangePerSec * _animSpeed;
                    if (_wrongWayText.fontSize <= _minFontSize)
                        _grow = true;
                }
                break;
            case AnimState.Idle:
                break;
        }
    }
}

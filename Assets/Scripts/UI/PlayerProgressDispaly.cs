using Assets.VehicleController;
using System.Collections;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class PlayerProgressDispaly : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _progressText;

    [SerializeField]
    private WrongWayDisplay _wrongWayDisplay;

    private const float TIMER_UPDATE_DELAY = 0.25f;
    private WaitForSeconds _updateProgressTimer = new WaitForSeconds(TIMER_UPDATE_DELAY);
    private float _time = 0;

    private float _distanceToDisplayWrongWay = 10;

    private float _progress = 0;
    private float _lastProgress = 0;

    private float _displayProgress = 0;

    private bool _wrongWay = false;

    // Start is called before the first frame update
    void Start()
    {
        RaceStartHandler.OnRaceStart += RaceStartHandler_OnRaceStart;
        _progressText.text = "0.0";
    }

    private void RaceStartHandler_OnRaceStart(SplineContainer raceLayout)
    {
        StartCoroutine(UpdateProgress());
    }

    private IEnumerator UpdateProgress()
    {
        while (true)
        {
            float t = 1;
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
        DisplayProgress();
        _wrongWayDisplay.HandleWrongWay(_wrongWay);
    }

    private void DisplayProgress()
    {
        _progressText.SetText(string.Format("{0:0.0}", Mathf.Lerp(_displayProgress, _progress, _time) * 100));
        _time += Time.deltaTime / TIMER_UPDATE_DELAY;
    }
}

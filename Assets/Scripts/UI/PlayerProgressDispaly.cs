using Assets.VehicleController;
using TMPro;
using UnityEngine;

public class PlayerProgressDispaly : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _progressText;
    [SerializeField]
    private TextMeshProUGUI _timeText;

    [SerializeField]
    private RaceStartHandler _raceStartHandler;

    [SerializeField]
    private WrongWayDisplay _wrongWayDisplay;

    private float _distanceToDisplayWrongWay = 10;

    private float _progress = 0;
    private float _lastProgress = 0;

    private bool _wrongWay = false;

    private float _raceTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        _timeText.gameObject.SetActive(false);
        _progressText.text = "0.0";
        _timeText.gameObject.SetActive(false);
        _timeText.text = "0:00:000";
    }

    public void UpdateProgress(float t)
    {
        HandleProgress(t);
        HandleWrongWay(t);
    }

    private void Update()
    {
        HandleTime();
    }

    private void HandleWrongWay(float t)
    {
        _wrongWay = _lastProgress - _distanceToDisplayWrongWay > t;
        _wrongWayDisplay.HandleWrongWay(_wrongWay);
    }

    private void HandleProgress(float t)
    {
        _lastProgress = t;
        if (t > _progress)
        {
            _progress = t;
            if (_progress > 1)
                _progress = 1;
        }
        _progressText.SetText(string.Format("{0:0.0}", _progress * 100));
    }

    private void HandleTime()
    {
        if (!_raceStartHandler.RaceStarted)
            return;

        if (_raceStartHandler.RaceFinished)
            return;

        _raceTime += Time.deltaTime;

        if(!_timeText.gameObject.activeSelf)
            _timeText.gameObject.SetActive(true);

        int minutes = (int)(_raceTime / 60);
        int seconds = (int)(_raceTime % 60);
        int milliseconds = (int)((_raceTime - (int)_raceTime) * 1000);

        _timeText.text = string.Format("{0:D2}:{1:D2}:{2:D3}", minutes, seconds, milliseconds);
    }
}

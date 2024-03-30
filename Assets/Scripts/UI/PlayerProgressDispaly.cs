using Assets.VehicleController;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Splines;

public class PlayerProgressDispaly : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _progressText;

    [SerializeField]
    private WrongWayDisplay _wrongWayDisplay;

    private float _distanceToDisplayWrongWay = 10;

    private float _progress = 0;
    private float _lastProgress = 0;

    private bool _wrongWay = false;

    // Start is called before the first frame update
    void Start()
    {
        _progressText.text = "0.0";
    }

    public void UpdateProgress(float t)
    {
        _wrongWay = _lastProgress - _distanceToDisplayWrongWay > t;
        _lastProgress = t;

        if (t > _progress)
        {
            _progress = t;
            if (_progress > 1)
                _progress = 1;
        }

        _progressText.SetText(string.Format("{0:0.0}", _progress * 100));
        _wrongWayDisplay.HandleWrongWay(_wrongWay);
    }

}

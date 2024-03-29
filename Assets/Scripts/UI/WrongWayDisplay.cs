using TMPro;
using UnityEngine;

public class WrongWayDisplay : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _wrongWayText;

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
    }

    public void HandleWrongWay(bool wrongWay)
    {
        UpdateState(wrongWay);
        UpdateFontSize();
    }

    private void UpdateState(bool wrongWay)
    {
        if (wrongWay)
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
            if (_state == AnimState.Anim || _state == AnimState.Start)
                _state = AnimState.End;

            if (_state == AnimState.End && _wrongWayText.fontSize < 0)
            {
                _state = AnimState.Idle;
            }
        }
    }

    private void UpdateFontSize()
    {
        float sizeChange = Time.deltaTime * _fontSizeChangePerSec * _animSpeed;
        switch (_state)
        {
            case AnimState.Start:
                _wrongWayText.fontSize += sizeChange;
                break;
            case AnimState.End:
                _wrongWayText.fontSize -= sizeChange;
                break;
            case AnimState.Anim:
                if (_grow)
                {
                    _wrongWayText.fontSize += sizeChange;
                    if (_wrongWayText.fontSize >= _maxFontSize)
                        _grow = false;
                }
                else
                {
                    _wrongWayText.fontSize -= sizeChange;
                    if (_wrongWayText.fontSize <= _minFontSize)
                        _grow = true;
                }
                break;
            case AnimState.Idle:
                break;
        }
    }
}

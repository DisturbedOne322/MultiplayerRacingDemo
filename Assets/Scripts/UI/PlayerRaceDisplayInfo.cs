using TMPro;
using Unity.Collections;
using UnityEngine;

public class PlayerRaceDisplayInfo : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _positionText;
    [SerializeField]
    private TextMeshProUGUI _playerNameText;
    [SerializeField]
    private TextMeshProUGUI _distanceText;

    private static string[] _positionNameArray = {"1st", "2nd", "3rd", "4th", "5th", "6th", "7th", "8th"};

    public void Initialize(int position, FixedString32Bytes name)
    {
        _positionText.text = _positionNameArray[position];
        _playerNameText.SetText(name.ToString());
    }

    public void UpdatePosition(FixedString32Bytes name)
    {
        if (_playerNameText.text == name)
            return;

        _playerNameText.SetText(name.ToString());
    }

    public void UpdateDistance(float distance) 
    {
        _distanceText.text = Mathf.Ceil(distance).ToString() + "m";
    }
    public void HideDistance()
    {
        if (_distanceText.text == "")
            return;
        _distanceText.text = "";
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TrackData", fileName = "TrackDataSO")]
public class TrackDataSO : ScriptableObject
{
    public string AuthorName;
    public string MusicName;
    public AudioClip MusicTrack;
}

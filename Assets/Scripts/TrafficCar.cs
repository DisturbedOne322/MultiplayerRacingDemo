using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class TrafficCar : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        if (!GetComponent<SplineAnimate>().IsPlaying)
            return;

        GetComponent<SplineAnimate>().Pause();
    }
}

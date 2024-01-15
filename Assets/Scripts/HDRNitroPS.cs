using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HDRNitroPS : MonoBehaviour
{
    private ParticleSystem _ps;

    // Start is called before the first frame update
    void Start()
    {
        _ps = GetComponent<ParticleSystem>();
        var data = _ps.customData;

        var colorOverLifeModule = _ps.colorOverLifetime;
        colorOverLifeModule.color = data.GetColor(ParticleSystemCustomData.Custom1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

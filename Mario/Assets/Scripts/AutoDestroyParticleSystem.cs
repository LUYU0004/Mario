using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AutoDestroyParticleSystem: MonoBehaviour
{
    private ParticleSystem _particleSystem; //TODO: 

    public void Start() {
        //_particleSystem = GetComponent<ParticleSystem>();

    }

    public void Update() {
        if (true)//_particleSystem.isPlaying)
            return;

        //Destroy(gameObject);
    }
}
using PG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarVFX : MonoBehaviour
{
    [SerializeField] TrailRenderer[] Trails;
    private bool isTrailActive;

    [SerializeField] ParticleSystem[] boostParticles;

    private void Awake()
    {
        foreach (var trail in Trails)
        {
            trail.emitting = false;
        }
    }

    public void UpdateBoost(bool isboost)
    {
        foreach (var boost in boostParticles)
        {
            boost.gameObject.SetActive(isboost);
        }
    }

    public void UpdateTrail(bool isDrifting)
    {
        if (isDrifting)
        {
            if (isTrailActive)
            {
                return;
            }
            isTrailActive = true;
            foreach (var trail in Trails)
            {
                trail.emitting = true;
            }
        }
        else
        {
            if (!isTrailActive)
            {
                return;
            }
            isTrailActive = false;
            foreach (var trail in Trails)
            {
                trail.emitting = false;
            }
        }
    }
}

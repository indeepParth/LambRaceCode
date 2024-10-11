// this script should be used for SPECIFIC Track/MAP ex. Miami, New york
using PG;
using SplineMesh;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.DemiLib;
using DG.Tweening;
using UnityEngine.Rendering.PostProcessing;

public class MyManager : MonoBehaviour
{
    public PostProcessVolume m_Volume;
    public SplineMesh.Spline spline;
    public MyCarController carLambController;
    public Transform carLamb;
    public Rigidbody carLambRigidbody;
    public PickupDropPassangerManager pickupDropPassangerManager;
    public ParticleSystem speedEffectParticle;
    public Camera mainCamera;

    public bool nitro = false;
    private void OnEnable()
    {
        MyGameController.instance.MyCarStateUI.carLambController = carLambController;
        MyGameController.instance.MyCarStateUI.InitializeCar();
    }
    public void OnBoostNitroEnableSpeedEffect()
    {
        mainCamera.DOFieldOfView(75, 1);
        MyGameController.instance.UIManager.BoostNitroReaction();
        speedEffectParticle.time = 0;
        speedEffectParticle.Play();
        MyGameController.instance.MySoundManager.WindSound(true);
        //ChromaticAberration chromaticAberration;
        //m_Volume.profile.TryGetSettings(out chromaticAberration);
        //chromaticAberration.active = true;
    }
    public void DisableSpeedEffect()
    {
        speedEffectParticle.Stop();
        mainCamera.DOFieldOfView(60, 1);
        MyGameController.instance.MySoundManager.WindSound(false);
        //ChromaticAberration chromaticAberration;
        //m_Volume.profile.TryGetSettings(out chromaticAberration);
        //chromaticAberration.active = false;
    }

}

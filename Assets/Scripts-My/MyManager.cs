// this script should be used for SPECIFIC Track/MAP ex. Miami, New york
using PG;
using SplineMesh;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.DemiLib;
using DG.Tweening;
using UnityEngine.Rendering.PostProcessing;
using TMPro;
using System;

[Serializable]
public class GameStartPosition
{
    public GameMode gameMode;
    public Transform carPos, flagPos, cameraPos;
}

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

    [System.NonSerialized]
    public BoxCharacterTrigger boxCharacterTrigger;

    public GameObject grandPrixCheckPoints;
    public TextMeshPro textStartFinish;
    public MinimapTracker minimapTracker;

    public Transform flagStartObject;
    public GameStartPosition[] gameStartPositions;

    ChromaticAberration chromaticAberration;
    LensDistortion distortion;
    DepthOfField depthOfField;
    public float speedEffectTime = 10f;

    public bool nitro = false;
    private void OnEnable()
    {
        SetPositionOfGameStart();
        MyGameController.instance.MyCarStateUI.carLambController = carLambController;
        MyGameController.instance.MyCarStateUI.InitializeCar();

        if (m_Volume.profile.TryGetSettings(out chromaticAberration))
        {
            chromaticAberration.intensity.value = 0f;
        }
        if (m_Volume.profile.TryGetSettings(out distortion))
        {
            distortion.intensity.value = 0f;
        }
        if (m_Volume.profile.TryGetSettings(out depthOfField))
        {
            depthOfField.focalLength.value = 0f;
        }

    }

    private void SetPositionOfGameStart()
    {
        GameStartPosition gameStartPosition = gameStartPositions[((int)MyGameController.instance.gameMode) - 1];
        carLamb.position = gameStartPosition.carPos.position;
        carLamb.rotation = gameStartPosition.carPos.rotation;
        mainCamera.transform.position = gameStartPosition.cameraPos.position;
        mainCamera.transform.rotation = gameStartPosition.cameraPos.rotation;
        flagStartObject.transform.position = gameStartPosition.flagPos.position;
        flagStartObject.transform.rotation = gameStartPosition.flagPos.rotation;
        carLamb.gameObject.SetActive(true);
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

    
    public void HighSpeedEffect(bool isHighSpeed)
    {
        if (isHighSpeed)
        {
            chromaticAberration.intensity.value = Mathf.Lerp(chromaticAberration.intensity.value, 1f, Time.deltaTime * speedEffectTime);
            distortion.intensity.value = Mathf.Lerp(distortion.intensity.value, -80f, Time.deltaTime * speedEffectTime);
        }
        else
        {
            chromaticAberration.intensity.value = Mathf.Lerp(chromaticAberration.intensity.value, 0f, Time.deltaTime * speedEffectTime * 1);
            distortion.intensity.value = Mathf.Lerp(distortion.intensity.value, 0f, Time.deltaTime * speedEffectTime * 1);
        }
    }

}

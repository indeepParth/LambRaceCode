using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MySoundManager : MonoBehaviour
{
    public AudioSource sourceOne;
    public AudioSource source_RaceTrack;
    public AudioSource source_Engine;
    public AudioSource source_Wind; // also used for drift sound
    public AudioSource source_HomeBackground;

    public AudioClip engine;
    public AudioClip wohooo;
    public AudioClip wind;
    public AudioClip beep321;
    public AudioClip drift;
    public AudioClip crashCar;
    public AudioClip breakSound;
    public AudioClip oilSlip;
    public AudioClip feelTheFOMOOOO;

    [Space(10)]
    public Image soundImage;
    public Sprite soundOnImage;
    public Sprite soundOffImage;

    [Space(10)]
    [Header("Engine Sound Settings")]
    public float minPitch = 0.8f;  // Idle pitch
    public float maxPitch = 2.0f;  // Max pitch at top speed
    public float topSpeed = 100f;  // Speed at which pitch is maxed out
    private bool isEnginePlaying = false;

    void Awake()
    {
        source_Engine.loop = true;
        if (PlayerPrefsX.GetBool(Utility.KEY_SOUND, true))
        {
            soundImage.sprite = soundOnImage;
            BackgroundOnHomeSound(true);
        }
        else
        {
            soundImage.sprite = soundOffImage;
        }
    }

    public void Btn_SoundOnOff()
    {
        if (PlayerPrefsX.GetBool(Utility.KEY_SOUND, true))
        {
            PlayerPrefsX.SetBool(Utility.KEY_SOUND, false);
            soundImage.sprite = soundOffImage;
            BackgroundOnHomeSound(false);
            RaceTrackSound(false);
            EngineSound(false);
            WindSound(false);
        }
        else
        {
            PlayerPrefsX.SetBool(Utility.KEY_SOUND, true);
            soundImage.sprite = soundOnImage;
            BackgroundOnHomeSound(true);
            // RaceTrackSound(true);
            // EngineSound(true);
            // WindSound(true);
        }
    }

    public void BackgroundOnHomeSound(bool toPlay)
    {
        if (toPlay && PlayerPrefsX.GetBool(Utility.KEY_SOUND, true))
        {
            source_HomeBackground.Play();
        }
        else
        {
            source_HomeBackground.Stop();
        }
    }

    public void RaceTrackSound(bool toPlay)
    {
        if (toPlay && PlayerPrefsX.GetBool(Utility.KEY_SOUND, true))
        {
            source_RaceTrack.Play();
        }
        else
        {
            source_RaceTrack.Stop();
        }
    }

    public void EngineSound(bool toPlay)
    {
        if (toPlay && PlayerPrefsX.GetBool(Utility.KEY_SOUND, true))
        {
            isEnginePlaying = true;
            if (!source_Engine.isPlaying)
                source_Engine.Play();
        }
        else
        {
            isEnginePlaying = false;
            if (source_Engine.isPlaying)
                source_Engine.Pause();
        }
    }
    void Update()
    {
        if (isEnginePlaying && PlayerPrefsX.GetBool(Utility.KEY_SOUND, true) && MyGameController.instance.isGameStart && !MyGameController.instance.isGameOver)
        {
            float speed = MyGameController.instance.MyManager.carLambController.currentSpeed;//.carLambRigidbody.velocity.magnitude;
            // Normalize speed and map it to pitch range
            float pitch = Mathf.Lerp(minPitch, maxPitch, speed / topSpeed);
            source_Engine.pitch = pitch;
        }
    }

    public void WindSound(bool toPlay)
    {
        if (toPlay && PlayerPrefsX.GetBool(Utility.KEY_SOUND, true))
        {
            source_Wind.clip = wind;
            source_Wind.volume = 0.7f;
            source_Wind.Play();
        }
        else
        {
            source_Wind.Stop();
        }
    }

    public void DriftSound(bool toPlay)
    {
        if (toPlay && PlayerPrefsX.GetBool(Utility.KEY_SOUND, true))
        {
            source_Wind.clip = drift;
            source_Wind.volume = 0.3f;
            if (!source_Wind.isPlaying)
                source_Wind.Play();
        }
        else
        {
            if (source_Wind.clip == drift && source_Wind.isPlaying)
                source_Wind.Stop();
        }
    }

    public void BreakSound(bool toPlay)
    {
        if (toPlay && PlayerPrefsX.GetBool(Utility.KEY_SOUND, true))
        {
            source_Wind.clip = breakSound;
            source_Wind.volume = 0.2f;
            if (!source_Wind.isPlaying)
                source_Wind.Play();
        }
        else
        {
            if (source_Wind.clip == breakSound && source_Wind.isPlaying)
                source_Wind.Stop();
        }
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (sourceOne != null && PlayerPrefsX.GetBool(Utility.KEY_SOUND, true))
        {
            sourceOne.PlayOneShot(clip);
        }
    }
    public void PlayWohooo()
    {
        PlayOneShot(wohooo);
    }
    public void PlayBeep321()
    {
        PlayOneShot(beep321);
    }
    public void PlayCrashCar()
    {
        PlayOneShot(crashCar);
    }
    public void PlayOilSlipCar()
    {
        PlayOneShot(oilSlip);
    }
    public void PlayCarSuperSpeedFEELFOMOSound()
    {
        PlayOneShot(feelTheFOMOOOO);
    }
}

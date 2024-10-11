using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MySoundManager : MonoBehaviour
{
    public AudioSource sourceOne;
    public AudioSource source_RaceTrack;
    public AudioSource source_Engine;
    public AudioSource source_Wind;

    public AudioClip engine;
    public AudioClip wohooo;
    public AudioClip wind;

    public void RaceTrackSound(bool toPlay)
    {
        if (toPlay)
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
        return;
        if (toPlay)
        {
            if(!source_Engine.isPlaying)
                source_Engine.Play();
        }
        else
        {
            if (source_Engine.isPlaying)
                source_Engine.Stop();
        }
    }

    public void WindSound(bool toPlay)
    {
        if (toPlay)
        {
            source_Wind.Play();
        }
        else
        {
            source_Wind.Stop();
        }
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (sourceOne != null)
        {
            sourceOne.PlayOneShot(clip);
        }
    }
    public void PlayWohooo()
    {
        PlayOneShot(wohooo);
    }
}

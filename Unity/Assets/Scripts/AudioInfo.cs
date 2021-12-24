using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script allows you to toggle music to play and stop.
//Assign an AudioSource to a GameObject and attach an Audio Clip in the Audio Source. Attach this script to the GameObject.

public class AudioInfo : MonoBehaviour
{
    public AudioSource adSource;
    public List<AudioClip> adClips;

    public void Start()
    {
        adSource = GameObject.FindGameObjectWithTag("AudioInfo").transform.GetChild(0).gameObject.GetComponent<AudioSource>();
    }

    public void PlayAudio(int i)
    {
        adSource.clip = adClips[i];
        adSource.Play();
    }
}
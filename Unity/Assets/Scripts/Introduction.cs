using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Introduction : MonoBehaviour
{
    public GameObject HoloIntro, HoloLogo, AudioInfo;

    // Start is called before the first frame update
    void Start()
    {
        HoloIntro = GameObject.FindGameObjectWithTag("HoloIntro");
        HoloLogo = GameObject.FindGameObjectWithTag("HoloLogo").transform.GetChild(0).gameObject;
        AudioInfo = GameObject.FindGameObjectWithTag("AudioInfo").transform.GetChild(0).gameObject;

        Invoke("HideIntro", 8);
    }

    void HideIntro()
    {
        if (HoloIntro.activeInHierarchy == true) HoloIntro.SetActive(false);
        if (HoloLogo.activeInHierarchy == false) HoloLogo.SetActive(true);
        if (AudioInfo.activeInHierarchy == false) AudioInfo.SetActive(true);

        SceneOrganiser.Instance.SetCameraStatus(" ");
    }
}

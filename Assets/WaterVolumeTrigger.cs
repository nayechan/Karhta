using System;
using UnityEngine;
using UnityEngine.Rendering;

public class WaterVolumeTrigger : MonoBehaviour
{
    public Volume localVolume;

    private void Awake()
    {
        localVolume = GetComponent<Volume>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            localVolume.weight = 1f;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            localVolume.weight = 0f;
        }
    }
}
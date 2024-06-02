using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Backdrop : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    public void Activate(string content)
    {
        gameObject.SetActive(true);
        text.text = content;
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}

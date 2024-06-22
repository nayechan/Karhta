using System;
using System.Collections;
using System.Collections.Generic;
using InGame.Player;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private TMP_Text xpBonusText, allStatBonusText, noBonusText;
    [SerializeField] private Transform bonusTransform, noBonusTransform;

    private int currentLevel;
    private int thresholdLevel;

    private float currentXpMultiplier, currentAllStatMultiplier;

    [SerializeField] private int levelIncrease = 2;
    [SerializeField] private float multiplier = 1.5f;
    
    private void OnEnable()
    {
        currentLevel = (int)player.currentLevel;
        thresholdLevel = PlayerPrefs.GetInt("thresholdLevel", 3);

        currentXpMultiplier = PlayerPrefs.GetFloat("currentXpMultiplier", 1);
        currentAllStatMultiplier = PlayerPrefs.GetFloat("currentAllStatMultiplier", 1);

        xpBonusText.text = 
            $"XP\nx{currentXpMultiplier:0.00} -> x{currentXpMultiplier * multiplier : 0.00}";
        
        allStatBonusText.text = 
            $"All Stat\nx{currentAllStatMultiplier:0.00} -> x{currentAllStatMultiplier * multiplier : 0.00}";
        
        noBonusText.text = $"No Bonus (Should reach Lv. {thresholdLevel})";

        bonusTransform.gameObject.SetActive(currentLevel >= thresholdLevel);
        noBonusTransform.gameObject.SetActive(currentLevel < thresholdLevel);
    }

    public void SelectXP()
    {
        PlayerPrefs.SetInt("thresholdLevel", thresholdLevel + levelIncrease);
        PlayerPrefs.SetFloat("currentXpMultiplier", currentXpMultiplier * multiplier);
        GoBackToMain();
    }

    public void SelectAllStat()
    {
        PlayerPrefs.SetInt("thresholdLevel", thresholdLevel + levelIncrease);
        PlayerPrefs.SetFloat("currentAllStatMultiplier", currentAllStatMultiplier * multiplier);
        GoBackToMain();
    }

    public void GoBackToMain()
    {
        SceneManager.LoadScene("Main");
    }
}

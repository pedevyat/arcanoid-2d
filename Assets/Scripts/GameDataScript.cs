using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "Game Data", order = 51)]
public class GameDataScript : ScriptableObject
{
    public bool resetOnStart;
    public int level = 1; 
    public int balls = 6; 
    public int points = 0;
    public bool music = true;
    public bool sound = true;
    public float musicVolume = 0.2f;
    public float soundVolume = 1f;
    public int pointsToBall = 0;

    [Header("Bonus appearance weights")]
    public int slowBonusWeight = 20;
    public int fastBonusWeight = 20;
    public int ballBonusWeight = 20;
    public int plus2BonusWeight = 20;
    public int plus10BonusWeight = 20;
    public int fireBonusWeight = 20;
    public int steelBonusWeight = 20;
    public int normBonusWeight = 20;
    public int expandBonusWeight = 20;
    public int shrinkBonusWeight = 20;
    public int stickyBonusWeight = 20;
    public int simpleBonusWeight = 20;

    public BonusType GetRandomBonus()
    {
        int totalWeight = slowBonusWeight + fastBonusWeight + ballBonusWeight +
                          plus2BonusWeight + plus10BonusWeight + fireBonusWeight +
                          steelBonusWeight + normBonusWeight + expandBonusWeight +
                          shrinkBonusWeight + stickyBonusWeight + simpleBonusWeight;

        if (totalWeight <= 0)
            return BonusType.Ball;

        int value = Random.Range(0, totalWeight);

        if ((value -= slowBonusWeight) < 0)
            return BonusType.Slow;
        if ((value -= fastBonusWeight) < 0)
            return BonusType.Fast;
        if ((value -= ballBonusWeight) < 0)
            return BonusType.Ball;
        if ((value -= plus2BonusWeight) < 0)
            return BonusType.Plus2;
        if ((value -= plus10BonusWeight) < 0)
            return BonusType.Plus10;
        if ((value -= fireBonusWeight) < 0)
            return BonusType.Fire;
        if ((value -= steelBonusWeight) < 0)
            return BonusType.Steel;
        if ((value -= normBonusWeight) < 0)
            return BonusType.Norm;
        if ((value -= expandBonusWeight) < 0)
            return BonusType.Expand;
        if ((value -= shrinkBonusWeight) < 0)
            return BonusType.Shrink;
        if ((value -= stickyBonusWeight) < 0)
            return BonusType.Sticky;
        return BonusType.Simple;
    }

    public void Reset()
    { 
        level = 1; 
        balls = 6; 
        points = 0;
        pointsToBall = 0;
    }

    public void Save() 
    { 
        PlayerPrefs.SetInt("level", level); 
        PlayerPrefs.SetInt("balls", balls);
        PlayerPrefs.SetInt("points", points);
        PlayerPrefs.SetInt("pointsToBall", pointsToBall); 
        PlayerPrefs.SetInt("music", music ? 1 : 0); 
        PlayerPrefs.SetInt("sound", sound ? 1 : 0); 
        PlayerPrefs.SetFloat("musicVolume", musicVolume);
        PlayerPrefs.SetFloat("soundVolume", soundVolume);
    }

    public void Load() 
    { 
        level = PlayerPrefs.GetInt("level", 1); 
        balls = PlayerPrefs.GetInt("balls", 6); 
        points = PlayerPrefs.GetInt("points", 0); 
        pointsToBall = PlayerPrefs.GetInt("pointsToBall", 0);
        music = PlayerPrefs.GetInt("music", 1) == 1; 
        sound = PlayerPrefs.GetInt("sound", 1) == 1; 
        musicVolume = PlayerPrefs.GetFloat("musicVolume", 0.2f);
        soundVolume = PlayerPrefs.GetFloat("soundVolume", 1f);
    }
}

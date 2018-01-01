using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

[Serializable]
public class Character
{
    public string name;
    public int id;
    public CharacterStatistic statistic;

    public delegate void SaveStatsCallback();
    public SaveStatsCallback SaveStats;

    public Character(string charName, SaveStatsCallback SaveStatsCallback)
    {
        name = charName;
        statistic = new CharacterStatistic();
        SaveStats = SaveStatsCallback;
    }

    public void Win()
    {
        Debug.Log("Win name " + name);
        statistic.win++;
        SaveStats();
    }

    public void Loose()
    {
        Debug.Log("Loose name " + name);
        statistic.loose++;
        SaveStats();
    }

    public void ResetStats()
    {
        statistic.Reset();        
    }
       
}

[Serializable]
public class CharacterStatistic
{
    public int win = 0;
    public int loose = 0;

    public void Reset()
    {
        win = 0;
        loose = 0;
    }
}
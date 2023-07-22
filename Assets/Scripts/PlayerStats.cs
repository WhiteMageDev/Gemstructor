using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static event Action<string, string, int> OnGetAchievement;
    private static IDataService DataService = new JsonDataService();
    private static Achievements achievements;
    public static List<Achievement> achievementsList;
    public static Achievements Achievements()
    {
        return achievements;
    }
    public static int CompletedLevelsCounter;
    public static void IncCompletedLevelsCounter()
    {
        CompletedLevelsCounter++;
        if (achievements.CompletedLevels.IncCapacityIsLvlUp())
        {
            string name = achievements.CompletedLevels.NAME();
            string desc = achievements.CompletedLevels.DESC();
            OnGetAchievement?.Invoke(name, desc, achievements.CompletedLevels.num);
        }
    }
    public static int DestroyedGemsCounter;
    public static void IncDestroyedGemsCounter()
    {
        DestroyedGemsCounter++;
        if (achievements.DestroyedGems.IncCapacityIsLvlUp())
        {
            string name = achievements.DestroyedGems.NAME();
            string desc = achievements.DestroyedGems.DESC();
            OnGetAchievement?.Invoke(name, desc, achievements.DestroyedGems.num);
        }
    }
    public static int DestroyedGlassCounter;
    public static void IncDestroyedGlassCounter()
    {
        DestroyedGlassCounter++;
        if (achievements.DestroyedGlass.IncCapacityIsLvlUp())
        {
            string name = achievements.DestroyedGlass.NAME();
            string desc = achievements.DestroyedGlass.DESC();
            OnGetAchievement?.Invoke(name, desc, achievements.DestroyedGlass.num);
        }
    }
    public static int CreatedRegularBombsCounter;
    public static void IncCreatedRegularBombsCounter()
    {
        CreatedRegularBombsCounter++;
        if (achievements.CreatedRegularBombs.IncCapacityIsLvlUp())
        {
            string name = achievements.CreatedRegularBombs.NAME();
            string desc = achievements.CreatedRegularBombs.DESC();
            OnGetAchievement?.Invoke(name, desc, achievements.CreatedRegularBombs.num);
        }
    }
    public static int CreatedTshapeBombsCounter;
    public static void IncCreatedTshapeBombsCounter()
    {
        CreatedTshapeBombsCounter++;
        if (achievements.CreatedTshapeBombs.IncCapacityIsLvlUp())
        {
            string name = achievements.CreatedTshapeBombs.NAME();
            string desc = achievements.CreatedTshapeBombs.DESC();
            OnGetAchievement?.Invoke(name, desc, achievements.CreatedTshapeBombs.num);
        }
    }
    public static int BreakStoneWallCounter;
    public static void IncBreakStoneWallCounter()
    {
        BreakStoneWallCounter++;
        if (achievements.BreakStoneWall.IncCapacityIsLvlUp())
        {
            string name = achievements.BreakStoneWall.NAME();
            string desc = achievements.BreakStoneWall.DESC();
            OnGetAchievement?.Invoke(name, desc, achievements.BreakStoneWall.num);
        }
    }
    public static int RandomLevelBCounter;
    public static void IncRandomLevelBCounter()
    {
        RandomLevelBCounter++;
        if (achievements.RandomLevelB.IncCapacityIsLvlUp())
        {
            string name = achievements.RandomLevelB.NAME();
            string desc = achievements.RandomLevelB.DESC();
            OnGetAchievement?.Invoke(name, desc, achievements.RandomLevelB.num);
        }
    }
    public static int RandomLevelRCounter;
    public static void IncRandomLevelRCounter(int s)
    {
        RandomLevelRCounter+=s;
        if (RandomLevelRCounter > achievements.RandomLevelR.capacity)
            if (achievements.RandomLevelR.IncCapacityIsLvlUp(RandomLevelRCounter))
            {
                string name = achievements.RandomLevelR.NAME();
                string desc = achievements.RandomLevelR.DESC();
                OnGetAchievement?.Invoke(name, desc, achievements.RandomLevelR.num);
            }
    }
    public static int RandomLevelTCounter;
    public static void IncRandomLevelTCounter(int s)
    {
        RandomLevelTCounter+=s;
        if(RandomLevelTCounter > achievements.RandomLevelT.capacity)
            if (achievements.RandomLevelT.IncCapacityIsLvlUp(RandomLevelTCounter))
            {
                string name = achievements.RandomLevelT.NAME();
                string desc = achievements.RandomLevelT.DESC();
                OnGetAchievement?.Invoke(name, desc, achievements.RandomLevelT.num);
            }

    }
    void LoadStatsDataFromFile()
    {
        achievements = DataService.LoadData<Achievements>(Utils.pathAchievements);
    }
    private void Awake()
    {
        LoadSavedStats();
    }
    private void OnDisable()
    {
        SaveStats();
    }
    void LoadSavedStats()
    {
        LoadStatsDataFromFile();

        achievementsList = new();
        CompletedLevelsCounter = achievements.CompletedLevels.capacity;
        DestroyedGemsCounter = achievements.DestroyedGems.capacity;
        DestroyedGlassCounter = achievements.DestroyedGlass.capacity;
        CreatedRegularBombsCounter = achievements.CreatedRegularBombs.capacity;
        CreatedTshapeBombsCounter = achievements.CreatedTshapeBombs.capacity;
        BreakStoneWallCounter = achievements.BreakStoneWall.capacity;
        RandomLevelBCounter = achievements.RandomLevelB.capacity;
        RandomLevelRCounter = 0;
        RandomLevelTCounter = 0;

        achievementsList.Add(achievements.CompletedLevels);
        achievementsList.Add(achievements.DestroyedGems);
        achievementsList.Add(achievements.DestroyedGlass);
        achievementsList.Add(achievements.CreatedRegularBombs);
        achievementsList.Add(achievements.CreatedTshapeBombs);
        achievementsList.Add(achievements.BreakStoneWall);
        achievementsList.Add(achievements.RandomLevelB);
        achievementsList.Add(achievements.RandomLevelR);
        achievementsList.Add(achievements.RandomLevelT);
    }
    void SaveStats()
    {
        if (DataService.SaveData(Utils.pathAchievements, achievements))
            Debug.Log("Achievements saved");
        else
            Debug.Log("Achievements save error");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Achievements
{
    public Achievements()
    {
        InitializeAchievements();
    }

    public Achievement CompletedLevels;
    private readonly string desc_CompletedLevels = "Levels completed with 5 stars";
    private readonly string name_CompletedLevels = "Perfection";
    private readonly List<int> lvlup_CompletedLevels = new() { 1, 5, 10, 20, 40 };

    public Achievement DestroyedGems;
    private readonly string desc_DestroyedGems = "Total gems destroyed";
    private readonly string name_DestroyedGems = "Exterminator";
    private readonly List<int> lvlup_DestroyedGems = new() { 50, 250, 1250, 6250, 31250 };

    public Achievement DestroyedGlass;
    private readonly string desc_DestroyedGlass = "Total glass broken";
    private readonly string name_DestroyedGlass = "Glass crusher";
    private readonly List<int> lvlup_DestroyedGlass = new() { 5, 25, 125, 625, 3125 };

    public Achievement CreatedRegularBombs;
    private readonly string desc_CreatedRegularBombs = "Created regular bombs";
    private readonly string name_CreatedRegularBombs = "Apprentice Bomber";
    private readonly List<int> lvlup_CreatedRegularBombs = new() { 2, 10, 50, 250, 1250 };

    public Achievement CreatedTshapeBombs;
    private readonly string desc_CreatedTshapeBombs = "Created boosted bombs";
    private readonly string name_CreatedTshapeBombs = "Expert Bomber";
    private readonly List<int> lvlup_CreatedTshapeBombs = new() { 1, 5, 25, 125, 625 };

    public Achievement BreakStoneWall;
    private readonly string desc_BreakStoneWall = "Stone walls broken";
    private readonly string name_BreakStoneWall = "Stonecrusher";
    private readonly List<int> lvlup_BreakStoneWall = new() { 1, 5, 25, 125, 625 };

    public Achievement RandomLevelB;
    private readonly string desc_RandomLevelB = "Level is passed";
    private readonly string name_RandomLevelB = "Random B";
    private readonly List<int> lvlup_RandomLevelB = new() { 1, 2, 3, 4, 5 };

    public Achievement RandomLevelR;
    private readonly string desc_RandomLevelR = "Absolute record";
    private readonly string name_RandomLevelR = "Random R";
    private readonly List<int> lvlup_RandomLevelR = new() { 750, 1500, 3000, 6000, 12000 };

    public Achievement RandomLevelT;
    private readonly string desc_RandomLevelT = "Absolute record";
    private readonly string name_RandomLevelT = "Random T";
    private readonly List<int> lvlup_RandomLevelT = new() { 100, 500, 1000, 2000, 4000 };

    public void InitializeAchievements()
    {
        CompletedLevels = new Achievement(name_CompletedLevels, desc_CompletedLevels, lvlup_CompletedLevels, 0);
        DestroyedGems = new Achievement(name_DestroyedGems, desc_DestroyedGems, lvlup_DestroyedGems, 1);
        DestroyedGlass = new Achievement(name_DestroyedGlass, desc_DestroyedGlass, lvlup_DestroyedGlass, 2);
        CreatedRegularBombs = new Achievement(name_CreatedRegularBombs, desc_CreatedRegularBombs, lvlup_CreatedRegularBombs, 3);
        CreatedTshapeBombs = new Achievement(name_CreatedTshapeBombs, desc_CreatedTshapeBombs, lvlup_CreatedTshapeBombs, 4);
        BreakStoneWall = new Achievement(name_BreakStoneWall, desc_BreakStoneWall, lvlup_BreakStoneWall, 5);
        RandomLevelB = new Achievement(name_RandomLevelB, desc_RandomLevelB, lvlup_RandomLevelB, 6);
        RandomLevelR = new Achievement(name_RandomLevelR, desc_RandomLevelR, lvlup_RandomLevelR, 7);
        RandomLevelT = new Achievement(name_RandomLevelT, desc_RandomLevelT, lvlup_RandomLevelT, 8);
    }
}

[System.Serializable]
public class Achievement
{
    public Achievement(string N, string D, List<int> list, int n)
    {
        num = n;
        level = 0;
        capacity = 0;
        capacityToLvlUp = new(list);

        _NAME = N;
        _DESC = D;
    }
    public bool IncCapacityIsLvlUp()
    {
        capacity++;
        if (level >= 5) return false;
        if (capacity >= capacityToLvlUp[level])
        {
            level++;
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool IncCapacityIsLvlUp(int c)
    {
        capacity = c;
        if (level >= 5) return false;
        if (capacity >= capacityToLvlUp[level])
        {
            level++;
            return true;
        }
        else
        {
            return false;
        }
    }
    public int num;
    public int level;
    public int capacity;
    private string _NAME;
    public string NAME()
    {
        return $"{_NAME} LVL {level}";
    }
    private string _DESC;
    public string DESC()
    {
        return $"{_DESC}: {capacity}";
    }

    public List<int> capacityToLvlUp;


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static readonly Collection Ñollection = LoadObject<Collection>("Collection");
    public static T LoadObject<T>(string name) where T : Object
    {
        return Resources.Load<T>(name);
    }
    public static readonly string pathAchievements = "/player-saved-data-achievements.json";
    public static readonly string pathLvlData = "/player-saved-data.json";
}

[System.Serializable]
public class LvlData
{
    public List<Lvl> Lvls;
    public int power;
    public LvlData()
    {
        Lvls = null;
        power = 0;
    }
    public List<Lvl> GetList()
    {
        return Lvls;
    }
    public void Add(Lvl item)
    {
        Lvls.Add(item);
    }
}
[System.Serializable]
public class Lvl
{
    public int num;
    public bool isLocked;
    public bool isDone;
    public int stars;
    public Lvl(int n, bool l, bool d, int s)
    {
        num = n;
        isLocked = l;
        isDone = d;
        stars = isDone ? s : -1;
    }
}
public class BotLinkInfo
{
    public BotLinkInfo()
    {
        one = null; two = null; size = 0; isNull = true; isGlassNear = false;
    }
    public Point one { get; set; }
    public Point two { get; set; }
    public int size { get; set; }
    public bool isGlassNear;
    public bool isNull;
}
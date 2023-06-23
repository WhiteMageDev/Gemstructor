using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class LevelSO : ScriptableObject
{
    public enum Limit
    {
        Time,
        Moves,
        Blocks,
        None,
    }
    public enum GoalType
    {
        Score,
        Glass,
        None,
    }
    public int w;
    public int h;
    public List<GemSO> gemList;
    public List<GemData> boardDataList;

    public int moves;
    public int time;
    public int goalScore;
    public int goalScoreMin;
    public int goalGlass;
    public int goalGlassMin;

    public Limit limit;
    public GoalType goalType;
    public void SetData(List<GemData> list)
    {
        boardDataList = new(list);
    }


}

[System.Serializable]
public class GemData
{
    public GemSO gemSO;
    public bool hasGlass;

    public GemData(GemSO g, bool glass)
    {
        gemSO = g;
        hasGlass = glass;
    }
}

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class SelectorSO : ScriptableObject
{
    public List<SelectorData> levels;

    public void SetData(List<SelectorData> item)
    {
        levels = new(item);
    }
}

[System.Serializable]
public class SelectorData
{
    public int num;
    public bool islocked;
    public bool isDone;
    public int stars;
    public SelectorData(int n, bool d, int s)
    {
        num = n;
        isDone = d;
        if (isDone)
            stars = s;
        else
            stars = -1;
    }
    public SelectorData(int n, bool d)
    {
        num = n;
        isDone = d;
        stars = -1;
    }
}

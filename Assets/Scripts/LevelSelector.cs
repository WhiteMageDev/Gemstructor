using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour
{
    public GameObject leveDone;
    public GameObject levelLocked;
    public Transform content;
    public Sprite starAtive;

    private IDataService DataService = new JsonDataService();

    private void Start()
    {
        List<bool> openLevels = new();
        openLevels.Add(true);
        for (int i = 0; i < 12; i++)
        {
            openLevels.Add(false);
        }
        LvlData lvlData = null;
        if (DataService.IsFileExists("/player-saved-data.json"))
        {
            lvlData = DataService.LoadData<LvlData>("/player-saved-data.json");

            for (int i = 0; i < lvlData.GetList().Count; i++)
            {
                if (lvlData.GetList()[i].isDone)
                    if (openLevels.Count > i + 1)
                        openLevels[i + 1] = true;
            }
        }
        for (int i = 0; i < 12; i++)
        {
            bool open = openLevels[i];

            if (open)
            {
                GameObject level = Instantiate(leveDone, content);
                level.name = (i + 1).ToString();
                level.GetComponent<Button>().onClick.AddListener(delegate { Onclick(Convert.ToInt32(level.name)); });
                level.transform.GetChild(0).GetComponent<TMP_Text>().text = (i + 1).ToString();
                if (lvlData != null)
                {
                    if (lvlData.GetList().Count > i)
                    {
                        int stars = lvlData.GetList()[i].stars;

                        for (int j = 0; j < stars; j++)
                        {
                            level.transform.GetChild(1).GetChild(j).GetComponent<Image>().sprite = starAtive;
                        }

                    }
                }

            }
            else
            {
                GameObject level = Instantiate(levelLocked, content);
            }
        }
    }
    public void Onclick(int i)
    {
        PlayerPrefs.SetInt("LVLTOLOAD", i);
        SoundManager.PlaySound(SoundManager.Sound.Click);
        GameObject.Find("Fader").GetComponent<SceneFader>().FadeTo("DefaultLevel");
    }
}

[System.Serializable]
public class LvlData
{
    public List<Lvl> Lvls;
    public LvlData()
    {
        Lvls = new();
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

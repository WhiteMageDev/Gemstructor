using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour
{
    private IDataService DataService = new JsonDataService();

    private GameObject leveDone;
    private GameObject levelLocked;
    private int levelsCount;
    public Transform content;
    private void Awake()
    {
        leveDone = Utils.Ñollection.LevelDone;
        levelLocked = Utils.Ñollection.LevelLocked;
        levelsCount = Utils.Ñollection.listOfLevels.Count;
    }
    private void Start()
    {
        List<bool> openLevels = new();
        openLevels.Add(true);
        for (int i = 0; i < levelsCount; i++)
        {
            openLevels.Add(false);
        }

        LvlData lvlData = DataService.LoadData<LvlData>(Utils.pathLvlData);

        if (lvlData.Lvls.Count > 0)
            for (int i = 0; i < lvlData.GetList().Count; i++)
            {
                if (lvlData.GetList()[i].isDone)
                    if (openLevels.Count > i + 1)
                        openLevels[i + 1] = true;
            }

        for (int i = 0; i < levelsCount; i++)
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
                            level.transform.GetChild(1).GetChild(j).GetComponent<Image>().sprite = Utils.Ñollection.activeStar;
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
        GameObject.Find("NavController-Fader").GetComponent<SceneFader>().FadeTo("DefaultLevel");
    }
}

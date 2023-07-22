using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementsController : MonoBehaviour
{
    public List<GameObject> achieveGO;

    public GameObject achievementInfo;
    public Image progressBar;
    private void Start()
    {
        int lvlSumm = 0;
        for (int i = 0; i < PlayerStats.achievementsList.Count; i++)
        {
            lvlSumm += SetAchievementUI(PlayerStats.achievementsList[i], achieveGO[i], Utils.Ñollection.AchievementSprites[i]);
        }
        progressBar.fillAmount = (float)lvlSumm / (PlayerStats.achievementsList.Count * 5);
    }
    int SetAchievementUI(Achievement achievement, GameObject achieveGO, Sprite sprite)
    {
        int lvl = achievement.level;
        if (lvl == 0)
        {
            achieveGO.transform.Find("Logo").GetComponent<Image>().sprite = Utils.Ñollection.AchievementLock;
            achieveGO.transform.Find("Level").gameObject.SetActive(false);
        }
        else
        {
            achieveGO.transform.Find("Logo").GetComponent<Image>().sprite = sprite;
            achieveGO.transform.Find("Level/Text").GetComponent<TMP_Text>().text = $"LVL {lvl}";
            if (lvl == 5)
            {
                achieveGO.transform.Find("Level").GetComponent<Image>().color = Color.yellow;
                achieveGO.GetComponent<Image>().color = Color.yellow;
            }
            achieveGO.GetComponent<Button>().onClick.AddListener(delegate { AchievementButton(achievement); });
        }
        return lvl;
    }
    public void AchievementButton(Achievement achievement)
    {
        achievementInfo.transform.Find("NAME").GetComponent<TMP_Text>().text = achievement.NAME();
        achievementInfo.transform.Find("DESC").GetComponent<TMP_Text>().text = achievement.DESC();

        Transform b = achievementInfo.transform.Find("g/b");
        List<Transform> g = new();

        for (int i = 0; i < b.childCount; i++)
        {
            g.Add(b.GetChild(i));
        }
        for (int i = 0; i < g.Count; i++)
        {
            g[i].GetChild(0).GetComponent<TMP_Text>().text = achievement.capacityToLvlUp[i].ToString();
            Image img = g[i].GetComponent<Image>();
            if (achievement.level == i + 1)
                img.color = Color.yellow;
            else
                img.color = Color.white;
        }
        achievementInfo.SetActive(true);
    }
}

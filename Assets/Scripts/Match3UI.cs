using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Match3UI : MonoBehaviour
{
    [SerializeField] private Match3 match3;
    public TMP_Text _timer;
    public TMP_Text _score;
    public GameObject scorePan;
    public TMP_Text _glass;
    public GameObject glassPan;
    public TMP_Text _moves;
    public GameObject gameOverPanel;
    public GameObject randomLevelPanel;

    public GameObject powerPanel;
    public TMP_Text _power;
    public TMP_Text _winlose;
    public GameObject startPanel;
    public List<Image> stars;
    public Sprite activeStar;
    public GameObject retry;
    public GameObject next;

    LevelSO levelSO;

    public static bool timerCheck;
    public static bool timeIsOver;
    bool timerStop;
    int power;
    bool isWin;
    public SelectorSO selector;
    private IDataService DataService = new JsonDataService();




    private void Awake()
    {
        timerCheck = true;
        timeIsOver = false;
        timerStop = false;
        match3.OnLevelSet += Match3_OnLevelSet;
        match3.OnTimerStart += Match3_OnTimerStart;
        match3.OnScoreChange += Match3_OnScoreChange;
        match3.OnUseMove += Match3_OnUseMove;
        match3.OnGameOver += Match3_OnGameOver;
        match3.OnGlassInc += Match3_OnGlassInc;
    }

    private void Match3_OnGlassInc()
    {
        _glass.text = $"{match3.GetGlass()} / {levelSO.goalGlass}";
    }

    private void Match3_OnGameOver()
    {
        timerStop = true;
        gameOverPanel.SetActive(true);
        powerPanel.SetActive(true);
        switch (levelSO.goalType)
        {
            default:
            case LevelSO.GoalType.Score:
                {

                    if (match3.GetScore() >= levelSO.goalScoreMin)
                    {
                        isWin = true;
                        _winlose.text = "You win!";
                        next.SetActive(true);
                        SoundManager.PlaySound(SoundManager.Sound.Win);
                    }
                    else
                    {
                        isWin = false;
                        _winlose.text = "You lose!";
                        retry.SetActive(true);
                        SoundManager.PlaySound(SoundManager.Sound.Lose);
                    }
                    CountStats();
                    break;
                }
            case LevelSO.GoalType.Glass:
                {

                    if (match3.GetGlass() >= levelSO.goalGlassMin)
                    {
                        isWin = true;
                        _winlose.text = "You win!";
                        next.SetActive(true);
                        SoundManager.PlaySound(SoundManager.Sound.Win);
                    }
                    else
                    {
                        isWin = false;
                        _winlose.text = "You lose!";
                        retry.SetActive(true);
                        SoundManager.PlaySound(SoundManager.Sound.Lose);
                    }
                    CountStatsGlassLevelType();
                    break;
                }
            case LevelSO.GoalType.None:
                {
                    isWin = false;
                    powerPanel.SetActive(false);
                    startPanel.SetActive(false);
                    _winlose.text = "You can't move!";
                    retry.SetActive(true);
                    SoundManager.PlaySound(SoundManager.Sound.Lose);
                    break;
                }
        }
    }

    public async void CountPower()
    {
        int a = 0;
        while (a < power)
        {

            _power.text = a.ToString();
            a++;
            await Task.Delay(25);
        }
    }
    private void CountStatsGlassLevelType()
    {
        int max = levelSO.goalGlass;
        int min = levelSO.goalGlassMin;
        int current = match3.GetGlass();

        int diff = (max - min) / 5;

        int starCounter = 0;
        for (int i = 1; i < 6; i++)
        {
            if (current >= min + diff * i)
            {
                starCounter++;
            }
        }

        power = 20 * starCounter + max / levelSO.moves;

        for (int j = 0; j < starCounter; j++)
        {
            stars[j].sprite = activeStar;
        }
        CountPower();
        SaveLevelData(isWin, starCounter);
    }
    void SaveLevelData(bool isWin, int stars)
    {
        if (!isWin) return;

        int num = PlayerPrefs.GetInt("LVLTOLOAD", 1);
        Lvl lvl = new(num, false, isWin, stars);
        LvlData lvlData = new();

        if (!DataService.IsFileExists("/player-saved-data.json"))
        {
            lvlData.Add(lvl);
        }
        else
        {
            lvlData = DataService.LoadData<LvlData>("/player-saved-data.json");
            bool check = false;
            foreach (Lvl item in lvlData.GetList())
            {
                if (item.num == num)
                {
                    check = true;
                    item.isDone = isWin;
                    if (item.stars < stars)
                        item.stars = stars;
                }
            }
            if (!check)
            {
                lvlData.Add(lvl);
            }
        }

        if (DataService.SaveData("/player-saved-data.json", lvlData))
            Debug.Log("File saved");
        else
            Debug.Log("error");
    }
    private void CountStats()
    {
        int max = levelSO.goalScore;
        int min = levelSO.goalScoreMin;
        int current = match3.GetScore();

        int diff = (max - min) / 5;

        int starCounter = 0;
        for (int i = 1; i < 6; i++)
        {
            if (current >= min + diff * i)
            {
                starCounter++;
            }
        }

        power = 20 * starCounter + max / levelSO.moves;

        for (int j = 0; j < starCounter; j++)
        {
            stars[j].sprite = activeStar;
        }
        CountPower();
        SaveLevelData(isWin, starCounter);
    }
    private void Match3_OnUseMove()
    {
        _moves.text = match3.GetMoves().ToString();
    }
    private void Match3_OnScoreChange()
    {
        if (levelSO.goalType != LevelSO.GoalType.None)
            _score.text = $"{match3.GetScore()} / {levelSO.goalScore}";
        else
            _score.text = $"{match3.GetScore()}";
    }
    private void Match3_OnTimerStart()
    {
        timerCheck = false;
        if (levelSO.limit == LevelSO.Limit.Time)
        {
            CountdownTimer();
        }
    }
    private void Match3_OnLevelSet(object sender, Match3.OnLevelSetEventArgs e)
    {
        levelSO = e.levelSO;
        switch (levelSO.goalType)
        {
            default:
            case LevelSO.GoalType.Score:
                {
                    scorePan.gameObject.SetActive(true);
                    _score.text = $"0 / {levelSO.goalScore}";
                    break;
                }
            case LevelSO.GoalType.Glass:
                {
                    glassPan.gameObject.SetActive(true);
                    _glass.text = $"0 / {levelSO.goalGlass}";

                    break;
                }
            case LevelSO.GoalType.None:
                {
                    scorePan.gameObject.SetActive(true);
                    _score.text = $"0";
                    break;
                }
        }
        switch (levelSO.limit)
        {
            default:
            case LevelSO.Limit.Time:
                {
                    _timer.gameObject.SetActive(true);
                    TimeSpan timeSpan = TimeSpan.FromSeconds(levelSO.time);
                    _timer.text = timeSpan.ToString("m':'ss");
                    break;
                }
            case LevelSO.Limit.Moves:
                {
                    _moves.gameObject.SetActive(true);
                    _moves.text = levelSO.moves.ToString();
                    break;
                }
            case LevelSO.Limit.None:
                {
                    randomLevelPanel.SetActive(true);
                    break;
                }
        }
    }
    public async void CountdownTimer()
    {
        int t = levelSO.time;
        while (t > 0)
        {
            if (timerStop) break;
            TimeSpan timeSpan = TimeSpan.FromSeconds(t);
            _timer.text = timeSpan.ToString("m':'ss");
            await Task.Delay(1000);
            t--;
        }
        _timer.text = "0:00";
        timeIsOver = true;
    }
}

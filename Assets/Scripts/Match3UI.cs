using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Match3UI : MonoBehaviour
{
    [SerializeField] private Match3 match3;

    [Header("Top Panel UI")]
    public GameObject scorePan;
    public TMP_Text _score;

    public GameObject glassPan;
    public TMP_Text _glass;

    public TMP_Text _moves;
    public TMP_Text _timer;
    public GameObject randomLevelPanel;

    [Header("Game Over Panel UI")]
    public GameObject gameOverPanel;

    public GameObject powerPanel;
    public TMP_Text _power;

    public TMP_Text _winlose;
    public GameObject startPanel;
    public List<Image> stars;
    public GameObject retry;
    public GameObject next;

    LevelSO levelSO;
    public static bool timerCheck;
    public static bool timeIsOver;
    bool timerStop;
    int power;
    public static int currentPower;
    bool isWin;

    public GameObject tipButton;
    public TMP_Text tipPowerText;

    private IDataService DataService = new JsonDataService();

    [Header("Achievemants")]
    public GameObject achievemantsPanel;
    public Image achievemantLogo;
    public TMP_Text achievemantName;
    public TMP_Text achievemantText;

    public GameObject finishButton;

    LvlData lvlData;

    public static event Action OnFinishButton;



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
        PlayerStats.OnGetAchievement += PlayerStats_OnGetAchievement;
        match3.OnTipRefresh += Match3_OnTipRefresh;
    }

    private void Match3_OnTipRefresh()
    {
        CheckTipButton();
    }

    private void Start()
    {
        LoadSavedData();
        currentPower = lvlData.power;
        CheckTipButton();
    }
    void CheckTipButton()
    {
        Button _tipButton = tipButton.GetComponent<Button>();
        if (currentPower >= 50)
        {
            _tipButton.interactable = true;
        }
        else
        {
            _tipButton.interactable = false;
        }
        tipPowerText.text = currentPower.ToString();
    }
    private void OnDisable()
    {
        PlayerStats.OnGetAchievement -= PlayerStats_OnGetAchievement;
        match3.OnLevelSet -= Match3_OnLevelSet;

        match3.OnScoreChange -= Match3_OnScoreChange;
        match3.OnUseMove -= Match3_OnUseMove;
        match3.OnGameOver -= Match3_OnGameOver;
        match3.OnGlassInc -= Match3_OnGlassInc;

        lvlData.power = currentPower + power;

        if (DataService.SaveData(Utils.pathLvlData, lvlData))
            Debug.Log("File saved");
        else
            Debug.Log("error");
    }

    private async void PlayerStats_OnGetAchievement(string name, string desc, int i)
    {
        if (achievemantsPanel != null && !achievemantsPanel.activeSelf)
        {
            achievemantName.text = name;
            achievemantText.text = desc;
            achievemantLogo.sprite = Utils.Ñollection.AchievementSprites[i];
            achievemantsPanel.SetActive(true);
        }
        else
        {
            await Task.Delay(3000);
            achievemantName.text = name;
            achievemantText.text = desc;
            achievemantLogo.sprite = Utils.Ñollection.AchievementSprites[i];
            achievemantsPanel.SetActive(true);
        }
    }

    private void Match3_OnGlassInc()
    {
        _glass.text = $"{match3.GetGlass()} / {levelSO.goalGlass}";
    }

    private void Match3_OnGameOver(int res)
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
                    powerPanel.SetActive(true);
                    startPanel.SetActive(true);
                    _winlose.text = "You can't move!";
                    retry.SetActive(true);
                    SoundManager.PlaySound(SoundManager.Sound.Win);
                    if(levelSO.limit == LevelSO.Limit.Blocks)
                    {
                        CountBlockResult(res);
                    }
                    else if(levelSO.limit == LevelSO.Limit.None)
                    {
                        powerPanel.SetActive(false);
                        startPanel.SetActive(false);
                    }
                    else if(levelSO.limit == LevelSO.Limit.Time)
                    {
                        powerPanel.SetActive(false);
                        startPanel.SetActive(false);
                        _winlose.text = "Time is over!";
                    }
                    break;
                }
        }
    }
    public void Finish()
    {
        Match3_OnGameOver(0);
        OnFinishButton?.Invoke();
    }

    private void CountBlockResult(int a)
    {
        int totalBlocks = levelSO.w *levelSO.h ;
        int survivedBlocks = a;
        float p = (float)survivedBlocks / totalBlocks;
        int diff = 100 - (int)(p * 100);

        int starCounter = 0;

        if (diff >= 90)
            starCounter = 5;
        else if (diff >= 80)
            starCounter = 4;
        else if (diff >= 75)
            starCounter = 3;
        else if (diff >= 70)
            starCounter = 2;
        else if (diff >= 65)
            starCounter = 1;
        else
            starCounter = 0;


        power = 20 * starCounter + UnityEngine.Random.Range(0, 10);

        for (int j = 0; j < starCounter; j++)
        {
            stars[j].sprite = Utils.Ñollection.activeStar;
        }
        if (PlayerStats.Achievements().RandomLevelB.capacity == 0 && starCounter == 5)
            for (int i = 0; i < starCounter; i++)
            {
                PlayerStats.IncRandomLevelBCounter();
            }
        CountPower();
    }

    public async void CountPower()
    {
        int a = 0;
        while (a <= power)
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

        power = 20 * starCounter;

        for (int j = 0; j < starCounter; j++)
        {
            stars[j].sprite = Utils.Ñollection.activeStar;
        }
        CountPower();
        SaveLevelData(isWin, starCounter, power);
    }

    private void LoadSavedData()
    {
        lvlData = DataService.LoadData<LvlData>(Utils.pathLvlData);
    }
    void SaveLevelData(bool isWin, int stars, int power)
    {
        if (isWin)
        {
            int num = PlayerPrefs.GetInt("LVLTOLOAD", 1);
            Lvl lvl = new(num, false, isWin, stars);

            bool check = false;
            foreach (Lvl item in lvlData.GetList())
            {
                if (item.num == num)
                {
                    check = true;
                    item.isDone = isWin;
                    if (item.stars < stars)
                    {
                        item.stars = stars;
                        if (stars == 5)
                        {
                            Debug.Log("5 star lvl odl lvl");
                            PlayerStats.IncCompletedLevelsCounter();
                        }
                    }
                }
            }
            if (!check)
            {
                lvlData.Add(lvl);
                if (stars == 5)
                {
                    Debug.Log("5 star lvl new lvl");
                    PlayerStats.IncCompletedLevelsCounter();
                }
            }
        }
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

        power = 20 * starCounter;

        for (int j = 0; j < starCounter; j++)
        {
            stars[j].sprite = Utils.Ñollection.activeStar;
        }
        CountPower();
        SaveLevelData(isWin, starCounter, power);
    }
    private void Match3_OnUseMove()
    {
        _moves.text = match3.GetMoves().ToString();
    }
    private void Match3_OnScoreChange(int s)
    {
        if (levelSO.goalType != LevelSO.GoalType.None)
            _score.text = $"{match3.GetScore()} / {levelSO.goalScore}";
        else
        {
            if(levelSO.limit == LevelSO.Limit.None)
                PlayerStats.IncRandomLevelRCounter(s);
            if (levelSO.limit == LevelSO.Limit.Time)
                PlayerStats.IncRandomLevelTCounter(s);
            _score.text = $"{match3.GetScore()}";
        }
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
                    finishButton.SetActive(true);
                    randomLevelPanel.SetActive(true);
                    break;
                }
            case LevelSO.Limit.Blocks:
                {
                    randomLevelPanel.SetActive(true);
                    break;
                }
        }
    }
    public async void CountdownTimer()
    {
        int startTime = levelSO.time;
        int remainingTime = startTime;

        while (remainingTime > 0)
        {
            if (timerStop) break;

            if (Time.timeScale > 0)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(remainingTime);
                _timer.text = timeSpan.ToString("m':'ss");
                remainingTime--;
            }

            await Task.Delay(1000);
        }

        _timer.text = "0:00";
        timeIsOver = true;
    }

    public void Tip()
    {
        if(currentPower >= 50)
        {
            match3.GetTip();
            currentPower -= 50;
            CheckTipButton();
        }
    }
}

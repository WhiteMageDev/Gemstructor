using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Match3))]
public class Match3Visual : MonoBehaviour
{
    [Header("Debug Info")]
    [SerializeField] private State state;
    [SerializeField] private float busyTimer;

    Match3 match3;
    private Transform gameBoard;
    private Transform gameBoardBackground;
    private Transform boardContainer;
    private List<GemVisual> gemVisualsList;
    private List<GameObject> destroyedRects;
    public enum State
    {
        Busy,
        WaitingForUser,
        TryFindMatches,
        GameOver,
    }
    private Point startDragPosition;
    private Point endDragPosition;
    private bool isDragging;

    public event EventHandler OnStateChanged;
    public static event Action<Gem> OnGlassDestroyed;
    private Action onBusyTimerElapsedAction;
    public static event Action OnCheckResize;
    private void Awake()
    {
        gameBoard = GameObject.Find("Game/GameBoardCanvas/BoardContainer/Board").transform;
        gameBoardBackground = GameObject.Find("Game/GameBoardCanvas/BoardContainer/BoardBG").transform;
        boardContainer = GameObject.Find("Game/GameBoardCanvas/BoardContainer").transform;


        match3 = GetComponent<Match3>();
        destroyedRects = new();
        gemVisualsList = new();

        state = State.Busy;
        match3.OnLevelSetupComplete += LevelSetup;
        Slot.OnGemDestroyed += GemDestroyed;
        match3.OnNewGemCreated += NewGemCreated;
        MovingPiece.OnDrop += MovingPiece_OnDrop;
        match3.OnBombCreated += Match3_OnBombCreated;
        Gem.OnGlassHPRedused += Gem_OnGlassHPRedused;
        Match3UI.OnFinishButton += Match3UI_OnFinishButton;
    }

    private void Match3UI_OnFinishButton()
    {
        SetState(State.GameOver);
    }

    private void Gem_OnGlassHPRedused(Gem gem, int hp)
    {
        foreach (GemVisual v in gemVisualsList)
        {
            if (v.GetGem() == gem)
            {
                if(hp == 0)
                    match3.IncGlass();
                v.GlassLoseHP(hp);
            }
        }
    }
    private void Match3_OnBombCreated(Gem bomb)
    {
        GameObject obj;
        if (destroyedRects.Count > 0)
        {
            obj = destroyedRects[0];
            destroyedRects.RemoveAt(0);
        }
        else
        {
            obj = Instantiate(Utils.Ñollection.GemPrefab, gameBoard);
        }
        obj.SetActive(true);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(-32 - (64 * bomb.GetIndex().x), 150);

        GemVisual gemVisual = new(bomb, rect);
        gemVisualsList.Add(gemVisual);
    }
    private void NewGemCreated(Gem gem)
    {
        GameObject obj;
        if (destroyedRects.Count > 0)
        {
            obj = destroyedRects[0];
            destroyedRects.RemoveAt(0);
        }
        else
        {
            obj = Instantiate(Utils.Ñollection.GemPrefab, gameBoard);
        }
        obj.SetActive(true);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(-32 - (64 * gem.GetIndex().x), 150);

        GemVisual gemVisual = new(gem, rect);
        gemVisualsList.Add(gemVisual);
    }
    private void GemDestroyed(Gem gem, bool silent)
    {
        PlayerStats.IncDestroyedGemsCounter();
        GemVisual target = null;
        foreach (GemVisual gemVisual in gemVisualsList)
        {
            Gem next = gemVisual.GetGem();
            if (gem == next)
            {
                GameObject obj = gemVisual.GetRect().gameObject;
                destroyedRects.Add(obj);
                if (!silent)
                {
                    if (!gem.IsBomb())
                    {

                        obj.GetComponent<Animator>().SetBool("gemDestroy", true);
                    }
                    else
                    {

                        if (gem.GetGemSO() == match3.listOfBombSO[1])
                            obj.GetComponent<Animator>().SetBool("bomb1Destroy", true);
                        else if (gem.GetGemSO() == match3.listOfBombSO[2])
                            obj.GetComponent<Animator>().SetBool("bomb2Destroy", true);
                        else if (gem.GetGemSO() == match3.listOfBombSO[3])
                            obj.GetComponent<Animator>().SetBool("bomb3Destroy", true);
                        else if (gem.GetGemSO() == match3.listOfBombSO[4])
                            obj.GetComponent<Animator>().SetBool("bomb4Destroy", true);
                    }
                }
                else
                    obj.SetActive(false);
                gemVisual.Clear();
                target = gemVisual;
                break;
            }
        }
        if (target != null)
            gemVisualsList.Remove(target);
    }
    private void LevelSetup()
    {
        gameBoard.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(match3.w * 64, match3.h * 64);
        gameBoardBackground.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(match3.w * 64, match3.h * 64);
        boardContainer.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(match3.w * 64 + 24, match3.h * 64 + 24);

        OnCheckResize?.Invoke();

        for (int x = 0; x < match3.w; x++)
        {
            for (int y = 0; y < match3.h; y++)
            {
                GameObject obj = Instantiate(Utils.Ñollection.GemPrefab, gameBoard);
                GameObject objBG = Instantiate(Utils.Ñollection.BackgroundPrefabs[y % 2 == 0 ? 0 : 1], gameBoardBackground);
                RectTransform objRect = obj.GetComponent<RectTransform>();
                RectTransform objBGRect = objBG.GetComponent<RectTransform>();
                objRect.anchoredPosition = new Vector2(-32 - (64 * x), -32);
                objBGRect.anchoredPosition = new Vector2(-32 - (64 * x), -32 - (64 * y));

                Gem gem = match3.gameBoard[x, y].GetGem();

                GemVisual gemVisual = new(gem, objRect);

                gemVisualsList.Add(gemVisual);
            }
        }
        SetBusyState(.5f, () => SetState(State.WaitingForUser));
    }

    public void BotMove()
    {
        List <Point> list = match3.BotExpertMove();
        if(list != null && list.Count > 0)
        {
            startDragPosition = list[0];
            endDragPosition = list[1];
            isDragging = true;
        }
    }

    private async void Update()
    {
        if (state == State.GameOver) return;

        UpdateVisual();
        switch (state)
        {
            case State.Busy:
                {
                    busyTimer -= Time.deltaTime;
                    if (busyTimer <= 0f)
                    {
                        onBusyTimerElapsedAction();
                    }
                    break;
                }
            case State.WaitingForUser:
                {
                    if (match3.IsBotOn())
                    {
                        BotMove();
                    }
                    if (isDragging)
                    {
                        isDragging = false;
                        SetBusyState(.6f, () => SetState(State.WaitingForUser));
                        if (await match3.FlipAsync(startDragPosition, endDragPosition))
                        {
                            //  We have match
                            SetBusyState(.1f, () => SetState(State.TryFindMatches));
                        }
                        else
                        {
                            SoundManager.PlaySound(SoundManager.Sound.Move);
                        }
                    }

                    break;
                }
            case State.TryFindMatches:
                {
                    if (match3.DestroyMatches())
                    {
                        SetBusyState(.3f, () =>
                        {
                            match3.ClearGemsSetBomb();
                            SetBusyState(.3f, () =>
                            {
                                match3.ApplyGravity();
                                SetBusyState(.3f, () =>
                                {
                                    match3.CreateNewGems();
                                    SetBusyState(.3f, () => SetState(State.TryFindMatches));
                                });
                            });
                        });
                    }
                    else
                        SetBusyState(.8f, () => TrySetStateWaitingForUser());
                    break;
                }
            case State.GameOver:
                {
                    break;
                }
        }
    }
    private void MovingPiece_OnDrop(Point arg1, Point arg2)
    {
        if (state != State.WaitingForUser) return;
        startDragPosition = arg1;
        endDragPosition = arg2;
        isDragging = true;
    }
    public void SetGameOver()
    {
        SetState(State.GameOver);
    }
    private void TrySetStateWaitingForUser()
    {
        if (match3.TryIsGameOver())
        {
            // Game Over
            SetState(State.GameOver);
        }
        else
        {
            // Keep Playing
            SetState(State.WaitingForUser);
        }
    }
    private void SetBusyState(float busyTimer, Action onBusyTimerElapsedAction)
    {
        SetState(State.Busy);
        this.busyTimer = busyTimer;
        this.onBusyTimerElapsedAction = onBusyTimerElapsedAction;
    }
    private void UpdateVisual()
    {
        foreach (GemVisual gemVisual in gemVisualsList)
        {
            gemVisual.Update();
        }
    }
    private void SetState(State state)
    {
        this.state = state;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }
}

public class GemVisual
{
    private Gem gem;
    private RectTransform rect;
    private bool destroyed;
    public Gem GetGem()
    {
        return gem;
    }
    public RectTransform GetRect()
    {
        return rect;
    }
    public void GlassLoseHP(int hp)
    {
        GameObject glass = rect.Find("Glass").gameObject;
        if (hp == 1)
        {
            glass.GetComponent<Image>().sprite = Utils.Ñollection.BrokenGlass;
        }
        else if (hp == 0)
        {
            rect.GetComponent<Animator>().SetTrigger("glassDestroy");
            glass.SetActive(false);
            gem.DestroyGlass();
        }
    }
    public void Clear()
    {
        gem = null;
        rect = null;
        destroyed = true;
    }
    public GemVisual(Gem g, RectTransform r)
    {
        gem = g;
        rect = r;
        destroyed = false;
        rect.GetChild(0).GetComponent<Image>().sprite = gem.GetGemSO().sprite;
        if (gem.HasGlass())
            rect.GetChild(1).gameObject.SetActive(true);
    }

    public void Update()
    {
        if (destroyed) return;

        Vector2 targetPos = new Vector2(-32 - (64 * gem.GetIndex().x), -32 - (64 * gem.GetIndex().y));

        if (Vector2.Distance(rect.anchoredPosition, targetPos) > 1)
        {
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, targetPos, Time.deltaTime * 8f);
        }
        else
        {
            rect.anchoredPosition = targetPos;
        }
    }
}

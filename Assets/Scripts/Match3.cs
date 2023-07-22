using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

public class Match3 : MonoBehaviour
{

    private List<LevelSO> listOfLevels;
    private LevelSO randomLevelR;
    private LevelSO randomLevelB;
    private LevelSO randomLevelT;
    [HideInInspector] public List<GemSO> listOfBombSO;

    [Header("Debug Info")]
    [SerializeField] private bool isBotOn;
    [SerializeField] private LevelSO levelSO;
    [SerializeField] private int moves;
    [SerializeField] private int score;
    [SerializeField] private int glass;

    [HideInInspector] public int w;
    [HideInInspector] public int h;
    [HideInInspector] public Slot[,] gameBoard;    
    private List<Point> lastMatch;    

    public event Action OnLevelSetupComplete;
    public event EventHandler<OnLevelSetEventArgs> OnLevelSet;
    public class OnLevelSetEventArgs : EventArgs
    {
        public LevelSO levelSO;
    }
    public event Action<Gem> OnNewGemCreated;
    public event Action OnTimerStart;
    public event Action OnUseMove;
    public event Action<int> OnScoreChange;
    public event Action<int> OnGameOver;
    public event Action<Gem> OnBombCreated;
    public event Action OnGlassInc;
    public event Action OnTipRefresh;
    private void Awake()
    {
        listOfLevels = new(Utils.—ollection.listOfLevels);
        randomLevelR = Utils.—ollection.randomLevelR;
        randomLevelB = Utils.—ollection.randomLevelB;
        randomLevelT = Utils.—ollection.randomLevelT;
        listOfBombSO = new(Utils.—ollection.listOfBombSO);

        LevelSetup();
    }
    public bool IsBotOn()
    {
        return isBotOn;
    }
    private void LevelSetup()
    {
        int levelNum = PlayerPrefs.GetInt("LVLTOLOAD", 0);
        if (levelNum == 0)
        {
            levelSO = randomLevelR;
            SetupRandomLevel(0);
        }
        else if (levelNum == -1)
        {
            levelSO = randomLevelB;
            SetupRandomLevel(-1);
        }
        else if (levelNum == -2)
        {
            levelSO = randomLevelT;
            SetupRandomLevel(-2);
        }
        else
        {
            levelSO = listOfLevels[levelNum - 1];

            if (levelSO != null)
            {
                w = levelSO.w;
                h = levelSO.h;
                List<GemData> gemDataList = new(levelSO.boardDataList);

                moves = levelSO.moves;
                gameBoard = new Slot[w, h];

                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        Point p = new(x, y);
                        bool isStone = gemDataList[0].gemSO == Utils.—ollection.stone;
                        Gem newGem = new(p, gemDataList[0].gemSO, gemDataList[0].hasGlass, listOfBombSO.Contains(gemDataList[0].gemSO), isStone);
                        Slot newSlot = new(p, newGem);
                        gameBoard[x, y] = newSlot;
                        gemDataList.RemoveAt(0);
                    }
                }
                OnLevelSetupComplete?.Invoke();
                OnLevelSet?.Invoke(this, new OnLevelSetEventArgs { levelSO = levelSO });
            }
            else
            {
                Debug.LogError("No level data found.");
                return;
            }
        }
    }
    private void SetupRandomLevel(int t)
    {
        if(t == -1)
        {
            w = 8; h = 8;
        }
        else if (t == -2)
        {
            w = 10; h = 10;
        }
        else
        {
            w = UnityEngine.Random.Range(6, 12);
            h = UnityEngine.Random.Range(6, 12);
        }
        moves = 0;
        gameBoard = new Slot[w, h];
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                int gemIndex = UnityEngine.Random.Range(0, randomLevelR.gemList.Count);
                bool isGlass = false;
                Point p = new(x, y);

                Gem newGem = new(p, randomLevelR.gemList[gemIndex], isGlass, false, false);
                Slot newSlot = new(p, newGem);
                gameBoard[x, y] = newSlot;
            }
        }
        OnLevelSetupComplete?.Invoke();
        OnLevelSet?.Invoke(this, new OnLevelSetEventArgs { levelSO = levelSO });
    }
    public List<Point> BotExpertMove()
    {
        //  Check if we have any bombs. If we have - explode.
        //  If there is no bombs around - find all possible matches.
        //  If we have match near glass block - return this move.
        //  Else return the longest match.

        List<Point> returnlist = new();
        List<BotLinkInfo> list = new();
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                Point p = new(x, y);
                List<Point> dir = new()
                {
                    Point.add(p, Point.up),
                    Point.add(p, Point.left),
                    Point.add(p, Point.right),
                    Point.add(p, Point.down),
                };
                if (GetSlotAtPoint(p) != null)
                    if(GetSlotAtPoint(p).GetGem() != null)
                        if (GetSlotAtPoint(p).GetGem().IsBomb())
                        {
                            returnlist.Add(p);
                            foreach (Point d in dir)
                            {
                                if (IsValidPoint(d))
                                {
                                    returnlist.Add(d);
                                    return returnlist;
                                }
                            } 
                        }
                foreach (Point d in dir)
                {
                    if (IsValidPoint(d))
                    {
                        BotLinkInfo info = FlipSilentBot(p, d);
                        if(info != null)
                            list.Add(info);
                    }
                }
            }
        }
        int maxSize = 0;
        BotLinkInfo maxLinkInfo = new();
        foreach (BotLinkInfo info in list)
        {
            if (info.isGlassNear)
            {
                returnlist.Add(info.one);
                returnlist.Add(info.two);
                return returnlist;
            }
            else if(info.size >= maxSize)
            {
                maxSize = info.size;
                maxLinkInfo = info;
            }
        }
        if (maxLinkInfo.size != 0)
        {
            returnlist.Add(maxLinkInfo.one);
            returnlist.Add(maxLinkInfo.two);
        }
        return returnlist;
    }
    public async void GetTip()
    {
        isBotOn = true;
        await Task.Delay(300);
        isBotOn = false;
    }
    public async Task<bool> FlipAsync(Point one, Point two)
    {
        if (Match3UI.timerCheck) OnTimerStart?.Invoke();

        if (!IsValidPoint(one) || !IsValidPoint(two)) return false;
        if (!GetSlotAtPoint(one).HasGem() || !GetSlotAtPoint(two).HasGem()) return false;
        if (GetSlotAtPoint(one).GetGem().HasGlass() || GetSlotAtPoint(two).GetGem().HasGlass()) return false;
        if (GetSlotAtPoint(one).GetGem().IsStone() || GetSlotAtPoint(two).GetGem().IsStone()) return false;

        bool successful = false;

        FlipGems(one, two, false);
        //  Wait moving animation
        await Task.Delay(350);

        if (HasLink(one) || HasLink(two))
        {
            successful = true;
            UseMove();
        }
        else
            FlipGems(two, one, false);
        return successful;
    }
    public bool FlipSilent(Point one, Point two)
    {
        if (!IsValidPoint(one) || !IsValidPoint(two)) return false;
        if (!GetSlotAtPoint(one).HasGem() || !GetSlotAtPoint(two).HasGem()) return false;
        if (GetSlotAtPoint(one).GetGem().HasGlass() || GetSlotAtPoint(two).GetGem().HasGlass()) return false;
        if (GetSlotAtPoint(one).GetGem().IsStone() || GetSlotAtPoint(two).GetGem().IsStone()) return false;

        bool successful = false;

        FlipGems(one, two, true);

        if (HasLink(one) || HasLink(two))
            successful = true;

        FlipGems(two, one, true);
        return successful;
    }
    public BotLinkInfo FlipSilentBot(Point one, Point two)
    {
        if (!IsValidPoint(one) || !IsValidPoint(two)) return null;
        if (!GetSlotAtPoint(one).HasGem() || !GetSlotAtPoint(two).HasGem()) return null;
        if (GetSlotAtPoint(one).GetGem().HasGlass() || GetSlotAtPoint(two).GetGem().HasGlass()) return null;
        if (GetSlotAtPoint(one).GetGem().IsStone() || GetSlotAtPoint(two).GetGem().IsStone()) return null;

        BotLinkInfo successful = new();

        FlipGems(one, two, true);

        if (HasLink(one))
        {
            int size = LinkSize(one);
            successful.one = one;
            successful.two = two;
            successful.size = size;
            successful.isNull = false;
            if (IsGlassNear(IsConnected(one, true)).Count > 0)
            {
                successful.isGlassNear = true;
            }
        }
        else if (HasLink(two))
        {
            int size = LinkSize(two);
            successful.one = two;
            successful.two = one;
            successful.size = size;
            successful.isNull = false;
            if (IsGlassNear(IsConnected(two, true)).Count > 0)
            {
                successful.isGlassNear = true;
            }
        }

        FlipGems(two, one, true);
        if(!successful.isNull)
            return successful;
        else
            return null;
    }
    public void FlipGems(Point one, Point two, bool silent)
    {
        if (!IsValidPoint(one) || !IsValidPoint(two)) return;

        Slot slotOne = GetSlotAtPoint(one);
        Slot slotTwo = GetSlotAtPoint(two);

        if (!slotOne.HasGem() || !slotTwo.HasGem()) return;


        Gem gemOne = slotOne.GetGem();
        Gem gemTwo = slotTwo.GetGem();

        slotOne.SetGem(gemTwo);
        slotTwo.SetGem(gemOne);

        if (silent) return;

        if(listOfBombSO.Contains(slotOne.GetGem().GetGemSO()) &&
            listOfBombSO.Contains(slotTwo.GetGem().GetGemSO()) &&
            !one.Equals(two))
        {
            DoubleExplode(slotOne.GetIndex());
        }
        else
        {
            if (listOfBombSO.Contains(slotOne.GetGem().GetGemSO()))
            {
                Explode(slotOne.GetIndex(), slotOne.GetGem().GetGemSO());
            }
            if (listOfBombSO.Contains(slotTwo.GetGem().GetGemSO()))
            {
                Explode(slotTwo.GetIndex(), slotTwo.GetGem().GetGemSO());
            }
        }
    }
    private void DoubleExplode(Point p)
    {
        Slot slot = GetSlotAtPoint(p);
        List<Point> radisu = new();
        IncScore(-4);
        for (int j = -1; j < 2; j++)
        {
            Point _a = new(p.x, p.y + j);
            for (int i = 0; i < w; i++)
            {
                Point a = new(i, _a.y);
                radisu.Add(a);
            }
            if (radisu.Contains(p))
                radisu.Remove(p);
        }
        for (int j = -1; j < 2; j++)
        {
            Point _a = new(p.x + j, p.y);
            for (int i = 0; i < h; i++)
            {
                Point a = new(_a.x, i);
                radisu.Add(a);
            }
            if (radisu.Contains(p))
                radisu.Remove(p);
        }
        foreach (Point point in radisu)
        {
            if (IsValidPoint(point))
                if (GetSlotAtPoint(point).HasGem())
                    if (!GetSlotAtPoint(point).GetGem().HasGlass())
                    {
                        GetSlotAtPoint(point).GetGem().SetGemSO(slot.GetGem().GetGemSO());
                    }
        }
    }
    public void ClearGemsSetBomb()
    {
        if (lastMatch == null) return;

        foreach (Point point in IsGlassNear(lastMatch))
        {
            Gem next = GetSlotAtPoint(point).GetGem();
            if (next.HasGlass() && next.HasGlassHP())
                next.GlassHPReduse();
        }
        foreach (Point p in lastMatch)
        {
            Slot slot = GetSlotAtPoint(p);
            slot.ClearGem(true);
        }
        CreateBomb(listOfBombSO[MatchType(lastMatch)]);
        lastMatch = null;
    }
    void CreateBomb(GemSO bombSO)
    {
        Point p = lastMatch[0];
        Slot slot = GetSlotAtPoint(p);
        Gem bomb = new(p, bombSO, false, true, false);
        slot.SetGem(bomb);
        OnBombCreated?.Invoke(bomb);
    }
    void Explode(Point p, GemSO bomb)
    {
        Slot slot = GetSlotAtPoint(p);
        List<Point> radisu = new();

        if (bomb == listOfBombSO[1])
        {
            IncScore(-1);
            radisu = new()
            {
                Point.add(Point.add(p, Point.up), Point.left),
                Point.add(Point.add(p, Point.up), Point.right),
                Point.add(p, Point.up),
                Point.add(p, Point.left),
                Point.add(p, Point.right),
                Point.add(p, Point.down),
                Point.add(Point.add(p, Point.down), Point.left),
                Point.add(Point.add(p, Point.down), Point.right),
            };
        }
        else if (bomb == listOfBombSO[2])
        {
            IncScore(-2);
            for (int i = 0; i < w; i++)
            {
                Point a = new(i, p.y);
                radisu.Add(a);
            }
            if (radisu.Contains(p))
                radisu.Remove(p);
        }
        else if (bomb == listOfBombSO[3])
        {
            IncScore(-2);
            for (int i = 0; i < h; i++)
            {
                Point a = new(p.x, i);
                radisu.Add(a);
            }
            if (radisu.Contains(p))
                radisu.Remove(p);
        }
        else if (bomb == listOfBombSO[4])
        {
            IncScore(-3);
            radisu = new()
            {
                Point.add(Point.add(p, Point.up), Point.left),
                Point.add(Point.add(p, Point.up), Point.right),
                Point.add(p, Point.up),
                Point.add(p, Point.left),
                Point.add(p, Point.right),
                Point.add(p, Point.down),
                Point.add(Point.add(p, Point.down), Point.left),
                Point.add(Point.add(p, Point.down), Point.right),
            };
            for (int i = 0; i < w; i++)
            {
                Point a = new(i, p.y);
                if (!radisu.Contains(a))
                    radisu.Add(a);
            }
            for (int i = 0; i < h; i++)
            {
                Point a = new(p.x, i);
                if (!radisu.Contains(a))
                    radisu.Add(a);
            }
            if (radisu.Contains(p))
                radisu.Remove(p);
        }
        foreach (Point point in radisu)
        {
            if (IsValidPoint(point))
                if (GetSlotAtPoint(point).HasGem())
                {
                    if (!GetSlotAtPoint(point).GetGem().HasGlass())
                    {
                        if (GetSlotAtPoint(point).GetGem().GetGemSO() == Utils.—ollection.stone)
                        {
                            IncScore(-5);
                            PlayerStats.IncBreakStoneWallCounter();
                        }
                        GetSlotAtPoint(point).GetGem().SetGemSO(slot.GetGem().GetGemSO());
                    }
                }
                else
                {
                    GetSlotAtPoint(point).SetGem(new Gem(point, slot.GetGem().GetGemSO(), false,false,false));
                }
        }
    }
    public int MatchType(List<Point> linked)
    {
        ConnectionType type = CheckConnectionType(linked);
        int size = linked.Count;

        if (size == 4 && type == ConnectionType.Square)
        {
            return 1;
        }
        if (size >= 4 && type == ConnectionType.HorizontalLine)
        {
            return 2;
        }
        if (size >= 4 && type == ConnectionType.VerticalLine)
        {
            return 3;
        }
        if (size > 4 && type == ConnectionType.TShape)
        {
            return 4;
        }
        if (size > 4 && type == ConnectionType.Corner)
        {
            return 4;
        }

        return 0;

    }
    List<Point> IsGlassNear(List<Point> connected)
    {
        List<Point> frozen = new();
        Point[] directions =
            {
            Point.up,
            Point.right,
            Point.down,
            Point.left
        };
        foreach (Point p in connected)
        {
            foreach (Point dir in directions)
            {
                Point n = Point.add(p, dir);
                if (!IsValidPoint(n) || !GetSlotAtPoint(n).HasGem()) continue;
                Slot next = GetSlotAtPoint(n);


                if (next.GetGem().HasGlass())
                {
                    if (!frozen.Contains(n))
                        frozen.Add(n);
                }
            }
        }
        return frozen;
    }
    public bool DestroyMatches()
    {
        bool check = false;
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                Point p = new Point(x, y);
                List<Point> linked = IsConnected(p, true);
                if (linked == null || linked.Count == 0 || GetSlotAtPoint(p).GetGem().HasGlass() || GetSlotAtPoint(p).GetGem().IsStone()) continue;

                foreach (Point point in IsGlassNear(linked))
                {
                    Gem next = GetSlotAtPoint(point).GetGem();
                    if (next.HasGlass() && next.HasGlassHP())
                        next.GlassHPReduse();
                    if (!next.HasGlassHP())
                        OnGlassInc?.Invoke();
                }
                int type;
                bool isBomb = false;
                if (listOfBombSO.Contains(GetSlotAtPoint(linked[0]).GetGem().GetGemSO()))
                {
                    isBomb = true;
                    type = 0;
                }
                else
                    type = MatchType(linked);

                switch (type)
                {
                    case 1:
                        {
                            IncScore(0);
                            foreach (Point linkedPoint in linked)
                            {
                                Slot slot = GetSlotAtPoint(linkedPoint);
                                slot.GetGem().SetIndex(linked[0]);

                            }
                            SoundManager.PlaySound(SoundManager.Sound.BombMatch);
                            PlayerStats.IncCreatedRegularBombsCounter();
                            break;
                        }
                    case 2:
                        {
                            IncScore(0);
                            foreach (Point linkedPoint in linked)
                            {
                                Slot slot = GetSlotAtPoint(linkedPoint);
                                slot.GetGem().SetIndex(linked[0]);

                            }
                            SoundManager.PlaySound(SoundManager.Sound.BombMatch);
                            PlayerStats.IncCreatedRegularBombsCounter();
                            break;
                        }
                    case 3:
                        {
                            IncScore(0);
                            foreach (Point linkedPoint in linked)
                            {
                                Slot slot = GetSlotAtPoint(linkedPoint);
                                slot.GetGem().SetIndex(linked[0]);

                            }
                            SoundManager.PlaySound(SoundManager.Sound.BombMatch);
                            PlayerStats.IncCreatedRegularBombsCounter();
                            break;
                        }
                    case 4:
                        {
                            IncScore(1);
                            foreach (Point linkedPoint in linked)
                            {
                                Slot slot = GetSlotAtPoint(linkedPoint);
                                slot.GetGem().SetIndex(linked[0]);

                            }
                            SoundManager.PlaySound(SoundManager.Sound.BombMatch);
                            PlayerStats.IncCreatedTshapeBombsCounter();
                            break;
                        }
                    default:
                        {
                            foreach (Point linkedPoint in linked)
                            {
                                Slot slot = GetSlotAtPoint(linkedPoint);
                                slot.ClearGem(false);
                            }
                            if (!isBomb)
                                SoundManager.PlaySound(SoundManager.Sound.SimpleMatch);
                            else
                                SoundManager.PlaySound(SoundManager.Sound.BombExplode);
                            IncScore(linked.Count);
                            lastMatch = null;
                            check = true;
                            return check;
                        }
                }
                IncScore(linked.Count);
                lastMatch = linked;
                check = true;
                return check;
            }
        }
        return check;
    }
    public void ApplyGravity()
    {
        for (int x = w - 1; x >= 0; x--)
        {
            for (int y = h - 1; y >= 0; y--)
            {
                Point p = new(x, y);
                Slot slot = GetSlotAtPoint(p);

                if (slot.HasGem()) continue;
                for (int i = y - 1; i >= 0; i--)
                {
                    Slot next = GetSlotAtPoint(new Point(x, i));

                    if (next.HasGem() && !next.GetGem().HasGlass())
                    {
                        if (next.GetGem().IsStone()) break;
                        slot.SetGem(next.GetGem());
                        next.SetGem(null);
                        break;
                    }
                }
            }
        }
    }
    public void CreateNewGems()
    {
        if (levelSO.limit == LevelSO.Limit.Blocks) return;
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                Point p = new Point(x, y);
                Slot slot = GetSlotAtPoint(p);
                if (!slot.HasGem())
                {
                    GemSO gemSO = levelSO.gemList[UnityEngine.Random.Range(0, levelSO.gemList.Count)];
                    Gem newGem = new Gem(slot.GetIndex(), gemSO, false, false, false);
                    slot.SetGem(newGem);
                    OnNewGemCreated?.Invoke(newGem);
                }
            }
        }
    }
    public void ChakeBoard()
    {
        List<Point> slotsToSwap = new();
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                Point p = new(x, y);
                Slot slot = GetSlotAtPoint(p);
                if(slot.HasGem())
                    if (slot.GetGem().HasGlass()) continue;

                slotsToSwap.Add(p);
            }
        }

        while (!IsMovePossible())
        {
            List<Point> slotsToSwapSecond = new(slotsToSwap);

            for (int i = 0; i < slotsToSwap.Count / 2; i++)
            {

                Point one = slotsToSwapSecond[UnityEngine.Random.Range(0, slotsToSwapSecond.Count)];
                slotsToSwapSecond.Remove(one);

                Point two = slotsToSwapSecond[UnityEngine.Random.Range(0, slotsToSwapSecond.Count)];
                slotsToSwapSecond.Remove(two);

                FlipGems(one, two, true);

            }
        }
    }
    int BlockLvlSurvivedBlocks()
    {
        int res = 0;
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                Point p = new(x, y);
                Slot slot = GetSlotAtPoint(p);
                if (IsValidPoint(p))
                {
                    if (slot.HasGem())
                        res++;
                }
            }
        }
        return res;
    }
    public bool TryIsGameOver()
    {
        if (!IsMovePossible())
        {
            if(levelSO.limit == LevelSO.Limit.Blocks)
            {
                OnGameOver?.Invoke(BlockLvlSurvivedBlocks());
                return true;
            }
            if(Match3UI.currentPower >= 100)
            {
                Match3UI.currentPower -= 100;
                OnTipRefresh?.Invoke();
                ChakeBoard();
                return false;
            }
            else
            {
                OnGameOver?.Invoke(0);
                return true;
            }

        }

        if (levelSO.limit == LevelSO.Limit.Moves)
        {
            if (moves <= 0)
            {
                OnGameOver?.Invoke(0);
                return true;
            }
        }
        else if (levelSO.limit == LevelSO.Limit.Time)
        {
            if (Match3UI.timeIsOver)
            {
                OnGameOver?.Invoke(0);
                return true;
            }
        }
        if (levelSO.goalType == LevelSO.GoalType.Score)
        {
            if (score >= levelSO.goalScore)
            {
                OnGameOver?.Invoke(0);
                return true;
            }

        }
        if (levelSO.goalType == LevelSO.GoalType.Glass)
        {
            if (glass >= levelSO.goalGlass)
            {
                OnGameOver?.Invoke(0);
                return true;
            }

        }
        return false;
    }
    public bool IsMovePossible()
    {
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                Point p = new Point(x, y);
                if (!GetSlotAtPoint(p).HasGem()) continue;
                if (listOfBombSO.Contains(GetSlotAtPoint(p).GetGem().GetGemSO()))
                {
                    return true;
                }

                List<Point> directions = new()
                {
                    Point.add(p, Point.up),
                    Point.add(p, Point.left),
                    Point.add(p, Point.right),
                    Point.add(p, Point.down),
                };
                foreach (Point dir in directions)
                {
                    if (FlipSilent(p, dir))
                    {
                        // has move
                        return true;
                    }
                    else
                    {
                        // no move
                        continue;
                    }
                }
            }
        }
        return false;
    }
    void UseMove()
    {
        moves--;
        OnUseMove?.Invoke();
    }
    public int GetMoves()
    {
        return moves;
    }
    void IncScore(int matchType)
    {
        int plus = 0;
        switch (matchType)
        {
            case -5:
                plus = 100;
                break;
            case -4:
                plus = 333;
                break;
            case -3:
                plus = 60;
                break;
            case -2:
                plus = 40;
                break;
            case -1:
                plus = 20;
                break;
            case 0:
                plus = 40;
                break;
            case 1:
                plus = 80;
                break;
            case 2:
                plus = 120;
                break;
            case 3:
                plus = 5;
                break;
            case 4:
                plus = 10;
                break;
            case 5:
                plus = 15;
                break;
            case 6:
                plus = 20;
                break;
            case 7:
                plus = 25;
                break;
            case > 7:
                plus = 30;
                break;
        }
        score += plus;
        OnScoreChange?.Invoke(plus);
    }
    public int GetScore()
    {
        return score;
    }
    public int GetGlass()
    {
        return glass;
    }
    public void IncGlass()
    {
        glass++;
    }
    bool HasLink(Point p)
    {
        List<Point> links = IsConnected(p, true);
        return links != null && links.Count > 0;
    }
    int LinkSize(Point p)
    {
        List<Point> links = IsConnected(p, true);
        return links != null? links.Count: 0;
    }
    List<Point> IsConnected(Point p, bool main)
    {
        GemSO val = GetGemSOAtPoint(p);
        if (val == null) return null;

        List<Point> connected = new List<Point>();

        Point[] directions =
        {
            Point.up,
            Point.right,
            Point.down,
            Point.left
        };

        foreach (Point dir in directions) //Checking if there is 2 or more same shapes in the directions
        {
            List<Point> line = new List<Point>();

            int same = 0;
            for (int i = 1; i < 3; i++)
            {
                Point check = Point.add(p, Point.mult(dir, i));
                if (GetGemSOAtPoint(check) == val && !GetSlotAtPoint(check).GetGem().HasGlass())
                {
                    line.Add(check);
                    same++;
                }
            }

            if (same > 1) //If there are more than 1 of the same shape in the direction then we know it is a match
                AddPoints(ref connected, line); //Add these points to the overarching connected list
        }

        for (int i = 0; i < 2; i++) //Checking if we are in the middle of two of the same shapes
        {
            List<Point> line = new List<Point>();

            int same = 0;
            Point[] check = { Point.add(p, directions[i]), Point.add(p, directions[i + 2]) };
            foreach (Point next in check) //Check both sides of the piece, if they are the same value, add them to the list
            {
                if (GetGemSOAtPoint(next) == val && !GetSlotAtPoint(next).GetGem().HasGlass())
                {
                    line.Add(next);
                    same++;
                }
            }

            if (same > 1)
                AddPoints(ref connected, line);
        }

        for (int i = 0; i < 4; i++) //Check for a 2x2
        {
            List<Point> square = new List<Point>();

            int same = 0;
            int next = i + 1;
            if (next >= 4)
                next -= 4;

            Point[] check = { Point.add(p, directions[i]), Point.add(p, directions[next]), Point.add(p, Point.add(directions[i], directions[next])) };
            foreach (Point pnt in check) //Check all sides of the piece, if they are the same value, add them to the list
            {
                if (GetGemSOAtPoint(pnt) == val && !GetSlotAtPoint(pnt).GetGem().HasGlass())
                {
                    square.Add(pnt);
                    same++;
                }
            }

            if (same > 2)
                AddPoints(ref connected, square);
        }

        if (main) //Checks for other matches along the current match
        {
            for (int i = 0; i < connected.Count; i++)
                AddPoints(ref connected, IsConnected(connected[i], false));
        }
        return connected;
    }
    void AddPoints(ref List<Point> points, List<Point> add)
    {
        foreach (Point p in add)
        {
            bool doAdd = true;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].Equals(p))
                {
                    doAdd = false;
                    break;
                }
            }

            if (doAdd) points.Add(p);
        }
    }
    bool IsValidPoint(Point p)
    {
        return p.x >= 0 && p.x < w && p.y >= 0 && p.y < h;
    }
    Slot GetSlotAtPoint(Point p)
    {
        return IsValidPoint(p) ? gameBoard[p.x, p.y] : null;
    }
    GemSO GetGemSOAtPoint(Point p)
    {
        return GetSlotAtPoint(p)?.GetGem()?.GetGemSO();
    }
    public ConnectionType CheckConnectionType(List<Point> connectedPoints)
    {
        bool match3 = connectedPoints.Count > 2;
        if (!match3) return 0;

        // œÓ‚ÂÍ‡ „ÓËÁÓÌÚ‡Î¸ÌÓÈ ÎËÌËË
        bool isHorizontalLine = true;
        foreach (Point p in connectedPoints)
        {
            if (p.y != connectedPoints[0].y)
            {
                isHorizontalLine = false;
                break;
            }
        }
        if (isHorizontalLine)
            return ConnectionType.HorizontalLine;

        // œÓ‚ÂÍ‡ ‚ÂÚËÍ‡Î¸ÌÓÈ ÎËÌËË
        bool isVerticalLine = true;
        foreach (Point p in connectedPoints)
        {
            if (p.x != connectedPoints[0].x)
            {
                isVerticalLine = false;
                break;
            }
        }
        if (isVerticalLine)
            return ConnectionType.VerticalLine;

        int aligned = 0;
        int adjacent = 0;
        int bonds = 0;
        foreach (Point p1 in connectedPoints)
        {
            int maxBonds = 0;
            foreach (Point p2 in connectedPoints)
            {
                if (ArePointsAligned(p1, p2))
                    aligned++;
                if (ArePointsAdjacent(p1, p2))
                {
                    maxBonds++;
                    adjacent++;
                }
            }
            if (maxBonds > bonds)
                bonds = maxBonds;
        }
        // œÓ‚ÂÍ‡ Í‚‡‰‡Ú‡
        bool isSquare = connectedPoints.Count == 4 && adjacent == 8 && aligned == 12 && bonds == 2;
        if (isSquare)
            return ConnectionType.Square;

        // œÓ‚ÂÍ‡ Û„Î‡
        bool isCorner = connectedPoints.Count == 5 && adjacent == 8 && aligned == 17 && bonds == 2;
        if (isCorner)
            return ConnectionType.Corner;

        // œÓ‚ÂÍ‡ ·ÛÍ‚˚ “
        bool isTShape = connectedPoints.Count == 5 && adjacent == 8 && aligned == 17 && bonds == 3;
        if (isTShape)
            return ConnectionType.TShape;

        return ConnectionType.None;
    }
    public enum ConnectionType
    {
        None,
        HorizontalLine,
        VerticalLine,
        Square,
        Corner,
        TShape
    }
    private bool ArePointsAdjacent(Point p1, Point p2)
    {
        return Mathf.Abs(p1.x - p2.x) + Mathf.Abs(p1.y - p2.y) == 1;
    }
    private bool ArePointsAligned(Point p1, Point p2)
    {
        return (p1.x == p2.x) || (p1.y == p2.y);
    }
}
[System.Serializable]
public class Slot
{
    private Point index;
    private Gem gem;
    public static event Action<Gem, bool> OnGemDestroyed;

    public Slot(Point i, Gem g)
    {
        index = i;
        gem = g;
        gem.SetIndex(index);
    }
    public Point GetIndex()
    {
        return index;
    }
    public Gem GetGem()
    {
        return HasGem() ? gem : null;
    }
    public void SetGem(Gem g)
    {
        gem = g;
        if (g != null)
            gem.SetIndex(index);
    }
    public bool HasGem()
    {
        return gem != null;
    }
    public void ClearGem(bool s)
    {
        OnGemDestroyed?.Invoke(gem, s);
        gem = null;
    }
}
[System.Serializable]
public class Gem
{
    private Point index;
    private GemSO gemSO;
    private bool hasGlass;
    private int glassHP;
    private bool isBomb;
    private bool isStone;

    public static event Action<Gem, int> OnGlassHPRedused;

    public Gem(Point i, GemSO obj, bool g, bool b, bool s)
    {
        index = i;
        gemSO = obj;
        hasGlass = g;
        isBomb = b;
        glassHP = 2;
        isStone = s;
    }
    public bool IsStone()
    {
        return isStone;
    }
    public void SetIndex(Point i)
    {
        index = i;
    }
    public Point GetIndex()
    {
        return index;
    }
    public GemSO GetGemSO()
    {
        return gemSO;
    }
    public void SetGemSO(GemSO b)
    {
        gemSO = b;
    }
    public bool HasGlass()
    {
        return hasGlass;
    }
    public void ClearGlass()
    {
        hasGlass = false;
    }
    public bool IsBomb()
    {
        return isBomb;
    }
    public void GlassHPReduse()
    {
        glassHP--;
        OnGlassHPRedused?.Invoke(this, glassHP);
    }
    public bool HasGlassHP()
    {
        return glassHP > 0;
    }
    public void DestroyGlass()
    {
        hasGlass = false;
        PlayerStats.IncDestroyedGlassCounter();
    }


}

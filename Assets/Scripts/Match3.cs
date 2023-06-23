using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Match3 : MonoBehaviour
{
    public List<LevelSO> listOfLevels;
    public LevelSO randomLevel;
    public List<GemSO> listOfBombSO;

    [HideInInspector] public int w;
    [HideInInspector] public int h;
    [HideInInspector] public Slot[,] gameBoard;

    private LevelSO levelSO;
    private List<Point> lastMatch;
    private int moves;
    private int score;
    private int glass;

    public event Action OnLevelSetupComplete;
    public event EventHandler<OnLevelSetEventArgs> OnLevelSet;
    public class OnLevelSetEventArgs : EventArgs
    {
        public LevelSO levelSO;
    }
    public event Action<Gem> OnNewGemCreated;
    public event Action OnTimerStart;
    public event Action OnUseMove;
    public event Action OnScoreChange;
    public event Action OnGameOver;
    public event Action<Gem> OnBombCreated;
    public event Action OnGlassInc;
    private void Awake()
    {
        LevelSetup();
    }
    private void LevelSetup()
    {
        int levelNum = PlayerPrefs.GetInt("LVLTOLOAD", 0);
        if (levelNum == 0)
        {
            levelSO = randomLevel;
            SetupRandomLevel();
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
                        Gem newGem = new(p, gemDataList[0].gemSO, gemDataList[0].hasGlass, false);
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
                //Debug.LogError("No level data found.");
                return;
            }
        }
    }

    private void SetupRandomLevel()
    {
        w = UnityEngine.Random.Range(6, 12);
        h = UnityEngine.Random.Range(6, 12);
        moves = 0;
        gameBoard = new Slot[w, h];
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                int gemIndex = UnityEngine.Random.Range(0, listOfLevels[0].gemList.Count);
                bool isGlass = false;
                Point p = new(x, y);
                Gem newGem = new(p, listOfLevels[0].gemList[gemIndex], isGlass, false);
                Slot newSlot = new(p, newGem);
                gameBoard[x, y] = newSlot;
            }
        }
        OnLevelSetupComplete?.Invoke();
        OnLevelSet?.Invoke(this, new OnLevelSetEventArgs { levelSO = levelSO });
    }

    public async Task<bool> FlipAsync(Point one, Point two)
    {
        if (Match3UI.timerCheck) OnTimerStart?.Invoke();

        if (!IsValidPoint(one) || !IsValidPoint(two)) return false;
        if (!GetSlotAtPoint(one).HasGem() || !GetSlotAtPoint(two).HasGem()) return false;
        if (GetSlotAtPoint(one).GetGem().HasGlass() || GetSlotAtPoint(two).GetGem().HasGlass()) return false;

        bool successful = false;

        FlipGems(one, two, false);
        // wait moving animation
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

        bool successful = false;

        FlipGems(one, two, true);

        if (HasLink(one) || HasLink(two))
            successful = true;

        FlipGems(two, one, true);
        return successful;
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

        if (listOfBombSO.Contains(slotOne.GetGem().GetGemSO()))
        {
            Explode(slotOne.GetIndex(), slotOne.GetGem().GetGemSO());
        }
        if (listOfBombSO.Contains(slotTwo.GetGem().GetGemSO()))
        {
            Explode(slotTwo.GetIndex(), slotTwo.GetGem().GetGemSO());
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
        Gem bomb = new(p, bombSO, false, true);
        slot.SetGem(bomb);
        OnBombCreated?.Invoke(bomb);
    }
    void Explode(Point p, GemSO bomb)
    {
        Slot slot = GetSlotAtPoint(p);
        List<Point> radisu = new();

        if (bomb == listOfBombSO[1])
        {
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
                    if (!GetSlotAtPoint(point).GetGem().HasGlass())
                    {
                        if (GetSlotAtPoint(point).GetGem().IsBomb())
                            IncScore(2);
                        GetSlotAtPoint(point).GetGem().SetGemSO(slot.GetGem().GetGemSO());
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
                if (linked == null || linked.Count == 0 || GetSlotAtPoint(p).GetGem().HasGlass()) continue;

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
                    Gem newGem = new Gem(slot.GetIndex(), gemSO, false, false);
                    slot.SetGem(newGem);
                    OnNewGemCreated?.Invoke(newGem);
                }
            }
        }
    }
    public bool TryIsGameOver()
    {
        if (!IsMovePossible())
        {
            OnGameOver?.Invoke();
            return true;
        }
        if (levelSO.goalType == LevelSO.GoalType.None) return false;

        if (levelSO.limit == LevelSO.Limit.Moves)
        {
            if (moves <= 0)
            {
                OnGameOver?.Invoke();
                return true;
            }
        }
        else if (levelSO.limit == LevelSO.Limit.Time)
        {
            if (Match3UI.timeIsOver)
            {
                OnGameOver?.Invoke();
                return true;
            }
        }
        if (levelSO.goalType == LevelSO.GoalType.Score)
        {
            if (score >= levelSO.goalScore)
            {
                OnGameOver?.Invoke();
                return true;
            }

        }
        if (levelSO.goalType == LevelSO.GoalType.Glass)
        {
            if (glass >= levelSO.goalGlass)
            {
                OnGameOver?.Invoke();
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
                if (listOfBombSO.Contains(GetSlotAtPoint(p).GetGem().GetGemSO())) return true;

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
        switch (matchType)
        {
            case 0:
                score += 50; // bomb type 1
                break;
            case 1:
                score += 100; // bomb type 2
                break;
            case 2:
                score += 150; // bomb type 3
                break;
            case 3:
                score += 5;
                break;
            case 4:
                score += 10;
                break;
            case 5:
                score += 15;
                break;
            case 6:
                score += 20;
                break;
            case 7:
                score += 25;
                break;
            case > 7:
                score += 30;
                break;
        }
        OnScoreChange?.Invoke();
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

        // Проверка горизонтальной линии
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

        // Проверка вертикальной линии
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
        // Проверка квадрата
        bool isSquare = connectedPoints.Count == 4 && adjacent == 8 && aligned == 12 && bonds == 2;
        if (isSquare)
            return ConnectionType.Square;

        // Проверка угла
        bool isCorner = connectedPoints.Count == 5 && adjacent == 8 && aligned == 17 && bonds == 2;
        if (isCorner)
            return ConnectionType.Corner;

        // Проверка буквы Т
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

    public static event Action<Gem, int> OnGlassHPRedused;

    public Gem(Point i, GemSO obj, bool g, bool b)
    {
        index = i;
        gemSO = obj;
        hasGlass = g;
        isBomb = b;
        glassHP = 2;
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
    }


}

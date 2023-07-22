using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class Collection : ScriptableObject
{
    [Header("Prefabs")]
    public GameObject GemPrefab;
    public List<GameObject> BackgroundPrefabs;
    public GameObject LevelDone;
    public GameObject LevelLocked;
    public GameObject EditorGem;
    [Header("Sprites")]
    public Sprite BrokenGlass;
    public Sprite Empty;
    public Sprite activeStar;

    public List<Sprite> AchievementSprites;
    public Sprite AchievementLock;

    public GemSO stone;
    public GemSO EmptyGem;
    public List<LevelSO> listOfLevels;
    public LevelSO randomLevelR;
    public LevelSO randomLevelB;
    public LevelSO randomLevelT;
    public List<GemSO> listOfBombSO;
}

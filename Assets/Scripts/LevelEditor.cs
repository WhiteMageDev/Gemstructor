using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditor : MonoBehaviour
{
    public static LevelEditor i;
    public int width;
    public int height;
    public GameObject obj;
    public Transform container;
    public Transform background;
    public LevelSO levelSO;
    public List<GemSO> gems;
    public List<GameObject> buttonsList;
    [HideInInspector] public Sprite selected;
    public Sprite empty;

    EditorGem[,] grid;
    private void Awake()
    {
        i = this;
        selected = gems[0].sprite;
    }
    private void Start()
    {
        if (levelSO == null)
            Debug.LogError("Select levelSO to edit!");
        if (buttonsList.Count != gems.Count)
            Debug.LogError("buttonsList and gems count must be equale");

        width = levelSO.w;
        height = levelSO.h;

        for (int i = 0; i < gems.Count; i++)
        {
            int index = i;
            buttonsList[i].GetComponent<Image>().sprite = gems[i].sprite;
            buttonsList[i].GetComponent<Button>().onClick.AddListener(delegate { PickGem(index); });
        }
        container.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(width * 64, height * 64);
        background.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(width * 64, height * 64);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject node = Instantiate(obj, background);
                node.GetComponent<RectTransform>().anchoredPosition = new Vector2(-32 - (64 * x), -32 - (64 * y));
            }
        }

        grid = new EditorGem[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject node = Instantiate(obj, container);
                node.GetComponent<RectTransform>().anchoredPosition = new Vector2(-32 - (64 * x), -32 - (64 * y));
                EditorGem e = node.GetComponent<EditorGem>();
                if (x + y <= levelSO.boardDataList.Count)
                    e.Initialize(levelSO.boardDataList[x + y].gemSO, levelSO.boardDataList[x + y].hasGlass);
                else
                    e.Initialize(null, false);
                grid[x, y] = e;
            }
        }
    }
    public void PickGem(int i)
    {
        selected = gems[i].sprite;
    }
    public void SaveLevelData()
    {
        List<GemData> boardDataList = new();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                EditorGem node = grid[x, y];
                if (node.img.sprite == empty)
                {
                    Debug.LogError("Set all gems!");
                    return;
                }

                boardDataList.Add(new GemData(node.gemSO, node.hasGlass));
            }
        }
        levelSO.SetData(boardDataList);
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(levelSO);
#endif
        Debug.Log("Level data saved!");
    }
    public void Randomize()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                EditorGem node = grid[x, y];
                node.Initialize(gems[UnityEngine.Random.Range(0, gems.Count)], false);
            }
        }
    }
    public void Clear()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                EditorGem node = grid[x, y];
                node.Initialize(null, false);
            }
        }
    }
}

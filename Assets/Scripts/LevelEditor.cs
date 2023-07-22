using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditor : MonoBehaviour
{
    public static LevelEditor i;
    private int width;
    private int height;

    private Transform container;
    private Transform background;

    public LevelSO levelSO;
    public List<GemSO> gemsForUse;
    public List<GameObject> buttonsList;
    public Sprite selected;
    public GemSO selectedGemSO;

    EditorGem[,] grid;

    public List<GameObject> editorNodeList;

    private void Awake()
    {
        i = this;
        container = GameObject.Find("Canvas/Board").transform;
        background = GameObject.Find("Canvas/Background").transform;
        selected = gemsForUse[0].sprite;
        selectedGemSO = gemsForUse[0];
    }
    private void Start()
    {
        for (int i = 0; i < gemsForUse.Count; i++)
        {
            int index = i;
            buttonsList[i].GetComponent<Image>().sprite = gemsForUse[i].sprite;
            buttonsList[i].GetComponent<Button>().onClick.AddListener(delegate { PickGem(index); });
        }
    }
    public void InitializeLevel()
    {
        foreach (GameObject item in editorNodeList)
        {
            Destroy(item);  
        }
        editorNodeList = new();
        grid = null;

        if (levelSO == null)
            Debug.LogError("Select levelSO to edit!");
        if (buttonsList.Count != gemsForUse.Count)
            Debug.LogError("buttonsList and gems count must be equale");

        width = levelSO.w;
        height = levelSO.h;


        container.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(width * 64, height * 64);
        background.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(width * 64, height * 64);

        grid = new EditorGem[width, height];

        List<GemData> dataList = new(levelSO.boardDataList);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject node = Instantiate(Utils.Ñollection.EditorGem, container);
                editorNodeList.Add(node);
                node.GetComponent<RectTransform>().anchoredPosition = new Vector2(-32 - (64 * x), -32 - (64 * y));
                EditorGem e = node.GetComponent<EditorGem>();

                e.Initialize(dataList[0].gemSO, dataList[0].hasGlass);
                dataList.RemoveAt(0);

                grid[x, y] = e;
            }
        }
    }
    public void PickGem(int i)
    {
        selected = gemsForUse[i].sprite;
        selectedGemSO = gemsForUse[i];
    }
    public void SaveLevelData()
    {
        List<GemData> boardDataList = new();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                EditorGem node = grid[x, y];
                if (node.img.sprite == Utils.Ñollection.Empty)
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
                node.Initialize(levelSO.gemList[Random.Range(0, levelSO.gemList.Count)], false);
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

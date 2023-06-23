using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EditorGem : MonoBehaviour, IPointerDownHandler
{
    public GemSO gemSO;
    public bool hasGlass;
    public Image img;
    private GameObject glass;

    private void Awake()
    {
        img = GetComponent<Image>();
        glass = transform.GetChild(0).gameObject;
    }
    public void Initialize(GemSO g, bool gl)
    {
        gemSO = g;
        hasGlass = gl;
        img.sprite = g != null ? gemSO.sprite : LevelEditor.i.empty;
        glass.SetActive(hasGlass);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (Input.GetMouseButton(0))
        {
            img.sprite = LevelEditor.i.selected;
        }
        else
        {
            hasGlass = !hasGlass;
            glass.SetActive(hasGlass);
        }
    }
}

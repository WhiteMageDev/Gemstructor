using System.Collections.Generic;
using UnityEngine;

public class GraphicResizer : MonoBehaviour
{
    public List<RectTransform> objectsToResize;
    private void Start()
    {
        ResizeAll();
    }
    private void ResizeAll()
    {
        Vector3 diff = new Vector3(0.05f, 0.05f, 0);
        foreach (RectTransform rect in objectsToResize)
        {
            for (int i = 0; i < 20; i++)
            {
                if (IsRectTransformOutsideScreen(rect))
                    rect.localScale -= diff;
            }
        }
    }
    public bool IsRectTransformOutsideScreen(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(corners[i]);
            if (screenPoint.x < 0 || screenPoint.x > Screen.width || screenPoint.y < 0 || screenPoint.y > Screen.height)
                return true;
        }
        return false;
    }
}

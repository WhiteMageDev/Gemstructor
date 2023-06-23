using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MovingPiece : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public static event Action<Point, Point> OnDrop;
    Point one;

    Point newIndex;
    Vector2 mouseStart;
    bool moving;

    void Update()
    {
        if (moving)
        {
            Vector2 dir = ((Vector2)Input.mousePosition - mouseStart);
            Vector2 nDir = dir.normalized;
            Vector2 aDir = new Vector2(Mathf.Abs(dir.x), Mathf.Abs(dir.y));

            Vector2 pos = transform.GetComponent<RectTransform>().anchoredPosition;

            one = new Point(Mathf.Abs((int)pos.x / 64), Mathf.Abs((int)pos.y / 64));

            newIndex = Point.clone(one);
            Point add = Point.zero;
            if (dir.magnitude > 32)
            {
                if (aDir.x > aDir.y)
                    add = (new Point((nDir.x > 0) ? -1 : 1, 0));
                else if (aDir.y > aDir.x)
                    add = (new Point(0, (nDir.y > 0) ? -1 : 1));
            }
            newIndex.add(add);
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        mouseStart = Input.mousePosition;
        moving = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        moving = false;
        OnDrop?.Invoke(one, newIndex);
    }
}

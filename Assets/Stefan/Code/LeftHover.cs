using UnityEngine;
using UnityEngine.EventSystems;

public class LeftHover : MonoBehaviour, IPointerEnterHandler
{
    public DiamondSwap swapScript;

    public void OnPointerEnter(PointerEventData eventData)
    {
        swapScript.OnHoverLeft();
    }
}
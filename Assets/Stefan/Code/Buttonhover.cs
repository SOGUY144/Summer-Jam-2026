using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject normalImage;
    public GameObject hoverImage;

    void Start()
    {
        normalImage.SetActive(true);
        hoverImage.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        normalImage.SetActive(false);
        hoverImage.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        normalImage.SetActive(true);
        hoverImage.SetActive(false);
    }
}
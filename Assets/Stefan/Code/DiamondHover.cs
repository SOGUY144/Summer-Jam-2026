using UnityEngine;
using UnityEngine.EventSystems;

public class DiamondSwap : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject pairA_Left;
    public GameObject pairA_Right;
    public GameObject pairB_Left;
    public GameObject pairB_Right;

    private bool isShowingA = true;
    private bool canSwap = true;

    void Start()
    {
        pairA_Left.SetActive(false);
        pairA_Right.SetActive(false);
        pairB_Left.SetActive(false);
        pairB_Right.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pairA_Left.SetActive(true);
        pairA_Right.SetActive(true);
        pairB_Left.SetActive(false);
        pairB_Right.SetActive(false);
        isShowingA = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pairA_Left.SetActive(false);
        pairA_Right.SetActive(false);
        pairB_Left.SetActive(false);
        pairB_Right.SetActive(false);
        isShowingA = true;
        canSwap = true;
    }

    void ShowPairA()
    {
        pairA_Left.SetActive(true);
        pairA_Right.SetActive(true);
        pairB_Left.SetActive(false);
        pairB_Right.SetActive(false);
        isShowingA = true;
        StartCoroutine(ResetCanSwap());
    }

    void ShowPairB()
    {
        pairA_Left.SetActive(false);
        pairA_Right.SetActive(false);
        pairB_Left.SetActive(true);
        pairB_Right.SetActive(true);
        isShowingA = false;
        StartCoroutine(ResetCanSwap());
    }

    System.Collections.IEnumerator ResetCanSwap()
    {
        canSwap = false;
        yield return new WaitForSeconds(0.2f);
        canSwap = true;
    }

    public void OnHoverLeft()
    {
        if (!canSwap) return;
        if (isShowingA) ShowPairB();
        else ShowPairA();
    }

    public void OnHoverRight()
    {
        if (!canSwap) return;
        if (!isShowingA) ShowPairA();
        else ShowPairB();
    }
}
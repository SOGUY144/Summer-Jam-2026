using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class DiamondHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image diamond1;
    public Image diamond2;

    public Color normalColor = Color.gray;
    public Color color1 = Color.cyan;
    public Color color2 = Color.yellow;

    public Vector3 normalScale = Vector3.one;
    public Vector3 hoverScale = new Vector3(1.2f, 1.2f, 1.2f);
    public float animSpeed = 0.15f;

    void Start()
    {
        diamond1.color = normalColor;
        diamond2.color = normalColor;
        diamond1.transform.localScale = normalScale;
        diamond2.transform.localScale = normalScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        diamond1.color = color1;
        diamond2.color = color2;

        StopAllCoroutines();
        StartCoroutine(ScaleTo(diamond1.transform, hoverScale));
        StartCoroutine(ScaleTo(diamond2.transform, hoverScale));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        diamond1.color = normalColor;
        diamond2.color = normalColor;

        StopAllCoroutines();
        StartCoroutine(ScaleTo(diamond1.transform, normalScale));
        StartCoroutine(ScaleTo(diamond2.transform, normalScale));
    }

    IEnumerator ScaleTo(Transform target, Vector3 targetScale)
    {
        Vector3 startScale = target.localScale;
        float time = 0;

        while (time < 1f)
        {
            time += Time.deltaTime / animSpeed;
            target.localScale = Vector3.Lerp(startScale, targetScale, time);
            yield return null;
        }

        target.localScale = targetScale;
    }
}
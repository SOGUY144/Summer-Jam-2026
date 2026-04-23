using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Vector3 normalScale = Vector3.one;
    public Vector3 hoverScale = new Vector3(1.2f, 1.2f, 1.2f);
    public float animSpeed = 0.15f;

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(hoverScale));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(normalScale));
    }

    IEnumerator ScaleTo(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float time = 0;

        while (time < 1f)
        {
            time += Time.deltaTime / animSpeed;
            transform.localScale = Vector3.Lerp(startScale, targetScale, time);
            yield return null;
        }

        transform.localScale = targetScale;
    }
}
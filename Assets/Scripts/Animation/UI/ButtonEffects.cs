using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonEffects : MonoBehaviour , IPointerEnterHandler , IPointerExitHandler , IPointerClickHandler
{
    private float animationTime = 0.2f;
    
    private void OnEnable()
    {
        transform.localScale = Vector3.one;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(1.1f, animationTime);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(1f, animationTime);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(1f, animationTime).SetEase(Ease.OutBack);
    }
}

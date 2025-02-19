using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PopupAnimation : MonoBehaviour
{
    [SerializeField] private Image blackBg;
    [SerializeField] private GameObject popup;

    private float animationDuration = 0.7f;

    private void OnEnable()
    {
        AnimatePopup();
    }

    private void AnimatePopup()
    {
        blackBg.color = new Color(0f, 0f, 0f, 0f);
        popup.transform.localScale = Vector3.zero;
        
        Sequence animation = DOTween.Sequence();
        
        animation.Append(blackBg.DOFade(0.9f, animationDuration));
        animation.Join(popup.transform.DOScale(Vector3.one, animationDuration).SetEase(Ease.OutBack));
    }
    
}
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class PopUpBase : MonoBehaviour
{
    [BoxGroup("Base Settings")] [SerializeField][Required][SceneObjectsOnly]
    private Image blackBg;
    
    [BoxGroup("Base Settings")] [SerializeField]
    private Ease selectedEase;

    [BoxGroup("Base Settings")] [SerializeField] [Range(0,2)]
    private float duration = 0.3f;
    
    [BoxGroup("Base Settings")] [SerializeField]
    private Vector2 startPosition = Vector2.zero;
    
    [BoxGroup("Base Settings")] [SerializeField]
    private Vector2 startScale = Vector2.one;
    
    [BoxGroup("Base Settings")] [SerializeField]
    private bool fadeCanvasGroup;

    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    protected bool isOpen;

    public virtual void Initialize()
    {
        this.isOpen = false;
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OpenPanel(Action callBack = null)
    {
        AudioManager.Instance.Play(SoundList.UIPopUp);
        if(this.isOpen) return;
        this.isOpen = true;
        //- init animation
        gameObject.SetActive(this.isOpen);
        blackBg.enabled = this.isOpen;
        _rectTransform.anchoredPosition = this.startPosition;
        _rectTransform.localScale = this.startScale;
        blackBg.DOFade(0.25f, 0);
        if (fadeCanvasGroup)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.DOFade(1, this.duration);
        }
        // start animating
        blackBg.DOFade(0.25f, this.duration);
        _rectTransform.DOAnchorPos(Vector2.zero, this.duration).SetEase(this.selectedEase);
        _rectTransform.DOScale(Vector3.one, this.duration).SetEase(this.selectedEase).OnComplete(() =>
        {
            callBack?.Invoke();
        });
    }

    public void ClosePanel(Action callBack = null)
    {
        if(!this.isOpen) return;
        if (fadeCanvasGroup)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.DOFade(0, this.duration);
        }
        blackBg.DOFade(0, this.duration);
        _rectTransform.DOAnchorPos(startPosition, this.duration).SetEase(this.selectedEase);
        _rectTransform.DOScale(startScale, this.duration).SetEase(this.selectedEase).OnComplete(() =>
        {
            this.isOpen = false;
            callBack?.Invoke();
            blackBg.enabled = this.isOpen;
            gameObject.SetActive(this.isOpen);
        });
        
    }
}

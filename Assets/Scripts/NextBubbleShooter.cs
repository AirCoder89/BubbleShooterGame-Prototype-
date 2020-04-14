using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class NextBubbleShooter : BubbleBase
{
    [BoxGroup("Animation")][SerializeField][Range(0,1)] private float duration = 0.3f;
    [BoxGroup("Animation")][SerializeField] private Vector2 startScale;
    [BoxGroup("Animation")][SerializeField] private Vector2 targetScale;
    
    public void SetNextType(int type)
    {
        transform.localScale = startScale;
        this.Sprite.DOFade(0, duration);
        transform.DOScale(targetScale, duration).OnComplete(() =>
        {
            this.Sprite.DOFade(1, duration);
            transform.DOScale(startScale, duration);
            SetType(type,true);
        });
    }
}

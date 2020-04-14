using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using PathologicalGames;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class NotificationText : MonoBehaviour
{
    [SerializeField] private float upAmount;
    private TextMeshProUGUI _txt;
    private CanvasGroup _canvasGroup;
    private Vector2 _startScale;
    
    private CanvasGroup CanvasGroup
    {
        get
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            return _canvasGroup;
        }
    }
    private TextMeshProUGUI Text
    {
        get
        {
            if (_txt == null) _txt = GetComponent<TextMeshProUGUI>();
            return _txt;
        }
    }

    public void PlayNotification(string value, AnimNotificationParameters parameters)
    {
        //- init animation
        _startScale = transform.localScale;
        transform.localScale = parameters.startScale;
        if (parameters.type != NotificationsType.Perfect) Text.text = value;
        transform.position = parameters.startPosition;
        CanvasGroup.alpha = 0f;
        
        //    1/4          2/4           1/4
        // |-FadeIn-|--- ShowTime ---|-FadeOut-|
        var durationSection = parameters.fullAnimDuration / 4;

        if (parameters.type == NotificationsType.BubbleValue)
        {
            transform.DOMove(new Vector3(transform.position.x, transform.position.y + upAmount),
                parameters.fullAnimDuration);
        }
        
        //- FadeIn
        CanvasGroup.DOFade(1, durationSection); 
        transform.DOScale(_startScale, durationSection).OnComplete(() =>
        {
            if (parameters.type == NotificationsType.MergeCombo)
            {
                transform.DOMove(new Vector3(transform.position.x, transform.position.y + upAmount),
                    (durationSection * 3));
            }
        });
        
        //-FadeOut
        CanvasGroup.DOFade(0, durationSection).SetDelay((durationSection * 3)).OnComplete(() => { Remove(); });
        if (parameters.type == NotificationsType.Perfect)
        {
            //Perfect
            transform.DOMove(new Vector3(transform.position.x, transform.position.y + upAmount),durationSection)
                .SetDelay((durationSection * 3));
        }
    }

    private void Remove()
    {
        PoolManager.Pools["FxPool"].Despawn(this.transform);
    }
}

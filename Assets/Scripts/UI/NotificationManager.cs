using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using PathologicalGames;
using Sirenix.OdinInspector;
using UnityEngine;

public enum NotificationsType
{
    BubbleValue,MergeCombo,Perfect
}
[System.Serializable]
public struct AnimNotificationParameters
{
    public string prefabName;
    [EnumToggleButtons]
    public NotificationsType type;
    [Range(0,5f)] public float fullAnimDuration;
    public Vector2 startScale;
    public Vector2 startPosition;
}

public class NotificationManager : MonoBehaviour
{
    [SerializeField][HideLabel][Title("Notifications Parameters")] private List<AnimNotificationParameters> notificationsParameters;
    
    public void PlayValueNotification(string textValue,Vector2 pos, NotificationsType type)
    {
        var parameter = this.notificationsParameters.FirstOrDefault(p => p.type == type);
        var notif = PoolManager.Pools["FxPool"].Spawn(parameter.prefabName, transform);
        if(notif == null) return;
            var valueNotification = notif.gameObject
                .GetComponent<NotificationText>();
            parameter.startPosition = pos;
            valueNotification.PlayNotification(textValue,parameter);
    }
     
    public void PlayValueNotification(string textValue, NotificationsType type)
    {
        var parameter = this.notificationsParameters.FirstOrDefault(p => p.type == type);
        var notif = PoolManager.Pools["FxPool"].Spawn(parameter.prefabName, transform);
        if(notif == null) return;
        var valueNotification = notif.gameObject
            .GetComponent<NotificationText>();
        parameter.startPosition = Vector2.zero;
        valueNotification.PlayNotification(textValue,parameter);
    }
}

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;

public class Bullet : BubbleBase
{
    //- Bullet Events
    public delegate void BulletEvents(Bullet bullet, BubbleScript target);
    public delegate void BulletEvents2(Bullet bullet, Dimension target);
    public static event BulletEvents OnHitBubble;
    public static event BulletEvents2 OnHitNeighbor;
    
    [BoxGroup("Trail Settings")] [SerializeField] [Range(0, 1)] private float startAlpha;
    [BoxGroup("Trail Settings")] [SerializeField] [Range(0, 1)] private float endAlpha;
    
    //- privates variables
    private TrailRenderer _trail;
    private Sequence _mySequence;
    private List<Vector3> currentPath;
    private float _currentSpeedUnit;
    private float _currentSpeed;
    private float currentDistance;
    private AimSetting _aim;

    public static bool HasEvent()
    {
        if (GameController.Instance.aimSetting == AimSetting.AimOnBubble)
        {
            return OnHitBubble != null;
        }
        else
        {
            return OnHitNeighbor != null;
        }
    }
    
    public TrailRenderer Trail
    {
        get
        {
            if (_trail == null) _trail = GetComponentInChildren<TrailRenderer>();
            return _trail;
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        var type = GameController.Instance.GetType(currentType);
        this.Trail.Clear();
        this.Trail.startColor = new Color(type.color.r, type.color.g, type.color.b,this.startAlpha);
        this.Trail.endColor = new Color(type.color.r, type.color.g, type.color.b,this.endAlpha);
        this._aim = GameController.Instance.aimSetting;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(_aim == AimSetting.AimOnAvailableNeighbor) return;
        if (!other.gameObject.CompareTag("ball")) return;
        var b = other.gameObject.GetComponent<BubbleScript>();
        if(b.isFall || !b.InsideGrid()) return;
        OnHitBubble?.Invoke(this,b);
        gameObject.SetActive(false);
    }

    private Dimension _targetNeighbor;
    
    public void SetTargetBubble(Dimension targetNeighbor)
    {
        _targetNeighbor = targetNeighbor;
    }
    
    public void Move(List<Vector3> wayPoints,float speed)
    {
        if(wayPoints.Count == 0) return;
        this.Trail.Clear();
        currentPath = wayPoints;
        _currentSpeedUnit = speed / wayPoints.Count;
        currentDistance = Vector3.Distance(transform.position, currentPath[0]);
        _currentSpeed = _currentSpeedUnit * currentDistance;
        MoveTo(currentPath[0], 0);
    }

    private void Update()
    {
        if(!_isOnMove) return;
        if (transform.position.y > 6f)
        {
            _isOnMove = false;
            StopAllCoroutines();
            transform.position = Vector3.zero;
            gameObject.SetActive(false);
            _targetNeighbor = null;
            GameController.Instance.bubbleGrid.EmptyShoot();
        }
    }

    private bool _isOnMove;
    private void MoveTo(Vector3 target,int index)
    {
        _isOnMove = true;
        StopCoroutine("DoMove");
        StartCoroutine(DoMove(
            target,
            _currentSpeed,
            () =>
            {
                //OnComplete
                if (index < currentPath.Count)
                {
                    index++;
                    try
                    {
                        currentDistance = Vector3.Distance(transform.position, currentPath[index]);
                        _currentSpeed = _currentSpeedUnit * currentDistance;
                        MoveTo(currentPath[index], index);
                    }
                    catch
                    {
                        gameObject.SetActive(false);
                        if (_aim == AimSetting.AimOnAvailableNeighbor && _targetNeighbor != null)
                        {
                            OnHitNeighbor?.Invoke(this,_targetNeighbor);
                            _targetNeighbor = null;
                        }
                        _targetNeighbor = null;
                    }
                   
                }
                else
                {
                    gameObject.SetActive(false);
                    if (_aim == AimSetting.AimOnAvailableNeighbor && _targetNeighbor != null)
                    {
                        OnHitNeighbor?.Invoke(this,_targetNeighbor);
                        _targetNeighbor = null;
                    }
                }
            }
        ));
    }
    
    private IEnumerator DoMove(Vector3 position, float timeToMove,Action callback = null,float delay = 0f)
    {
        if(delay > 0) yield return new WaitForSeconds(delay);
        var currentPos = transform.position;
        var t = 0f;
        while(t < 1)
        {
            t += Time.deltaTime / timeToMove;
            transform.position = Vector3.Lerp(currentPos, position, t);
            yield return null;
        }
        transform.position = position;
        callback?.Invoke();
    }

}

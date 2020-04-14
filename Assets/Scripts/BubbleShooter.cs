using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BubbleShooter : BubbleBase
{
    //- Main Settings
    [TabGroup("Main Settings")][SerializeField]private BubbleGrid grid;
    [TabGroup("Main Settings")][SerializeField]private NextBubbleShooter nextBubble;
    [TabGroup("Main Settings")][SerializeField]private TargetMark targetMark;
        //- Main Settings > Animations
        [TabGroup("Main Settings")][Title("Animations")] [SerializeField] [Range(0, 2)]
        private float duration;
        [TabGroup("Main Settings")][SerializeField]
        private Vector2 punchScale;
        [TabGroup("Main Settings")][SerializeField] [Range(0, 1)]
        private float elasticity;
        [TabGroup("Main Settings")][SerializeField] [Range(0, 20)]
        private int vibrato = 10;
        [TabGroup("Main Settings")][SerializeField]
        private bool fadeInOut;
    
    //- Aim & Shoot
    [TabGroup("Aim & Shoot")] [SerializeField]private Bullet bullet;
    [TabGroup("Aim & Shoot")] [SerializeField] private Transform aimPoint;
    [TabGroup("Aim & Shoot")] [SerializeField]
    private bool snapToBubble = true;
    [TabGroup("Aim & Shoot")] [SerializeField]
    private bool applyBubbleColorToAimLine = false;
    [TabGroup("Aim & Shoot")][SerializeField][Range(0, 255)] private float lineStartAlpha;
    [TabGroup("Aim & Shoot")][SerializeField][Range(0, 255)] private float lineEndAlpha;
    
    [TabGroup("Aim & Shoot")] [SerializeField][Range(0,1)] private float speed;
    [TabGroup("Aim & Shoot")] [SerializeField][Range(2,20)] private float maxDistance;
    [TabGroup("Aim & Shoot")] [SerializeField][Range(-5,5)] float angleRange;
    [TabGroup("Aim & Shoot")] [SerializeField][Range(1,10)] private int maxReflections = 5;
    
    //- private variables
    private LineRenderer _line;
    private bool _isMouseDown;
    private List<Vector3> _points;
    private int _currentReflections = 0;
    private int _bulletCounter = 0;
    private Dimension _targetDimension;
    public LineRenderer Line
    {
        get
        {
            if (_line == null) _line = GetComponent<LineRenderer>();
            return _line;
        }
    }

    private void Start()
    {
        _points = new List<Vector3>();
        _bulletCounter = 0;
    }

    public void MouseDown()
    {
        if(GameController.Instance.aimSetting == AimSetting.AimOnAvailableNeighbor)
        {grid.UpdateBubblesNeighbors(false,true);}
        grid.ResetCombo();
        grid.Subscribe();
        _isMouseDown = true;
        Line.enabled = true;
    }

    public void MouseUp()
    {
        if (_isMouseDown) Shoot();
        _isMouseDown = false;
        Line.enabled = false;
        targetMark.Hide();
    }
    
    void Update()
    {
        if(!_isMouseDown) return;
        if(!GameController.Instance.isGameStarted || GameController.Instance.isGameOnPause) return;
        if(bullet.gameObject.activeSelf) return;
        
        Line.enabled = true;
        Aim();
    }

    private void ShootAnimation()
    {
        transform.DOPunchScale(punchScale, duration, vibrato, elasticity);
        if (fadeInOut)
        {
            Sprite.DOFade(0, 0);
            Sprite.DOFade(1, duration);
        }
    }
    
    public void InitNextType()
    {
        nextBubble.SetNextType(grid.GetRandomType());
    }
    
    public void GetNextType()
    {
        if (applyBubbleColorToAimLine)
        {
            var typeColor = GameController.Instance.GetType(nextBubble.currentType).color;
            Line.startColor = new Color(typeColor.r,typeColor.g,typeColor.b,this.lineStartAlpha);
            Line.endColor = new Color(typeColor.r,typeColor.g,typeColor.b,this.lineEndAlpha);
        }
        SetType(nextBubble.currentType,true);
        nextBubble.SetNextType(grid.GetRandomType());
        _bulletCounter++;
        if (_bulletCounter >= GameController.Instance.newGridLineFreq)
        {
            _bulletCounter = 0;
            grid.shouldAddNewLine = true;
        }
    }
    
    private void Aim()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var direction = new Vector2(mousePos.x - transform.position.x, mousePos.y - transform.position.y);
        if(mousePos.y < angleRange) direction = new Vector2(mousePos.x - transform.position.x, angleRange - transform.position.y);
        aimPoint.transform.up = direction;
        RayCast(aimPoint.position, direction);
    }

    private void RayCast( Vector2 startPoint, Vector2 direction)
    {
        var hitData = Physics2D.Raycast(startPoint, direction.normalized, maxDistance);
        Debug.DrawRay(startPoint,direction,Color.yellow);
        
        _currentReflections = 0;
        _points.Clear();
        _points.Add(startPoint);
        
        BubbleScript bubble;
        if (hitData.collider.gameObject.CompareTag("ball"))
        {
            bubble = hitData.collider.gameObject.GetComponent<BubbleScript>();
        }
        else bubble = null;
        
        if (hitData && hitData.collider.gameObject.CompareTag("ball")  && bubble != null && !bubble.isFall && bubble.InsideGrid())
        {
            float bubbleDist; 
            if (GameController.Instance.aimSetting == AimSetting.AimOnAvailableNeighbor)
            {
                OnHitBubble(bubble, hitData,true);
                if(snapToBubble) _points.Add(targetMark.GetPosition());
                else
                {
                    bubbleDist= Vector2.Distance(transform.position,targetMark.GetPosition());
                    _points.Add(direction * bubbleDist);
                }
            }
            else
            {
                if(snapToBubble) _points.Add(hitData.collider.gameObject.transform.position);
                else
                {
                    bubbleDist= Vector2.Distance(transform.position,hitData.collider.gameObject.transform.position);
                    _points.Add(direction * bubbleDist);
                }
            }
        }
        else if (hitData && hitData.collider.gameObject.CompareTag("SideWall"))
        {
            NoBubbleDetected();
            Reflect(startPoint, hitData);
        }
        else
        {
            NoBubbleDetected();
            _points.Add(direction * maxDistance);
        }

        Line.positionCount = _points.Count;
        Line.SetPositions(_points.ToArray());
    }
    
    private void Reflect(Vector2 origin, RaycastHit2D hitData)
    {
        if (_currentReflections > maxReflections) return;
        _points.Add(hitData.point);
        _currentReflections++;

        var inDirection = (hitData.point - origin).normalized;
        var newDirection = Vector2.Reflect(inDirection, hitData.normal);
        BubbleScript bubble;
        var newHitData = Physics2D.Raycast(hitData.point + (newDirection * 0.0001f), newDirection * 100, maxDistance);
        Debug.DrawRay(origin,newHitData.point,Color.green);
        if (newHitData.collider.gameObject.CompareTag("ball"))
        {
            bubble = newHitData.collider.gameObject.GetComponent<BubbleScript>();
        }
        else bubble = null;
        
        if (newHitData && newHitData.collider.gameObject.CompareTag("ball") && bubble != null && !bubble.isFall && bubble.InsideGrid())
        {
            float bubbleDist;
            if (GameController.Instance.aimSetting == AimSetting.AimOnAvailableNeighbor)
            {
                 OnHitBubble(bubble, newHitData,false);
                 if(snapToBubble)_points.Add(targetMark.GetPosition());
                 else
                 {
                     bubbleDist = Vector2.Distance(transform.position,targetMark.GetPosition());
                     _points.Add(newHitData.point * bubbleDist);
                 }
            }
            else
            {
                if(snapToBubble) _points.Add(newHitData.collider.gameObject.transform.position);
                else
                {
                    bubbleDist = Vector2.Distance(transform.position,newHitData.collider.gameObject.transform.position);
                    _points.Add(newHitData.point * bubbleDist);
                }
            }
        }
        else if (newHitData && newHitData.collider.gameObject.CompareTag("SideWall"))
        {
            NoBubbleDetected();
            Reflect(hitData.point, newHitData);
        }
        else
        {
            NoBubbleDetected();
            _points.Add(hitData.point + newDirection * maxDistance);
        }
    }

    private void NoBubbleDetected()
    {
        targetMark.Hide();
    }
    private void OnHitBubble(BubbleScript bubble,RaycastHit2D hit,bool doAnim)
    {
        var neighborsCellList = bubble.neighbors.GetAvailableNeighbors();
        var minDistance = 10000f;
        var targetPos = Vector2.zero;
        var targetColor = GameController.Instance.GetType(currentType).color;
        _targetDimension = null;
        foreach (var n in neighborsCellList) {
            if(n.gridPosition == null) continue;
            var d = Vector2.Distance (grid.GetScenePosition(n.gridPosition), hit.point);
            if ( d < minDistance && n.canUseIt && n.bubble == null) {
                minDistance = d;
                targetPos = grid.GetScenePosition(n.gridPosition);
                _targetDimension = n.gridPosition;
            }
        }
        if (_targetDimension == null)
        {
            print("target neighbor not found");
            return;
        }
        
        targetMark.Show(targetPos,targetColor,_targetDimension,doAnim);
    }
    
    private void Shoot()
    {
        AudioManager.Instance.Play(SoundList.Shoot,Random.Range(1,1.3f));
        bullet.SetType (this.currentType,true);
        bullet.gameObject.SetActive (true);
        bullet.transform.position = transform.position;
        if (GameController.Instance.aimSetting == AimSetting.AimOnAvailableNeighbor && this._targetDimension != null)
        {
            bullet.GetComponent<Bullet>().SetTargetBubble(this._targetDimension);
        }
        bullet.GetComponent<Bullet>().Move(_points,this.speed);
        
        GetNextType();
        ShootAnimation();
    }
}

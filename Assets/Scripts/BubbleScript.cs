using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using PathologicalGames;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

public enum BubbleAnimations
{
	None,Spawn,NewLine,MoveAndExplode,ConvertType,ScaleAndExplode
}
public class BubbleScript : BubbleBase
{
	[TabGroup("Initialize")][SerializeField][LabelText("Bubble Scale")] private Vector3 _startScale;
	[TabGroup("Initialize")][SerializeField][Title("Aim On Neighbor Collider Size")][HideLabel] private float neighborColliderSize;
	[TabGroup("Initialize")][SerializeField][Title("Aim On Bubble Collider Size")][HideLabel] private float bubbleColliderSize;
	
	[TabGroup("Neighbors")] public Neighbors neighbors;

	[TabGroup("Physics")] [SerializeField]
	private ForceMode2D forceMode;
	[TabGroup("Physics")] [SerializeField][MinMaxSlider(-10, 10,true)]
	private Vector2 torqueForceRange;
	[TabGroup("Physics")] [SerializeField] [MinMaxSlider(0, 5,true)]
	private Vector2 impulseForce;
	[TabGroup("Physics")] [SerializeField]
	private float gravityScale = 1;
	
	//- Read Only
	[BoxGroup("Tracking Values")][ReadOnly][HideLabel][Title("Grid Position")] 
	public Dimension gridPosition;
	[BoxGroup("Tracking Values")][ReadOnly][SerializeField][HideLabel][Title("Scene Position")] 
	private Vector3 scenePosition;
	[BoxGroup("Tracking Values")][ReadOnly] public bool isShifted;
	[BoxGroup("Tracking Values")][ReadOnly] public bool isConnected;
	[BoxGroup("Tracking Values")][ReadOnly] public bool isVisited;
	[BoxGroup("Tracking Values")][ReadOnly] public bool isFall;
	
	//- privates
	private BubbleGrid _parentGrid;
	private float _lastOffset;
	private int _newTypeIndex;
	private Action<BubbleScript> _newTypeCallback;
	private Action _moveAndExplodeCallback;
	private bool _isRemovedFromGrid;
	private BubbleAnimations animation;
	
	[BoxGroup("Test")] [SerializeField] private Color neighborsColor;
	private Rigidbody2D _rigidbody;
	public Rigidbody2D Rigidbody
	{
		get
		{
			if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody2D>();
			return _rigidbody;
		}
	}
	private CircleCollider2D _collider;
	public CircleCollider2D collider
	{
		get
		{
			if (_collider == null) _collider = GetComponent<CircleCollider2D>();
			return _collider;
		}
	}
	
	public override void Initialize()
	{
		base.Initialize();
		neighborsColor = Color.red;
		isVisited = false;
		animation = BubbleAnimations.None;
		_isRemovedFromGrid = false;
		if (GameController.Instance.aimSetting == AimSetting.AimOnAvailableNeighbor)
		{
			collider.radius = this.neighborColliderSize;
		}
		else
		{
			collider.radius = this.bubbleColliderSize;
		}
	}

	public void SpawnNewBubble(BubbleGrid parentGrid, Dimension position ,float offset)
	{
		this._parentGrid = parentGrid;
		this.gridPosition = position;
		if (gridPosition.rows == 0) isConnected = true;
		if (offset <= -100)
		{
			this._lastOffset = position.rows % 2 == 0
				? 0f
				: _parentGrid.rowsOffset;
		}
		else this._lastOffset = offset;
		
		isShifted = _lastOffset > 0;
		
		this.scenePosition = new Vector3 ( 
			((this.gridPosition.columns * _parentGrid.bubbleSize) - _parentGrid.firstBubblePos.x) + _lastOffset + _parentGrid.leftPadding, 
			(((-this.gridPosition.rows  * _parentGrid.bubbleSize) + _parentGrid.firstBubblePos.y) + _parentGrid.topPadding), 
			0f);

		transform.localPosition = this.scenePosition;
		transform.localScale = Vector3.zero;
		if(!HandleAnimation(BubbleAnimations.Spawn)) return;
		StopCoroutine("DOScale");
		animation = BubbleAnimations.Spawn;
		StartCoroutine(DOScale(this._startScale, _parentGrid.spawnSpeed));
	}

	public void SetNewLinePosition(BubbleGrid parentGrid, Dimension position)
	{
		this._parentGrid = parentGrid;
		this.gridPosition = position;
		if (gridPosition.rows == 0)
		{
			isConnected = true;
		}
		isShifted = this._lastOffset > 0;
		this.scenePosition = new Vector3 ( 
			((this.gridPosition.columns * _parentGrid.bubbleSize) - _parentGrid.firstBubblePos.x) + this._lastOffset + _parentGrid.leftPadding, 
			(((-this.gridPosition.rows  * _parentGrid.bubbleSize) + _parentGrid.firstBubblePos.y) + _parentGrid.topPadding), 
			0f);

		if(!HandleAnimation(BubbleAnimations.NewLine)) return;
		StopCoroutine("DOMove");
		animation = BubbleAnimations.NewLine;
		StartCoroutine(DOMove(scenePosition, _parentGrid.newLineSpeed, () =>
		{
			transform.localScale = this._startScale;
		}));
	}

	public void SetPosition (BubbleGrid parentGrid, Dimension position)
	{
		neighborsColor = Color.red;
		this._parentGrid = parentGrid;
		this.gridPosition = position;
		if (gridPosition.rows == 0)
		{
			isConnected = true;
		}
		var firstBubble = this._parentGrid.GetFirstRowBubble();
		if (firstBubble != null && firstBubble.isShifted)
		{
			this._lastOffset = position.rows % 2 == 0
				? _parentGrid.rowsOffset
				: 0f;
		}
		else
		{
			this._lastOffset = position.rows % 2 == 0
				? 0f
				: _parentGrid.rowsOffset;
		}
		
		isShifted = _lastOffset > 0;
		
		this.scenePosition = new Vector3 ( 
			((this.gridPosition.columns * _parentGrid.bubbleSize) - _parentGrid.firstBubblePos.x) + _lastOffset + _parentGrid.leftPadding, 
			(((-this.gridPosition.rows  * _parentGrid.bubbleSize) + _parentGrid.firstBubblePos.y) + _parentGrid.topPadding), 
			0f);

		transform.position = scenePosition;
	}

	private IEnumerator DOMove(Vector3 position, float timeToMove,Action callback = null,float delay = 0f)
	{
		if(delay > 0) yield return new WaitForSeconds(delay);
		var currentPos = transform.position;
		var t = 0f;
		while(t < 1)
		{
			if(!gameObject.activeInHierarchy) yield break;
			t += Time.deltaTime / timeToMove;
			transform.position = Vector3.Lerp(currentPos, position, t);
			yield return null;
		}
		animation = BubbleAnimations.None;
		transform.position = position;
		callback?.Invoke();
	}
	
	private IEnumerator DOScale(Vector3 scale, float timeToMove,Action callback = null,float delay = 0f)
	{
		if(delay > 0) yield return new WaitForSeconds(delay);
		var currentScale = transform.localScale;
		var t = 0f;
		while(t < 1)
		{
			if(!gameObject.activeInHierarchy) yield break;
			t += Time.deltaTime / timeToMove;
			transform.localScale = Vector3.Lerp(currentScale, scale, t);
			yield return null;
		}
		animation = BubbleAnimations.None;
		transform.localScale = scale;
		callback?.Invoke();
	}
	
	[BoxGroup("Test")][Button("Explode With Neighbors", ButtonSizes.Medium)]
	public void ExplodeWithNeighbors()
	{
		foreach (var n in neighbors.GetNeighborsList())
		{
			if(n.bubble == null) continue;
			n.bubble.MoveToAndExplode(transform.position,_parentGrid.mergeExplodeDelay);
		}
		
		if(!HandleAnimation(BubbleAnimations.ScaleAndExplode)) return;
		RemoveFromParentGrid();
		Sprite.DOFade(0.2f, _parentGrid.mergeDuration).SetDelay((_parentGrid.mergeExplodeDelay * 2));
		StopCoroutine("DOScale");
		animation = BubbleAnimations.ScaleAndExplode;
		StartCoroutine(DOScale(new Vector3(0.6f, 0.6f), _parentGrid.mergeDuration, () =>
		{
			transform.localScale = this._startScale;
			Explode(true);
		}));
	}
	
	private void OnRemoved(bool updatePosGrid)
	{
		StopAllCoroutines();
		TurnPhysicsOff();
		isConnected = false;
		this.isShifted = false;
		this.neighbors = new Neighbors();
		if (_isRemovedFromGrid) _parentGrid.RemoveVisualBubble(this, updatePosGrid);
		else _parentGrid.RemoveBubble(this,updatePosGrid);
	}

	private void RemoveFromParentGrid()
	{
		if(_parentGrid == null) return;
		_isRemovedFromGrid = true;
		_parentGrid.BubblesGrid[gridPosition.rows, gridPosition.columns] = null;
	}
	
	public bool InsideGrid()
	{
		return transform.parent == this._parentGrid.transform;
	}

	[BoxGroup("Test")][Button("Get Matches",ButtonSizes.Medium)]
	public void GetMatches()
	{
		_parentGrid.CheckMatchesForBubbles(this);
		foreach (var b in _parentGrid._matchBubblesList)
		{
			b.Sprite.color = this.neighborsColor;
		}
	}

	[BoxGroup("Test")][Button("Turn Physics On", ButtonSizes.Medium)]
	public void TurnPhysicsOn()
	{
		isFall = true;
		RemoveFromParentGrid();
		Rigidbody.isKinematic = false;
		Rigidbody.gravityScale = this.gravityScale;
		Rigidbody.AddForce(Vector2.up * Random.Range(this.impulseForce.x,this.impulseForce.y) ,this.forceMode);
		Rigidbody.AddTorque(Random.Range(torqueForceRange.x,torqueForceRange.y),this.forceMode);
	}
	
	public void TurnPhysicsOff()
	{
		isFall = false;
		Rigidbody.isKinematic = true;
		Rigidbody.gravityScale = 0f;
	}

	[BoxGroup("Test")] [Button("Explode", ButtonSizes.Medium)]
	public void Explode(bool updateGridPos)
	{
		StopAllCoroutines();
		OnRemoved(updateGridPos);
		var particle = PoolManager.Pools["FxPool"].Spawn("BubbleExploseFx");
		if (particle == null) return;
			var bubbleFx = particle.gameObject.GetComponent<BubbleExploseFx>();
			bubbleFx.Play(GameController.Instance.GetType(currentType).color,transform.position);
	}
	
	public void ResetWhenCleaningGrid()
	{
		StopAllCoroutines();
		neighbors = new Neighbors();
		isVisited = false;
		isConnected = false;
		isFall = false;
		isShifted = false;
		transform.localScale = this._startScale;
		TurnPhysicsOff();
	}
	
	public void MoveToAndExplode(Vector2 pos,float delay = 0f,Action callback = null)
	{
		RemoveFromParentGrid();

		if(!HandleAnimation(BubbleAnimations.MoveAndExplode)) return;
		_moveAndExplodeCallback = callback;
		StopCoroutine("DOMove");
		animation = BubbleAnimations.MoveAndExplode;
		StartCoroutine(DOMove(pos, _parentGrid.mergeDuration, () =>
		{
			Explode(true);
			callback?.Invoke();
			_moveAndExplodeCallback = null;
		}, delay));
	}

	public void ConvertToNewType(int typeIndex,Action<BubbleScript> callback)
	{
		if(!HandleAnimation(BubbleAnimations.ConvertType)) return;
		_newTypeIndex = typeIndex;
		_newTypeCallback = callback;
		if (_parentGrid.mergeFadeOut)
		{
			Sprite.DOFade(0.2f, _parentGrid.mergeDuration);
		}
		StopCoroutine("DOScale");
		animation = BubbleAnimations.ConvertType;
		StartCoroutine(DOScale(new Vector3(0.6f, 0.6f), _parentGrid.mergeDuration, () =>
		{
			transform.localScale = this._startScale;
			if (_parentGrid.mergeFadeOut) Sprite.DOFade(1, 0);
			SetType(typeIndex, true);
			callback?.Invoke(this);
			_newTypeCallback = null;
		}, _parentGrid.mergeExplodeDelay));
	}
	
	public void SetNeighbors(Neighbors n)
	{
		this.neighbors = n;
		if (gridPosition.rows == 0)
		{
			isConnected = true;
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if(!other.gameObject.CompareTag("BottomWall")) return;
		Explode(false);
	}
	
	//- Set Animation Priority
	private bool HandleAnimation(BubbleAnimations newAnimation)
	{
		if (!gameObject.activeInHierarchy) return false;
		switch (newAnimation)
		{
			case BubbleAnimations.Spawn:
				if (this.animation == BubbleAnimations.None) return true;
				else return false;
			
			case BubbleAnimations.NewLine:
				if (this.animation == BubbleAnimations.None) return true;
				else if (this.animation == BubbleAnimations.Spawn)
				{
					transform.localScale = _startScale;
					return true;
				}
				else if (this.animation == BubbleAnimations.ConvertType)
				{
					transform.localScale = _startScale;
					Sprite.DOFade(1, 0);
					SetType(_newTypeIndex, true);
					_newTypeCallback?.Invoke(this);
					return true;
				}
				else if (this.animation == BubbleAnimations.MoveAndExplode)
				{
					_moveAndExplodeCallback?.Invoke();
					Explode(true);
					return false;
				}
				else if (this.animation == BubbleAnimations.ScaleAndExplode)
				{
					transform.localScale = this._startScale;
					Explode(true);
					return false;
				}
				break;
			case BubbleAnimations.ConvertType:
				if (this.animation == BubbleAnimations.None) return true;
				else if (this.animation == BubbleAnimations.Spawn)
				{
					transform.localScale = _startScale;
					return true;
				}
				else if (this.animation == BubbleAnimations.NewLine) return true;
				else if (this.animation == BubbleAnimations.MoveAndExplode)
				{
					_moveAndExplodeCallback?.Invoke();
					Explode(true);
					return false;
				}
				else if (this.animation == BubbleAnimations.ScaleAndExplode)
				{
					transform.localScale = this._startScale;
					Explode(true);
					return false;
				}
				break;
			case BubbleAnimations.MoveAndExplode: return true;
			case BubbleAnimations.ScaleAndExplode: return true;
			default: return true;
		}

		return true;
	}
	
}

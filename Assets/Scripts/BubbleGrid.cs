using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using PathologicalGames;
using UnityEngine;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;
[System.Serializable]
public class Dimension
{
    [HorizontalGroup(5,5)]
    public int rows;
    [HorizontalGroup(5,5)]
    public int columns;

    public Dimension(int r,int c)
    {
        this.rows = r;
        this.columns = c;
    }

    public Dimension()
    {
        this.rows = 0;
        this.columns = 0;
    }
}
public enum Directions
{
    Left,Right,TopLeft,TopRight,DownLeft,DownRight
}
[System.Serializable]
public struct NeighborCell
{
    public Directions direction;
    public BubbleScript bubble;
    public Dimension gridPosition;
    public bool canUseIt;
    
    public NeighborCell(Directions dir)
    {
        this.direction = dir;
        this.bubble = null;
        gridPosition = new Dimension();
        canUseIt = false;
    }
    public NeighborCell(Directions dir, BubbleScript b)
    {
        this.direction = dir;
        this.bubble = b;
        gridPosition = new Dimension();
        canUseIt = false;
    }
    public NeighborCell(Directions dir, BubbleScript b,Dimension pos)
    {
        this.direction = dir;
        this.bubble = b;
        this.gridPosition = pos;
        canUseIt = false;
    }
}

[System.Serializable]
public struct Neighbors
{
    [Title("Neighbors Bubbles")]
    public NeighborCell left;
    public NeighborCell right;
    public NeighborCell topLeft;
    public NeighborCell topRight;
    public NeighborCell downLeft;
    public NeighborCell downRight;

    public void Initialize()
    {
        this.left = new NeighborCell(Directions.Left);
        this.right = new NeighborCell(Directions.Right);
        this.topLeft = new NeighborCell(Directions.TopLeft);
        this.topRight = new NeighborCell(Directions.TopRight);
        this.downLeft = new NeighborCell(Directions.DownLeft);
        this.downRight = new NeighborCell(Directions.DownRight);
    }
    
    public List<NeighborCell> GetAvailableNeighbors()
    {
        return GetNeighborsList().Where(n => n.bubble == null).ToList();
    }
   
    public List<NeighborCell> GetEnabledNeighborsList()
    {
        return GetNeighborsList().Where(n => n.bubble != null).ToList();
    }
    public List<NeighborCell> GetNeighborsList()
    {
        return new List<NeighborCell>()
        {
            this.left,
            this.right,
            this.topLeft,
            this.topRight,
            this.downLeft,
            this.downRight
        };
    }
}
public class BubbleGrid : MonoBehaviour
{
    //- Initialize Tab
    [TabGroup("Initialize")][SerializeField] private string poolName;
    [TabGroup("Initialize")][SerializeField] private string bubblePrefabName;
    [TabGroup("Initialize")][Range(2,12)] public int startRows = 5;
    [TabGroup("Initialize")][HideLabel][Title("Dimension")] public Dimension gridDimension;
    
    //- Settings Tab
    [TabGroup("Settings")][Range(-5,5)] public float topPadding;
    [TabGroup("Settings")][Range(-5,5)] public float leftPadding;
    [TabGroup("Settings")][Range(-5,5)] public float rowsOffset = 0.25f;
    [TabGroup("Settings")]public float bubbleSize = 0.7f;
    
    //- Animations tab
    [TabGroup("Animations")] [Title("New Line animation")]
    [Range(0, 5f)][LabelText("Speed")] public float newLineSpeed;
    [TabGroup("Animations")] [Title("Spawn animation")]
    [Range(0, 5f)][LabelText("Speed")]public float spawnSpeed;
    [TabGroup("Animations")] [Title("Merge Bubbles animation")]
    [Range(0, 5f)][LabelText("duration")] public float mergeDuration;
    [TabGroup("Animations")] [Range(0, 1f)][LabelText("Explode Delay")]
    public float mergeExplodeDelay;
    [TabGroup("Animations")] [LabelText("Use FadeOut")]
    public bool mergeFadeOut;
    
    [ShowInInspector][HideLabel][Title("Grid View")]
    private BubbleScript[,] _gridBubbles;
    
    [HideInInspector] public List<BubbleScript> _matchBubblesList;
    [HideInInspector] public Vector2 firstBubblePos;
    [HideInInspector] public bool shouldAddNewLine;
    
    //-privates
    private bool _isInitialized;
    private List<int> _typePool; //a list of random type indexes
    private int _lastType;
    private System.Random _rnd;
    private List<NeighborCell> _connectedList;
    private int _nbCombo;
    private float _newLineOffset;
    private Action _createNewLineCallback;
    private BubbleScript _newBubble;
    
    public BubbleScript[,] BubblesGrid
    {
        get { return this._gridBubbles; }
    }

    private void Start()
    {
        if(GameController.Instance.aimSetting == AimSetting.AimOnBubble)
        {
            Bullet.OnHitBubble += AddBubble;
            Bullet.OnHitNeighbor -= AddNeighbor;
        }
        else if(GameController.Instance.aimSetting == AimSetting.AimOnAvailableNeighbor)
        {
            Bullet.OnHitBubble -= AddBubble;
            Bullet.OnHitNeighbor += AddNeighbor;
        }
    }

    public void Initialize()
    {
        _gridBubbles = new BubbleScript[gridDimension.rows, gridDimension.columns];
        _isInitialized = true;
        shouldAddNewLine = false;
    }

    public void RestartGrid()
    {
        ClearGrid();
        BuildGrid();
    }
    
    [Title("Testing")][Button("Build",ButtonSizes.Gigantic)][GUIColor(0,1,0)]
    private void BuildGrid()
    {
        if(!_isInitialized) Initialize();
       
        firstBubblePos = new Vector2(
            (gridDimension.columns * bubbleSize) * 0.5f,
            (gridDimension.rows * bubbleSize) * 0.5f
        );
		
        firstBubblePos.x -= bubbleSize * 0.5f;
        firstBubblePos.y -= bubbleSize * 0.5f;

        for (var row = 0; row < gridDimension.rows; row++)
        {
            for (var column = 0; column < gridDimension.columns; column++)
            {
               var bubble = PoolManager.Pools[this.poolName].Spawn(this.bubblePrefabName, this.transform).gameObject.GetComponent<BubbleScript>();
              
                bubble.SpawnNewBubble(this, new Dimension(row,column),-100f);
                bubble.SetType(GetRandomType(),true);
                bubble.isConnected = true;
                _gridBubbles[row, column] = bubble;
            }

            if (row == startRows - 1)
            {
                break;
            }
        }
        
        UpdateBubblesNeighbors(false,true);
    }

    [Button("Clear", ButtonSizes.Medium)][GUIColor(1,0,0)]
    private void ClearGrid()
    {
        for (var row = 0; row < gridDimension.rows; row++)
        {
            for (var column = 0; column < gridDimension.columns; column++)
            {
                var bubble = _gridBubbles[row, column];
                if(bubble == null) continue;
                bubble.ResetWhenCleaningGrid();
                bubble.transform.SetParent(PoolManager.Pools[this.poolName].transform);
                PoolManager.Pools[this.poolName].Despawn(bubble.transform);
            }
        }
        _gridBubbles = new BubbleScript[gridDimension.rows, gridDimension.columns];
    }

    private void CheckPerfect()
    {
        var firstBubble = GetFirstRowBubble();
        if (firstBubble == null)
        {
            AddNewLine(UpdateDisconnectedBubbles);
            //board is Empty
            AudioManager.Instance.Play(SoundList.Perfect,Random.Range(1f,1.2f));
            UIManager.Instance.notifications.PlayValueNotification("",NotificationsType.Perfect);
        }
    }
    
    [Button("Add New Line",ButtonSizes.Medium)]
    public void AddNewLine(Action callback)
    {
        var firstBubble = GetFirstRowBubble();
        var newLineOffset = 0f;
        if (firstBubble != null)
        {
            newLineOffset = firstBubble.isShifted
                ? 0
                : rowsOffset;
        }
        else
        {
            //board is Empty
            AudioManager.Instance.Play(SoundList.Perfect);
            UIManager.Instance.notifications.PlayValueNotification("",NotificationsType.Perfect);
        }
        this._newLineOffset = newLineOffset;
        this._createNewLineCallback = callback;
        
        for (var row = gridDimension.rows-1; row >= 0; row--)
        {
            for (var column = gridDimension.columns-1; column >= 0; column--)
            {
                var bubble = _gridBubbles[row, column];
                if(bubble == null) continue;
                var newPos = new Dimension()
                {
                    rows = bubble.gridPosition.rows + 1,
                    columns = bubble.gridPosition.columns
                };
                bubble.isConnected = true;
                bubble.SetNewLinePosition(this,newPos);
                this._gridBubbles[row, column] = null;
                this._gridBubbles[newPos.rows, newPos.columns] = bubble;
            }
        }
        Invoke("GenerateLine",newLineSpeed);
        shouldAddNewLine = false;
    }

    private void GenerateLine()
    {
        //On Animation Complete ! Generate New Bubbles.
        for (var i = 0; i < gridDimension.columns; i++)
        {
            var newBubble = PoolManager.Pools[this.poolName].Spawn(this.bubblePrefabName, this.transform)
                .GetComponent<BubbleScript>();
            newBubble.ResetWhenCleaningGrid();
            newBubble.SpawnNewBubble(this, new Dimension(0,i),_newLineOffset);
            newBubble.SetType(GetRandomType(),true);
            newBubble.isConnected = true;
            this._gridBubbles[0, i] = newBubble;
        }     
        _createNewLineCallback?.Invoke();
    }
    
    public void RemoveBubble(BubbleScript bubble,bool updatePosition)
    {
        this._gridBubbles[bubble.gridPosition.rows, bubble.gridPosition.columns] = null;
        bubble.transform.SetParent(PoolManager.Pools[this.poolName].transform);
        PoolManager.Pools[this.poolName].Despawn(bubble.transform);
        if(updatePosition) UpdateBubblesNeighbors(false,true);
    }
    
    public void RemoveVisualBubble(BubbleScript bubble,bool updatePosition)
    {
        bubble.transform.SetParent(PoolManager.Pools[this.poolName].transform);
        PoolManager.Pools[this.poolName].Despawn(bubble.transform);
        if(updatePosition) UpdateBubblesNeighbors(false,true);
    }
    
    [Button("Update Position", ButtonSizes.Medium)]
    public void UpdateBubblesPosition()
    {
        for (var row = 0; row < gridDimension.rows; row++)
        {
            for (var column = 0; column < gridDimension.columns; column++)
            {
                var bubble = _gridBubbles[row, column];
                if(bubble == null) continue;
                bubble.SetPosition(this, new Dimension(row,column));
            }
        }
    }
    
    public void UpdateBubblesNeighbors(bool checkLastRow = true,bool updatePosition = false)
    {
        for (var row = 0; row < gridDimension.rows; row++)
        {
            for (var column = 0; column < gridDimension.columns; column++)
            {
                var bubble = _gridBubbles[row, column];
                if(bubble == null) continue;
                bubble.SetNeighbors(GetBubbleNeighbors(bubble.gridPosition,bubble.isShifted));
                if(updatePosition) bubble.SetPosition(this, new Dimension(row,column));
            }
        }
        if(!checkLastRow) return;
        if (IsLastRowHasBubble())
        {
            GameController.Instance.GameOver();
        }
    }

    private Neighbors GetBubbleNeighbors(Dimension gridPos,bool isShifted)
    {
        var neighbors = new Neighbors();
        neighbors.Initialize();
        
        //Left & Right
        neighbors.left.bubble = gridPos.columns > 0
            ? this._gridBubbles[gridPos.rows, gridPos.columns - 1]
            : null;
        neighbors.left.gridPosition = new Dimension(gridPos.rows,gridPos.columns - 1);
        neighbors.left.canUseIt = gridPos.columns > 0;
            
        neighbors.right.bubble = (gridPos.columns + 1) < gridDimension.columns
            ? this._gridBubbles[gridPos.rows, gridPos.columns + 1]
            : null;
        neighbors.right.gridPosition = new Dimension(gridPos.rows,gridPos.columns + 1);
        neighbors.right.canUseIt = (gridPos.columns + 1) < gridDimension.columns;
            
        //- Top Right & Left
        if (gridPos.rows > 0)
        {
            if (isShifted)
            {
                neighbors.topLeft.gridPosition = new Dimension(gridPos.rows - 1, gridPos.columns);
                neighbors.topLeft.bubble = this._gridBubbles[gridPos.rows - 1, gridPos.columns];
                
                neighbors.topRight.gridPosition = new Dimension(gridPos.rows - 1,gridPos.columns + 1);
                neighbors.topRight.bubble = (gridPos.columns + 1) < gridDimension.columns 
                    ? this._gridBubbles[gridPos.rows - 1, gridPos.columns + 1] 
                    : null;
            }
            else
            {
                neighbors.topLeft.gridPosition = new Dimension(gridPos.rows - 1, gridPos.columns - 1);
                neighbors.topLeft.bubble = (gridPos.columns - 1) >= 0
                    ? this._gridBubbles[gridPos.rows - 1, gridPos.columns - 1] 
                    : null;
                
                neighbors.topRight.gridPosition = new Dimension(gridPos.rows - 1, gridPos.columns);
                neighbors.topRight.bubble = this._gridBubbles[gridPos.rows - 1, gridPos.columns];
            }
        }
        else
        {
            neighbors.topLeft.canUseIt = false;
            neighbors.topRight.canUseIt = false;
            neighbors.topLeft.bubble = null;
            neighbors.topRight.bubble = null;
        }

        //- Down Left & Right
        if ((gridPos.rows + 1) >= gridDimension.rows)
        {
            if (isShifted)
            {
                neighbors.downLeft.canUseIt = true;
                neighbors.downRight.canUseIt = ((gridPos.columns + 1) < gridDimension.columns);
            }
            else
            {
                neighbors.downLeft.canUseIt = gridPos.columns > 0;
                neighbors.downRight.canUseIt = true;
            }
            neighbors.downLeft.bubble = null;
            neighbors.downRight.bubble = null;
        }
        else
        {
            if (isShifted)
            {
                neighbors.downLeft.gridPosition = new Dimension(gridPos.rows + 1, gridPos.columns);
                neighbors.downLeft.bubble = this._gridBubbles[gridPos.rows + 1, gridPos.columns];
                neighbors.downLeft.canUseIt = true;
                
                neighbors.downRight.gridPosition = new Dimension(gridPos.rows + 1, gridPos.columns + 1);
                neighbors.downRight.bubble = ((gridPos.columns + 1) < gridDimension.columns)
                    ? this._gridBubbles[gridPos.rows + 1, gridPos.columns + 1]
                    : null;
                neighbors.downRight.canUseIt = ((gridPos.columns + 1) < gridDimension.columns);

            }
            else
            {
                neighbors.downLeft.gridPosition = new Dimension(gridPos.rows + 1, gridPos.columns - 1);
                neighbors.downLeft.bubble = (gridPos.columns > 0)
                    ? this._gridBubbles[gridPos.rows + 1, gridPos.columns - 1]
                    : null;
                neighbors.downLeft.canUseIt = gridPos.columns > 0;
                
                neighbors.downRight.gridPosition = new Dimension(gridPos.rows + 1, gridPos.columns);
                neighbors.downRight.bubble = this._gridBubbles[gridPos.rows + 1, gridPos.columns];
                neighbors.downRight.canUseIt = true;
            }
        }
        
        return neighbors;
    }

    private void ResetVisitedAndConnectedBubbles()
    {
        for (var row = 0; row < gridDimension.rows; row++)
        {
            for (var column = 0; column < gridDimension.columns; column++)
            {
                if(_gridBubbles[row,column] == null) continue;
                _gridBubbles[row, column].isVisited = false;
                _gridBubbles[row, column].isConnected = false;
            }
        }
    }

    private List<BubbleScript> GetMatchesAroundBubble (BubbleScript bubble) 
    {
        var result = new List<BubbleScript> () { bubble };
        var neighbors = bubble.neighbors.GetEnabledNeighborsList();
        bubble.isVisited = true;
        foreach (var b in neighbors) {
            if (b.bubble.currentType == bubble.currentType) {
                result.Add (b.bubble);
            }
        }
        return result;
    }
    
    private void AddMatches (List<BubbleScript> matches) {
        foreach (var bubble in matches) {
            if (!_matchBubblesList.Contains(bubble))
                _matchBubblesList.Add(bubble);
        }
        _matchBubblesList.Distinct();
    }
    
    private void AddNeighbor(Bullet bullet, Dimension target)
    {
        UpdateDisconnectedBubbles();
        Bullet.OnHitNeighbor -= AddNeighbor;
        UpdateBubblesNeighbors(false);
        this._newBubble = PoolManager.Pools[this.poolName].Spawn(this.bubblePrefabName,this.transform).gameObject
            .GetComponent<BubbleScript>();
        this._newBubble.SetType(bullet.currentType,true);
        
        this._newBubble.isConnected = true;
        this._gridBubbles[target.rows,target.columns] = this._newBubble;
        this._newBubble.SpawnNewBubble(this,target,-100f);
        Invoke("OnNewBubbleConnected",spawnSpeed);
        UpdateBubblesNeighbors(false);
    }

    public void AddBubble(Bullet bullet, BubbleScript targetBubble)
    {
        UpdateDisconnectedBubbles();
        Bullet.OnHitBubble -= AddBubble; //unsubscribe
        UpdateBubblesNeighbors(false);
        this._newBubble = PoolManager.Pools[this.poolName].Spawn(this.bubblePrefabName,this.transform).gameObject
            .GetComponent<BubbleScript>();
        this._newBubble.SetType(bullet.currentType,true);
        
        var neighborsCellList = targetBubble.neighbors.GetAvailableNeighbors();
        var minDistance = 10000f; //set a big dist as min
        var newBubblePos = new Dimension();
        foreach (var n in neighborsCellList) {
            var d = Vector2.Distance (GetScenePosition(n.gridPosition), bullet.transform.position);
            if ( d < minDistance && n.canUseIt && n.bubble == null) {
                minDistance = d;
                newBubblePos = n.gridPosition;
            }
        }
        this._newBubble.isConnected = true;
        this._gridBubbles[newBubblePos.rows,newBubblePos.columns] = this._newBubble;
        this._newBubble.SpawnNewBubble(this,newBubblePos,-100f);
        Invoke("OnNewBubbleConnected",spawnSpeed);
        UpdateBubblesNeighbors(false);
    }

    private void OnNewBubbleConnected()
    {
        AudioManager.Instance.Play(SoundList.Connect,Random.Range(0.9f,1.2f));
        UpdateDisconnectedBubbles();
        if (IsLastRowHasBubble())
        {
           GameController.Instance.GameOver();
           return;
        }
        
        UpdateBubblesPosition();
        //- Merge bubbles
        CheckMatchesForBubbles(_newBubble);
        //make a tmp copy
        var initialList = new List<BubbleScript>(this._matchBubblesList);
        this._matchBubblesList.Clear();
        
        if (initialList.Count < 2)
        {
            if (shouldAddNewLine)
            {
                AddNewLine(null);
            }
            Subscribe();
            return;
        }
            AudioManager.Instance.Play(SoundList.Merge,Random.Range(1.3f,1.5f));
            var startType = _newBubble.currentType;
            var newType = GameController.Instance.GetNewTypeIndex(startType,initialList.Count);
            var targetIndex = -1;
            var maxBubble = 1;
            
            for (var i = 0; i < initialList.Count; i++)
            {
                var b = initialList[i];
                b.SetType(newType,false);
                CheckMatchesForBubbles(b);
                var possibility = new List<BubbleScript>(this._matchBubblesList);
                this._matchBubblesList.Clear();
                if (possibility.Count > maxBubble)
                {
                    maxBubble = possibility.Count;
                    targetIndex = i; //store the index of the biggest Possibility!
                }
                b.SetType(startType,false);
            }
            
            _matchBubblesList.Clear();
            BubbleScript finalBubble = null;
            var index = 0;
            _nbCombo++;
            if (_nbCombo > 1)
            {
                var comboPitch = 1f;
                switch (_nbCombo)
                {
                    case 2: comboPitch = Random.Range(0.9f, 1.1f); break;
                    case 3:  comboPitch = Random.Range(1.2f, 1.4f); break;
                    case 4:  comboPitch = Random.Range(1.5f, 1.7f); break;
                    default: comboPitch = Random.Range(1.5f, 1.7f); break;
                }
                
                AudioManager.Instance.Play(SoundList.Combo,comboPitch);
            }
            
            if (targetIndex == -1)
            {
                //- highest bubble Then biggest neighbors count
                switch (GameController.Instance.mergeBehaviour)
                {
                    case MergeBubbleBehaviours.Highest_Bubble:
                        finalBubble = initialList.FirstOrDefault(bubble => 
                            bubble.neighbors.GetEnabledNeighborsList().Count == initialList.Max(b => b.neighbors.GetEnabledNeighborsList().Count)
                        );
                        break;
                        case MergeBubbleBehaviours.Biggest_Neighbors_Count:
                            finalBubble = initialList.FirstOrDefault(bubble => 
                                bubble.neighbors.GetEnabledNeighborsList().Count == initialList.Max(b => b.neighbors.GetEnabledNeighborsList().Count)
                            );
                            break;
                    case MergeBubbleBehaviours.Highest_Bubble_Then_Biggest_Neighbors_Count:
                        var topBubbles = initialList.Where(
                            bubble => bubble.gridPosition.rows == initialList.Min( b => b.gridPosition.rows)
                        ).ToList();
                
                        finalBubble = topBubbles.FirstOrDefault(bubble => 
                            bubble.neighbors.GetEnabledNeighborsList().Count == topBubbles.Max(b => b.neighbors.GetEnabledNeighborsList().Count)
                        );
                        break;
                    case MergeBubbleBehaviours.Biggest_Neighbors_Count_Then_Highest_Bubble:
                        var biggestNeighbors = initialList.Where(
                            bubble => bubble.neighbors.GetEnabledNeighborsList().Count == initialList.Max(b => b.neighbors.GetEnabledNeighborsList().Count)
                        ).ToList();
                        
                        finalBubble = biggestNeighbors.FirstOrDefault(
                            bubble => bubble.gridPosition.rows == biggestNeighbors.Min( b => b.gridPosition.rows)
                        );
                        break;
                }
                
                if(finalBubble == null) throw new Exception("Error : final bubble is null !");
                finalBubble.ConvertToNewType(newType,OnConnectAfterMerge);
                
                foreach (var b in initialList)
                {
                    if(b == finalBubble) continue;
                    index++;
                    if(index >= 5) b.Explode(false);
                    else b.MoveToAndExplode(finalBubble.gameObject.transform.position);
                }
            }
            else
            {
                finalBubble = initialList[targetIndex];
                finalBubble.ConvertToNewType(newType,OnConnectAfterMerge);
                
                foreach (var b in initialList)
                {
                    if(b == finalBubble) continue;
                    index++;
                    if(index >= 5) b.Explode(false);
                    else b.MoveToAndExplode(finalBubble.gameObject.transform.position);
                }
            }
    }

    public void ResetCombo()
    {
        _nbCombo = 0;
    }
    
    public void Subscribe()
    {
        if (!Bullet.HasEvent())
        {
            if(GameController.Instance.aimSetting == AimSetting.AimOnBubble)
            {Bullet.OnHitBubble += AddBubble;}
            else{Bullet.OnHitNeighbor += AddNeighbor;}
        }
    }

    public void EmptyShoot()
    {
        if (shouldAddNewLine)
        {
            AddNewLine(() =>
            {
                UpdateDisconnectedBubbles();
                Subscribe();
            });
        }
        else  UpdateDisconnectedBubbles();
    }
    
    private void OnConnectAfterMerge(BubbleScript newBubble)
    {
        _newBubble = newBubble;
        if (_nbCombo > 1)
        {
            var combo = "x" + _nbCombo;
            UIManager.Instance.notifications.PlayValueNotification(combo,NotificationsType.MergeCombo);
        }
        var value = GameController.Instance.GetType(newBubble.currentType).value.ToString();
        if (GameController.Instance.IsMaxValue(newBubble.currentType))
        {
            //reached max (2024) -> explode with neighbors
            newBubble.ExplodeWithNeighbors();
            if (shouldAddNewLine)  AddNewLine(UpdateDisconnectedBubbles);
            else
            {
                CheckPerfect();
                UpdateDisconnectedBubbles();
            }
            Subscribe();
            value = "Awesome " + value + "!";
            AudioManager.Instance.Play(SoundList.Achieve2028,Random.Range(0.9f,1.1f));
            UIManager.Instance.notifications.PlayValueNotification(value,newBubble.transform.position,NotificationsType.BubbleValue);
            return;
        }
       
        CheckPerfect();
        UIManager.Instance.notifications.PlayValueNotification(value,newBubble.transform.position,NotificationsType.BubbleValue);
        UpdateDisconnectedBubbles();
        OnNewBubbleConnected();
    }

    private bool IsLastRowHasBubble()
    {
        for (var i = 0; i < gridDimension.columns; i++)
        {
            if (_gridBubbles[gridDimension.rows - 1, i] != null) return true;
        }
        return false;
    }
    
    public BubbleScript GetFirstRowBubble()
    {
        for (var i = 0; i < gridDimension.columns; i++)
        {
            if (_gridBubbles[0, i] != null) return _gridBubbles[0, i];
        }
        return null;
    }
    
    public Vector2 GetScenePosition(Dimension posGrid)
    {
        var offset = posGrid.rows % 2 == 0
            ? 0f
            : rowsOffset;
		
        return new Vector3 ( 
            ((posGrid.columns * bubbleSize) - firstBubblePos.x) + offset + leftPadding, 
            (((-posGrid.rows  * bubbleSize) + firstBubblePos.y) + topPadding), 
            0f);
    }

    [Button("Update Disconnected Bubbles",ButtonSizes.Medium)]
    private void UpdateDisconnectedBubbles()
    {
        _connectedList = new List<NeighborCell>();
        ResetVisitedAndConnectedBubbles();
        
        for (var i = 0; i < gridDimension.columns; i++)
        {
            var bubble = _gridBubbles[0, i];
            if(bubble == null) continue;
            SetAllNeighborsConnected(bubble);
        }
        
        UpdateBubblesNeighbors(false,true);
        for (var row = 0; row < gridDimension.rows; row++)
        {
            for (var column = 0; column < gridDimension.columns; column++)
            {
                var bubble = _gridBubbles[row, column];
                if(bubble == null) continue; 
                if(!bubble.isConnected) bubble.TurnPhysicsOn();
            }
        }
    }
    
    private void SetAllNeighborsConnected(BubbleScript targetBubble)
    {
        _connectedList.Clear();
        UpdateBubblesNeighbors(false,true);
        var targetNeighbors = targetBubble.neighbors.GetEnabledNeighborsList();
        if(targetNeighbors.Count == 0) return;
            AddActiveNeighbors(targetNeighbors);
            while (true)
            {
                var allVisited = true;
                for (var i = 0; i < _connectedList.Count; i++)
                {
                    var n = _connectedList[i];
                    if (!n.bubble.isVisited)
                    {
                        n.bubble.isVisited = true;
                        n.bubble.isConnected = true;
                        AddActiveNeighbors(n.bubble.neighbors.GetEnabledNeighborsList());
                        allVisited = false;
                    }
                }
                if (allVisited) return;
            }
    }
    
    private void AddActiveNeighbors (List<NeighborCell> neighborsList) {
        foreach (var neighbor in neighborsList)
        {
            if (!_connectedList.Contains(neighbor))
                _connectedList.Add(neighbor);
        }
        _connectedList.Distinct();
    }
    
    public void CheckMatchesForBubbles (BubbleScript startBubble) 
    {
        this._matchBubblesList.Clear();
        ResetVisitedAndConnectedBubbles();
        UpdateBubblesNeighbors(false,true);
        
        var initialResult = GetMatchesAroundBubble(startBubble);
        _matchBubblesList.AddRange(initialResult);
       
        while (true) {
            var allVisited = true;
            for (var i = _matchBubblesList.Count - 1; i >= 0 ; i--) {
                var b = _matchBubblesList [i];
                if (!b.isVisited)
                {
                    AddMatches (GetMatchesAroundBubble(b));
                    allVisited = false;
                }
            }
            if (allVisited)  return;
        }
    }

    private int GetBubbleType () {
        var random = Random.Range (0f, 1f);
        if (random > GameController.Instance.changingTypeRate) {
            _lastType = Random.Range (0, GameController.Instance.TypeCount);
        }
        return _lastType;
    }
    
    public int GetRandomType()
    {
        if (_typePool == null || _typePool.Count == 0)
        {
            _rnd = new System.Random();
            this._typePool = new List<int> ();
            for (var i = 0; i < 100; i++) _typePool.Add(GetBubbleType());
            ShuffleList(_typePool);
            _typePool = _typePool.OrderBy(a => Guid.NewGuid()).ToList();
        }
        var t = _typePool[0];
        _typePool.RemoveAt (0);
        return t;
    }
    
    private void ShuffleList<T>(IList<T> list)  {  
        var n = list.Count;  
        while (n > 1) {  
            n--;  
            var k = _rnd.Next(n + 1);  
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }
}



using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
[System.Serializable]
public struct BubbleType
{
    public int value;
    [ColorPalette("BubbleColors")]
    public Color color;
}

public enum AimSetting
{
    AimOnBubble,AimOnAvailableNeighbor
}
public enum MergeBubbleBehaviours
{
    Highest_Bubble,
    Biggest_Neighbors_Count,
    Highest_Bubble_Then_Biggest_Neighbors_Count,
    Biggest_Neighbors_Count_Then_Highest_Bubble,
}

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    [BoxGroup("Game Elements")] public BubbleGrid bubbleGrid;
    [BoxGroup("Game Elements")] public BubbleShooter shooter;
    [BoxGroup("Game Elements")] public Transform particleCollider;

    [BoxGroup("Tracking Values")][ReadOnly] public bool isGameStarted;
    [BoxGroup("Tracking Values")][ReadOnly] public bool isGameOnPause;
    
    [TabGroup("Bubbles Types")]
    public int maxType = 2048;
    
    [TabGroup("Bubbles Types")]
    public int maxGeneratedType = 32;
    
    [TabGroup("Bubbles Types")]
    [TableList]
    public List<BubbleType> types;

    [TabGroup("Game Settings")][Title("Merging Behaviour")] [EnumToggleButtons]
    public MergeBubbleBehaviours mergeBehaviour; 
    
    [TabGroup("Game Settings")] [Title("Aim Settings")][EnumToggleButtons]
    public AimSetting aimSetting;
    
    [TabGroup("Game Settings")]
    [Range(1,10)]
    public int newGridLineFreq = 1;
    
    [TabGroup("Game Settings")]
    [Range(0f,1f)] public float changingTypeRate = 0.5f;

    public int TypeCount
    {
        get
        {
            if (types == null)
            {
                throw new Exception("Bubble type list is Empty or null");
            }
            return this.types.Where(t => t.value <= this.maxGeneratedType).ToList().Count;
        }
    }
    
    private void Awake()
    {
        if(Instance != null) return;
        Instance = this;
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        this.bubbleGrid.Initialize();
        this.shooter.Initialize();
        this.shooter.InitNextType();
        this.shooter.GetNextType();
    }

    [Button("Start Game",ButtonSizes.Gigantic)][GUIColor(0,1,0)]
    public void StartGame()
    {
        isGameStarted = true;
        isGameOnPause = false;
        this.bubbleGrid.RestartGrid();
    }

    public void GameOver()
    {
        UIManager.Instance.gameOver.OpenPanel(() =>
        {
            isGameStarted = false;
            isGameOnPause = false;
        });
    }

    public void PauseGame()
    {
        UIManager.Instance.pausePopUp.OpenPanel(() =>
        {
            isGameStarted = false;
            isGameOnPause = true;
        });
    }

    public void ResumeGame()
    {
        isGameStarted = true;
        isGameOnPause = false;
    }
    
    public int GetNewTypeIndex(int typeIndex,int nbMatches)
    {
        var value = GetType(typeIndex).value;
        if (value == 2 && nbMatches > 11) nbMatches = 11;
        else if (value == 4 && nbMatches > 6) nbMatches = 6;
        else if (value == 8 && nbMatches > 4) nbMatches = 4;
        else if (nbMatches > 3) nbMatches = 3;
        var p = (int) Mathf.Pow(value, nbMatches);
        var result =  GetIndexTypeByValue(p);
        return result;
    }

    public bool IsMaxValue(int typeIndex)
    {
        return GetType(typeIndex).value == maxType;
    }
    private int GetIndexTypeByValue(int value)
    {
        var v = Mathf.Abs(value) > maxType ? maxType : value;
        var type =  this.types.FirstOrDefault(t => t.value == v);
        return this.types.IndexOf(type);
    }
    public BubbleType GetType(int typeIndex)
    {
        return this.types[typeIndex];
    }
    
}

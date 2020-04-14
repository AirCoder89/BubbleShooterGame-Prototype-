using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    public NotificationManager notifications;
    [SerializeField] private Button pauseBtn;
    [BoxGroup("PopUps")] public PausePopUp pausePopUp;
    [BoxGroup("PopUps")] public GameOverPopUp gameOver;
    [BoxGroup("PopUps")] public MainGamePanel mainGame;
    private void Awake()
    {
        if(Instance != null) return;
        Instance = this;
    }

    private void Start()
    {
        this.pausePopUp.Initialize();
        this.gameOver.Initialize();
        this.mainGame.Initialize();
        pauseBtn.onClick.AddListener(() =>
        {
            GameController.Instance.PauseGame();
        });
    }

}

using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPopUp : PopUpBase
{
    [BoxGroup("GameOver Popup Elements")][SerializeField][Required][SceneObjectsOnly]
    private Button backToHomeBtn;
    [BoxGroup("GameOver Popup Elements")][SerializeField][Required][SceneObjectsOnly]
    private Button replayBtn;

    public override void Initialize()
    {
        base.Initialize();
        this.backToHomeBtn.onClick.AddListener(OnBackToHome);
        this.replayBtn.onClick.AddListener(OnReplayGame);
    }

    private void OnReplayGame()
    {
        AudioManager.Instance.Play(SoundList.UIButton);
        ClosePanel(() =>
        {
          GameController.Instance.StartGame();
        });
    }

    private void OnBackToHome()
    {
        AudioManager.Instance.Play(SoundList.UIButton);
        ClosePanel(() =>
        {
            UIManager.Instance.mainGame.OpenPanel();
        });
    }
}

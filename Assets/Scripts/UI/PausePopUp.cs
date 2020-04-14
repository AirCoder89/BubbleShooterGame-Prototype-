using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class PausePopUp : PopUpBase
{
   [BoxGroup("Pause Popup Elements")] [SerializeField] [Required] [SceneObjectsOnly]
   private Button resumeGameBtn;
   [BoxGroup("Pause Popup Elements")][SerializeField][Required][SceneObjectsOnly]
   private Button backToHomeBtn;
   [BoxGroup("Pause Popup Elements")][SerializeField][Required][SceneObjectsOnly]
   private Button replayBtn;

   public override void Initialize()
   {
      base.Initialize();
      this.resumeGameBtn.onClick.AddListener(OnClickResume);
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

   private void OnClickResume()
   {
      AudioManager.Instance.Play(SoundList.UIButton);
      ClosePanel(() =>
      {
         GameController.Instance.ResumeGame();
      });
   }
}

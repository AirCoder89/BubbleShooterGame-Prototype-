using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class MainGamePanel : PopUpBase
{
   [BoxGroup("MainGame Popup Elements")][SerializeField][Required][SceneObjectsOnly]
   private Button startGameBtn;

   public override void Initialize()
   {
      base.Initialize();
      this.isOpen = true;
      startGameBtn.onClick.AddListener(OnStartGame);
   }

   private void OnStartGame()
   {
      AudioManager.Instance.Play(SoundList.StartGame);
      ClosePanel(() =>
      {
         GameController.Instance.StartGame();
      });
   }
}

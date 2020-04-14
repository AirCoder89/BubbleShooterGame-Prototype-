using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TargetMark : MonoBehaviour
{
   [BoxGroup("Animation")][SerializeField][Range(0,5f)] private float duration;
   [BoxGroup("Animation")][SerializeField][Range(0,1f)] private float fadeAmount;
   [BoxGroup("Animation")][SerializeField] private Vector2 startScale;
   
   //- private variables
   private SpriteRenderer _sprite;
   private Dimension _dimension;
   public SpriteRenderer Sprite
   {
      get
      {
         if (_sprite == null)
         {
            _sprite = GetComponent<SpriteRenderer>();
         }
         return _sprite;
      }
   }
   
   public void Show(Vector2 position, Color color,Dimension d,bool doAnim)
   {
      if(_dimension == d) return;
      _dimension = d;
      transform.position = position;
      this.Sprite.color = color;
      gameObject.SetActive(true);

      if (doAnim)
      {
         this.Sprite.DOFade(0, 0);
         transform.localScale = Vector3.zero;
         this.Sprite.DOFade(fadeAmount, this.duration);
         transform.DOScale(startScale, this.duration);
      }
      else
      {
         this.Sprite.color = new Color(color.r,color.g,color.b,fadeAmount);
         transform.localScale = startScale;
      }
      
   }

   public void Hide()
   {
      _dimension = null;
      gameObject.SetActive(false);
   }

   public Vector3 GetPosition()
   {
      return transform.position;
   }
}

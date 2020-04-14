using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BubbleBase : MonoBehaviour
{
    [HideInInspector] public int currentType;
    [BoxGroup("Base variables")] public string sortingLayerName;
    [BoxGroup("Base variables")] public int sortingOrder;
    
    //- private variables
    private SpriteRenderer _sprite;
    private TextMesh _textValue;
    private MeshRenderer _textMesh;
    
    protected TextMesh TextValue
    {
        get
        {
            if (_textValue == null) _textValue = GetComponentInChildren<TextMesh>();
            return _textValue;
        }
    }
    
    protected MeshRenderer TextMesh
    {
        get
        {
            if (_textMesh == null) _textMesh = TextValue.gameObject.GetComponent<MeshRenderer>();
            return _textMesh;
        }
    }
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

    public virtual void Initialize()
    {
        TextMesh.sortingLayerName = this.sortingLayerName;
        TextMesh.sortingOrder = this.sortingOrder;
        TextValue.text = GameController.Instance.GetType(this.currentType).value.ToString();
        Sprite.color = GameController.Instance.GetType(this.currentType).color;
    }
    
    public void SetType (int typeIndex,bool reInit = false) {

        this.currentType = typeIndex;
        if(!reInit) return;
         Initialize();
    }
}

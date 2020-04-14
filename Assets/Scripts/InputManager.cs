using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private BubbleShooter bubbleShooter;
    private void OnMouseDown()
    {
        bubbleShooter.MouseDown();
       
    }

    private void OnMouseUp()
    {
        bubbleShooter.MouseUp();
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour, IInputListener
{
    public void OnKeyInput(KeyCode key)
    {
        Log.warn(key.ToString());
    }

    public void OnMouseInput()
    {
        // nothing
    }
}

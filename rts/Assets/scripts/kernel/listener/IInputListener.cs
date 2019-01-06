using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInputListener
{
    void OnMouseInput();
    void OnKeyInput(KeyCode key);
}

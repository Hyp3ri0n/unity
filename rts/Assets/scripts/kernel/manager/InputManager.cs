using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{

    public static InputManager instance { get; private set; }

    public List<IInputListener> _subscribers = new List<IInputListener>();

    void Awake()
    {
        instance = this;

        var ss = FindObjectsOfType<MonoBehaviour>().OfType<IInputListener>();
        foreach (IInputListener s in ss)
        {
            _subscribers.Add(s);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            Log.info("[INPUT] key down, i repeat key down !");
            foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(kcode))
                {
                    foreach (IInputListener s in _subscribers)
                    {
                        s.OnKeyInput(kcode);
                    }
                }
                    
            }
            
        }
    }
}

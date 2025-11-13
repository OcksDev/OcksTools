using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InputBuffer : MonoBehaviour
{
    public static InputBuffer Instance;
    /*
     * how to use:
     *     BufferListen() to run every time you want to check for a new input of a given key
     *     GetBuffer() gets if the button exists in the buffer (aka was pressed by the user)
     *     RemoveBuffer() removes the button from the buffer if it exists
     */
    private void Awake()
    {
        Instance = this;
    }

    public bool GetBuffer(string name)
    {
        return buffer.ContainsKey(name);
    }
    public void RemoveBuffer(string name)
    {
        if (buffer.ContainsKey(name)) buffer.Remove(name);
    }

    public void BufferListen(string key, string ide, string name, float time, bool isdown = true)
    {
        //would be run every frame
        if(isdown? InputManager.IsKeyDown(key, ide) : InputManager.IsKey(key, ide))
        {
            var b = new BufferedInput();
            b.Name = name;
            b.Time = time;
            if (buffer.ContainsKey(name))
            {
                buffer[name] = b;
            }
            else
            {
                buffer.Add(name, b);
            }
        }
    }
    public void BufferListen(KeyCode key, string ide, string name, float time, bool isdown = true)
    {
        //would be run every frame
        if(isdown? InputManager.IsKeyDown(key, ide) : InputManager.IsKey(key, ide))
        {
            var b = new BufferedInput();
            b.Name = name;
            b.Time = time;
            if (buffer.ContainsKey(name))
            {
                buffer[name] = b;
            }
            else
            {
                buffer.Add(name, b);
            }
        }
    }

    public Dictionary<string, BufferedInput> buffer = new Dictionary<string, BufferedInput>();

    public void Update()
    {
        for (int i = 0; i < buffer.Count; i++)
        {
            var p = buffer.ElementAt(i);
            p.Value.Time -= Time.deltaTime;
            if (p.Value.Time <= 0)
            {
                buffer.Remove(p.Key);
                i--;
            }
        }
    }

}

public class BufferedInput
{
    //just a data holding class
    public float Time;
    public string Name;
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InputBuffer : SingleInstance<InputBuffer>
{
    /*
     * how to use:
     *     BufferListen() to run every time you want to check for a new input of a given key
     *     GetBuffer() gets if the button exists in the buffer (aka was pressed by the user)
     *     RemoveBuffer() removes the button from the buffer if it exists
     */
    public bool GetBuffer(string name)
    {
        return buffer.ContainsKey(name);
    }
    public void RemoveBuffer(string name)
    {
        if (buffer.ContainsKey(name)) buffer.Remove(name);
    }

    public void ActiveListen(InputManagerKeyVal key, BetterList<string> ide, string name, float time, bool isdown = true)
    {
        //run every frame
        if (isdown ? InputManager.IsKeyDown(key, ide) : InputManager.IsKey(key, ide))
        {
            var b = new BufferedInput
            {
                Name = name,
                Time = time
            };
            buffer.AddOrUpdate(name, b);
        }
    }

    public void EventListen(string key, BetterList<string> ide, string name, float time, bool isdown = true)
    {
        //run once
        var eevent = InputManager.IsKeyEvent(key, ide);
        eevent.Append(name, () =>
        {
            var b = new BufferedInput
            {
                Name = name,
                Time = time
            };
            buffer.AddOrUpdate(name, b);
        });
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

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
[HideInInspector]
public class OcksNetworkVar : MonoBehaviour
{
    public string Value;
    public string Name = "";
    public bool HasRecievedData = false;
    public event RandomFunctions.JustFuckingRunTheMethods OnValueChanged;
    public event RandomFunctions.JustFuckingRunTheMethods OnInitialDataRecieved;
    public NetworkObject NetOb;
    public int index;
    public OcksNetworkVar(NetworkObject sexy, string data)
    {
        Value = data;
        NetOb = sexy;
        if (ServerGamer.OcksVars2.ContainsKey(sexy))
        {
            index = ServerGamer.OcksVars2[sexy].Count;
            ServerGamer.OcksVars2[sexy].Add(this);
        }
        else
        {
            index = 0;
            ServerGamer.OcksVars2.Add(sexy, new List<OcksNetworkVar>() { this });
        }
        ServerGamer.OcksVars.Add($"{NetOb.NetworkObjectId}_{index}", this);
        if (NetOb.IsOwner)
        {
            HasRecievedData = true;
            //transmit data to all other clients
        }
        else
        {
            //request data pull from host
        }
    }
    public void SetData(string data)
    {
        Value = data;
        if (!HasRecievedData)
        {
            HasRecievedData = true;
            OnInitialDataRecieved?.Invoke();
        }
        else
        {
            HasRecievedData = true;
        }
        OnValueChanged?.Invoke();
    }


}


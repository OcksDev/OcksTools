using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
[HideInInspector]
public class OcksNetworkVar : MonoBehaviour
{
    /*
            heavily W.I.P idea of mine
    */
    public string Value;
    public string Name = "";
    public string NetID = "";
    public bool HasRecievedData = false;
    public event RandomFunctions.JustFuckingRunTheMethods OnValueChanged;
    public event RandomFunctions.JustFuckingRunTheMethods OnInitialDataRecieved;
    public NetworkObject NetOb;
    public int index;
    public OcksNetworkVar(NetworkObject sexy, string data = "")
    {
        Value = data;
        NetOb = sexy;
        Name = Tags.GenerateID();
        if(ONVManager.Instance != null)
        {
            ONVManager.Instance.StartCoroutine(ONVManager.Instance.Gaming(this));
        }
        else
        {
            ONVManager.UndefinedVars.Add(this);
        }
    }
    public void FinishSetup()
    {
        NetID = NetOb.NetworkObjectId.ToString();
        if (NetOb.IsOwner)
        {
            //send data
        }
        else
        {
            //recieve data
        }
    }
    public void SetValue(string data)
    {
        Value = data;
    }
    public void ThisIsNotForYou_SetData(string data)
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


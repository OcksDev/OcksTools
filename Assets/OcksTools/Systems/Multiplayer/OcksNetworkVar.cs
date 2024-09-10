using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[HideInInspector]
public class OcksNetworkVar
{
    /*
            heavily W.I.P idea of mine
    */
    public string Name = "Unassigned";
    public string NetID = "Unassigned";
    public bool HasRecievedData = false;
    public NetworkObject NetOb;
    string initdat = "";
    public OcksNetworkVar(NetworkObject sexy, string name, string data = "")
    {
        initdat = data;
        NetOb = sexy;
        //Name = Tags.GenerateID();
        Name = name;
        if(sexy == null)
        {
            NetID = "Global";
            CreateDataHolder();
            ServerGamer.Instance.RequestOcksVar(NetID, Name);
        }
        else
        {
            if(ONVManager.Instance != null)
            {
                ONVManager.Instance.StartCoroutine(ONVManager.Instance.Gaming(this));
            }
            else
            {
                ONVManager.UndefinedVars.Add(this);
            }
        }
    }
    public void FinishSetup()
    {
        NetID = NetOb.NetworkObjectId.ToString();
        if (NetOb.IsOwner)
        {
            SetValue(initdat);
        }
        else
        {
            ServerGamer.Instance.RequestOcksVar(NetID, Name);
        }
    }
    public void SetValue(string data, bool SendToOthers = true)
    {
        CreateDataHolder();
        if (data == ONVManager.OcksVars[NetID][Name]) return;
        ONVManager.OcksVars[NetID][Name] = data;
        HasRecievedData = true;
        if (SendToOthers)
        {
            ServerGamer.Instance.SendOcksVar(NetID, Name, data);
        }
    }
    public string GetValue()
    {
        CreateDataHolder();
        return ONVManager.OcksVars[NetID][Name];
    }

    public void CreateDataHolder()
    {
        if (!ONVManager.OcksVars.ContainsKey(NetID))
        {
            ONVManager.OcksVars.Add(NetID, new Dictionary<string, string>());
        }
        if (!ONVManager.OcksVars[NetID].ContainsKey(Name))
        {
            ONVManager.OcksVars[NetID].Add(Name, "");
        }
    }

}

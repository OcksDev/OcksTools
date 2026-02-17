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
    private string initdat = "";
    public OXEvent OnInitialDataLoad = new OXEvent();
    public OXEvent OnDataChanged = new OXEvent();
    public OcksNetworkVar(NetworkObject sexy, string name, string data = "")
    {
        initdat = data;
        NetOb = sexy;
        //Name = Tags.GenerateID();
        Name = name;
        if (sexy == null)
        {
            NetID = "Global";
            CreateDataHolder();
            Server.Send().RequestForOcksVar(NetID, Name);
        }
        else
        {
            if (ONVManager.Instance != null)
            {
                ONVManager.Instance.StartCoroutine(ONVManager.Instance.Gaming(this));
            }
            else
            {
                ONVManager.UndefinedVars.Add(this);
            }
        }
    }
    public void _FinishSetup()
    {
        NetID = NetOb.NetworkObjectId.ToString();
        if (NetOb.IsOwner)
        {
            SetValue(initdat);
        }
        else
        {
            Server.Send().RequestForOcksVar(NetID, Name);
        }
    }
    public void SetValue(string data, bool SendToOthers = true)
    {
        CreateDataHolder();
        if (data == ONVManager.OcksVars[NetID][Name].Data) return;
        ONVManager.OcksVars[NetID][Name].Data = data;
        Server.Instance.PassAlongUpdate(ONVManager.OcksVars[NetID][Name]); //?
        if (SendToOthers)
        {
            Server.Send().OcksVar(NetID, Name, data);
        }
    }

    public void _DataWasChangedByServer()
    {
        //Console.Log("Data C");
        if (!HasRecievedData)
            OnInitialDataLoad?.Invoke();
        HasRecievedData = true;
        OnDataChanged?.Invoke();
    }

    public string GetValue()
    {
        CreateDataHolder();
        return ONVManager.OcksVars[NetID][Name].Data;
    }

    public void CreateDataHolder()
    {
        if (!ONVManager.OcksVars.ContainsKey(NetID))
        {
            ONVManager.OcksVars.Add(NetID, new Dictionary<string, OcksNetworkVarData>());
        }
        if (!ONVManager.OcksVars[NetID].ContainsKey(Name))
        {
            ONVManager.OcksVars[NetID].Add(Name, new OcksNetworkVarData());
        }
        var wank = ONVManager.OcksVars[NetID][Name];
        if (!wank.OcksNetworkVars.Contains(this))
        {
            wank.OcksNetworkVars.Add(this);
        }
    }

}

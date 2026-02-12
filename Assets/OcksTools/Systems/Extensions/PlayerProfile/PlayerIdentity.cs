using System;
using System.Text.RegularExpressions;

public class PlayerIdentity : IComparable<PlayerIdentity>, IEquatable<PlayerIdentity>
{

    public string NameRaw;
    public string UUID;
    public string GetCleanedUsername()
    {
        var s = NameRaw;
        s = Regex.Replace(s, "<", "<<i></i>");
        s = s.Replace("\\", "\\\\");
        return s;
    }

    public PlayerIdentity(string n, string u)
    {
        NameRaw = n;
        UUID = u;
    }
    public override string ToString()
    {
        return $"[{NameRaw},{UUID}]";
    }

    public PlayerIdentity FromString(string s)
    {
        s = s.Substring(1, s.Length - 2);
        var x = s.StringToList(",");
        NameRaw = x[0];
        UUID = x[x.Count - 1];
        if (NameRaw.Length > ProfileHandler.MaxUsernameLength) NameRaw = NameRaw.Substring(0, ProfileHandler.MaxUsernameLength);
        return this;
    }


    //boilerplate-ish code below
    public int CompareTo(PlayerIdentity other)
    {
        if (other == null) return -1;
        if (NameRaw.CompareTo(other.NameRaw) != 0) return NameRaw.CompareTo(other.NameRaw);
        return UUID.CompareTo(other.UUID);
    }

    public bool Equals(PlayerIdentity other)
    {
        if (other == null) return false;
        return NameRaw.Equals(other.NameRaw) && UUID.Equals(other.UUID);
    }

    public override bool Equals(object other)
    {
        var nerd = other as PlayerIdentity;

        if (nerd == null)
        {
            return false;
        }

        return Equals(nerd);
    }

    public override int GetHashCode()
    {
        return UUID.GetHashCode();
    }

}

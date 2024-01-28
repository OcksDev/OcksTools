using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityOXS : MonoBehaviour
{
    public float Health = 100f;
    public float Shield = 100f;
    public float Max_Health = 100f;
    public float Max_Shield = 100f;
    public List<string> Effects = new List<string>();
    public List<float> Effect_Times = new List<float>();
    public List<float> Effect_Stacks = new List<float>();
    public void Hit(float damage, string type = "", string effects = "")
    {
        Shield -= damage;
        if (Shield < 0)
        {
            Health += Shield;
        }
    }
    public void Kill()
    {
        Destroy(gameObject);
    }
    private void Update()
    {
        Health = Mathf.Clamp(Health, 0, Max_Health);
        Shield = Mathf.Clamp(Shield, 0, Max_Shield);
        if (Health <= 0)
        {
            Kill();
        }
    }

    public void AddEffect(int type, float time, int add_method)
    {
        //by default none of the effects actually do anything
        //instead, this gives the programmer the ability to make them how they see fit
        //and this method only provides the base data structure for that
        string ef_name = "";
        int max_stack = 0;
        switch (type)
        {
            case 0:
                ef_name = "Burning";
                break;
            case 1:
                ef_name = "Healing Energy";
                max_stack = 6;
                break;
        }
        if(ef_name != "")
        {
            switch (add_method)
            {
                default:
                case 0:
                    // just apply a new one every time
                    goto AddNewStack;
                case 2:
                    // the same as case 1, except it doesn't refresh the cooldown
                case 1:
                    // applies a new stack if none exist, otherwise increments stack until max is reached while refreshing cooldown
                    if (Effects.Contains(ef_name))
                    {
                        int x = Effects.IndexOf(ef_name);
                        if(add_method == 1)Effect_Times[x] = time;
                        if(max_stack <= 0 || Effect_Stacks[x] < max_stack) Effect_Stacks[x] += 1;
                    }
                    else
                    {
                        goto AddNewStack;
                    }
                    break;
                AddNewStack:
                    Effects.Add(ef_name);
                    Effect_Times.Add(time);
                    Effect_Stacks.Add(1);
                    break;
            }
        }
    }


}

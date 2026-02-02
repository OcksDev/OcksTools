using System.Collections;
using UnityEngine;

public class MenuTester : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        MenuHandler.MenuMethods.Append(AssignTestStuff);
    }

    public void AssignTestStuff()
    {
        MenuHandler.CurrentMenuStates["m1"].OnOpen.Append(() => Debug.Log("m1_open"));
        MenuHandler.CurrentMenuStates["m1"].OnClose.Append(() => Debug.Log("m1_close"));

        MenuHandler.CurrentMenuStates["m3"].OpeningAnimation = OXAnimationSet.FromBasic(OXDefaultAnimations.WobbleInEven);
        MenuHandler.CurrentMenuStates["m3"].ClosingAnimation = OXAnimationSet.FromBasic(OXDefaultAnimations.TVOut);


        MenuHandler.CurrentMenuStates["m2"].OpeningAnimation = OXAnimationSet.FromBasic(OXDefaultAnimations.WobbleInVH);
        MenuHandler.CurrentMenuStates["m2"].ClosingAnimation = OXAnimationSet.FromBasic(OXDefaultAnimations.EaseOutVH);

        MenuHandler.CurrentMenuStates["m1"].OpeningAnimation = OXAnimationSet.FromBasic(OXDefaultAnimations.SpinInLeft);
        MenuHandler.CurrentMenuStates["m1"].ClosingAnimation = OXAnimationSet.FromBasic(OXDefaultAnimations.SpinOutRight);
    }

    public IEnumerator m3open(MenuState cum)
    {
        var d = cum.Objects[0].transform;
        for (int i = 0; i < 25; i++)
        {
            d.localScale = Vector3.one * ((float)i) / 25;
            yield return new WaitForFixedUpdate();
        }
        d.localScale = Vector3.one;
    }
    public IEnumerator m3close(MenuState cum)
    {
        var d = cum.Objects[0].transform;
        for (int i = 0; i < 25; i++)
        {
            d.localScale = Vector3.one * ((float)(25 - i)) / 25;
            yield return new WaitForFixedUpdate();
        }
        d.localScale = Vector3.zero;
    }

}

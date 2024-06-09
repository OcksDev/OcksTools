using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[RequireComponent(typeof(Animator))]
public class OXAnimator : MonoBehaviour
{
    Animator animator;
    OXAnimRuleset curanim;
    OXAnimRuleset nextanim;
    public UnityAnimationEvent OnAnimationStart;
    public UnityAnimationEvent OnAnimationComplete;
    public void Start()
    {
        animator = GetComponent<Animator>();
        for (int i = 0; i < animator.runtimeAnimatorController.animationClips.Length; i++)
        {
            AnimationClip clip = animator.runtimeAnimatorController.animationClips[i];

            AnimationEvent animationStartEvent = new AnimationEvent();
            animationStartEvent.time = 0;
            animationStartEvent.functionName = "AnimationStartHandler";
            animationStartEvent.stringParameter = clip.name;

            AnimationEvent animationEndEvent = new AnimationEvent();
            animationEndEvent.time = clip.length;
            animationEndEvent.functionName = "AnimationCompleteHandler";
            animationEndEvent.stringParameter = clip.name;

            clip.AddEvent(animationStartEvent);
            clip.AddEvent(animationEndEvent);
        }
    }



    public void AnimationStartHandler(string name)
    {
        OnAnimationStart?.Invoke(name);
    }
    public void AnimationCompleteHandler(string name)
    {
        OnAnimationComplete?.Invoke(name);
    }
    public void PlayAnim(OXAnimRuleset pp = null)
    {
        if (curanim.name == pp.name) return;
        if (pp.priority < curanim.priority) return;
        if (pp.priority > curanim.priority) goto fard;
        if (!curanim.canbeoverwritten) return;
        fard:
        if(pp.crosstime > 0)
        {
            animator.CrossFade(pp.name, pp.crosstime);
        }
        else
        {
            animator.Play(pp.name);
        }
        curanim = pp;
        nextanim = pp.PlayNextAnim;
    }

}

public class UnityAnimationEvent : UnityEvent<string> { };
public class OXAnimRuleset
{
    //required parameters
    public string name;
    public float crosstime;
    public OXAnimRuleset(string name, float crosstime)
    {
        this.name = name;
        this.crosstime = crosstime;
    }
    //optional parameters
    public int priority = 0;
    public bool canbeoverwritten = false;
    public OXAnimRuleset PlayNextAnim;
}
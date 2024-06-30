using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentAnimation : MonoBehaviour
{
    private AnimationClip[] myClips;
    private Animator animator;
    public bool runAutoAnimation = true;
    // Start is called before the first frame update
    void Start()
    {
        if (runAutoAnimation)
        {
            animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play("locom_f_basicWalk_30f");


            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}

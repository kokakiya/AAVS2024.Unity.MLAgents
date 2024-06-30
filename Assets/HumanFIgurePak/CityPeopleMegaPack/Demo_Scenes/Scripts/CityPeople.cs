using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CityPeople
{
    public class CityPeople : MonoBehaviour
    {
        private AnimationClip[] myClips;
        private Animator animator;
        public bool runAutoAnimation = true;

        void Start()
        {
            if (runAutoAnimation){
            animator = GetComponent<Animator>();
            if (animator != null)
            {
                myClips = animator.runtimeAnimatorController.animationClips;
                PlayAnyClip();
                StartCoroutine(ShuffleClips());
            }
            }

        }

        void PlayAnyClip()
        {
            if (runAutoAnimation){
            var cl = myClips[Random.Range(0, myClips.Length)];
            animator.CrossFadeInFixedTime(cl.name, 1.0f, -1, Random.value * cl.length);
            }
        }

        IEnumerator ShuffleClips()
        {
            if (runAutoAnimation){
            while (true)
            {
                yield return new WaitForSeconds(15.0f + Random.value * 5.0f);
                PlayAnyClip();
            }
            }
        }

    }
}

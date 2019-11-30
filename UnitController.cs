using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fire_Emblem_Engine
{
    public class UnitController : MonoBehaviour
    {
        public enum Jobs { None, Warrior, Mage };
        public Jobs job = Jobs.None;
        public Animator an;

        // Use this for initialization
        void Start()
        {
            float randomIdleStart = Random.Range(0, an.GetCurrentAnimatorStateInfo(0).length); //Set a random part of the animation to start from
            an.Play("Idle", 0, randomIdleStart);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

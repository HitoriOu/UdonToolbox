
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolboxV2
{
    public class Trashbin : UdonSharpBehaviour
    {
        [Tooltip("Disables object instead of de-spawning")]
        public bool Pool_System = false;

        [Header("Only object childed by these objects gets affected")]
        public GameObject[] Designated_Parents;

        [Header("Events")]
        public bool Event_OnCollisionEnter = false;
        public bool Event_OnCollisionExit = false;
        public bool Event_OnTriggerEnter = false;
        public bool Event_OnTriggerExit = false;

        void OnCollisionEnter(Collision other) { if (Event_OnCollisionEnter) { check_parent(other.gameObject); } }
        void OnCollisionExit(Collision other) { if (Event_OnCollisionExit) { check_parent(other.gameObject); } }
        void OnTriggerEnter(Collider other) { if (Event_OnTriggerEnter) { check_parent(other.gameObject); } }
        void OnTriggerExit(Collider other) { if (Event_OnTriggerExit) { check_parent(other.gameObject); } }

        public void check_parent(GameObject target)
        {
            for (uint i = 0; i < Designated_Parents.Length; i++)
            {
                if (target.transform.parent == Designated_Parents[i].transform)
                {
                    Execute(target);
                }
            }
        }

        private void Execute(GameObject target)
        {
            if (Pool_System)
            {
                target.SetActive(false);
            }
            else
            {
                Destroy(target);
            }
        }
    }
}

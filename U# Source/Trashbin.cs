
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolboxV2
{
    /// <summary>
    /// Trashbin
    /// Used to delete/disable objects that been spawned/enabled by other scripts like spawning/pool systems.
    /// Created by Hitori Ou
    /// Last edit: 26-11-2020 Version 2.4
    /// </summary>
    public class Trashbin : UdonSharpBehaviour
    {
        #region PublicVariables
        [Tooltip("Select for disabling objects instead of de-spawning them.")]
        public bool Pool_System = false;

        [Header("Only children to these objects gets affected")]
        [Tooltip("The script will only work on the hierarchy placed children of these selected objects (mandatory).")]
        public GameObject[] Designated_Parents;

        [Header("Events")]
        public bool Event_OnCollisionEnter = false;
        public bool Event_OnCollisionExit = false;
        public bool Event_OnTriggerEnter = false;
        public bool Event_OnTriggerExit = false;
        #endregion

        #region Events
        void OnCollisionEnter(Collision other) { if (Event_OnCollisionEnter) { CheckParent(other.gameObject); } }
        void OnCollisionExit(Collision other) { if (Event_OnCollisionExit) { CheckParent(other.gameObject); } }
        void OnTriggerEnter(Collider other) { if (Event_OnTriggerEnter) { CheckParent(other.gameObject); } }
        void OnTriggerExit(Collider other) { if (Event_OnTriggerExit) { CheckParent(other.gameObject); } }

        void Start()
        {
            if (Networking.LocalPlayer == null)
            {
                if(Designated_Parents == null || Designated_Parents.Length == 0 || Designated_Parents[0] == null)
                {
                    Debug.LogError("Trashbin has no parents designated (Element 0: is mandatory).");
                }
            }
        }
        #endregion

        #region PrivateFunctions
        private void CheckParent(GameObject target)
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
        #endregion
    }
}

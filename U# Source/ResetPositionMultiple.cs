
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolboxV2
{
    public class ResetPositionMultiple : UdonSharpBehaviour
    {
        private VRCPlayerApi[] old_Owner;
        public GameObject[] Reset_This;
        public GameObject[] Place_Here;

        [Header("Synching")]
        [Tooltip("All players in world are affected.")]
        public bool Global_Synched = true;
        //public bool Late_Join_Synched = true;

        [Header("Events")]
        public bool EventInteract = true;
        public bool Event_OnCollisionEnter = false;
        public bool Event_OnCollisionExit = false;
        public bool Event_OnTriggerEnter = false;
        public bool Event_OnTriggerExit = false;

        public override void Interact() { if (EventInteract) { SendCustomEvent("Run"); } }
        void OnCollisionEnter(Collision other) { if (Event_OnCollisionEnter) { SendCustomEvent("Run"); } }
        void OnCollisionExit(Collision other) { if (Event_OnCollisionExit) { SendCustomEvent("Run"); } }
        void OnTriggerEnter(Collider other) { if (Event_OnTriggerEnter) { SendCustomEvent("Run"); } }
        void OnTriggerExit(Collider other) { if (Event_OnTriggerExit) { SendCustomEvent("Run"); } }

        public override void OnPlayerCollisionEnter(VRCPlayerApi player) { if (Event_OnCollisionEnter && player.isLocal) { SendCustomEvent("Run"); } }
        public override void OnPlayerCollisionExit(VRCPlayerApi player) { if (Event_OnCollisionExit && player.isLocal) { SendCustomEvent("Run"); } }
        public override void OnPlayerTriggerEnter(VRCPlayerApi player) { if (Event_OnTriggerEnter && player.isLocal) { SendCustomEvent("Run"); } }
        public override void OnPlayerTriggerExit(VRCPlayerApi player) { if (Event_OnTriggerExit && player.isLocal) { SendCustomEvent("Run"); } }

        void Start()
        {
            if (Networking.LocalPlayer == null)
            { Global_Synched = false; }

            if (Reset_This != null)
            { old_Owner = new VRCPlayerApi[Reset_This.Length]; }
        }

        public void Run()
        {
            if (Global_Synched)
            { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Reset"); }
            else
            { SendCustomEvent("Reset"); }

        }

        public void Reset()
        {
            if (Reset_This.Length == Place_Here.Length)
            {
                for (uint i = 0; i < Reset_This.Length; i++)
                { Reset_This[i].transform.SetPositionAndRotation(Place_Here[i].transform.position, Place_Here[i].transform.rotation); }
            }
        }
    }
}

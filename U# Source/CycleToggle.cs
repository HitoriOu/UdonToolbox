
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolboxV2
{
    public class CycleToggle : UdonSharpBehaviour
    {
        [UdonSynced(UdonSyncMode.None)] int synch_mem = -1;
        int index_mem = 0;

        [Tooltip("List of objects to toggle")]
        public GameObject[] Targets = new GameObject[1];
        [Tooltip("Current focus target to toggle from list")]
        public int Index = 0;
        [Tooltip("Active status of current focus target (others are set opposite)")]
        public bool Index_ON = true;

        [Header("Synching")]
        [Tooltip("All players in world are affected.")]
        public bool Global_Synched = true;
        [Tooltip("Players who join will see what others see. \r\n(If set to Global_Synched)")]
        public bool Late_Join_Synched = true;

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

        public override void OnPlayerTriggerEnter(VRCPlayerApi player) { if (Event_OnTriggerEnter && player.isLocal) { SendCustomEvent("Run"); } }
        public override void OnPlayerTriggerExit(VRCPlayerApi player) { if (Event_OnTriggerExit && player.isLocal) { SendCustomEvent("Run"); } }
        public override void OnPlayerCollisionEnter(VRCPlayerApi player) { if (Event_OnCollisionEnter && player.isLocal) { SendCustomEvent("Run"); } }
        public override void OnPlayerCollisionExit(VRCPlayerApi player) { if (Event_OnCollisionExit && player.isLocal) { SendCustomEvent("Run"); } }

        void Start()
        {
            if (Networking.LocalPlayer == null)
            { Global_Synched = false; }
            if (synch_mem != -1 && Targets.Length != 0)/*"Global_Synched&&Late_Join_Synched" explicit checked by "synch_mem!=-1"*/
            {
                if (synch_mem == 0)
                { index_mem = Targets.Length - 1; }
                else
                { index_mem = synch_mem; }
                SendCustomEvent("Cycle");
            }
            else
            { index_mem = Index; }
        }

        public void Run()
        {
            if (Targets.Length != 0)
            {
                if (Global_Synched)
                { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Cycle"); }
                else
                { SendCustomEvent("Cycle"); }
            }
        }

        public void Cycle()
        {
            for (int i = 0; i < Targets.Length; i++)
            {
                if (Targets[i] != null)
                {
                    if (i == index_mem)
                    { Targets[i].SetActive(Index_ON); }
                    else
                    { Targets[i].SetActive(!Index_ON); }
                }
            }
            SendCustomEvent("Set_next");
        }

        public void Set_next()
        {
            if (index_mem + 1 >= Targets.Length)
            { index_mem = 0; }
            else
            { index_mem = index_mem++; }

            if (Global_Synched && Late_Join_Synched)
            { synch_mem = index_mem; }
        }
    }
}

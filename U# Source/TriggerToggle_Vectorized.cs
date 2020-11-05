
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolboxV2
{
    public class TriggerToggle_Vectorized : UdonSharpBehaviour
    {
        /*
         Dev Notes:
         The impacting player/object position relative to the vector vs the script objects location when triggered, 
         determines if it's on the vectors side or opposite direction
         */
        [UdonSynced(UdonSyncMode.None)] uint Synch_memmory = 2;

        [Tooltip("Reference object used to measure from the main objects center position for getting the vector calculations done")]
        public GameObject Vector;

        [Space(6)]
        [Tooltip("Set/desired Active state when triggered")]
        public bool On_Vector_Enable = false;
        [Tooltip("What objects to set active state on")]
        public GameObject[] OnVector = new GameObject[0];

        [Space(6)]
        [Tooltip("Set/desired Active state when triggered")]
        public bool Not_On_Vector_Enable = true;
        [Tooltip("What objects to set active state on")]
        public GameObject[] Not_OnVector = new GameObject[0];

        [Header("Synching")]
        [Tooltip("All players in world are affected.")]
        public bool Global_Synched = false;
        [Tooltip("Players who join will see what others see. \r\n(If set to Global_Synched)")]
        public bool Late_Join_Synched = false;

        //public bool Event_Interact = true;
        [Header("Events")]
        [Tooltip("Event trigger when player detected")]
        public bool Detect_Player = true;
        [Tooltip("Event trigger when object collider detected")]
        public bool Detect_Object = true;
        [Space(3)]
        public bool Event_OnCollisionEnter = false;
        public bool Event_OnCollisionExit = false;
        public bool Event_OnTriggerEnter = true;
        public bool Event_OnTriggerExit = true;

        //void Interact() { if (Event_Interact) { SendCustomEvent("Run"); } }
        void OnCollisionEnter(Collision other) { if (Event_OnCollisionEnter) { getVector3Col(other); } }
        void OnCollisionExit(Collision other) { if (Event_OnCollisionExit) { getVector3Col(other); } }
        void OnTriggerEnter(Collider other) { if (Event_OnTriggerEnter) { getVector3Tri(other); } }
        void OnTriggerExit(Collider other) { if (Event_OnTriggerExit) { getVector3Tri(other); } }

        public override void OnPlayerCollisionEnter(VRCPlayerApi player) { if (Event_OnCollisionEnter && player.isLocal) { getPlayer(player); } }
        public override void OnPlayerCollisionExit(VRCPlayerApi player) { if (Event_OnCollisionExit && player.isLocal) { getPlayer(player); } }
        public override void OnPlayerTriggerEnter(VRCPlayerApi player) { if (Event_OnTriggerEnter && player.isLocal) { getPlayer(player); } }
        public override void OnPlayerTriggerExit(VRCPlayerApi player) { if (Event_OnTriggerExit && player.isLocal) { getPlayer(player); } }

        void Start()
        {
            if (Networking.LocalPlayer == null)
            { Global_Synched = false; }
            if (Synch_memmory == 1)//&& Global_Synched && Late_Join_Synched //implicit verified by Synch_memmory
            { SendCustomEvent("Run_OnVector"); }
            else if (Synch_memmory == 0)
            { SendCustomEvent("Run_Not_OnVector"); }
        }

        private void getPlayer(VRCPlayerApi player)
        {
            if (Detect_Player && player.isLocal) { Run_Vector_Check(Networking.LocalPlayer.GetPosition()); }
        }

        private void getVector3Col(Collision other)
        {
            if (other != null && Detect_Object && Networking.LocalPlayer.IsOwner(other.gameObject)) { Run_Vector_Check(other.transform.position); }
        }

        private void getVector3Tri(Collider other)
        {
            if (other != null && Detect_Object && Networking.LocalPlayer.IsOwner(other.gameObject)) { Run_Vector_Check(other.transform.position); }
        }

        private void Run_Vector_Check(Vector3 impact_point)
        {
            // Calculate the opposite position from this to vector.
            Vector3 Not_vector_imaginary = (this.gameObject.transform.position - Vector.transform.position) + this.gameObject.transform.position;

            //Get distance between opposite position and vector towards the impact point.
            float vector_to_impact_point = Vector3.Distance(Vector.transform.position, impact_point);
            float Not_vector_imaginary_to_impact_point = Vector3.Distance(Not_vector_imaginary, impact_point);

            if (Not_vector_imaginary_to_impact_point >= vector_to_impact_point)
            {
                if (OnVector.Length != 0 && OnVector[0] != null)
                {
                    if (Global_Synched)
                    {
                        if (Late_Join_Synched)
                        { Synch_memmory = 1; }
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Run_OnVector");
                    }
                    else
                    {
                        SendCustomEvent("Run_OnVector");
                    }
                }
            }
            else
            {
                if (Not_OnVector.Length != 0 && Not_OnVector[0] != null)
                {
                    if (Global_Synched)
                    {
                        if (Late_Join_Synched)
                        { Synch_memmory = 0; }
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Run_Not_OnVector");
                    }
                    else
                    {
                        SendCustomEvent("Run_Not_OnVector");
                    }
                }
            }
        }

        public void Run_OnVector()
        {
            for (uint i = 0; i < OnVector.Length; i++)
            {
                if (OnVector[i] != null)
                { OnVector[i].SetActive(On_Vector_Enable); }
            }
        }

        public void Run_Not_OnVector()
        {
            for (uint i = 0; i < Not_OnVector.Length; i++)
            {
                if (Not_OnVector[i] != null)
                { Not_OnVector[i].SetActive(Not_On_Vector_Enable); }
            }
        }
    }
}

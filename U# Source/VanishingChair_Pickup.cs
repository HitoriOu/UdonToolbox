
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolboxV2
{
    /// <summary>
    /// VanishingChair_Pickup
    /// Station management script that hides meshes when in use and disables the pickup collider for the local user.
    /// Created by Hitori Ou
    /// Last edit: 26-11-2020 Version 2.4
    /// https://docs.vrchat.com/docs/vrc_station
    /// </summary>
    public class VanishingChair_Pickup : UdonSharpBehaviour
    {
        /*
         Dev Notes:
         Using a Menu is not mandatory
         Code removes the abillity to grab your own seat (infinite loop)
         */
        private bool AutoEject = false;

        [UdonSynced(UdonSyncMode.None)]
        uint synch_mem = 2;
        [Tooltip("Prevents seated user from grabbing selected collider")]
        public Collider Pickup_Collider = null;

        [Space(3)]
        [Tooltip("Events are swapped when entering/exiting")]
        public bool Invert_Invisible = false;
        [Tooltip("Disables Mesh renderer when triggered")]
        public MeshRenderer[] Turn_Invisible = new MeshRenderer[1];
        [Space(3)]
        [Tooltip("This object is enabled for seated player/user.")]
        public GameObject Optional_Menu = null;

        [Header("Synching")]
        [Tooltip("All players in world are affected.")]
        public bool Global_Synched = true;
        [Tooltip("Players who join will see what others see. \r\n(If set to Global_Synched)")]
        public bool Late_Join_Synched = true;

        [Header("Events")]
        public bool EventInteract = true;
        public bool Event_OnTriggerEnter = false;
        public bool Event_OnTriggerExit = false;

        public override void Interact() { if (EventInteract) { SendCustomEvent("EnterStation"); } }
        void OnTriggerEnter(Collider other) { if (Event_OnTriggerEnter) { SendCustomEvent("EnterStation"); } }
        void OnTriggerExit(Collider other) { if (Event_OnTriggerExit) { SendCustomEvent("EnterStation"); } }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player) { if (Event_OnTriggerEnter && player.isLocal) { SendCustomEvent("EnterStation"); } }
        public override void OnPlayerTriggerExit(VRCPlayerApi player) { if (Event_OnTriggerExit && player.isLocal) { SendCustomEvent("EnterStation"); } }

        public override void OnPickup()
        {
            if(Networking.LocalPlayer.isLocal)
            {
                SendCustomEvent("Set_Owner");
            }
        }
        //public override void OnDrop() { SendCustomEvent(""); }
        
        public override void OnStationEntered(VRCPlayerApi player) { SendCustomEvent("Hide"); CheckAutoEject(player); }
        public override void OnStationExited(VRCPlayerApi player) { ExitStation(player); }

        public void EnterStation()
        {
            VRC_Pickup temp = (VRC_Pickup)this.gameObject.GetComponent(typeof(VRC_Pickup));
            if (Pickup_Collider != null && Networking.LocalPlayer.isLocal)
            { Pickup_Collider.enabled = false; }
            if (Networking.LocalPlayer == null || Networking.LocalPlayer.isLocal)
            {
                if (temp != null && temp.IsHeld && (Networking.LocalPlayer == null || temp.currentPlayer == Networking.LocalPlayer)) /*Eliminates grab-seat infinite physics loop*/
                {
                    temp.Drop();
                }
                if(Networking.LocalPlayer != null)
                {
                    Networking.LocalPlayer.UseAttachedStation();
                }
                    Debug.Log("Player got in seat/station.");
                // SendCustomEvent("Hide");

                Collider tempCollider = (Collider)this.gameObject.GetComponent(typeof(Collider));
                if(tempCollider != null)
                {
                    tempCollider.enabled = false;
                }
            }
        }

        // Used to fix non-seated immobilize bug.
        private void CheckAutoEject(VRCPlayerApi player)
        {
            if(player == Networking.LocalPlayer)
            {
                VRCStation tempStation = (VRCStation)this.gameObject.GetComponent(typeof(VRCStation));
                if (tempStation != null && AutoEject)
                {
                    AutoEject = false;
                    tempStation.ExitStation(player);
                    tempStation.seated = false;
                }
            }
        }

        private void ExitStation(VRCPlayerApi player)
        {
            // Used to fix non-seated immobilize bug.
            if (player == Networking.LocalPlayer)
            {
                VRCStation tempStation = (VRCStation)this.gameObject.GetComponent(typeof(VRCStation));
                if (tempStation != null && !tempStation.seated && !AutoEject)
                {
                    tempStation.seated = true;
                    AutoEject = true;

                    player.UseAttachedStation();
                }

                Collider tempCollider = (Collider)this.gameObject.GetComponent(typeof(Collider));
                if (tempCollider != null)
                {
                    tempCollider.enabled = true;
                }
            }

            Debug.Log("Player Exited seat/station.");
            SendCustomEvent("Show");
        }

        public void Set_Owner()
        {
            if(Networking.GetOwner(this.gameObject) != Networking.LocalPlayer)
            {
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            }
        }

        public void Start()
        {
            if (Networking.LocalPlayer == null)
            { Global_Synched = false; }

            if (Optional_Menu != null && this.gameObject == Optional_Menu.gameObject)
            {
                Optional_Menu = null;
                /// double checks to make sure unpredictable bugs do not occur (when you exit chair the entire game-object will be toggeled off instead).
                // Debug.LogWarning("[Udon Toolbox] Note: Optional menu cannot be same as script holder, must be either none/NULL or different gameobject!", this);
            }

            if (synch_mem == 0)
            { SendCustomEvent("OFF"); }
            else if (synch_mem == 1)
            { SendCustomEvent("ON"); }
        }

        public void Hide()
        {
            //if (Pickup_Collider != null && Networking.LocalPlayer.isLocal)
            //{ Pickup_Collider.enabled = false; }
            if (Optional_Menu != null && Optional_Menu != this.gameObject)
            { Optional_Menu.SetActive(true); }
            if (Global_Synched)
            {
                if (Late_Join_Synched) { synch_mem = 0; }
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "OFF");
            }
            else
            { SendCustomEvent("OFF"); }
        }

        public void Show()
        {
            if (Pickup_Collider != null)
            { Pickup_Collider.enabled = true; }
            if (Optional_Menu != null && Optional_Menu != this.gameObject)
            { Optional_Menu.SetActive(false); }
            if (Global_Synched)
            {
                if (Late_Join_Synched) { synch_mem = 1; }
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ON");
            }
            else
            { SendCustomEvent("ON"); }
        }

        public void ON()
        {
            for (int i = 0; i < Turn_Invisible.Length; i++)
            {
                if (Turn_Invisible[i] != null)
                {
                    if (Invert_Invisible) { Turn_Invisible[i].enabled = false; }
                    else { Turn_Invisible[i].enabled = true; }
                }
            }
        }

        public void OFF()
        {
            for (int i = 0; i < Turn_Invisible.Length; i++)
            {
                if (Turn_Invisible[i] != null)
                {
                    if (Invert_Invisible) { Turn_Invisible[i].enabled = true; }
                    else { Turn_Invisible[i].enabled = false; }
                }
            }
        }
    }
}

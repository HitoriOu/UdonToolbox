
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolboxV2
{
    public class AvatarPedestal : UdonSharpBehaviour
    {
        /* Dev Notes:
         * U# Script made by "Hitori Ou" for free use with "VR Chat" using UDON on Unity.
         * 
         * Usable Functions:
         * local_update_pedestals [locally sets pedestal to Avatar Element number indicated by "Avatar_Index"]
         * set_prev [sets previous avatar from list]
         * set_next [sets next avatar from list]
         */

        //Used for error correction
        private int PedestalShift = 0;

        [Tooltip("Avatar pedestals used (same avatar on all)")]
        public VRC.SDK3.Components.VRCAvatarPedestal[] Pedestals;
        [Tooltip("List of avatars ID's to use")]
        public string[] Avatars;

        [Tooltip("What avatar to use from list/array")]
        public int Avatar_Index = 0;
        private int Avatar_Index_synch = -1;

        [Header("Gallery Mode Setup")]
        [Tooltip("Displays multiple avatars instead of distributing one to several pedestals")]
        public bool Use_Cycle_Gallery = false;
        [Tooltip("All events instead of selecting the avatar will use the set_next function/event (only works with ''Use_Cycle_Gallery'' enabled)")]
        public bool Events_Cycle_Gallery = false;

        [Header("Synching")]
        [Tooltip("All players in world are affected.")]
        public bool Global_Synched = true;
        [Tooltip("Players who join will see what others see. \r\n(If set to Global_Synched)")]
        public bool Late_Join_Synched = true;
        //[Tooltip("Networked function calls are only made to object owner.")]
        //public bool Owner_Only = false;

        #region Events
        [Header("Events")]
        [Tooltip("Event trigger when player detected")]
        public bool Detect_Player = true;
        [Tooltip("Event trigger when object collider detected")]
        public bool Detect_Object = true;
        [Space(3)]
        public bool Event_Interact = true;
        public bool Event_OnCollisionEnter = false;
        public bool Event_OnCollisionExit = false;
        public bool Event_OnTriggerEnter = false;
        public bool Event_OnTriggerExit = false;

        public override void Interact() { if (Event_Interact) { SendCustomEvent("Run"); } }
        void OnCollisionEnter(Collision other) { if (other != null && Event_OnCollisionEnter) { SendCustomEvent("Run"); } }
        void OnCollisionExit(Collision other) { if (other != null && Event_OnCollisionExit) { SendCustomEvent("Run"); } }
        void OnTriggerEnter(Collider other) { if (other != null && Event_OnTriggerEnter) { SendCustomEvent("Run"); } }
        void OnTriggerExit(Collider other) { if (other != null && Event_OnTriggerExit) { SendCustomEvent("Run"); } }

        public override void OnPlayerCollisionEnter(VRCPlayerApi player) { if (Detect_Player && Event_OnCollisionEnter && player.isLocal) { SendCustomEvent("Run"); } }
        public override void OnPlayerCollisionExit(VRCPlayerApi player) { if (Detect_Player && Event_OnCollisionExit && player.isLocal) { SendCustomEvent("Run"); } }
        public override void OnPlayerTriggerEnter(VRCPlayerApi player) { if (Detect_Player && Event_OnTriggerEnter && player.isLocal) { SendCustomEvent("Run"); } }
        public override void OnPlayerTriggerExit(VRCPlayerApi player) { if (Detect_Player && Event_OnTriggerExit && player.isLocal) { SendCustomEvent("Run"); } }

        public bool Event_OnPickup = false;
        public bool Event_OnDrop = false;
        public override void OnPickup() { if (Event_OnPickup) { SendCustomEvent("Run"); } }
        public override void OnDrop() { if (Event_OnDrop) { SendCustomEvent("Run"); } }

        public bool Event_OnPickupUseDown = false;
        public bool Event_OnPickupUseUp = false;
        public override void OnPickupUseDown() { if (Event_OnPickupUseDown) { SendCustomEvent("Run"); } }
        public override void OnPickupUseUp() { if (Event_OnPickupUseUp) { SendCustomEvent("Run"); } }
        #endregion

        #region Run
        public void Run()
        {
            if (Networking.LocalPlayer == null)
            {
                if (Use_Cycle_Gallery && Events_Cycle_Gallery)
                {
                    this.set_next();
                    Debug.Log("Udon Toolbox: Avatar Gallery ''set_next'' function triggered.", this);
                }
                else
                {
                    // testing code
                    local_update_pedestals();
                    for (uint i = 0; i < Avatars.Length; i++)
                    {
                        if (Avatars[i] == Pedestals[PedestalShift].blueprintId)
                        {
                            Debug.Log("Udon Toolbox: Player changed avatar, used avatar in Element:" + i.ToString() + " Intended index was:" + ResolveShift(Avatar_Index, Avatars.Length, PedestalShift), this);
                        }
                    }
                }
            }
            else if (Use_Cycle_Gallery && Events_Cycle_Gallery)
            {
                this.set_next();
            }
            else
            {
                Pedestals[ResolveShift(Avatar_Index, Avatars.Length, PedestalShift)].SetAvatarUse(Networking.LocalPlayer);
            }
        }
        #endregion

        #region ShiftFunctions
        /// <summary>
        /// Calculates actuall value based on shift value.
        /// </summary>
        /// <param name="index">Current array index.</param>
        /// <param name="size">Current Array size.</param>
        /// <param name="shift">Value to shift index by/with.</param>
        /// <returns>Shifted index.</returns>
        private int ResolveShift(int index, int size, int shift)
        {
            if ((index + shift) >= size)
            {
                return index + shift - size;
            }
            else if ((index + shift) < 0)
            {
                return size + (index + shift);
            }
            else
            {
                return index + shift;
            }
        }

        /// <summary>
        /// Used to determine if and by how much the interactable podium is shifted by.
        /// </summary>
        /// <returns>Shifted amount.</returns>
        private int DeterminePedestalShift()
        {
            for (int i = 0; i < Pedestals.Length; i++)
            {
                if (Pedestals[i].gameObject == this.gameObject)
                {
                    return i;
                }
            }
            return 0;
        }
        #endregion

        public void local_update_pedestals()
        {
            set_avatar(Avatar_Index);
        }

        #region SetAvatar
        private void set_avatar(int number)
        {
            if (number >= Avatars.Length || number < 0)
            { number = 0; }
            Avatar_Index = number;
            if (Global_Synched && Late_Join_Synched)
            {
                Avatar_Index_synch = Avatar_Index;
            }

            if (Use_Cycle_Gallery)
            {
                int temp = number;
                for (uint i = 0; i < Pedestals.Length; i++)
                {
                    if (Pedestals != null && Pedestals[i] != null)
                    {
                        Pedestals[i].blueprintId = Avatars[temp];
                    }

                    if (temp + 1 >= Avatars.Length)
                    { temp = 0; }
                    else
                    { temp++; }
                }
            }
            else
            {
                for (uint i = 0; i < Pedestals.Length; i++)
                {
                    if (Pedestals != null && Pedestals[i] != null)
                    {
                        Pedestals[i].blueprintId = Avatars[number];
                    }
                }
            }
        }
        #endregion

        #region Previous
        public void set_prev()
        {
            if (Global_Synched)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "set_prev_net");
            }
            else
            {
                SendCustomEvent("set_prev_net");
            }
        }

        public void set_prev_net()
        {

            if (Avatar_Index - 1 < 0)
            { Avatar_Index = Avatars.Length - 1; }
            else
            { Avatar_Index--; }
            if (Late_Join_Synched)
            {
                Avatar_Index_synch = Avatar_Index;
            }
            local_update_pedestals();
        }
        #endregion

        #region Next
        public void set_next()
        {
            if (Global_Synched)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "set_next_net");
            }
            else
            {
                SendCustomEvent("set_next_net");
            }
        }

        public void set_next_net()
        {
            if (Avatar_Index + 1 >= Avatars.Length)
            { Avatar_Index = 0; }
            else
            { Avatar_Index++; }
            if (Late_Join_Synched)
            {
                Avatar_Index_synch = Avatar_Index;
            }
            local_update_pedestals();
        }
        #endregion

        #region StartEvent
        void Start()
        {
            if (Pedestals == null || Pedestals.Length == 0)
            {
                Pedestals = new VRC.SDK3.Components.VRCAvatarPedestal[1];
            }
            if (Pedestals[0] == null)
            {
                Pedestals[0] = (VRC.SDK3.Components.VRCAvatarPedestal)this.GetComponent(typeof(VRC.SDK3.Components.VRCAvatarPedestal));
                if (Pedestals[0] != null && Networking.LocalPlayer == null)
                {
                    Debug.Log("No pedestal selected in Element 0 (used from objects component instead)", this);
                }
                else if (Networking.LocalPlayer == null)
                {
                    Debug.LogError("No Avatar Pedestal script found in Variable: Pedestals Element 0 and no ''vrc avatar pedestal'' script found on object", this);
                }
            }

            PedestalShift = DeterminePedestalShift();

            if (Avatars.Length == 0 || Avatars.Length == 1 && (Avatars[0] == null || Avatars[0].Length == 0))
            {
                Avatars = new string[1];
                Avatars[0] = Pedestals[0].blueprintId;
                if (Networking.LocalPlayer == null)
                {
                    Debug.Log("No avatar ID placed in Element 0 (used Blueprint ID of variable: Pedestals Element 0 instead)", this);
                }
            }

            if (Networking.LocalPlayer == null)
            { Global_Synched = false; }

            if (Global_Synched && Late_Join_Synched && Avatar_Index_synch > -1)
            {
                set_avatar(Avatar_Index_synch);
            }
            else
            {
                if (Avatar_Index < 0 || Avatar_Index >= Avatars.Length)
                {
                    set_avatar(0);
                    Avatar_Index = 0;
                }
                else
                {
                    set_avatar(Avatar_Index);
                }
            }
        }
        #endregion
    }
}

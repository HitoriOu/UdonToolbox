
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolboxV2
{
    public class VanishingPickup : UdonSharpBehaviour
    {
        [UdonSynced(UdonSyncMode.None)]
        uint synch_mem = 2;

        public bool Invert_Events = false;
        public MeshRenderer[] Turn_Invisible = new MeshRenderer[1];

        [Header("Synching")]
        [Tooltip("All players in world are affected.")]
        public bool Global_Synched = true;
        [Tooltip("Players who join will see what others see. \r\n(If set to Global_Synched)")]
        public bool Late_Join_Synched = true;

        public override void OnPickup() { SendCustomEvent("Hide"); }
        public override void OnDrop() { SendCustomEvent("Show"); }
        // void Interact() { SendCustomEvent("Hide"); } /*Dev test code*/

        public void Start()
        {
            if (Networking.LocalPlayer == null)
            { Global_Synched = false; }
            if (synch_mem == 0)
            { SendCustomEvent("OFF"); }
            else if (synch_mem == 1)
            { SendCustomEvent("ON"); }
        }

        public void Hide()
        {
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
                    if (Invert_Events) { Turn_Invisible[i].enabled = false; }
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
                    if (Invert_Events) { Turn_Invisible[i].enabled = true; }
                    else { Turn_Invisible[i].enabled = false; }
                }
            }
        }
    }
}

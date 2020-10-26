
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolboxV2
{
    public class DespawnTimer : UdonSharpBehaviour
    {
        float Time_mem = 0;
        bool started = false;
        bool paused = false;


        [Tooltip("Disables object instead of de-spawning")]
        public bool Pool_System = false;
        [Tooltip("Time untill despawned")]
        public float Countdown = 60;
        [Tooltip("Countdown is never resetted (only paused)")]
        public bool No_Countdown_Reset = false;

        [Header("Synching")]
        [Tooltip("All players in world are affected.")]
        public bool Global_Synched = true;

        [Header("Events")]
        public bool Pause_on_OnPickup = true;
        public override void OnPickup() { if (Pause_on_OnPickup) { SendCustomEvent("Pause"); } }
        public override void OnDrop() { if (Pause_on_OnPickup) { SendCustomEvent("Reset"); } }

        public bool Pause_on_OnPickupUseDown = false;
        public override void OnPickupUseDown() { if (Pause_on_OnPickupUseDown) { SendCustomEvent("Pause"); } }
        public override void OnPickupUseUp() { if (Pause_on_OnPickupUseDown) { SendCustomEvent("Reset"); } }

        public bool Reset_on_Interact = false;
        public override void Interact() { if (Reset_on_Interact) { SendCustomEvent("Reset"); } }

        public bool Pause_on_CollisionEnter = false;
        void OnCollisionEnter(Collision other) { if (Pause_on_CollisionEnter) { SendCustomEvent("Pause"); } }
        void OnCollisionExit(Collision other) { if (Pause_on_CollisionEnter) { SendCustomEvent("Reset"); } }
        public override void OnPlayerCollisionEnter(VRCPlayerApi player) { if (Pause_on_CollisionEnter && player.isLocal) { SendCustomEvent("Pause"); } }
        public override void OnPlayerCollisionExit(VRCPlayerApi player) { if (Pause_on_CollisionEnter && player.isLocal) { SendCustomEvent("Reset"); } }

        public bool Pause_on_TriggerEnter = false;
        void OnTriggerEnter(Collider other) { if (Pause_on_TriggerEnter) { SendCustomEvent("Pause"); } }
        void OnTriggerExit(Collider other) { if (Pause_on_TriggerEnter) { SendCustomEvent("Reset"); } }
        public override void OnPlayerTriggerEnter(VRCPlayerApi player) { if (Pause_on_TriggerEnter && player.isLocal) { SendCustomEvent("Pause"); } }
        public override void OnPlayerTriggerExit(VRCPlayerApi player) { if (Pause_on_TriggerEnter && player.isLocal) { SendCustomEvent("Reset"); } }

        public void Start()
        {
            if (Networking.LocalPlayer == null)
            { Global_Synched = false; }
        }

        public void LateUpdate()
        {

            if (started)
            {

                if (!paused && Time.time > Time_mem)
                {
                    if (Pool_System)
                    {
                        this.gameObject.SetActive(false);
                        started = false;
                    }
                    else
                    {
                        Destroy(this.gameObject);
                    }
                }
            }
            else
            {
                started = true;
                paused = false;
                Time_mem = Time.time + Countdown;
            }
        }

        public void Reset()
        {
            if (Global_Synched)
            { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Reset_Timer"); }
            else
            { SendCustomEvent("Reset_Timer"); }
        }

        public void Reset_Timer()
        {
            Time_mem = Time.time + Countdown;
            paused = false;
        }

        public void Pause()
        {
            if (Global_Synched)
            { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Pause_Timer"); }
            else
            { SendCustomEvent("Pause_Timer"); }
        }

        public void Pause_Timer()
        { paused = true; }
    }
}

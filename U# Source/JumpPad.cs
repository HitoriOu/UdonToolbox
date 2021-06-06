
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
namespace UdonToolboxV2
{

    /// <summary>
    /// JumpPad 
    /// Used to alter objects and players velocity, (adds/sets velocity to any player/object that triggers it).
    /// Vector and VelocityOffset are accumilative on each other with FixedVelocity being a option on top of that for adding the original velocity when set FixedVelocity false.
    /// Created by Hitori Ou
    /// Last edit: 21-12-2020 Version 2.4
    /// </summary>
    public class JumpPad : UdonSharpBehaviour
    {
        [Tooltip("Use vector's Global or Local space values (local is recomended for simplisity).")]
        public bool UseVectorLocal = true;
        [Tooltip("Adds vector data on top of 'VelocityOffset'.")]
        public Transform Vector;

        [Tooltip("Sets Velocity in Global space.")]
        public Vector3 VelocityOffset= new Vector3(0,0,0);
        [Tooltip("The set VelocityOffset + Vector are absolute (uncheck to also use player/objects original/current velocity).")]
        public bool FixedVelocity = true;

        [Header("Synching")]
        [Tooltip("All players in world are affected.")]
        public bool Global_Synched = true;

        #region Events
        [Header("Events")]
        [Tooltip("All players in world are affected.")]
        public bool DetectPlayer = true;
        public bool DetectObject = true;
        [Space(3)]
        public bool EventInteract = true;
        [Space(3)]
        public bool EventOnPickup = false;
        public bool EventOnDrop = false;
        [Space(3)]
        public bool EventOnPickupUseDown = false;
        public bool EventOnPickupUseUp = false;
        [Space(3)]
        public bool EventOnCollisionEnter = false;
        public bool EventOnCollisionExit = false;
        [Space(3)]
        public bool EventOnTriggerEnter = true;
        public bool EventOnTriggerExit = false;
        public bool EventOnTriggerStay = false;

        public override void Interact() { if (EventInteract) { PlayerJump(Networking.LocalPlayer); } }
        void OnCollisionEnter(Collision other) { if (EventOnCollisionEnter && DetectObject) { ObjectJump(other.collider); } }
        void OnCollisionExit(Collision other) { if (EventOnCollisionExit && DetectObject) { ObjectJump(other.collider); } }
        void OnTriggerEnter(Collider other) { if (EventOnTriggerEnter && DetectObject) { ObjectJump(other); } }
        void OnTriggerExit(Collider other) { if (EventOnTriggerExit && DetectObject) { ObjectJump(other); } }
        void OnTriggerStay(Collider other) { if (EventOnTriggerStay && DetectObject) { ObjectJump(other); } }

        public override void OnPickup() { if (EventOnPickup) { PlayerJump(Networking.LocalPlayer); } }
        public override void OnDrop() { if (EventOnDrop) { PlayerJump(Networking.LocalPlayer); } }

        public override void OnPickupUseDown() { if (EventOnPickupUseDown) { PlayerJump(Networking.LocalPlayer); } }
        public override void OnPickupUseUp() { if (EventOnPickupUseUp) { PlayerJump(Networking.LocalPlayer); } }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player) { if (EventOnTriggerEnter && DetectPlayer) { PlayerJump(player); } }
        public override void OnPlayerTriggerExit(VRCPlayerApi player) { if (EventOnTriggerExit && DetectPlayer) { PlayerJump(player); } }
        public override void OnPlayerTriggerStay(VRCPlayerApi player) { if (EventOnTriggerStay && DetectPlayer) { PlayerJump(player); } }
        public override void OnPlayerCollisionEnter(VRCPlayerApi player) { if (EventOnCollisionEnter && DetectPlayer) { PlayerJump(player); } }
        public override void OnPlayerCollisionExit(VRCPlayerApi player) { if (EventOnCollisionExit && DetectPlayer) { PlayerJump(player); } }
        //public override void OnPlayerCollisionStay(VRCPlayerApi player) { if (Event_OnTriggerStay && DetectPlayer) { PlayerJump(player); } }
        //public override void OnPlayerParticleCollision(VRCPlayerApi player) { PlayerJump(player);  }
        #endregion

        private void PlayerJump(VRCPlayerApi player)
        {
            if(Networking.LocalPlayer == player || Global_Synched && Networking.LocalPlayer.isLocal)
            {
                // Set default Velocity change.
                Vector3 tempVector = VelocityOffset;

                // Add on vector to velocity.
                if (Vector != null)
                {
                    if(UseVectorLocal)
                    {
                        tempVector += Vector.localPosition;
                    }
                    else
                    {
                        tempVector += Vector.position;
                    }
                }

                if(FixedVelocity)
                {
                    Networking.LocalPlayer.SetVelocity(tempVector);
                }
                // Add on current velocity to vector velocity.
                else
                {
                    tempVector += Networking.LocalPlayer.GetVelocity();
                    Networking.LocalPlayer.SetVelocity(tempVector);
                }
            }
        }

        private void ObjectJump(Collider other)
        {
            if (Networking.IsOwner(Networking.LocalPlayer, other.gameObject))
            {
                // Set default Velocity change.
                Vector3 tempVector = VelocityOffset;

                // Add on vector to velocity.
                if (Vector != null)
                {
                    if (UseVectorLocal)
                    {
                        tempVector += Vector.localPosition;
                    }
                    else
                    {
                        tempVector += Vector.position;
                    }
                }

                if (FixedVelocity)
                {
                    other.attachedRigidbody.velocity = tempVector;
                }
                // Add on current velocity to vector velocity.
                else
                {
                    tempVector += other.attachedRigidbody.velocity;
                    other.attachedRigidbody.velocity = tempVector;
                }
            }
        }

        void Start()
        {
            if(!DetectPlayer)
            {
                Global_Synched = false;
                Debug.Log("DetectPlayer not selected, Global_Synched will be turned off.",this.gameObject);
            }
        }
    }
}

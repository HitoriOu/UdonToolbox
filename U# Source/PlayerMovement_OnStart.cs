
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolboxV2
{
    public class PlayerMovement_OnStart : UdonSharpBehaviour
    {
        public float WalkSpeed = 2;
        public float RunSpeed = 4;
        public float Jump = 0;
        public float Gravity = 1;

        void Start()
        {
            if (Networking.LocalPlayer != null)
            {
                Networking.LocalPlayer.SetWalkSpeed(WalkSpeed);
                Networking.LocalPlayer.SetRunSpeed(RunSpeed);
                Networking.LocalPlayer.SetJumpImpulse(Jump);
                Networking.LocalPlayer.SetGravityStrength(Gravity);
            }
        }
    }
}

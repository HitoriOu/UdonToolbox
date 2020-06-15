
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MobilityZone : UdonSharpBehaviour
{
    public float Exit_WalkSpeed = 2;
    public float Exit_RunSpeed = 4;
    public float Exit_Jump = 0;
    public float Exit_Gravity = 1;
    [Space(6)]
    public float Enter_WalkSpeed = 5;
    public float Enter_RunSpeed = 10;
    public float Enter_Jump = 10;
    public float Enter_Gravity = 5;

    [Header("Synching")]
    [Tooltip("All players in world are affected.")]
    public bool Global_Synched = false;

    [Header("Events")]
    [Tooltip("Swaps Enter & Exit values")]
    public bool Invert_Events = false;

    public bool Event_OnPickup = false;
    void OnPickup() { if (Event_OnPickup) { SendCustomEvent("Enter"); } }
    void OnDrop() { if (Event_OnPickup) { SendCustomEvent("Exit"); } }

    public bool Event_OnPickupUseDown = false;
    void OnPickupUseDown() { if (Event_OnPickupUseDown) { SendCustomEvent("Enter"); } }
    void OnPickupUseUp() { if (Event_OnPickupUseDown) { SendCustomEvent("Exit"); } }

    public bool Event_CollisionEnter = false;
    void OnCollisionEnter(Collision other) { if (Event_CollisionEnter) { SendCustomEvent("Enter"); } }
    void OnCollisionExit(Collision other) { if (Event_CollisionEnter) { SendCustomEvent("Exit"); } }

    public bool Event_TriggerEnter = true;
    void OnTriggerEnter(Collider other) { if (Event_TriggerEnter) { SendCustomEvent("Enter"); } }
    void OnTriggerExit(Collider other) { if (Event_TriggerEnter) { SendCustomEvent("Exit"); } }

    public void Start()
    {/*this code not actually needed*/
        if (Networking.LocalPlayer == null)
        { Global_Synched = false; }
    }

    public void Enter()
    {
        if (Global_Synched)
        {
            if(Invert_Events)
            { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Player_Exit"); }
            else
            { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Player_Enter"); }
        }
        else
        {
            if (Invert_Events)
            { SendCustomEvent("Player_Exit"); }
            else
            { SendCustomEvent("Player_Enter"); }
        }
    }

    public void Exit()
    {
        if (Global_Synched)
        {
            if (!Invert_Events)
            { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Player_Exit"); }
            else
            { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Player_Enter"); }
        }
        else
        {
            if (!Invert_Events)
            { SendCustomEvent("Player_Exit"); }
            else
            { SendCustomEvent("Player_Enter"); }
        }
    }

    public void Player_Enter()
    {
        if (Networking.LocalPlayer != null)
        {
            Networking.LocalPlayer.SetWalkSpeed(Enter_WalkSpeed);
            Networking.LocalPlayer.SetRunSpeed(Enter_RunSpeed);
            Networking.LocalPlayer.SetJumpImpulse(Enter_Jump);
            Networking.LocalPlayer.SetGravityStrength(Enter_Gravity);
        }
    }

    public void Player_Exit()
    {
        if (Networking.LocalPlayer != null)
        {
            Networking.LocalPlayer.SetWalkSpeed(Exit_WalkSpeed);
            Networking.LocalPlayer.SetRunSpeed(Exit_RunSpeed);
            Networking.LocalPlayer.SetJumpImpulse(Exit_Jump);
            Networking.LocalPlayer.SetGravityStrength(Exit_Gravity);
        }
    }
}

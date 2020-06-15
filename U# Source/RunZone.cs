
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class RunZone : UdonSharpBehaviour
{
    /* Dev Notes:
     * speed throtteling script usefull for long travels
     * work for any volumized collider(trigger) and any size
     * increases the speed modification depending on how far from the middle of the scripts object you are
     */

    bool running = false;
    float distance_mem;
    float Enter_WalkSpeed;
    float Enter_RunSpeed;

    public float Enter_Jump = 0;
    public float Enter_Gravity = 1;

    [Space(6)]
    public float Exit_WalkSpeed = 2;
    public float Exit_RunSpeed = 4;
    public float Exit_Jump = 0;
    public float Exit_Gravity = 1;

    [Header("Zone Modifiers & Safety")]
    [Tooltip("Adjusts the speed modifications top speed")]
    public float Speed_Multiplier = 2;
    [Space(3)]
    [Tooltip("Inverts the modifications to slow down instead of speed up")]
    public bool SlowZone = false;
    [Tooltip("safety value to prevent being frozen at 0 speed")]
    public float mimimum_speed = 0.01f;

    [Header("Events")]
    public bool Event_OnCollision = false;
    void OnCollisionEnter(Collision other) { if (Event_OnCollision) { SendCustomEvent("Enter"); } }
    void OnCollisionExit(Collision other) { if (Event_OnCollision) { SendCustomEvent("Exit"); } }

    public bool Event_OnTrigger = true;
    void OnTriggerEnter(Collider other) { if (Event_OnTrigger) { SendCustomEvent("Enter"); } }
    void OnTriggerExit(Collider other) { if (Event_OnTrigger) { SendCustomEvent("Exit"); } }

    public void Update()
    {
        if(running)
        {
            if (SlowZone)
            {
                float temp_distance = distance_mem - Vector3.Distance(this.gameObject.transform.position, Networking.LocalPlayer.GetPosition());

                if (Enter_WalkSpeed - (temp_distance / Speed_Multiplier) > mimimum_speed)
                 { Networking.LocalPlayer.SetWalkSpeed(Enter_WalkSpeed - (temp_distance / Speed_Multiplier)); }
                else
                { Networking.LocalPlayer.SetWalkSpeed(mimimum_speed); }

                if (Enter_RunSpeed - (temp_distance / Speed_Multiplier) > mimimum_speed)
                 { Networking.LocalPlayer.SetRunSpeed(Enter_RunSpeed - (temp_distance / Speed_Multiplier)); }
                else
                { Networking.LocalPlayer.SetRunSpeed(mimimum_speed); }
            }
            else
            {
                float temp_distance = distance_mem - Vector3.Distance(this.gameObject.transform.position, Networking.LocalPlayer.GetPosition());

                if ((temp_distance * Speed_Multiplier) + Enter_WalkSpeed  > mimimum_speed)
                 { Networking.LocalPlayer.SetWalkSpeed((temp_distance * Speed_Multiplier) + Enter_WalkSpeed); }
                else
                { Networking.LocalPlayer.SetWalkSpeed(mimimum_speed); }

                if ((temp_distance * Speed_Multiplier) + Enter_RunSpeed > mimimum_speed)
                 { Networking.LocalPlayer.SetRunSpeed((temp_distance * Speed_Multiplier) + Enter_RunSpeed); }
                else
                { Networking.LocalPlayer.SetRunSpeed(mimimum_speed); }
            }
        }
    }

    public void Enter()
    {
        Enter_WalkSpeed=Networking.LocalPlayer.GetWalkSpeed();
        Enter_RunSpeed= Networking.LocalPlayer.GetRunSpeed();

        Networking.LocalPlayer.SetJumpImpulse(Enter_Jump);
        Networking.LocalPlayer.SetGravityStrength(Enter_Gravity);

        distance_mem = Vector3.Distance(this.gameObject.transform.position, Networking.LocalPlayer.GetPosition());
        running = true;
    }

    public void Exit()
    {
        running = false;
        Networking.LocalPlayer.SetWalkSpeed(Exit_WalkSpeed);
        Networking.LocalPlayer.SetRunSpeed(Exit_RunSpeed);
        Networking.LocalPlayer.SetJumpImpulse(Exit_Jump);
        Networking.LocalPlayer.SetGravityStrength(Exit_Gravity);
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

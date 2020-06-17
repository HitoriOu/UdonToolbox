
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Teleportplayer : UdonSharpBehaviour
{
    ushort count_mem = 0;
    float time_mem = 0;

    [Tooltip("Add multiple to cycle use one at a time")]
    public Transform[] Teleport_to = new Transform[1];
    [Tooltip("Select to have destination picked at random from Teleport_To list")]
    public bool Random =false;
    [Tooltip("Prevents spamming or infinite looping")]
    public float Cooldown = 1;

    [Header("Events")]
    public bool EventInteract = true;
    //public bool EventOnCollisionEnter = false;
    //public bool EventOnCollisionExit = false;
    public bool EventOnTriggerEnter = false;
    public bool EventOnTriggerExit = false;

    void Interact() { if (EventInteract) { SendCustomEvent("Teleport"); } }
    //void OnCollisionEnter(Collision other) { if (EventOnCollisionEnter) { SendCustomEvent("Teleport"); } }
    //void OnCollisionExit(Collision other) { if (EventOnCollisionExit) { SendCustomEvent("Teleport"); } }
    void OnTriggerEnter(Collider other) { if (EventOnTriggerEnter) { SendCustomEvent("Teleport"); } }
    void OnTriggerExit(Collider other) { if (EventOnTriggerExit) { SendCustomEvent("Teleport"); } }

    public void Teleport()
    {
        if (Teleport_to.Length != 0 && Teleport_to[0]!=null &&time_mem<Time.time)
        {
            time_mem = Time.time + Cooldown;
            if (Random)
            {
                int rand = (int) UnityEngine.Random.Range((int) 0, (int) Teleport_to.Length);/*rand min/max are different compare*/
                Networking.LocalPlayer.TeleportTo(Teleport_to[rand].position, Teleport_to[rand].rotation);
            }
            else
            {
                Networking.LocalPlayer.TeleportTo(Teleport_to[count_mem].position, Teleport_to[count_mem].rotation);
                if (count_mem + 1 >= Teleport_to.Length)
                { count_mem = 0; }
                else
                { count_mem++; }
            }
        }
    }
}
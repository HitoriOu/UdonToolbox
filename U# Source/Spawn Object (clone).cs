
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SpawnObjectclone : UdonSharpBehaviour
{
    float Time_mem = 0;

    public GameObject Spawn_This;
    public GameObject Spawn_Here;
    [Tooltip("Prevents button/event spamming")]
    public float Cooldown = 0;

    [Header("Synching")]
    [Tooltip("Spawns synched object.")]
    public bool Global_Synched = false;

    [Header("Events")]
    public bool EventInteract = true;
    public bool EventOnCollisionEnter = false;
    public bool EventOnCollisionExit = false;
    public bool EventOnTriggerEnter = false;
    public bool EventOnTriggerExit = false;

    void Interact() { if (EventInteract) { SendCustomEvent("Run"); } }
    void OnCollisionEnter(Collision other) { if (EventOnCollisionEnter) { SendCustomEvent("Run"); } }
    void OnCollisionExit(Collision other) { if (EventOnCollisionExit) { SendCustomEvent("Run"); } }
    void OnTriggerEnter(Collider other) { if (EventOnTriggerEnter) { SendCustomEvent("Run"); } }
    void OnTriggerExit(Collider other) { if (EventOnTriggerExit) { SendCustomEvent("Run"); } }

    public void Start()
    {
        if (Networking.LocalPlayer == null)
        { Global_Synched = false; }
    }

    public void Spawn()
    {
        if(Spawn_Here!=null && Spawn_This != null)
        { 
         GameObject temp = VRCInstantiate(Spawn_This);
         temp.transform.position = Spawn_Here.transform.position;
         temp.transform.rotation = Spawn_Here.transform.rotation;
         temp.SetActive(true);
        }
    }

    public void Run()
    {
        if (Time.time > Time_mem)
        {
            Time_mem = Time.time + Cooldown;
            if (Global_Synched)
             { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Spawn"); }
            else
             { SendCustomEvent("Spawn"); }
        }
    }
}

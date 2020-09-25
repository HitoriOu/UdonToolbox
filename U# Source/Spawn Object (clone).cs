
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SpawnObjectclone : UdonSharpBehaviour
{
    float Time_mem = 0;

    [Tooltip("Pool system uses pre-instanced objects instead of cloning new ones.\r\nUses child objects of 'Spawn_This'")]
    public bool use_pool = false;
    private GameObject[] pool;

    public GameObject Spawn_This;
    public GameObject Spawn_Here;
    [Tooltip("Prevents button/event spamming")]
    public float Cooldown = 0;

    [Header("Synching")]
    [Tooltip("Spawns synched object.")]
    public bool Global_Synched = false;

    [Header("Events")]
    public bool EventInteract = true;
    public bool Event_OnCollisionEnter = false;
    public bool Event_OnCollisionExit = false;
    public bool Event_OnTriggerEnter = false;
    public bool Event_OnTriggerExit = false;

    void Interact() { if (EventInteract) { SendCustomEvent("Run"); } }
    void OnCollisionEnter(Collision other) { if (Event_OnCollisionEnter) { SendCustomEvent("Run"); } }
    void OnCollisionExit(Collision other) { if (Event_OnCollisionExit) { SendCustomEvent("Run"); } }
    void OnTriggerEnter(Collider other) { if (Event_OnTriggerEnter) { SendCustomEvent("Run"); } }
    void OnTriggerExit(Collider other) { if (Event_OnTriggerExit) { SendCustomEvent("Run"); } }

    public void OnPlayerCollisionEnter(VRCPlayerApi player) { if (Event_OnCollisionEnter) { SendCustomEvent("Run"); } }
    public void OnPlayerCollisionExit(VRCPlayerApi player) { if (Event_OnCollisionExit) { SendCustomEvent("Run"); } }
    public void OnPlayerTriggerEnter(VRCPlayerApi player) { if (Event_OnTriggerEnter) { SendCustomEvent("Run"); } }
    public void OnPlayerTriggerExit(VRCPlayerApi player) { if (Event_OnTriggerExit) { SendCustomEvent("Run"); } }

    public void Start()
    {
        if (Networking.LocalPlayer == null)
        { Global_Synched = false; }

        if (use_pool)
        {
                if (Spawn_This != null)
                {
                    pool = new GameObject[Spawn_This.transform.childCount];
                    for (int b = 0; b < pool.Length; b++)
                    {
                        pool[b] = Spawn_This.transform.GetChild(b).gameObject;
                    }
                }
        }
    }

    public void Spawn()
    {
        if(Spawn_Here!=null && Spawn_This != null)
        {
            if (use_pool)
            {
                for (int i = 0; i < pool.Length; i++)
                {
                    if (pool[i] != null && !pool[i].activeSelf)
                    {
                        GameObject temp = pool[i];
                        if (Spawn_Here != null)
                        {
                            temp.transform.position = Spawn_Here.transform.position;
                            temp.transform.rotation = Spawn_Here.transform.rotation;
                        }
                        temp.SetActive(true);
                        i = pool.Length;
                    }
                }
            }
            else
            {
                GameObject temp = VRCInstantiate(Spawn_This);
                temp.transform.SetParent(Spawn_This.transform.parent);
                temp.transform.position = Spawn_Here.transform.position;
                temp.transform.rotation = Spawn_Here.transform.rotation;
                temp.SetActive(true);
            }
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

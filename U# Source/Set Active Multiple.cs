
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SetActiveMultipleUSharp : UdonSharpBehaviour
{
    bool memmory = false;

    [Tooltip("Game Objects on this list are toggled on on event")]
    public GameObject[] Set_This_ON;
    [Tooltip("Game Objects on this list are toggled off on event")]
    public GameObject[] Set_This_OFF;

    [Header("Synching")]
    [Tooltip("All players in world are affected.")]
    public bool Global_Synched = false;
    [Tooltip("Players who join will see what others see. \r\n(If set to Global_Synched)")]
    public bool Late_Join_Synched = false;

    [Header("Events")]
    public bool EventInteract = true;
    public bool EventOnCollisionEnter = false;
    public bool EventOnCollisionExit = false;
    public bool EventOnTriggerEnter = false;
    public bool EventOnTriggerExit = false;

    public void ON()
    {
        for(uint i=0;i< Set_This_ON.Length;i++)
        {
            if (Set_This_ON[i] != null)
            { Set_This_ON[i].SetActive(true); }
        }
    }

    public void OFF()
    {
        for (uint i = 0; i < Set_This_OFF.Length; i++)
        {
            if (Set_This_OFF[i] != null)
            { Set_This_OFF[i].SetActive(false); }
        }
    }

    public void Set()
    {
        if(Global_Synched)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ON");
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "OFF");
            if(Late_Join_Synched) { memmory = true; }
        }
        else
        {
           SendCustomEvent("ON"); 
           SendCustomEvent("OFF");
        }
    }

    void Start()
    {
        if (Networking.LocalPlayer == null)
        { Global_Synched = false; }
        if (memmory)
        {
            SendCustomEvent("ON");
            SendCustomEvent("OFF");
        }
    }

    void Interact() { if (EventInteract) { SendCustomEvent("Set"); } }
    void OnCollisionEnter(Collision other) { if(EventOnCollisionEnter){ SendCustomEvent("Set"); } }
    void OnCollisionExit(Collision other) { if (EventOnCollisionExit) { SendCustomEvent("Set"); } }
    void OnTriggerEnter(Collider other) { if (EventOnTriggerEnter) { SendCustomEvent("Set"); } }
    void OnTriggerExit(Collider other) { if (EventOnTriggerExit) { SendCustomEvent("Set"); } }

    /*
    void OnDrop() { SendCustomEvent("Set"); }
    void OnOwnershipTransferred() { SendCustomEvent("Set"); }
    void OnPickup() { SendCustomEvent("Set"); }
    void OnPickupUseDown() { SendCustomEvent("Set"); }
    void OnPickupUseUp() { SendCustomEvent("Set"); }
    void OnPlayerJoined() { SendCustomEvent("Set"); }
    void OnPlayerLeft() { SendCustomEvent("Set"); }
    */
}

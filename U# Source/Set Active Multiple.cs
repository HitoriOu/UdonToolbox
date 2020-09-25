
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
    public bool Event_OnCollisionEnter = false;
    public bool Event_OnCollisionExit = false;
    public bool Event_OnTriggerEnter = false;
    public bool Event_OnTriggerExit = false;

    void Interact() { if (EventInteract) { SendCustomEvent("Set"); } }
    void OnCollisionEnter(Collision other) { if (Event_OnCollisionEnter) { SendCustomEvent("Set"); } }
    void OnCollisionExit(Collision other) { if (Event_OnCollisionExit) { SendCustomEvent("Set"); } }
    void OnTriggerEnter(Collider other) { if (Event_OnTriggerEnter) { SendCustomEvent("Set"); } }
    void OnTriggerExit(Collider other) { if (Event_OnTriggerExit) { SendCustomEvent("Set"); } }

    public void OnPlayerCollisionEnter(VRCPlayerApi player) { if (Event_OnCollisionEnter) { SendCustomEvent("Set"); } }
    public void OnPlayerCollisionExit(VRCPlayerApi player) { if (Event_OnCollisionExit) { SendCustomEvent("Set"); } }
    public void OnPlayerTriggerEnter(VRCPlayerApi player) { if (Event_OnTriggerEnter) { SendCustomEvent("Set"); } }
    public void OnPlayerTriggerExit(VRCPlayerApi player) { if (Event_OnTriggerExit) { SendCustomEvent("Set"); } }
    
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

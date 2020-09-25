
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ToggleMultiple : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None)] uint Synch_memmory = 2;

    public bool ON_is_default=false;
    public GameObject[] Toggle_This = new GameObject[1];

    [Header("Synching")]
    [Tooltip("All players in world are affected (if UTC zone is changed).")]
    public bool Global_Synched = false;
    [Tooltip("Players who join will see what UTC time zone others see. \r\n(If set to Global_Synched)")]
    public bool Late_Join_Synched = false;

    [Header("Events")]
    public bool Event_Interact = true;
    public bool Event_OnCollisionEnter = false;
    public bool Event_OnCollisionExit = false;
    public bool Event_OnTriggerEnter = false;
    public bool Event_OnTriggerExit = false;

    void Interact() { if (Event_Interact) { SendCustomEvent("Run"); } }
    void OnCollisionEnter(Collision other) { if (Event_OnCollisionEnter) { SendCustomEvent("Run"); } }
    void OnCollisionExit(Collision other) { if (Event_OnCollisionExit) { SendCustomEvent("Run"); } }
    void OnTriggerEnter(Collider other) { if (Event_OnTriggerEnter) { SendCustomEvent("Run"); } }
    void OnTriggerExit(Collider other) { if (Event_OnTriggerExit) { SendCustomEvent("Run"); } }

    public void OnPlayerCollisionEnter(VRCPlayerApi player) { if (Event_OnCollisionEnter) { SendCustomEvent("Run"); } }
    public void OnPlayerCollisionExit(VRCPlayerApi player) { if (Event_OnCollisionExit) { SendCustomEvent("Run"); } }
    public void OnPlayerTriggerEnter(VRCPlayerApi player) { if (Event_OnTriggerEnter) { SendCustomEvent("Run"); } }
    public void OnPlayerTriggerExit(VRCPlayerApi player) { if (Event_OnTriggerExit) { SendCustomEvent("Run"); } }

    void Start()
    {
        if (Networking.LocalPlayer == null)
        { Global_Synched = false; }
        if (Synch_memmory == 1)//&& Global_Synched && Late_Join_Synched //implicit verified by Synch_memmory
        { SendCustomEvent("ON"); }
        else if (Synch_memmory == 0)
        { SendCustomEvent("OFF"); }
        else
        {
            if (ON_is_default)
             { SendCustomEvent("ON"); }
            else
             { SendCustomEvent("OFF"); }
        }
    }

    public void Run()
    {
        if (Toggle_This[0] != null)
        {
         if (Global_Synched)
         {
            if (!Toggle_This[0].activeSelf)
            {
             if(Late_Join_Synched)
              { Synch_memmory = 1; }
             SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ON");
            }
            else
            {
             if (Late_Join_Synched)
              { Synch_memmory = 0; }
             SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "OFF");
            }
         }
         else
         {
            if (!Toggle_This[0].activeSelf)
            { SendCustomEvent("ON"); }
            else
            { SendCustomEvent("OFF"); }
         }
        }
    }

    public void ON()
    {
        for (uint i = 0; i < Toggle_This.Length; i++)
        {
            if (Toggle_This[i] != null)
            { Toggle_This[i].SetActive(true); }
        }
    }

    public void OFF()
    {
        for (uint i = 0; i < Toggle_This.Length; i++)
        {
            if (Toggle_This[i] != null)
            { Toggle_This[i].SetActive(false); }
        }
    }
}

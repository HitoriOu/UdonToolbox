
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TriggerToggle : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None)] uint Synch_memmory = 2;

    [Tooltip("Set/desired Active state when triggered")]
    public bool Enter_Enable = false;
    [Tooltip("What objects to set active state on")]
    public GameObject[] This_on_enter = new GameObject[0];

    [Space(6)]
    [Tooltip("Set/desired Active state when triggered")]
    public bool Exit_Enable = true;
    [Tooltip("What objects to set active state on")]
    public GameObject[] This_on_exit = new GameObject[0];

    [Header("Synching")]
    [Tooltip("All players in world are affected.")]
    public bool Global_Synched = false;
    [Tooltip("Players who join will see what others see. \r\n(If set to Global_Synched)")]
    public bool Late_Join_Synched = false;

    [Header("Events")]
    [Tooltip("Event trigger when player detected")]
    public bool Detect_Player = true;
    [Tooltip("Event trigger when object collider detected")]
    public bool Detect_Object = true;
    [Space(3)]
    public bool Event_OnTrigger = true;
    public bool Event_OnCollision = false;

    void OnCollisionEnter(Collision other) { if (Detect_Object && other != null && Event_OnCollision) { SendCustomEvent("RunEnter"); } }
    void OnCollisionExit(Collision other) { if (Detect_Object && other != null && Event_OnCollision) { SendCustomEvent("RunExit"); } }
    void OnTriggerEnter(Collider other) { if (Detect_Object && other != null && Event_OnTrigger) { SendCustomEvent("RunEnter"); } }
    void OnTriggerExit(Collider other) { if (Detect_Object && other != null && Event_OnTrigger) { SendCustomEvent("RunExit"); } }

    public override void OnPlayerCollisionEnter(VRCPlayerApi player) { if (Detect_Player && Event_OnCollision && player.isLocal) { SendCustomEvent("RunEnter"); } }
    public override void OnPlayerCollisionExit(VRCPlayerApi player) { if (Detect_Player && Event_OnCollision && player.isLocal) { SendCustomEvent("RunExit"); } }
    public override void OnPlayerTriggerEnter(VRCPlayerApi player) { if (Detect_Player && Event_OnTrigger && player.isLocal) { SendCustomEvent("RunEnter"); } }
    public override void OnPlayerTriggerExit(VRCPlayerApi player) { if (Detect_Player && Event_OnTrigger && player.isLocal) { SendCustomEvent("RunExit"); } }
    
    void Start()
    {
        if (Networking.LocalPlayer == null)
        { Global_Synched = false; }
        if (Synch_memmory == 1)//&& Global_Synched && Late_Join_Synched //implicit verified by Synch_memmory
        { SendCustomEvent("Enter"); }
        else if (Synch_memmory == 0)
        { SendCustomEvent("Exit"); }
    }

    public void RunEnter()
    {
        if (This_on_enter.Length != 0 && This_on_enter[0] != null)
        {
            if (Global_Synched)
            {
                    if (Late_Join_Synched)
                    { Synch_memmory = 1; }
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Enter");
            }
            else
            {
                SendCustomEvent("Enter");
            }
        }
    }

    public void RunExit()
        {
        if (This_on_exit.Length != 0 && This_on_exit[0] != null)
            {
                if (Global_Synched)
                {
                    if (Late_Join_Synched)
                     { Synch_memmory = 0; }
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Exit");
                }
                else
                {
                    SendCustomEvent("Exit"); 
                }
            }
    }

    public void Enter()
    {
        for (uint i = 0; i < This_on_enter.Length; i++)
        {
            if (This_on_enter[i] != null)
            { This_on_enter[i].SetActive(Enter_Enable); }
        }
    }

    public void Exit()
    {
        for (uint i = 0; i < This_on_exit.Length; i++)
        {
            if (This_on_exit[i] != null)
            { This_on_exit[i].SetActive(Exit_Enable); }
        }
    }
}

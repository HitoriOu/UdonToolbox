
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CustomEventTrigger : UdonSharpBehaviour
{
    /* Dev Notes:
     * U# Script made by "Hitori Ou" for free use with "VR Chat" using UDON on Unity.
     * Code designed to act as a set on/off with toggle function optional by calling intended function
     * Interact event will always swap events when "Toggle_Type" in use as it does not have a active/inactive type of behaviour
     * When Swap_events_on_toggle is off keep in mind to activate both the paired events or it will behave as a "Set" instead of "Toggle_Type"
     */

    [UdonSynced(UdonSyncMode.None)]
    uint synch_mem=2;
    private bool current_state = false;

    [Header("Single Type Setup")]
    public GameObject[] Udon_scripts_1;
    public string[] Event_Name_1;

    [Header("Toggle Type Setup")]
    [Tooltip("Turns Toggle type on")]
    public bool Toggle_Type = false;
    [Tooltip("When disabled: active and inactive type events are fixed events according to their variable_1/0")]
    public bool Swap_events_on_toggle = false;
    public GameObject[] Udon_scripts_0;
    public string[] Event_Name_0;

    [Header("Synching")]
    [Tooltip("Networked function calls are only made to object owner, this prevents a function being called multiple times once for each player in world.\r\n(Disables Late_Join_Synched)")]
    public bool Owner_Only = false;
    [Tooltip("All players in world are affected.")]
    public bool Global_Synched = false;
    [Tooltip("Players who join will see what others see. \r\n(If set to Global_Synched)")]
    public bool Late_Join_Synched = false;

    [Header("Events")]
    public bool Event_Interact = true;
    [Space(10)]
    public bool Event_OnCollisionEnter = false;
    public bool Event_OnCollisionExit = false;
    [Space(10)]
    public bool Event_OnTriggerEnter = false;
    public bool Event_OnTriggerExit = false;

    [Space(10)]
    public bool Event_OnPickup = false;
    public bool Event_OnDrop = false;
    [Space(10)]
    public bool Event_OnPickupUseDown = false;
    public bool Event_OnPickupUseUp = false;

    public override void Interact() { if (Event_Interact) { SendCustomEvent("Run_interact_toggle"); } }
    void OnCollisionEnter(Collision other) { if (Event_OnCollisionEnter) { SendCustomEvent("Run_1"); } }
    void OnCollisionExit(Collision other) { if (Event_OnCollisionExit) { SendCustomEvent("Run_0"); } }
    void OnTriggerEnter(Collider other) { if (Event_OnTriggerEnter) { SendCustomEvent("Run_1"); } }
    void OnTriggerExit(Collider other) { if (Event_OnTriggerExit) { SendCustomEvent("Run_0"); } }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player) { if (Event_OnTriggerEnter && player.isLocal) { SendCustomEvent("Run_1"); } }
    public override void OnPlayerTriggerExit(VRCPlayerApi player) { if (Event_OnTriggerExit && player.isLocal) { SendCustomEvent("Run_0"); } }
    public override void OnPlayerCollisionEnter(VRCPlayerApi player) { if (Event_OnCollisionEnter && player.isLocal) { SendCustomEvent("Run_1"); } }
    public override void OnPlayerCollisionExit(VRCPlayerApi player) { if (Event_OnCollisionExit && player.isLocal) { SendCustomEvent("Run_0"); } }

    public override void OnPickup() { if (Event_OnPickup) { SendCustomEvent("Run_1"); } }
    public override void OnDrop() { if (Event_OnDrop) { SendCustomEvent("Run_0"); } }
    public override void OnPickupUseDown() { if (Event_OnPickupUseDown) { SendCustomEvent("Run_1"); } }
    public override void OnPickupUseUp() { if (Event_OnPickupUseUp ) { SendCustomEvent("Run_0"); } }

    public void Start()
    {
        if (Networking.LocalPlayer == null)
        {
            Global_Synched = false;

            if (Event_Name_1 == null || Udon_scripts_1 == null)
            { Debug.LogWarning("Udon Toolbox error notice: Udon_scripts_1 && Event_Name_1 are mandatory", this.gameObject); }
            else if(Event_Name_1.Length != Udon_scripts_1.Length)
            { Debug.LogWarning("Udon Toolbox error notice: Udon_scripts_1 && Event_Name_1 have to be same size", this.gameObject);  }
            else
            {
                for(uint i=0;i< Udon_scripts_1.Length; i++)
                {
                    if(Udon_scripts_1[i] != null && Event_Name_1[i] == null || Udon_scripts_1[i] != null && Event_Name_1[i].Length ==0)
                     { Debug.LogWarning("Udon Toolbox error notice: Udon_scripts_1 element:" + i.ToString()+" is missing Event_Name_1 for element:"+i.ToString(), this.gameObject); }
                }
            }
            if(Toggle_Type)
            {
                if (Event_Name_1 == null || Udon_scripts_1 == null)
                { Debug.LogWarning("Udon Toolbox error notice: Udon_scripts_0 && Event_Name_0 are mandatory when Toggle_Type in use", this.gameObject); }
                else if (Event_Name_0.Length != Udon_scripts_0.Length)
                { Debug.LogWarning("Udon Toolbox error notice: Udon_scripts_0 && Event_Name_0  have to be same size", this.gameObject); }
                else
                {
                    for (uint i = 0; i < Udon_scripts_0.Length; i++)
                    {
                        if (Udon_scripts_0[i] != null && Event_Name_0[i] == null || Udon_scripts_0[i] != null && Event_Name_0[i].Length == 0)
                        { Debug.LogWarning("Udon Toolbox error notice: Udon_scripts_0 element:" + i.ToString() + " is missing Event_Name_0 for element:" + i.ToString(), this.gameObject); }
                    }
                }
            }
        }

        if (Global_Synched && Late_Join_Synched && !Owner_Only)
        {
            if (synch_mem == 0)
            { SendCustomEvent("Run_event_0_NET"); }
            else if (synch_mem == 1)
            { SendCustomEvent("Run_event_1_NET"); }
        }
    }

    public void Run_interact_toggle()
    {
        if (Toggle_Type)
        {
            if (current_state)
            {
                if (Global_Synched)
                {
                    if (Owner_Only)
                    { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "Run_event_0_NET"); }
                    else
                    {
                        if (Late_Join_Synched)
                        { synch_mem = 0; }
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Run_event_0_NET");
                    }
                }
                else
                { SendCustomEvent("Run_event_0_NET"); }
            }
            else
            {
                if (Global_Synched)
                {
                    if (Owner_Only)
                    { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "Run_event_1_NET"); }
                    else
                    {
                        if (Late_Join_Synched)
                        { synch_mem = 1; }
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Run_event_1_NET");
                    }
                }
                else
                { SendCustomEvent("Run_event_1_NET"); }
            }
        }
        else
        {
            if (Global_Synched)
            {
                if (Owner_Only)
                { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "Run_event_1_NET"); }
                else
                {
                    if (Late_Join_Synched)
                    { synch_mem = 1; }
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Run_event_1_NET");
                }
            }
            else
            { SendCustomEvent("Run_event_1_NET"); }
        }
    }

    public void Run_1()
    {
        if(Toggle_Type)
        {
            if(Swap_events_on_toggle)
            {
                if(current_state)
                {
                    if (Global_Synched)
                    {
                        if (Owner_Only)
                        { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "Run_event_0_NET"); }
                        else
                        {
                            if (Late_Join_Synched)
                            { synch_mem = 0; }
                            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Run_event_0_NET");
                        }
                    }
                    else
                    { SendCustomEvent("Run_event_0_NET"); }
                }
                else
                {
                    if (Global_Synched)
                    {
                        if (Owner_Only)
                        { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "Run_event_1_NET"); }
                        else
                        {
                            if (Late_Join_Synched)
                            { synch_mem = 1; }
                            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Run_event_1_NET");
                        }
                    }
                    else
                    { SendCustomEvent("Run_event_1_NET"); }
                }
            }
            else
            {
                if (Global_Synched)
                {
                    if (Owner_Only)
                    { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "Run_event_1_NET"); }
                    else
                    {
                        if (Late_Join_Synched)
                        { synch_mem = 1; }
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Run_event_1_NET");
                    }
                }
                else
                { SendCustomEvent("Run_event_1_NET"); }
            }
        }
        else
        {
            if (Global_Synched)
            {
                if(Owner_Only)
                { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "Run_event_1_NET"); }
                else
                {
                    if (Late_Join_Synched)
                    { synch_mem = 1; }
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Run_event_1_NET");
                }
            }
            else
            { SendCustomEvent("Run_event_1_NET"); }
        }
    }

    public void Run_0()
    {
        if (Toggle_Type)
        {
            if (Swap_events_on_toggle)
            {
                if (current_state)
                {
                    if (Global_Synched)
                    {
                        if (Owner_Only)
                        { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "Run_event_0_NET"); }
                        else
                        {
                            if (Late_Join_Synched)
                            { synch_mem = 0; }
                            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Run_event_0_NET");
                        }
                    }
                    else
                    { SendCustomEvent("Run_event_0_NET"); }
                }
                else
                {
                    if (Global_Synched)
                    {
                        if (Owner_Only)
                        { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "Run_event_1_NET"); }
                        else
                        {
                            if (Late_Join_Synched)
                            { synch_mem = 1; }
                            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Run_event_1_NET");
                        }
                    }
                    else
                    { SendCustomEvent("Run_event_1_NET"); }
                }
            }
            else
            {
                if (Global_Synched)
                {
                    if (Owner_Only)
                    { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "Run_event_0_NET"); }
                    else
                    {
                        if (Late_Join_Synched)
                        { synch_mem = 0; }
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Run_event_0_NET");
                    }
                }
                else
                { SendCustomEvent("Run_event_0_NET"); }
            }
        }
        else
        {
            if (Global_Synched)
            {
                if (Owner_Only)
                { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "Run_event_1_NET"); }
                else
                {
                    if (Late_Join_Synched)
                    { synch_mem = 1; }
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Run_event_1_NET");
                }
            }
            else
            { SendCustomEvent("Run_event_1_NET"); }
        }
    }

    public void Run_event_1_NET()
    {
        for (uint i=0;i<Udon_scripts_1.Length;i++)
        {
            if(Udon_scripts_1[i] != null)
            {
                UdonBehaviour temp = (UdonBehaviour)Udon_scripts_1[i].GetComponent(typeof(UdonBehaviour));
                if (temp != null)
                {
                    temp.SendCustomEvent(Event_Name_1[i]);
                }
            }
        }
        current_state = true;
    }

    public void Run_event_0_NET()
    {
        for (uint i = 0; i < Udon_scripts_0.Length; i++)
        {
            UdonBehaviour temp = (UdonBehaviour)Udon_scripts_0[i].GetComponent(typeof(UdonBehaviour));
            if (temp != null)
            {
                temp.SendCustomEvent(Event_Name_0[i]);
            }
        }
        current_state = false;
    }
}

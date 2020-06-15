
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ToggleUdonBool : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None)] uint synch_mem=2;

    [Tooltip("List of game objects containing a valid Udon Behaviour component")]
    public UdonSharpBehaviour[] Where_to_toggle;
    [Tooltip("Name of variable to toggle")]
    public string What_to_toggle = new string(new char[0]);
    [Tooltip("Name of update function to call (Optional)")]
    public string Call_Event_Name = new string(new char[0]);

    [Header("Synching")]
    [Tooltip("All players in world are affected.")]
    public bool Global_Synched = true;
    [Tooltip("Players who join will see what others see. \r\n(If set to Global_Synched)")]
    public bool Late_Join_Synched = true;

    [Header("Events")]
    [Tooltip("Custom Event for UI to call/use")]
    public bool Event_UI_Update = true; /*custom event for UI to use on UI updates*/
    public bool EventInteract = true;
    public bool EventOnCollisionEnter = false;
    public bool EventOnCollisionExit = false;
    public bool EventOnTriggerEnter = false;
    public bool EventOnTriggerExit = false;

    public void UI_Update() { if (Event_UI_Update) { SendCustomEvent("Run"); } }
    void Interact() { if (EventInteract) { SendCustomEvent("Run"); } }
    void OnCollisionEnter(Collision other) { if (EventOnCollisionEnter) { SendCustomEvent("Run"); } }
    void OnCollisionExit(Collision other) { if (EventOnCollisionExit) { SendCustomEvent("Run"); } }
    void OnTriggerEnter(Collider other) { if (EventOnTriggerEnter) { SendCustomEvent("Run"); } }
    void OnTriggerExit(Collider other) { if (EventOnTriggerExit) { SendCustomEvent("Run"); } }

    void Start()
    {
        if (Networking.LocalPlayer == null)
        { Global_Synched = false; }
        if (synch_mem != 2)/*"Global_Synched&&Late_Join_Synched" explicit checked by "synch_mem!=2"*/
            {
                if (synch_mem == 0)
                { SendCustomEvent("OFF"); }
                else if (synch_mem == 1)
                { SendCustomEvent("ON"); }
            }
    }

    public void Run()
    {
        if (Where_to_toggle.Length>=0 && Where_to_toggle[0]!=null)
        {
            bool state_of_first = (bool) Where_to_toggle[0].GetProgramVariable(What_to_toggle);
            if (Global_Synched)
            {
                for(int i=0;i<Where_to_toggle.Length;i++)
                {
                    if (!state_of_first)
                    { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ON"); }
                    else
                    { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "OFF"); }
                }
                if(Late_Join_Synched)
                {
                    if(state_of_first)
                    { synch_mem = 1; }
                    else
                    { synch_mem = 0; }
                }
            }
            else
            {
                for (int i = 0; i < Where_to_toggle.Length; i++)
                {
                    if (!state_of_first)
                    { SendCustomEvent("ON"); }
                    else
                    { SendCustomEvent("OFF"); }
                }
            }
        }
    }

    public void ON()
    {
        for (int i = 0; i < Where_to_toggle.Length; i++)
        {
            Where_to_toggle[i].SetProgramVariable(What_to_toggle, true);

            if (Call_Event_Name != null && Call_Event_Name.Length > 0)
            { Where_to_toggle[i].SendCustomEvent(Call_Event_Name); }
        }
    }

    public void OFF()
    {
        for (int i = 0; i < Where_to_toggle.Length; i++)
        {
            Where_to_toggle[i].SetProgramVariable(What_to_toggle, false);

            if (Call_Event_Name != null && Call_Event_Name.Length > 0)
            { Where_to_toggle[i].SendCustomEvent(Call_Event_Name); }
        }
    }

}

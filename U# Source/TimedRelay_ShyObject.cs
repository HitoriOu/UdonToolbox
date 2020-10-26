
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TimedRelay_ShyObject : UdonSharpBehaviour
{
    /*
     * Dev notes:
     *Designed to be a optimiced timer that disables it's idle cycle once timer has run out (disables it's own game object)
     *If you want to use "Late join" the recomended setup is to have the relay active on start else nothing happens
     */
    ushort synch_mem = 2;
    float timer_mem=0;
    bool next_toggle_state;
    bool running = false;

    [Tooltip("Time untill relay triggers and auto shutdown/hide")]
    public float Timer = 10;

    [Header("Relay Setup")]
    public bool Same_active_state_as_relay = false;
    public GameObject[] Targets;

    [Header("Synching")]
    [Tooltip("All players in world are affected.")]
    public bool Global_Synched = true;
    [Tooltip("Players who join will see what others see. \r\n(If set to Global_Synched)")]
    public bool Late_Join_Synched = false;
    
    void Start()
    {
        if (Networking.LocalPlayer == null)
        { Global_Synched = false; }
        if (synch_mem != 2)
        {
            if(synch_mem==0)
            { SendCustomEvent("OFF"); }
            else if(synch_mem==1)
            { SendCustomEvent("ON"); }
        }
    }

    public void Update()
    {
        if(running)
        {
            if(Time.time>timer_mem)
            {
                next_toggle_state = !Same_active_state_as_relay;
                SendCustomEvent("run_toggle_state");
                running = false;
                this.gameObject.SetActive(false);
            }
        }
        else
        {
            running = true;
            timer_mem = Timer + Time.time;
            next_toggle_state = Same_active_state_as_relay; 
            SendCustomEvent("run_toggle_state");
        }
    }

    public void run_toggle_state()
    {
        if(Targets.Length>0)
        {
            if(Global_Synched)
            {
                if(next_toggle_state)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ON");
                    if (Late_Join_Synched)
                    { synch_mem = 1; }
                }
                else
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "OFF");
                    if (Late_Join_Synched)
                    { synch_mem = 0; }
                }
            }
            else
            {
                if (next_toggle_state)
                { SendCustomEvent("ON"); }
                else
                { SendCustomEvent("OFF"); }
            }
        }
    }

    public void ON()
    {
        for(uint i=0;i<Targets.Length ;i++)
        { if (Targets[i] != null) { Targets[i].SetActive(true); } }
    }

    public void OFF()
    {
        for (uint i = 0; i < Targets.Length; i++)
        { if (Targets[i] != null) { Targets[i].SetActive(false); } }
    }
}

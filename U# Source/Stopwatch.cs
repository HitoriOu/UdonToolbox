
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Stopwatch : UdonSharpBehaviour
{
    /* Dev Notes:
     * U# Script made by "Hitori Ou" for free use with "VR Chat" using UDON on Unity.
     * Function "Stopwatch_Reset_All" resets all times to 0
     * Function "Stopwatch_Reset" resets current time to 0
     * Function "OnPickup" can be used to turn the stopwatch on
     * Function "OnDrop" can be used to turn the stopwatch off
     */
     

    [UdonSynced(UdonSyncMode.None)]
    float current_time_synched = 0;
    [UdonSynced(UdonSyncMode.None)]
    float previous_time_synched = 0;
    [UdonSynced(UdonSyncMode.None)]
    float min_time_synched = 0;
    [UdonSynced(UdonSyncMode.None)]
    float max_time_synched = 0;

    [UdonSynced(UdonSyncMode.None)]
    bool started_synch = false;

    private float reset_check = 0; /*double click check value*/
    private float current_time_MEM =0;
    private float previous_time_MEM = 0;
    private float min_time_MEM = 0;
    private float max_time_MEM = 0;
    private bool running = true;


    [Header("Text fields")]
    [Tooltip("Current time elapsed")]
    public UnityEngine.UI.Text[] Current_time;
    [Tooltip("Previous time recorded")]
    public UnityEngine.UI.Text[] Prev_time;
    [Tooltip("Minimum time recorded")]
    public UnityEngine.UI.Text[] Min_time;
    [Tooltip("Maximum time recorded")]
    public UnityEngine.UI.Text[] Max_time;


    [Header("Synching")]
    [Tooltip("All players in world are affected.")]
    public bool Global_Synched = true;
    [Tooltip("Players who join will see what others see. \r\n(If set to Global_Synched)")]
    public bool Late_Join_Synched = true;

    [Header("Events")]
    public bool Event_Interact = true;
    public bool Event_OnCollisionEnter = false;
    public bool Event_OnCollisionExit = false;
    public bool Event_OnTriggerEnter = false;
    public bool Event_OnTriggerExit = false;


    void Interact() { if (Event_Interact) { SendCustomEvent("Stopwatch_Click"); } }
    void OnCollisionEnter(Collision other) { if (Event_OnCollisionEnter) { SendCustomEvent("Stopwatch_Click"); } }
    void OnCollisionExit(Collision other) { if (Event_OnCollisionExit) { SendCustomEvent("Stopwatch_Click"); } }
    void OnTriggerEnter(Collider other) { if (Event_OnTriggerEnter) { SendCustomEvent("Stopwatch_Click"); } }
    void OnTriggerExit(Collider other) { if (Event_OnTriggerExit) { SendCustomEvent("Stopwatch_Click"); } }

    [Space(6)]
    [Tooltip("Stopwatch is enabled while held/picked-up\r\n(uncheck to set always on)")]
    public bool Event_EnableOnPickup = false;
    public void OnPickup() { if (Event_EnableOnPickup) { SendCustomEvent("run_update_start"); } }
    public void OnDrop() { if (Event_EnableOnPickup) { SendCustomEvent("run_update_stop"); } }

    public bool Event_OnPickupUseDown = false;
    public bool Event_OnPickupUseUp = false;
    public void OnPickupUseDown() { if (Event_OnPickupUseDown) { SendCustomEvent("Stopwatch_Click"); } }
    public void OnPickupUseUp() { if (Event_OnPickupUseUp) { SendCustomEvent("Stopwatch_Click"); } }


    public void Stopwatch_Click()
    {
        float value = Time.time - current_time_MEM;
        current_time_MEM = Time.time;
        if(Global_Synched)
        { previous_time_synched = value; }
        previous_time_MEM = value;

        if(previous_time_MEM > max_time_MEM)
        {
            max_time_MEM = previous_time_MEM;
            if(Global_Synched)
            { max_time_synched = max_time_MEM; }
        }

        if(previous_time_MEM<min_time_MEM || min_time_MEM==0)
        {
            min_time_MEM = previous_time_MEM;
            if (Global_Synched)
            { min_time_synched = min_time_MEM; }
        }

        Update_time();
    }

    private void Update_time()
    {
        if (Prev_time != null && Prev_time.Length > 0)
        {
            for (uint i = 0; i < Prev_time.Length; i++)
            {
                Prev_time[i].text = previous_time_MEM.ToString();
            }
        }

        if (Min_time != null && Min_time.Length > 0)
        {
            for (uint i = 0; i < Min_time.Length; i++)
            {
                Min_time[i].text = min_time_MEM.ToString();
            }
        }

        if (Max_time != null && Max_time.Length > 0)
        {
            for (uint i = 0; i < Max_time.Length; i++)
            {
                Max_time[i].text = max_time_MEM.ToString();
            }
        }
    }

    public void Stopwatch_Reset()
    {
        current_time_MEM = Time.time;
    }

    public void Stopwatch_Reset_All()
    {
        if(Time.time<reset_check)
        {
         if(Global_Synched)
          {
                previous_time_synched = 0;
                min_time_synched = 0;
                max_time_synched = 0;
          }
         current_time_MEM = 0;
         previous_time_MEM = 0;
         min_time_MEM = 0;
         max_time_MEM = 0;
        }
        else
         { reset_check = Time.time + 1; }
        Update_time();
    }

    void Start()
    {
        if(!Event_EnableOnPickup)
         { SendCustomEvent("update_start"); }

        if (Networking.LocalPlayer == null)
         { Global_Synched = false; }

        if(Global_Synched && Late_Join_Synched)
         {
            running = started_synch;
            previous_time_MEM = previous_time_synched;
            max_time_MEM = max_time_synched;
            min_time_MEM = min_time_synched;
         }

        current_time_MEM = Time.time;
    }

    public void run_update_start()
    {
        if(Global_Synched)
         { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "update_start"); }
        else
         { SendCustomEvent("update_start"); }
    }

    public void run_update_stop()
    {
        if (Global_Synched)
         { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "update_stop"); }
        else
         { SendCustomEvent("update_stop"); }
    }

    public void update_start()
    {
        if(Late_Join_Synched)
        { started_synch = true; }
        running = true;
    }

    public void update_stop()
    {
        if (Late_Join_Synched)
        { started_synch = false; }
        running = false;
    }

    private float cut_decimal(float number , int value)
    {
        //number = 1.2345678f; //test value
        number = number * (value*10);
        number= Mathf.Round(number);
        number = number / (value * 10);

        return number;
    }

    public void FixedUpdate()
    {
        if(running)
        {
            if(Current_time != null && Current_time.Length>0)
            {
                for(uint i=0;i< Current_time.Length;i++)
                {
                   // Current_time[i].text = cut_decimal((Time.time-current_time_MEM),1).ToString();
                    Current_time[i].text = (Time.time - current_time_MEM).ToString();
                }
            }
        }
    }

}


using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Alarmclock : UdonSharpBehaviour
{
    /*Devnotes:
     * "Run" function is default event trigger
     * "ResetTimer" function is used to restart the countdown locally
     */

    private float update_time_MEM = 0;
    private float time_to_elapse = 0;
    private bool running = false;
    private bool alarm_on = false;


    [Header("Timer & Alarm Setup")]
    [Tooltip("Time that elapse after reset")]
    public float Timer = 30;

    [Space(3)]
    public GameObject[] Alarm_Objects = new GameObject[0];
    [Tooltip("How long the alarm lasts")]
    public float Alarm_Duration = 10;
    [Tooltip("Alarm turns Alarm_Objects ON or OFF")]
    public bool Alarm_Set_Active_ON = true;

    [Header("Display Setup & Options")]
    public UnityEngine.UI.Text[] Text_Display = new UnityEngine.UI.Text[0];

    public bool Show_In_Seconds = false;

    public bool Flip_Horisontal = false;
    public bool Flip_Format = false;
    public bool Show_Days = true;
    public string Padding = " : ";
    string[] Text_Display_MEM;

    [Header("Synching")]
    [Tooltip("All players in world are affected")]
    public bool Global_Synched = true;
    [Tooltip("Players who join has counter started (locally)")]
    public bool Countdown_On_Start = true;

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

    public void OnPlayerTriggerEnter(VRCPlayerApi player) { if (Event_OnTriggerEnter) { SendCustomEvent("Run"); } }
    public void OnPlayerTriggerExit(VRCPlayerApi player) { if (Event_OnTriggerExit) { SendCustomEvent("Run"); } }
    public void OnPlayerCollisionEnter(VRCPlayerApi player) { if (Event_OnCollisionEnter) { SendCustomEvent("Run"); } }
    public void OnPlayerCollisionExit(VRCPlayerApi player) { if (Event_OnCollisionExit) { SendCustomEvent("Run"); } }

    void Start()
    {
        if (Networking.LocalPlayer == null)
        { Global_Synched = false; }
        
        if (Text_Display.Length > 0) { Text_Display_MEM = copy_text(Text_Display); }
        //world_time_start = Time.fixedTime;
        if(Countdown_On_Start)
         { SendCustomEvent("ResetTimer"); }
    }

    private string[] copy_text(UnityEngine.UI.Text[] original)
    {
        /* Saves the old text in all the text fields for easy titles */
        string[] copy = new string[original.Length];
        for (int i = 0; i < copy.Length; i++)
        {
            if (original[i] != null)
            { copy[i] = original[i].text; }
        }
        return copy;
    }

    public void Run()
    {
        if (Global_Synched)
         { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ResetTimer"); }
        else
         { SendCustomEvent("ResetTimer"); }
    }

    public void ResetTimer()
    {
        time_to_elapse = Timer + Time.time;
        running = true;
        alarm_on = false;
        SendCustomEvent("Alarm_OFF");
    }

    private void set_text(UnityEngine.UI.Text[] destination, string[] old_text, string text)
    {
        /* Uses the old text in all the text fields for easy titles */
        for (int i = 0; i < destination.Length; i++)
        {
            if (destination[i] != null && old_text[i] != null)
            {
                if (Flip_Horisontal)
                { destination[i].text = text + old_text[i]; }
                else
                { destination[i].text = old_text[i] + text; }
            }
        }
    }

    private string time_from_sec(float total)
    {
        string text = "";
        int time = (int)Mathf.Floor(total);

        // displaySeconds = (time % 60);
        // displayMinutes = ((time / 60) % 60);
        // displayHours = ((time / 3600) % 24);
        // displayDays = (time / 86400);
        if (Flip_Format)
        {
            if (Show_Days)
            { text += (time / 86400).ToString() + Padding; }
            text += ((time / 3600) % 24).ToString() + Padding;
            text += ((time / 60) % 60).ToString() + Padding;
            text += (time % 60).ToString();
        }
        else
        {
            text += (time % 60).ToString() + Padding;
            text += ((time / 60) % 60).ToString() + Padding;
            text += ((time / 3600) % 24).ToString();
            if (Show_Days)
            { text += Padding + (time / 86400).ToString(); }
        }
        return text;
    }

    public void LateUpdate()
    {
        /*Updates are set to 1 sec interval for performance.*/
        if (running && Time.time > update_time_MEM)
        {
            if (Text_Display.Length > 0)
            {
                float temp_time = time_to_elapse - Time.time;
                if (temp_time>-2)
                 { Update_Timer(time_to_elapse - Time.time); }
                if(!alarm_on && temp_time < 0.25f && temp_time >= -Alarm_Duration)
                {
                    if (Global_Synched)
                    { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Alarm_ON"); }
                    else
                    { SendCustomEvent("Alarm_ON"); }
                    alarm_on = true;
                }
                else if(alarm_on && temp_time < -Alarm_Duration)
                {
                    if (Global_Synched)
                    { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Alarm_OFF"); }
                    else
                    { SendCustomEvent("Alarm_OFF"); }
                    alarm_on = false;
                    running = false;
                }
            }
            update_time_MEM = Time.time + 1;
        }
    }

    public void Alarm_ON()
    {
        for (int i = 0; i < Alarm_Objects.Length; i++)
         { Alarm_Objects[i].SetActive(true); }
    }

    public void Alarm_OFF()
    {
        for (int i = 0; i < Alarm_Objects.Length; i++)
         { Alarm_Objects[i].SetActive(false); }
    }

    private void Update_Timer(float time_to_show)
    {
       string text = "";
       if(time_to_show<0)
        { time_to_show = 0; }
       time_to_show = (int)Mathf.Floor(time_to_show);

        if (Show_In_Seconds)
        { text = time_to_show.ToString(); }
       else
        { text = time_from_sec(time_to_show); }

       set_text(Text_Display, Text_Display_MEM, text);
    }
}

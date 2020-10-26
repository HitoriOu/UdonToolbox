
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TimeAndDate : UdonSharpBehaviour
{

    /* Dev Notes:
     * Saves the old text in all the text fields for easy titles 
     * Functions "GMT_UTC_Plus" and "GMT_UTC_Minus" can be called/used by for example UI buttons to adjust the time
     * Function "Manual_Synch" can be used to force a network synch event 
     *  (only usefull if your forcing a desynch by toggling "Global_Synched" during game).
     */

    private float update_time_MEM = 0;
    [Header("System Clock & Date")]
    public UnityEngine.UI.Text[] Local_Time = new UnityEngine.UI.Text[0];
    string[] Local_Time_MEM;

    public UnityEngine.UI.Text[] Local_Time_Date = new UnityEngine.UI.Text[0];
    string[] Local_Time_Date_MEM;

    [Header("Game Clock")]
    [Tooltip("The clock will show how many days have elapsed")]
    public bool Show_Days = true;
    public UnityEngine.UI.Text[] Total_Game_Time = new UnityEngine.UI.Text[0];
    string[] Total_Game_Time_MEM;

    float world_time_start =0;
    public UnityEngine.UI.Text[] World_Time = new UnityEngine.UI.Text[0];
    string[] World_Time_MEM;

    [UdonSynced(UdonSyncMode.None)]
    int synched_GMT_UTC = 0;

    [Header("Time & Date (GMT/UTC)")]
    [Tooltip("Show zone number on display")]
    public bool Show_GMT_UTC_value = true;
    [Tooltip("What time zone number")]
    public int GMT_UTC;
    public UnityEngine.UI.Text[] GMT_Zone_Time = new UnityEngine.UI.Text[0];
    string[] GMT_UTC_TimeZone_MEM;

    public UnityEngine.UI.Text[] GMT_Zone_Time_Date = new UnityEngine.UI.Text[0];
    string[] GMT_UTC_TimeZone_Date_MEM;

    [Header("Format Setup")]
    [Tooltip("What orientation the original text is shown")]
    public bool Flip_Horisontal = false;
    [Tooltip("What orientation the clock display (sec:min:hour)")]
    public bool Flip_Format = false;
    [Tooltip("Text/character used between number/values")]
    public string Padding = " : ";

    [Header("Synching")]
    [Tooltip("All players in world are affected (if UTC zone is changed).")]
    public bool Global_Synched = false;
    [Tooltip("Players who join will see what UTC time zone others see. \r\n(If set to Global_Synched)")]
    public bool Late_Join_Synched = false;

    void Start()
    {
        if (Networking.LocalPlayer == null)
        { Global_Synched = false; }
        if (Late_Join_Synched && Global_Synched)
          { GMT_UTC = synched_GMT_UTC; }
         if (Local_Time.Length > 0) { Local_Time_MEM=copy_text(Local_Time); }
         if (Local_Time_Date.Length > 0) { Local_Time_Date_MEM = copy_text(Local_Time_Date); }
         if (Total_Game_Time.Length > 0) { Total_Game_Time_MEM = copy_text(Total_Game_Time); }
         world_time_start = Time.fixedTime;
         if (World_Time.Length > 0) { World_Time_MEM = copy_text(World_Time); }
         if (GMT_Zone_Time.Length > 0) { GMT_UTC_TimeZone_MEM = copy_text(GMT_Zone_Time); }
         if (GMT_Zone_Time_Date.Length > 0) { GMT_UTC_TimeZone_Date_MEM = copy_text(GMT_Zone_Time_Date); }
            //if (this > 0) { =storage_old_text(); }
    }

    public void GMT_UTC_Plus()
    {
        if (Global_Synched)
        {
            if (Late_Join_Synched)
            { synched_GMT_UTC = GMT_UTC+1; }
             SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Plus"); 
        }
        else
        { SendCustomEvent("Plus"); }
    }

    public void Plus()
    { GMT_UTC++; }

    public void Manual_Synch()
    { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Call_Manual_Synch"); }

    public void Call_Manual_Synch()
    { GMT_UTC = 0;  synched_GMT_UTC=0; }

    public void Minus()
    { GMT_UTC--; }

    public void GMT_UTC_Minus()
    {
        if (Global_Synched)
        {
            if (Late_Join_Synched)
            {  synched_GMT_UTC = GMT_UTC-1; }
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Minus");
        }
        else
        { SendCustomEvent("Minus"); }
    }

    private string[] copy_text(UnityEngine.UI.Text[] original)
    {
        /* Saves the old text in all the text fields for easy titles */
        string[] copy = new string[original.Length];
        for (int i = 0; i < copy.Length; i++)
        {
            if (original[i] != null)
            { copy[i] = original[i].text;  }
        }
        return copy;
    }

    private void set_text(UnityEngine.UI.Text[] destination, string[] old_text,  string text)
    {
        /* Uses the old text in all the text fields for easy titles */
        for (int i = 0; i < destination.Length; i++)
        {
            if (destination[i] != null&& old_text[i]!=null)
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
        if(Flip_Format)
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
        /*Updates are set to 1 sec interval divided by 3 sections with 0.33 sec offset for smoother performance.*/
        if (Time.time>update_time_MEM)
        {
            if (Local_Time.Length > 0) { Update_Local_Time(); }
            if (GMT_Zone_Time.Length > 0) { Update_GMT_Zone_Time(); }
            update_time_MEM = Time.time + 1;
        }
        if (Time.time > update_time_MEM-0.33f)
        {
            if (Total_Game_Time.Length > 0) { Update_Total_Game_Time(); }
            if (GMT_Zone_Time_Date.Length > 0) { Update_GMT_Zone_Date(); }
        }
        if (Time.time > update_time_MEM-0.66f)
        {
            if (World_Time.Length > 0) { Update_World_Time(); }
            if (Local_Time_Date.Length > 0) { Update_Local_Time_Date(); }
        }
    }

    private void Update_Local_Time()
    {
        string text="";
        if(Flip_Format)
        {
            text += System.DateTime.Now.Hour.ToString() + Padding;
            text += System.DateTime.Now.Minute.ToString() + Padding;
            text += System.DateTime.Now.Second.ToString();
        }
        else
        {
            text += System.DateTime.Now.Second.ToString() + Padding;
            text += System.DateTime.Now.Minute.ToString() + Padding;
            text += System.DateTime.Now.Hour.ToString();
        }

        set_text(Local_Time, Local_Time_MEM, text);
    }

    private void Update_Local_Time_Date()
    {
        string text = "";

        if (Flip_Format)
        {
            text += System.DateTime.Now.Year.ToString() + Padding;
            text += System.DateTime.Now.Month.ToString() + Padding;
            text += System.DateTime.Now.Day.ToString();
        }
        else
        {
            text += System.DateTime.Now.Day.ToString() + Padding;
            text += System.DateTime.Now.Month.ToString() + Padding;
            text += System.DateTime.Now.Year.ToString();
        }

        set_text(Local_Time_Date, Local_Time_Date_MEM, text);
    }

    private void Update_Total_Game_Time()
    {
        string text = "";
        //text += System.DateTime.Now.Year.ToString();
        //int total =(int) 55.6f;// (int)Time.realtimeSinceStartup;
        //text += Time.fixedTime.ToString();
        //text = Time.realtimeSinceStartup.ToString();
        text = time_from_sec(Time.realtimeSinceStartup);
        set_text(Total_Game_Time, Total_Game_Time_MEM, text);
    }

    private void Update_World_Time()
    {
        string text = "";
        // displaySeconds = (time % 60);
        // displayMinutes = ((time / 60) % 60);
        // displayHours = ((time / 3600) % 24);
        // displayDays = (time / 86400);

        text = time_from_sec(Time.timeSinceLevelLoad);
        set_text(World_Time, World_Time_MEM, text);
    }

    private void Update_GMT_Zone_Time()
    {
        string text = "";

        System.DateTime temp= System.DateTime.UtcNow;
        temp=temp.AddHours((double)GMT_UTC);

        if (Flip_Format)
        {
            if (Show_GMT_UTC_value)
            {
                if (GMT_UTC >= 0)
                { text += "(+" + GMT_UTC + ") "; }
                else
                { text += "(" + GMT_UTC + ") "; }
            }
            text += temp.Hour.ToString() + Padding;
            text += temp.Minute.ToString() + Padding;
            text += temp.Second.ToString();
        }
        else
        {
            text += temp.Second.ToString() + Padding;
            text += temp.Minute.ToString() + Padding;
            text += temp.Hour.ToString();
            if (Show_GMT_UTC_value)
            {
                if (GMT_UTC >= 0)
                { text += " (+" + GMT_UTC + ")"; }
                else
                { text += " (" + GMT_UTC + ")"; }
            }
        }

        set_text(GMT_Zone_Time, GMT_UTC_TimeZone_MEM, text);
    }

    private void Update_GMT_Zone_Date()
    {
        string text = "";

        System.DateTime temp = System.DateTime.UtcNow;
        temp = temp.AddHours((double)GMT_UTC);

        if (Flip_Format)
        {
            if (Show_GMT_UTC_value)
            {
                if (GMT_UTC >= 0)
                { text += "(+" + GMT_UTC + ") "; }
                else
                { text += "(" + GMT_UTC + ") "; }
            }
            text += temp.Year.ToString() + Padding;
            text += temp.Month.ToString() + Padding;
            text += temp.Day.ToString();
        }
        else
        {
            text += temp.Day.ToString() + Padding;
            text += temp.Month.ToString() + Padding;
            text += temp.Year.ToString();
            if (Show_GMT_UTC_value)
            {
                if (GMT_UTC >= 0)
                { text += " (+" + GMT_UTC + ")"; }
                else
                { text += " (" + GMT_UTC + ")"; }
            }
        }

        set_text(GMT_Zone_Time_Date, GMT_UTC_TimeZone_Date_MEM, text);
    }

    /*
    private void Update_()
    {
        string text = "filler7";

        set_text(, text);
    }
    */
}

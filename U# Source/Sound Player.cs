
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SoundPlayer : UdonSharpBehaviour
{
    /*Dev Notes:
     * Music/sound player
     * Main features:
     * Playback of sounds selected from a list of sound files
     * playback using loop, list loop and random selection from list or end after played if nothing selected
     * playback on track change or queue manual track selection for next to play
     * use of multiple audio/sound sources
     * display of track name on multiple UI displays
     * use of file name for text display of track or use of manual track names without file name change (Override)
     * forward and reverce time skip with variable for how much to skip
     * see function list for full list of buttons/options
     * 
     * Most functions are synched using "Audio_Sources Element 0"
     * List of functions:
     * "Toggle_Play" Main play/Stop button
     * "Toggle_Mute" Main mute/un-mute
     * "Toggle_Pause" main pause/un-pause
     * "Next" sets next track
     * "Prev" Sets previous track
     * "Forward" skips forward in time
     * "Reverce" skips backwards in time
     * "Volume_Update" used to update players after "Volume" variable been changed
     */
     
    private float timer_mem = 0;
    private float timer_update_rate = 1;

    /*time synching*/
    private bool time_requested = false;
    [UdonSynced(UdonSyncMode.None)]
    private float current_time_synch = 0;
    private float old_time_synch = 0;
    private float expected_Latency = 0.2f+0.25f;

    /*track variable & sycnhing*/
    private uint Next_Track = 0; /*not public due to compatibillity issues*/
    [UdonSynced(UdonSyncMode.None)]
    private uint current_track_synch = 0;

    /*status variables*/
    private bool playing = false;
    private bool paused = false;
    private bool skip_loop = false; /*used to disable loop behaviour when changing track in loop mode*/

    [Header("Play options")]
    [Tooltip("Starts playing when world is loaded")]
    public bool Play_on_Start = true;
    [Tooltip("Auto starts new track when Next/Prev events are triggered")]
    public bool Play_on_track_change = false;

    [Space(6)]
    [Tooltip("Loop same track (disables 'Random' & 'Loop_Play_All'")]
    public bool Loop = false;
    [Tooltip("Auto play All repeatedly\r\n(enables random)")]
    public bool Loop_Play_ALL = false;
    [Tooltip("Random picks a new track when ended.")]
    public bool Random = false;

    [Space(6)]
    [Tooltip("Volume setting between 0.0-1.0")]
    public float Volume = 0.5f;
    [Tooltip("How many seconds Forward/Reverce events skips")]
    public float Time_Forward_Reverce = 10f;

    [Header("Player & Tracks Setup")]
    [Tooltip("Audio players\r\n(what is used to play sound)")]
    public AudioSource[] Audio_Sources = new AudioSource[1];
    [Tooltip("Audio clips\r\n(what sound files are played)")]
    public AudioClip[] Audio_Clips = new AudioClip[1];

    [Header("Text Output")]
    [Tooltip("Text fields to put the track name in")]
    public UnityEngine.UI.Text[] Audio_Clip_text;
    [Tooltip("Text override that replaces specified elements with custom text for tracks")]
    public string[] Override_Audio_Clip_text = new string[0];


    [Header("Synching")]
    [Tooltip("All players in world are affected.")]
    public bool Global_Synched = true;
    [Tooltip("Players who join will hear what others hear. \r\n(If set to Global_Synched)")]
    public bool Late_Join_Synched = true;

    void Start()
    {
        if (Audio_Sources[0] == null) { Debug.LogWarning("Udon Toolbox: Warning! Param/variable ''Audio_Sources'' cannot have 'Element 0' empty!", this.gameObject); }

        if (Networking.LocalPlayer == null)
        { Global_Synched = false; }

        if (Global_Synched && Late_Join_Synched)
        {
            Next_Track = current_track_synch;
            SendCustomEvent("synch_time");
        }
                
        if (Play_on_Start)
        { SendCustomEvent("Play"); }
        else
        {
            set_text(Next_Track);
            set_track(Next_Track);
        }

        /* //Dev leftover code
        //Audio_Sources[0].PlayDelayed(5); //delayed play start in seconds
        //AudioSources[0].PlayScheduled(100);

        //AudioClip tune = Audio_Sources[0].clip;
        //tune.length
        //tune.loadInBackground
        Audio_Player.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
        Audio_Player.PlayScheduled(60);
        Audio_Player.panStereo=0.5f;
        bool test=Audio_Player.isVirtual;
        Audio_Player.pitch = 0.5f;
        */
    }

    public void Interact()
    {
        Next();
        Volume_Update();
    }

    public void synch_time()
    {
        old_time_synch = current_time_synch;
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "request_time");
        time_requested = true;
    }

    public void request_time()
    {
        if (Networking.LocalPlayer.IsOwner(this.gameObject))
        { current_time_synch = Audio_Sources[0].time+ expected_Latency; }
    }

    public void Volume_Update()
    {
        if (Audio_Sources[0].volume != Volume)
        { set_volume(Volume); }
    }

    public void LateUpdate()
    {
      if(timer_mem<=Time.time)
      {
        timer_mem = Time.time +timer_update_rate;
            
        if (time_requested && Global_Synched && old_time_synch != current_time_synch)
        {
          Set_Time(current_time_synch);
          time_requested = false;
        }
        else if (Audio_Sources[0].isPlaying)//playing)
        {
            if (Loop && Audio_Sources[0].clip != Audio_Clips[Next_Track])
            {
                if (Play_on_track_change)
                { SendCustomEvent("Play"); }
                else
                {
                    skip_loop = true;
                    set_loop_OFF();
                }
            }
            else if (Loop != Audio_Sources[0].loop)
            {
                if (Loop)
                { set_loop_ON(); }
                else
                { set_loop_OFF(); }
            }
        }
        else if (!playing || paused || skip_loop)/*if  not playing, paused or skipping loop for custom track selection*/
        {
            if (!paused && skip_loop)
            {
                set_loop_ON();
                skip_loop = false;
                SendCustomEvent("Play");
            }
        }
        else /*if not playing, not paused not skipping loop*/
        {
            if (!Loop && Loop_Play_ALL)
            {
                if (Random)
                {
                    if (Global_Synched)
                    {
                        if (Networking.LocalPlayer.IsOwner(this.gameObject)) /*if not playing owner sets next track*/
                        { current_track_synch = (uint)UnityEngine.Random.Range((int)0, (int)Audio_Clips.Length); }
                    }
                    else
                    {
                        if (Audio_Sources[0].clip == Audio_Clips[Next_Track])
                        { Next_Track = (uint)UnityEngine.Random.Range((int)0, (int)Audio_Clips.Length); }
                        SendCustomEvent("Play");
                    }
                }
                else
                {
                    if (Audio_Sources[0].clip == Audio_Clips[Next_Track])
                    { Next(); }
                    SendCustomEvent("Play");
                }
            }
            else if (!Loop)/*if stopped and not on loop*/
            { playing = false; }
            else /*auto start if on loop and not playing*/
            { set_loop_ON(); }
        }
      }
    }

    private void set_text(uint number)
    {
        if (Override_Audio_Clip_text.Length > number && Override_Audio_Clip_text[number] != null && Override_Audio_Clip_text[number].Length > 0)
        {
            for (uint i = 0; i < Audio_Clip_text.Length; i++)
            { Audio_Clip_text[i].text = Override_Audio_Clip_text[number]; }
        }
        else if (Audio_Clips.Length > number)
        {
            string text = " (UnityEngine.AudioClip)";/*default text to remove*/
            if (Audio_Clips[number].ToString().EndsWith(text))
            {
                text = Audio_Clips[number].ToString();
                text=text.Remove(text.Length-24);
            }
            else
             { text = Audio_Clips[number].ToString(); }

            for (uint i = 0; i < Audio_Clip_text.Length; i++)
            { Audio_Clip_text[i].text = text; }
        }
    }

    public void toggle_Loop()
    {
        if(Global_Synched)
        {
            if (!Loop)
            { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "set_loop_ON"); }
            else
            { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "set_loop_OFF"); }
        }
        else
        {
            if (!Loop)
            { SendCustomEvent("set_loop_ON"); }
            else
            { SendCustomEvent("set_loop_OFF"); }
        }
    }

    public void set_loop_ON()
    {
        for (uint i = 0; i < Audio_Sources.Length; i++)
        { if (Audio_Sources[i] != null) { Audio_Sources[i].loop = true; } }
    }

    public void set_loop_OFF()
    {
        for (uint i = 0; i < Audio_Sources.Length; i++)
        { if (Audio_Sources[i] != null) { Audio_Sources[i].loop = false; } }
    }

    private void set_volume(float vol)
    {
        for (uint i = 0; i < Audio_Sources.Length; i++)
        { if (Audio_Sources[i] != null) { Audio_Sources[i].volume = vol; } }
    }

    private void Set_Time(float time)
    {
        for (uint i = 0; i < Audio_Sources.Length; i++)
        { if (Audio_Sources[i] != null) { Audio_Sources[i].time = time; } }
    }

    private void set_track(uint number)
    {
        for (uint i = 0; i < Audio_Sources.Length; i++)
        { if (Audio_Sources[i] != null && Audio_Clips[number] != null) { Audio_Sources[i].clip = Audio_Clips[number]; } }
    }

    public void Next()
    {
        if (Global_Synched)
        {
            if (Late_Join_Synched)
            { 
                if (Next_Track + 1 >= Audio_Clips.Length)
                { current_track_synch = 0; }
                else
                { current_track_synch = (uint)(Next_Track + 1); }
            }
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Next_Net");
        }
        else
        { SendCustomEvent("Next_Net"); }
    }

    public void Next_Net()
    {
        if (Next_Track + 1 >= Audio_Clips.Length)
        { Next_Track = 0; }
        else
        { Next_Track++; }
        set_text(Next_Track);
        if (Play_on_track_change)
        {
            set_track(Next_Track);
            SendCustomEvent("Play");
        }
    }

    public void Prev()
    {
        if (Global_Synched)
        {
            if (Late_Join_Synched)
            {
                if (Next_Track - 1 < 0)
                { current_track_synch = (uint)(Audio_Clips.Length - 1); }
                else
                { current_track_synch = (uint)(Next_Track - 1); }
            }
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Prev_Net");
        }
        else
        { SendCustomEvent("Prev_Net"); }
    }

    public void Prev_Net()
    {
        if (Next_Track - 1 < 0)
        { Next_Track = (uint)(Audio_Clips.Length - 1); }
        else
        { Next_Track--; }
        set_text(Next_Track);
        if (Play_on_track_change)
        {
            set_track(Next_Track);
            SendCustomEvent("Play");
        }
    }

    public void Forward()
    {
        if (Global_Synched)
         { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Forward_Net"); }
        else
         { SendCustomEvent("Forward_Net"); }
    }

    public void Forward_Net()
    {
        Set_Time(Audio_Sources[0].time + Time_Forward_Reverce);
    }

    public void Reverce()
    {
        if (Global_Synched)
        { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Reverce_Net"); }
        else
        { SendCustomEvent("Reverce_Net"); }
    }

    public void Reverce_Net()
    {
        Set_Time(Audio_Sources[0].time - Time_Forward_Reverce);
    }

    public void Toggle_Play()
    {
        if (Audio_Sources[0].isPlaying)
        {
            if (Global_Synched)
            {
                if (Late_Join_Synched)
                { current_track_synch = Next_Track; }
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Stop");
            }
            else
            { SendCustomEvent("Stop"); }
        }
        else
        {
            if (Global_Synched)
            {
                if (Late_Join_Synched)
                { current_track_synch = Next_Track; }
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Play");
            }
            else
            { SendCustomEvent("Play"); }
        }
    } 

    public void Play()
    {
        set_text(Next_Track);
        set_track(Next_Track);
        for (uint i = 0; i < Audio_Sources.Length; i++)
        { if (Audio_Sources[i] != null && Audio_Sources[i].clip != null) { Set_Time(0); Audio_Sources[i].Play(); } }
        paused = false;
        playing = true;
    }

    public void Stop()
    {
        for (uint i = 0; i < Audio_Sources.Length; i++)
        { if (Audio_Sources[i] != null && Audio_Sources[i].clip != null) { Audio_Sources[i].Stop(); } }
        playing = false;
    }

    public void Toggle_Pause()
    {
        if (paused)
        {
            if (Global_Synched)
            {
                if (Late_Join_Synched)
                { current_track_synch = Next_Track; }
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Un_Pause");
            }
            else
            { SendCustomEvent("Un_Pause"); }
        }
        else
        {
            if (Global_Synched)
            {
                if (Late_Join_Synched)
                { current_track_synch = Next_Track; }
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Pause");
            }
            else
            { SendCustomEvent("Pause"); }
        }
    }

    public void Pause()
    {
        paused = true;
        for (uint i = 0; i < Audio_Sources.Length; i++)
        { if (Audio_Sources[i] != null && Audio_Sources[i].clip != null) { Audio_Sources[i].Pause(); } }
    }

    public void Un_Pause()
    {
        paused = false;
        for (uint i = 0; i < Audio_Sources.Length; i++)
        { if (Audio_Sources[i] != null && Audio_Sources[i].clip != null) { Audio_Sources[i].UnPause(); } }
    }

    public void Toggle_Mute()
    {
        if (Audio_Sources[0].mute)
        {
            if (Global_Synched)
            {
                if (Late_Join_Synched)
                { current_track_synch = Next_Track; }
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Un_Mute");
            }
            else
            { SendCustomEvent("Un_Mute"); }
        }
        else
        {
            if (Global_Synched)
            {
                if (Late_Join_Synched)
                { current_track_synch = Next_Track; }
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Mute");
            }
            else
            { SendCustomEvent("Mute"); }
        }
    }

    public void Mute()
    {
        for (uint i = 0; i < Audio_Sources.Length; i++)
        { if (Audio_Sources[i] != null && Audio_Sources[i].clip != null) { Audio_Sources[i].mute=true; } }
    }

    public void Un_Mute()
    {
        for (uint i = 0; i < Audio_Sources.Length; i++)
        { if (Audio_Sources[i] != null && Audio_Sources[i].clip != null) { Audio_Sources[i].mute = false; } }
    }
}

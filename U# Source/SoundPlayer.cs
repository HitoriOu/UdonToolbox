
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolboxV2
{
    /// <summary>
    /// SoundPlayer
    /// Music/sound player
    /// Created by Hitori Ou
    /// Last edit: 1-11-2020 Version 2.3
    /// 
    /// Main features:
    /// Playback of sounds selected from a list of sound files
    /// playback using loop, list loop and random selection from list or end after played if nothing selected
    /// playback on track change or queue manual track selection for next to play
    /// use of multiple audio/sound sources
    /// display of track name on multiple UI displays
    /// use of file name for text display of track or use of manual track names without file name change (Override_Audio_Clip_text)
    /// forward and reverce time skip with variable for how much to skip
    /// see function lists for full list of buttons/options
    /// Most functions are synched using "Audio_Sources Element 0"
    ///  
    /// List of playback functions:
    /// "Toggle_Play" Main play/Stop button
    /// "Toggle_Pause" main pause/un-pause
    /// "Next" sets next track
    /// "Prev" Sets previous track
    /// "Forward" skips forward in time
    /// "Reverce" skips backwards in time
    ///  
    /// List of Options functions:
    /// "VolumeUpdate" used to update sound sources after "Volume" variable been changed
    /// "TogglePlayOnTrackChange"
    /// "ToggleLoop"
    /// "ToggleLoopAll"
    /// "ToggleRandom"
    /// "ToggleMute" Main mute/un-mute
    /// </summary>
    public class SoundPlayer : UdonSharpBehaviour
    {
        private float timer_mem = 0;
        private float timer_update_rate = 1;

        /*time synching*/
        private bool time_requested = false;
        [UdonSynced(UdonSyncMode.None)]
        private float current_time_synch = 0;
        private float old_time_synch = 0;
        private float expected_Latency = 0.2f + 0.25f;

        /*track variable & sycnhing*/
        private uint Next_Track = 0; /*not public due to compatibillity issues*/
        [UdonSynced(UdonSyncMode.None)]
        private uint current_track_synch = 0;
        private uint current_track_synch_MEM = 0;

        /*status variables*/
        private bool playing = false;
        private bool Paused = false;
        private bool skip_loop = false; /*used to disable loop behaviour when changing track in loop mode*/
        private bool Repeat = false;

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
        public bool Muted = false;
        [Tooltip("How many seconds Forward/Reverce events skips")]
        public float Time_Forward_Reverce = 10f;

        [Header("Player & Tracks Setup")]
        [Tooltip("Audio players\r\n(what is used to play sound)")]
        public AudioSource[] Audio_Sources = new AudioSource[1];
        [Tooltip("Audio clips\r\n(what sound files are played)")]
        public AudioClip[] Audio_Clips = new AudioClip[1];
        [Tooltip("Text override that replaces specified elements with custom text for tracks/Audio_Clips")]
        public string[] Override_Audio_Clip_text = new string[0];


        #region UiSetup
        [Header("UI Setup (optional)")]
        [Tooltip("Text fields to put the track name in")]
        public UnityEngine.UI.Text[] UiAudioClipText;
        [Tooltip("When enabled the program prioritizes Ui values for settings.")]
        public bool EnableUiSettings = true;
        [Tooltip("UI interface (Controlls AutoPlay when calling the function named ''TogglePlayOnTrackChange'')")]
        public UnityEngine.UI.Toggle UiPlayOnTrackChange;
        [Tooltip("UI interface (Controlls Volume when calling the function named ''UpdateVolume'')")]
        public Slider UiVolumeSlider;
        [Tooltip("UI interface (Controlls Mute when calling the function named ''ToggleMute'')")]
        public UnityEngine.UI.Toggle UiMute;
        [Tooltip("UI interface (Controlls Loop when calling the function named ''ToggleLoop'')")]
        public UnityEngine.UI.Toggle UiLoop;
        [Tooltip("UI interface (Controlls Loop when calling the function named ''ToggleLoopAll'')")]
        public UnityEngine.UI.Toggle UiLoopAll;
        [Tooltip("UI interface (Controlls Loop when calling the function named ''ToggleRandom'')")]
        public UnityEngine.UI.Toggle UiRandom;
        #endregion

        [Header("Synching")]
        [Tooltip("All players in world are affected.")]
        public bool Global_Synched = true;
        [Tooltip("Players who join will hear what others hear. \r\n(If set to Global_Synched)")]
        public bool Late_Join_Synched = true;

        void Start()
        {
            if (Audio_Sources[0] == null) { Debug.LogWarning("Udon Toolbox: Warning! Param/variable ''Audio_Sources'' cannot have 'Element 0' empty (mandatory)!", this.gameObject); }

            if (Networking.LocalPlayer == null)
            { Global_Synched = false; }



            if (Global_Synched && Late_Join_Synched && Networking.LocalPlayer != null && !Networking.LocalPlayer.IsOwner(this.gameObject))
            {
                NetworkUpdateSettings();
                Next_Track = current_track_synch;
                current_track_synch_MEM = current_track_synch;
                SendCustomEvent("synch_time");
            }
            else
            {
                set_volume(Volume);
                SetPlayOnTrackChange(Play_on_track_change);
                SetMute(Muted);
                SetLoop(Loop);
                SetLoopAll(Loop_Play_ALL);
                SetRandom(Random);
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

        public void UpdateSettings()
        {
            if(Global_Synched)
            {
                if(Play_on_track_change)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "NetworkSetPlayOnTrackChangeOn");
                }
                else
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "NetworkSetPlayOnTrackChangeOff");
                }

                if (Muted)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "NetworkSetMuteOn");
                }
                else
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "NetworkSetMuteOff");
                }

                if (Loop)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "NetworkSetLoopOn");
                }
                else
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "NetworkSetLoopOn");
                }

                if (Loop_Play_ALL)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "NetworkSetLoopAllOn");
                }
                else
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "NetworkSetLoopAllOff");
                }

                if (Random)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "NetworkSetRandomOn");
                }
                else
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "NetworkSetRandomOff");
                }
            }
            else
            {
                SetPlayOnTrackChange(Play_on_track_change);
                SetMute(Muted);
                SetLoop(Loop);
                SetLoopAll(Loop_Play_ALL);
                SetRandom(Random);
            }
        }

        public void NetworkUpdateSettings()
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "UpdateSettings");
        }

        public override void Interact()
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
            { current_time_synch = Audio_Sources[0].time + expected_Latency; }
        }

        public void VolumeUpdate()
        {
            if(UiVolumeSlider != null && EnableUiSettings)
            {
                Volume = UiVolumeSlider.value;
            }
            if (Audio_Sources[0].volume != Volume)
            { set_volume(Volume); }
        }

        // Old depricated code.
        public void Volume_Update()
        {
            VolumeUpdate();
        }

        public void LateUpdate()
        {
            if (timer_mem <= Time.time)
            {
                timer_mem = Time.time + timer_update_rate;

                if(current_track_synch != current_track_synch_MEM && current_track_synch != Next_Track)
                {
                    current_track_synch_MEM = current_track_synch;
                    Next_Track = current_track_synch;
                    // set_track(current_track_synch);
                    // Play();
                }

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
                            for (uint i = 0; i < Audio_Sources.Length; i++)
                            {
                                if (Audio_Sources[i] != null)
                                {
                                    Audio_Sources[i].loop = false;
                                }
                            }
                        }
                    }
                    else if (Loop != Audio_Sources[0].loop)
                    {
                        if (Loop)
                        {
                            SetLoop(true);
                        }
                        else
                        {
                            SetLoop(false);
                        }
                    }
                }
                else if (!playing || Paused || skip_loop)/*if  not playing, paused or skipping loop for custom track selection*/
                {
                    if (!Paused && skip_loop)
                    {
                        SetLoop(true);
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
                                {
                                    if (Audio_Sources[0].clip == Audio_Clips[Next_Track])
                                    {
                                        Next_Track = (uint)UnityEngine.Random.Range((int)0, (int)Audio_Clips.Length);
                                    }
                                    if(current_track_synch == Next_Track)
                                    {
                                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RepeatTrack"); 
                                        Repeat = false;
                                    }
                                    current_track_synch = Next_Track;
                                }
                                else if (Audio_Sources[0].clip == Audio_Clips[current_track_synch] && Repeat)
                                {
                                    Repeat = false;
                                    Play();
                                }
                                else if(Audio_Sources[0].clip != Audio_Clips[current_track_synch])
                                {
                                    Next_Track = current_track_synch;
                                    current_track_synch_MEM = current_track_synch;
                                    Play();
                                }
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
                    {
                        SetLoop(true);
                    }
                }
            }
        }

        public void RepeatTrack()
        {
            Repeat = true;
        }

        private void set_text(uint number)
        {
            if (Override_Audio_Clip_text.Length > number && Override_Audio_Clip_text[number] != null && Override_Audio_Clip_text[number].Length > 0)
            {
                for (uint i = 0; i < UiAudioClipText.Length; i++)
                { UiAudioClipText[i].text = Override_Audio_Clip_text[number]; }
            }
            else if (Audio_Clips.Length > number)
            {
                string text = " (UnityEngine.AudioClip)";/*default text to remove*/
                if (Audio_Clips[number].ToString().EndsWith(text))
                {
                    text = Audio_Clips[number].ToString();
                    text = text.Remove(text.Length - 24);
                }
                else
                { text = Audio_Clips[number].ToString(); }

                for (uint i = 0; i < UiAudioClipText.Length; i++)
                { UiAudioClipText[i].text = text; }
            }
        }



        private void set_volume(float vol)
        {
            for (uint i = 0; i < Audio_Sources.Length; i++)
            { if (Audio_Sources[i] != null) { Audio_Sources[i].volume = vol; } }
        }

        private void Set_Time(float time)
        {
            for (uint i = 0; i < Audio_Sources.Length; i++)
            {
                if (Audio_Sources[i] != null)
                {
                    if(time < 0)
                    {
                        Audio_Sources[i].time = 0;
                    }
                    else if(time > Audio_Sources[i].clip.length)
                    {
                        Audio_Sources[i].time = Audio_Sources[i].clip.length-0.001f;
                    }
                    else
                    {
                        Audio_Sources[i].time = time;
                    }
                }
            }
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

            if (Global_Synched)
            {
              //  current_track_synch = Next_Track;
            }

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

            if (Global_Synched)
            {
              //  current_track_synch = Next_Track;
            }

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
            Paused = false;
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
            if (Paused)
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
            Paused = true;
            for (uint i = 0; i < Audio_Sources.Length; i++)
            { if (Audio_Sources[i] != null && Audio_Sources[i].clip != null) { Audio_Sources[i].Pause(); } }
        }

        public void Un_Pause()
        {
            Paused = false;
            for (uint i = 0; i < Audio_Sources.Length; i++)
            { if (Audio_Sources[i] != null && Audio_Sources[i].clip != null) { Audio_Sources[i].UnPause(); } }
        }

        #region ToggleMute
        // Old depricated code.
        public void Toggle_Mute()
        {
            ToggleMute();
        }

        public void ToggleMute()
        {
            if (UiMute != null && EnableUiSettings && UiMute.isOn == Muted)
            {
                return;
            }
            //Set Flag
            if (UiMute != null && EnableUiSettings)
            {
                Muted = UiMute.isOn;
            }
            else
            {
                Muted = !Muted;
            }

            // Decide Global vs local
            if (Global_Synched)
            {
                // On vs Off
                if (Muted)
                {
                    if (Late_Join_Synched)
                    { current_track_synch = Next_Track; }
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "NetworkSetMuteOn");
                }
                else
                {
                    if (Late_Join_Synched)
                    { current_track_synch = Next_Track; }
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "NetworkSetMuteOff");
                }
            }
            else
            {
                // On vs Off
                if (Muted)
                {
                    SetMute(true);
                }
                else
                {
                    SetMute(false);
                }
            }
        }

        private void SetMute(bool value)
        {
            Muted = value;
            if (UiMute != null)
            {
                UiMute.isOn = value;
            }

            for (uint i = 0; i < Audio_Sources.Length; i++)
            {
                if (Audio_Sources[i] != null && Audio_Sources[i].clip != null)
                {
                    Audio_Sources[i].mute = value;
                }
            }
            if (value)
            {
                // Turn on/enable
            }
            else
            {
                // Turn off/enable
            }
        }

        public void NetworkSetMuteOn()
        {
            if (Global_Synched)
            {
                SetMute(true);
            }
        }

        public void NetworkSetMuteOff()
        {
            if (Global_Synched)
            {
                SetMute(false);
            }
        }
        #endregion

        #region ToggleRandom
        public void ToggleRandom()
        {
            if (UiRandom != null && EnableUiSettings && UiRandom.isOn == Random)
            {
                return;
            }
            //Set Flag
            if (UiRandom != null && EnableUiSettings)
            {
                Random = UiRandom.isOn;
            }
            else
            {
                Random = !Random;
            }

            // Decide Global vs local
            if (Global_Synched)
            {
                // On vs Off
                if (Random)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "NetworkSetRandomOn");
                }
                else
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "NetworkSetRandomOff");
                }
            }
            else
            {
                // On vs Off
                if (Random)
                {
                    SetRandom(true);
                }
                else
                {
                    SetRandom(false);
                }
            }
        }

        private void SetRandom(bool value)
        {
            Random = value;
            if (UiRandom != null)
            {
                UiRandom.isOn = value;
            }
            if (value)
            {
                // Turn on/enable
            }
            else
            {
                // Turn off/enable
            }
        }

        public void NetworkSetRandomOn()
        {
            if (Global_Synched)
            {
                SetRandom(true);
            }
        }

        public void NetworkSetRandomOff()
        {
            if (Global_Synched)
            {
                SetRandom(false);
            }
        }
        #endregion

        #region TogglePlayOnTrackChange
        public void TogglePlayOnTrackChange()
        {
            if (UiPlayOnTrackChange != null && EnableUiSettings && UiPlayOnTrackChange.isOn == Play_on_track_change)
            {
                return;
            }
            //Set Flag
            if (UiPlayOnTrackChange != null && EnableUiSettings)
            {
                Play_on_track_change = UiPlayOnTrackChange.isOn;
            }
            else
            {
                Play_on_track_change = !Play_on_track_change;
            }

            // Decide Global vs local
            if (Global_Synched)
            {
                // On vs Off
                if (Play_on_track_change)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "NetworkSetPlayOnTrackChangeOn");
                }
                else
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "NetworkSetPlayOnTrackChangeOff");
                }
            }
            else
            {
                // On vs Off
                if (Play_on_track_change)
                {
                    SetPlayOnTrackChange(true);
                }
                else
                {
                    SetPlayOnTrackChange(false);
                }
            }
        }

        private void SetPlayOnTrackChange(bool value)
        {
            Play_on_track_change = value;
            if (UiPlayOnTrackChange != null)
            {
                UiPlayOnTrackChange.isOn = value;
            }
            if (value)
            {
                // Turn on/enable
            }
            else
            {
                // Turn off/enable
            }
        }

        public void NetworkSetPlayOnTrackChangeOn()
        {
            if (Global_Synched)
            {
                SetPlayOnTrackChange(true);
            }
        }

        public void NetworkSetPlayOnTrackChangeOff()
        {
            if (Global_Synched)
            {
                SetPlayOnTrackChange(false);
            }
        }
        #endregion

        #region ToggleLoop
        // old depricated code
        public void toggle_Loop()
        {
            ToggleLoop();
        }

        public void ToggleLoop()
        {
            if (UiLoop != null && EnableUiSettings && UiLoop.isOn == Loop)
            {
                return;
            }
            //Set Flag
            if (UiLoop != null && EnableUiSettings)
            {
                Loop = UiLoop.isOn;
            }       
            else
            {
                Loop = !Loop;
            }

            if (Global_Synched)
            {
                if (Loop)
                {
                   // NetworkSetLoopOff();
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "NetworkSetLoopOn");
                }
                else
                {
                   // NetworkSetLoopOn();
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "NetworkSetLoopOff");
                }
            }
            else
            {
                if (Loop)
                {
                    SetLoop(true);
                }
                else
                {
                    SetLoop(false);
                }
            }
        }

        private void SetLoop(bool value)
        {
            Loop = value;
            if (UiLoop != null)
            {
                UiLoop.isOn = value;
            }
            // Turn on/enable
            for (uint i = 0; i < Audio_Sources.Length; i++)
            {
                if (Audio_Sources[i] != null)
                {
                    Audio_Sources[i].loop = value;
                }
            }
            if (value)
            {
            }
            else
            {
            }
        }

        public void NetworkSetLoopOn()
        {
            if (Global_Synched)
            {
                SetLoop(true);
            }
        }

        public void NetworkSetLoopOff()
        {
            if (Global_Synched)
            {
                SetLoop(false);
            }
        }
        #endregion

        #region ToggleLoopAll
        public void ToggleLoopAll()
        {
            if (UiLoopAll != null && EnableUiSettings && UiLoopAll.isOn == Loop_Play_ALL)
            {
                return;
            }
            //Set Flag
            if (UiLoopAll != null && EnableUiSettings)
            {
                Loop_Play_ALL = UiLoopAll.isOn;
            }
            else
            {
                Loop_Play_ALL = !Loop_Play_ALL;
            }

            // Decide Global vs local
            if (Global_Synched)
            {
                // On vs Off
                if (Loop_Play_ALL)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "NetworkSetLoopAllOn");
                }
                else
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "NetworkSetLoopAllOff");
                }
            }
            else
            {
                // On vs Off
                if (Loop_Play_ALL)
                {
                    SetLoopAll(true);
                }
                else
                {
                    SetLoopAll(false);
                }
            }
        }

        private void SetLoopAll(bool value)
        {
            Loop_Play_ALL = value;
            if (UiLoopAll != null)
            {
                UiLoopAll.isOn = value;
            }
            if (value)
            {
                // Turn on/enable
            }
            else
            {
                // Turn off/enable
            }
        }

        public void NetworkSetLoopAllOn()
        {
            if (Global_Synched)
            {
                SetLoopAll(true);
            }
        }

        public void NetworkSetLoopAllOff()
        {
            if (Global_Synched)
            {
                SetLoopAll(false);
            }
        }
        #endregion
    }
}

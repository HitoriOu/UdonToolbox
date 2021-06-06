//Feature not available in udon yet (NotExposedInUdon).
#define NotExposedInUdon
#undef NotExposedInUdon

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolboxV2
{
    /// <summary>
    /// AudioSourceParamTester
    /// Testing tool for audio sources.
    /// Created by Hitori Ou
    /// Last edit: 21-12-2020 Version 2.4
    /// 
    /// Does not fully support use of Volume Rolloff custom curves, but should permitt one that is pre-set.
    /// The input fields min & max values are regulated by the sliders, change the slider setting to change the range of the input field code.
    /// 
    /// Usable Functions:
    /// SaveSettings
    /// LoadSettings
    /// Play
    /// Pause
    /// Stop
    /// Next
    /// Forward
    /// Reverce
    /// </summary>
    public class AudioSourceTestingTool : UdonSharpBehaviour
    {
        #region SaveLoadValues
        private bool SavedMute = false;
      // private bool SavedBypassListenerEffects = false;
        private bool SavedBypassReverbZones = false;
        private bool SavedPlayOnAwake = false;
        private bool SavedLoop = false;

        private int SavedPriority = 128;
        private float SavedVolume = 0.5f;
        private float SavedPitch = 1f;
        private float SavedStereoPan = 0;

        private float SavedSpatialBlend = 0;
        private float SavedReverbZoneMix = 1;
        private float SavedDopplerLevel = 1;
        private float SavedSpread = 0;

        private AudioRolloffMode SavedVolumeRolloff = AudioRolloffMode.Linear;
        private float SavedMinDistance = 1;
        private float SavedMaxDistance = 500;

#if NotExposedInUdon
        private float SavedGain = 10;
        private float SavedFar = 40;
        private float SavedNear = 0;
        private float SavedVolumetricRadius = 0;
        private bool SavedEnableSpatialization = true;
        private bool SavedAudioSourceVolumeCurve = false;
#endif
        #endregion

        private uint NextTrack = 0; /*not public due to compatibillity issues*/
        private bool Paused = true;
        // Used to disable Ui Update triggers.
        private bool UiEnabled = false;

        #region PublicVariables
        public AudioSource[] AudioSources;
        [Tooltip("If checked in the script settings overwrite the AudioSource settings, else it uses the settings from AudioSources element:0")]
        public bool OverrideAudioSource = true;
        public AudioClip[] Audioclips;
        ///public UnityEngine.audiomixer

        [Header("Audio Source options")]
        [Tooltip("Used on start if 'OverrideAudioSource' is selected.")]
        public bool Mute = false;
        // public bool BypassListenerEffects = false;
        [Tooltip("Used on start if 'OverrideAudioSource' is selected.")]
        public bool BypassReverbZones = false;
        [Tooltip("Used on start if 'OverrideAudioSource' is selected.")]
        public bool PlayOnAwake = true;
        [Tooltip("Used on start if 'OverrideAudioSource' is selected.")]
        public bool Loop = false;

        [Tooltip("Used on start if 'OverrideAudioSource' is selected.")]
        public int Priority = 128;
        [Tooltip("Used on start if 'OverrideAudioSource' is selected.")]
        public float Volume = 0.5f;
        [Tooltip("Used on start if 'OverrideAudioSource' is selected.")]
        public float Pitch = 1f;
        [Tooltip("Used on start if 'OverrideAudioSource' is selected.")]
        public float StereoPan = 0;

        public float SpatialBlend = 0;
        [Tooltip("Used on start if 'OverrideAudioSource' is selected.")]
        public float ReverbZoneMix = 1;
        [Tooltip("Used on start if 'OverrideAudioSource' is selected.")]
        public float DopplerLevel = 1;
        [Tooltip("Used on start if 'OverrideAudioSource' is selected.")]
        public float Spread = 0;

        [Tooltip("Used on start if 'OverrideAudioSource' is selected.")]
        public AudioRolloffMode VolumeRolloff = AudioRolloffMode.Linear;
        [Tooltip("Used on start if 'OverrideAudioSource' is selected.")]
        public float MinDistance = 1;
        [Tooltip("Used on start if 'OverrideAudioSource' is selected.")]
        public float MaxDistance = 500;

#if NotExposedInUdon
        [Header("Vrc Spatial Audio Source options")]
        public float Gain = 10;
        public float Far = 40;
        public float Near = 0;
        public float VolumetricRadius = 0;
        public bool EnableSpatialization = true;
        public bool AudioSourceVolumeCurve = false;
#endif

        [Header("Ui Toggle Boxes")]
        public UnityEngine.UI.Toggle UiMute;
        // public UnityEngine.UI.Toggle UiBypassListenerEffects;
        public UnityEngine.UI.Toggle UiBypassReverbZones;
        public UnityEngine.UI.Toggle UiPlayOnAwake;
        public UnityEngine.UI.Toggle UiLoop;

#if NotExposedInUdon
        public UnityEngine.UI.Toggle UiEnableSpatialization;
        public UnityEngine.UI.Toggle UiAudioSourceVolumeCurve;
#endif

        [Header("UI Sliders")]
        public UnityEngine.UI.Slider UiSliderPriority;
        public UnityEngine.UI.Slider UiSliderVolume;
        public UnityEngine.UI.Slider UiSliderPitch;
        public UnityEngine.UI.Slider UiSliderStereoPan;
        public UnityEngine.UI.Slider UiSliderSpatialBlend;
        public UnityEngine.UI.Slider UiSliderReverbZoneMix;
        public UnityEngine.UI.Slider UiSliderDopplerLevel;
        public UnityEngine.UI.Slider UiSliderSpread;
        public UnityEngine.UI.Slider UiSliderMinDistance;
        public UnityEngine.UI.Slider UiSliderMaxDistance;

#if NotExposedInUdon
        public UnityEngine.UI.Slider UiSliderGain;
        public UnityEngine.UI.Slider UiSliderFar;
        public UnityEngine.UI.Slider UiSliderNear;
        public UnityEngine.UI.Slider UiSliderVolumetricRadius;
#endif

        [Header("UI InputFields")]
        public UnityEngine.UI.InputField UiInputFieldPriority;
        public UnityEngine.UI.InputField UiInputFieldVolume;
        public UnityEngine.UI.InputField UiInputFieldPitch;
        public UnityEngine.UI.InputField UiInputFieldStereoPan;
        public UnityEngine.UI.InputField UiInputFieldSpatialBlend;
        public UnityEngine.UI.InputField UiInputFieldReverbZoneMix;
        public UnityEngine.UI.InputField UiInputFieldDopplerLevel;
        public UnityEngine.UI.InputField UiInputFieldSpread;
        public UnityEngine.UI.InputField UiInputFieldMinDistance;
        public UnityEngine.UI.InputField UiInputFieldMaxDistance;

#if NotExposedInUdon
        public UnityEngine.UI.InputField UiInputFieldGain;
        public UnityEngine.UI.InputField UiInputFieldFar;
        public UnityEngine.UI.InputField UiInputFieldNear;
        public UnityEngine.UI.InputField UiInputFieldVolumetricRadius;
#endif

        [Header("UI Other")]
        public UnityEngine.UI.Dropdown UIDropdownVolumeRolloff;
#endregion

        public void Start()
        {
            if(OverrideAudioSource)
            {
                SaveSettings();
                UseScriptSettings();
                UiRefresh();
            }
            else
            {
                SaveSettingsFromComponent();
                LoadSettings();
            }
            UiEnabled = true;
        }

#region LoadSave
        /// <summary>
        /// Load saved settings into local variables.
        /// </summary>
        public void LoadSettings()
        {
            Mute = SavedMute;
            // BypassListenerEffects = SavedBypassListenerEffects;
            BypassReverbZones = SavedBypassReverbZones;
            PlayOnAwake = SavedPlayOnAwake;
            Loop = SavedLoop;

            Priority = SavedPriority;
            Volume = SavedVolume;
            Pitch = SavedPitch;
            StereoPan = SavedStereoPan;

            SpatialBlend = SavedSpatialBlend;
            ReverbZoneMix = SavedReverbZoneMix;
            DopplerLevel = SavedDopplerLevel;
            Spread = SavedSpread;

            VolumeRolloff = SavedVolumeRolloff;

            MinDistance = SavedMinDistance;
            MaxDistance = SavedMaxDistance;

#if NotExposedInUdon
            Gain = SavedGain;
            Far = SavedFar;
            Near = SavedNear;
            VolumetricRadius = SavedVolumetricRadius;
            AudioSourceVolumeCurve = SavedAudioSourceVolumeCurve;
#endif

            UseScriptSettings();
            UiRefresh();
        }

        /// <summary>
        /// Saves the original settings from the component in element:0.
        /// </summary>
        public void SaveSettingsFromComponent()
        {
            SavedMute = AudioSources[0].mute;
           // SavedBypassListenerEffects = AudioSources[0].bypassListenerEffects;
            SavedBypassReverbZones = AudioSources[0].bypassReverbZones;
            SavedPlayOnAwake = AudioSources[0].playOnAwake;
            SavedLoop = AudioSources[0].loop;

            SavedPriority = AudioSources[0].priority;
            SavedVolume = AudioSources[0].volume;
            SavedPitch = AudioSources[0].pitch;
            SavedStereoPan = AudioSources[0].panStereo;

            SavedSpatialBlend = AudioSources[0].spatialBlend;
            SavedReverbZoneMix = AudioSources[0].reverbZoneMix;
            SavedDopplerLevel = AudioSources[0].dopplerLevel;
            SavedSpread = AudioSources[0].spread;

            SavedVolumeRolloff = AudioSources[0].rolloffMode;

            SavedMinDistance = AudioSources[0].minDistance;
            SavedMaxDistance = AudioSources[0].maxDistance;

#if NotExposedInUdon
            VRC_SpatialAudioSource temp = (VRC_SpatialAudioSource) AudioSources[0].gameObject.GetComponent(typeof(VRC_SpatialAudioSource));
            if(temp != null)
            {
                SavedGain = temp.Gain;
                SavedFar = temp.Far;
                SavedNear = temp.Near;
                SavedVolumetricRadius = temp.VolumetricRadius;
                SavedEnableSpatialization = temp.EnableSpatialization;
                SavedAudioSourceVolumeCurve = temp.UseAudioSourceVolumeCurve;
            }
#endif
        }

        /// <summary>
        /// Save local settings into saved profile.
        /// </summary>
        public void SaveSettings()
        {
            SavedMute = Mute;
            // SavedBypassListenerEffects = BypassListenerEffects;
            SavedBypassReverbZones = BypassReverbZones;
            SavedPlayOnAwake = PlayOnAwake;
            SavedLoop = Loop;

            SavedPriority = Priority;
            SavedVolume = Volume;
            SavedPitch = Pitch;
            SavedStereoPan = StereoPan;

            SavedSpatialBlend = SpatialBlend;
            SavedReverbZoneMix = ReverbZoneMix;
            SavedDopplerLevel = DopplerLevel;
            SavedSpread = Spread;

            SavedVolumeRolloff = VolumeRolloff;

            SavedMinDistance = MinDistance;
            SavedMaxDistance = MaxDistance;

#if NotExposedInUdon
            SavedGain = Gain;
            SavedFar = Far;
            SavedNear = Near;
            SavedVolumetricRadius = VolumetricRadius;
            SavedAudioSourceVolumeCurve = AudioSourceVolumeCurve;
#endif
        }

        private void UseScriptSettings()
        {
            for (int i = 0; i < AudioSources.Length; i++)
            {
                if (AudioSources[i] != null)
                {
                    AudioSources[i].mute = Mute;
                    // AudioSources[i].bypassListenerEffects = BypassListenerEffects;
                    AudioSources[i].bypassReverbZones = BypassReverbZones;
                    AudioSources[i].playOnAwake = PlayOnAwake;
                    AudioSources[i].loop = Loop;

                    AudioSources[i].priority = Priority;
                    AudioSources[i].volume = Volume;
                    AudioSources[i].pitch = Pitch;
                    AudioSources[i].panStereo = StereoPan;
                    AudioSources[i].reverbZoneMix = ReverbZoneMix;
                    AudioSources[i].spatialBlend = SpatialBlend;
                    AudioSources[i].dopplerLevel = DopplerLevel;
                    AudioSources[i].spread = Spread;

                    AudioSources[i].rolloffMode = VolumeRolloff;
                    AudioSources[i].minDistance = MinDistance;
                    AudioSources[i].maxDistance = MaxDistance;

#if NotExposedInUdon
                    VRC_SpatialAudioSource temp = (VRC_SpatialAudioSource)AudioSources[i].gameObject.GetComponent(typeof(VRC_SpatialAudioSource));
                    if (temp != null)
                    {
                        temp.Gain = Gain;
                        temp.Far = Far;
                        temp.Near = Near;
                        temp.VolumetricRadius = VolumetricRadius;
                        temp.EnableSpatialization = EnableSpatialization;
                        temp.UseAudioSourceVolumeCurve = AudioSourceVolumeCurve;
                    }
#endif
                }
            }
            UiRefresh();
        }
        #endregion

        #region UiFunctions
        /// <summary>
        /// Sets current settings onto the Ui.
        /// </summary>
        private void UiRefresh()
        {
            UiEnabled = false;
            UiMute.isOn = Mute;
            // UiBypassListenerEffects.isOn = BypassListenerEffects;
            UiBypassReverbZones.isOn = BypassReverbZones;
            UiPlayOnAwake.isOn = PlayOnAwake;
            UiLoop.isOn = Loop;

            SetUiSliderAndInputfield(UiSliderPriority, UiInputFieldPriority, Priority, "Priority=");
            SetUiSliderAndInputfield(UiSliderVolume, UiInputFieldVolume, Volume, "Volume=");
            SetUiSliderAndInputfield(UiSliderPitch, UiInputFieldPitch, Pitch, "Pitch=");
            SetUiSliderAndInputfield(UiSliderStereoPan, UiInputFieldStereoPan, StereoPan, "StereoPan=");
            SetUiSliderAndInputfield(UiSliderSpatialBlend, UiInputFieldSpatialBlend, SpatialBlend, "SpatialBlend=");
            SetUiSliderAndInputfield(UiSliderReverbZoneMix, UiInputFieldReverbZoneMix, ReverbZoneMix, "ReverbZoneMix=");
            SetUiSliderAndInputfield(UiSliderDopplerLevel, UiInputFieldDopplerLevel, DopplerLevel, "DopplerLevel=");
            SetUiSliderAndInputfield(UiSliderSpread, UiInputFieldSpread, Spread, "Spread=");

#if NotExposedInUdon
            UiEnableSpatialization.isOn = EnableSpatialization;
            UiAudioSourceVolumeCurve.isOn = AudioSourceVolumeCurve;

            SetUiSliderAndInputfield(UiSliderGain, UiInputFieldGain, Gain, "Gain=");
            SetUiSliderAndInputfield(UiSliderFar, UiInputFieldFar, Far, "Far=");
            SetUiSliderAndInputfield(UiSliderNear, UiInputFieldNear, Near, "Near=");
            SetUiSliderAndInputfield(UiSliderVolumetricRadius, UiInputFieldVolumetricRadius, VolumetricRadius, "VolumetricRadius=");
#endif

            UIDropdownVolumeRolloff.value = (int) VolumeRolloff;

        SetUiSliderAndInputfield(UiSliderMinDistance, UiInputFieldMinDistance, MinDistance, "MinDistance=");
        SetUiSliderAndInputfield(UiSliderMaxDistance, UiInputFieldMaxDistance, MaxDistance, "MaxDistance=");

        UiEnabled = true;
    }

    private void SetUiSliderAndInputfield(UnityEngine.UI.Slider slider, UnityEngine.UI.InputField inputField, float currentValue, string inputFieldText)
    {
        if (slider != null)
        {
            slider.value = currentValue;
        }
        if (inputField != null)
        {
            inputField.text = inputFieldText + currentValue.ToString();
        }
    }

    private float GetSliderAndInputFieldUpdate(UnityEngine.UI.Slider slider, UnityEngine.UI.InputField inputField, float currentValue,string inputFieldText)
    {
        if (slider != null && currentValue != slider.value)
        {
            if (inputField != null)
            { inputField.text = inputFieldText + slider.value.ToString(); }
            return slider.value;
        }
        else if (inputField != null && !inputField.text.StartsWith(inputFieldText))
        {
            float temp = -1;
            if (float.TryParse(inputField.text, out temp))
            {
                if (slider != null)
                {
                    if(temp < slider.minValue)
                    {
                        temp = slider.minValue;
                    }
                    else if(temp > slider.maxValue)
                    {
                        temp = slider.maxValue;
                    }
                    else
                    {
                        slider.value = temp;
                    }
                }

                if (inputField != null)
                { inputField.text = inputFieldText + temp.ToString(); }

                return temp;
            }
            else if (!inputField.text.StartsWith(inputFieldText))
            {
                inputField.text = "Input error: using " + currentValue.ToString() + " instead.";
                return currentValue;
            }
        }
        if(slider == null && inputField == null)
        {
            Debug.LogError("AudioSourceParamTester error: Failed to detect Slider or InputFields for -->" + inputFieldText + " Related setting will default to the value " + currentValue.ToString());
        }
        return currentValue;
    }

    public void CheckUi()
    {
        if(UiEnabled)
        {
            if (UiMute != null)
            {
                Mute = UiMute.isOn;
            }
            // if (UiBypassListenerEffects != null)
            {
                // BypassListenerEffects = UiBypassListenerEffects.isOn;
            }
            if (UiBypassReverbZones != null)
            {
                BypassReverbZones = UiBypassReverbZones.isOn;
            }
            if (UiPlayOnAwake != null)
            {
                PlayOnAwake = UiPlayOnAwake.isOn;
            }
            if (UiLoop != null)
            {
                Loop = UiLoop.isOn;
            }

            Priority = (int)GetSliderAndInputFieldUpdate(UiSliderPriority, UiInputFieldPriority, Priority, "Priority=");
            Volume = GetSliderAndInputFieldUpdate(UiSliderVolume, UiInputFieldVolume, Volume, "Volume=");
            Pitch = GetSliderAndInputFieldUpdate(UiSliderPitch, UiInputFieldPitch, Pitch, "Pitch=");
            StereoPan = GetSliderAndInputFieldUpdate(UiSliderStereoPan, UiInputFieldStereoPan, StereoPan, "StereoPan=");
            SpatialBlend = GetSliderAndInputFieldUpdate(UiSliderSpatialBlend, UiInputFieldSpatialBlend, SpatialBlend, "SpatialBlend=");
            ReverbZoneMix = GetSliderAndInputFieldUpdate(UiSliderReverbZoneMix, UiInputFieldReverbZoneMix, ReverbZoneMix, "ReverbZoneMix=");
            DopplerLevel = GetSliderAndInputFieldUpdate(UiSliderDopplerLevel, UiInputFieldDopplerLevel, DopplerLevel, "DopplerLevel=");
            Spread = GetSliderAndInputFieldUpdate(UiSliderSpread, UiInputFieldSpread, Spread, "Spread=");

            if (UIDropdownVolumeRolloff != null)
            {
                switch (UIDropdownVolumeRolloff.value)
                {
                    case 0:
                        VolumeRolloff = AudioRolloffMode.Logarithmic;
                        break;
                    case 1:
                        VolumeRolloff = AudioRolloffMode.Linear;
                        break;
                    case 2:
                        VolumeRolloff = AudioRolloffMode.Custom;
                        break;
                };
            }
            MinDistance = GetSliderAndInputFieldUpdate(UiSliderMinDistance, UiInputFieldMinDistance, MinDistance, "MinDistance=");
            MaxDistance = GetSliderAndInputFieldUpdate(UiSliderMaxDistance, UiInputFieldMaxDistance, MaxDistance, "MaxDistance=");

#if NotExposedInUdon
            if (UiEnableSpatialization != null)
            {
                EnableSpatialization = UiEnableSpatialization.isOn;
            }
            if (UiAudioSourceVolumeCurve != null)
            {
                AudioSourceVolumeCurve = UiAudioSourceVolumeCurve.isOn;
            }

            Gain = GetSliderAndInputFieldUpdate(UiSliderGain, UiInputFieldGain, Gain, "Gain=");
            Far = GetSliderAndInputFieldUpdate(UiSliderFar, UiInputFieldFar, Far, "Far=");
            Near = GetSliderAndInputFieldUpdate(UiSliderNear, UiInputFieldNear, Near, "Near=");
            VolumetricRadius = GetSliderAndInputFieldUpdate(UiSliderVolumetricRadius, UiInputFieldVolumetricRadius, VolumetricRadius, "VolumetricRadius=");
#endif

            UseScriptSettings();
        }
    }
#endregion

#region PlayFunctions
        private void set_track(uint number)
        {
            for (uint i = 0; i < AudioSources.Length; i++)
            { if (AudioSources[i] != null && Audioclips[number] != null) { AudioSources[i].clip = Audioclips[number]; } }
        }

        public void Next()
        {
            if (NextTrack + 1 >= Audioclips.Length)
            { NextTrack = 0; }
            else
            { NextTrack++; }
            if (true)//Play_on_track_change)
            {
                set_track(NextTrack);
                Play();
            }
        }

        public void Forward()
        {
            SetTime(AudioSources[0].time + 20);
        }

        public void Reverce()
        {
            SetTime(AudioSources[0].time - 10);
        }

        public void Play()
        {
            set_track(NextTrack);
            for (uint i = 0; i < AudioSources.Length; i++)
            { if (AudioSources[i] != null && AudioSources[i].clip != null) { AudioSources[i].Play(); } }
            Paused = false;
        }

        public void Stop()
        {
            for (uint i = 0; i < AudioSources.Length; i++)
            { if (AudioSources[i] != null && AudioSources[i].clip != null) { AudioSources[i].Stop(); } }
        }
        public void Pause()
        {
            Paused = true;
            for (uint i = 0; i < AudioSources.Length; i++)
            { if (AudioSources[i] != null && AudioSources[i].clip != null) { AudioSources[i].Pause(); } }
        }
        /*
        public void Un_Pause()
        {
            Paused = false;
            for (uint i = 0; i < AudioSources.Length; i++)
            { if (AudioSources[i] != null && AudioSources[i].clip != null) { AudioSources[i].UnPause(); } }
        }
        */
        private void SetTime(float time)
        {
            for (uint i = 0; i < AudioSources.Length; i++)
            {
                if (AudioSources[i] != null)
                {
                    if (time < 0)
                    {
                        AudioSources[i].time = 0;
                    }
                    else if (time > AudioSources[i].clip.length)
                    {
                        AudioSources[i].time = AudioSources[i].clip.length - 0.001f;
                    }
                    else
                    {
                        AudioSources[i].time = time;
                    }
                }
            }
        }
#endregion
    }
}

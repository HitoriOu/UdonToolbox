
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
//using System.Collections;


namespace UdonToolboxV2
{
    /// <summary>
    /// LightController
    /// Used to change light settings (can use negative color values if so desired).
    /// Created by Hitori Ou
    /// Last edit: 20-01-2020 Version 2.4
    /// Usable functions:
    /// LightReset [Resets the light according to values saved on start]
    /// UiUpdate [Used by Ui components to check new values]
    /// UseScriptSetting [Used with interact (for example in editor playmode)]
    /// </summary>
    public class LightController : UdonSharpBehaviour
    {
        // Used to disable Ui Update triggers.
        private bool UiEnabled = false;

        private float Timer = 0;

        [UdonSynced(UdonSyncMode.None)]
        private float NetIntensity=0;

        [UdonSynced(UdonSyncMode.None)]
        private Color NetColor;

        private float SavedIntensity = 1;
        Color SavedColor;

        [Tooltip("Light sources this script affects (element:0 is mandatory)")]
        public UnityEngine.Light[] Lights;

        [Tooltip("If checked/true it will use the settings from Lights Element:0 (ignores the scripts intensity & color settings)")]
        public bool UseLightComponentSetting=true;

        [Tooltip("Intensity setting (how bright the light is)")]
        public float Intensity = 0.5f;

        [Tooltip("Assign RGB-A values (Red, Green, Blue, Alpha")]
        public Color Colors;

        [Header("RGB Value Override")]
        [Tooltip("Use RGB at specific values (allows using negative values)")]
        public bool UseOverrideValues = false;
        public float ColorR = 1;
        public float ColorG = 1;
        public float ColorB = 1;

        [Header("UI Sliders")]
        public UnityEngine.UI.Slider UiSliderIntensity;
        public UnityEngine.UI.Slider UiSliderColorR;
        public UnityEngine.UI.Slider UiSliderColorG;
        public UnityEngine.UI.Slider UiSliderColorB;


        [Header("Synching")]
        [Tooltip("All players in world are affected.")]
        public bool Global_Synched = true;

        [Tooltip("How often Global_Synched checks if the setting been altered.")]
        public float UpdateRate = 1;

        [Tooltip("Players who join will see what others see. \r\n(If set to Global_Synched)")]
        public bool Late_Join_Synched = true;
        
        public override void Interact() { SendCustomEvent("UseScriptSetting"); }

        /// <summary>
        /// Resets the light according to values saved on start.
        /// </summary>
        public void LightReset()
        {
            if (Global_Synched)
            {
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            }
            SetColors(SavedColor.r, SavedColor.g, SavedColor.b);
            SetIntensitys(SavedIntensity);
            if(Global_Synched && (Networking.LocalPlayer != null))
            {
                NetColor = SavedColor;
                NetIntensity = SavedIntensity;
            }
            RefreshUi();
        }

        private void SetColors(float red, float green, float blue)
        {
            if(Lights != null && Lights[0] != null)
            {
                Colors = (Color)Lights[0].color;
                Colors.r = red;
                Colors.g = green;
                Colors.b = blue;

                for (int i = 0; i < Lights.Length; i++)
                {
                    if(Lights[i] != null)
                    {
                        Lights[i].color = Colors;
                    }
                }
            }
        }

        private void SetIntensitys(float value)
        {
            for (int i = 0; i < Lights.Length; i++)
            {
                if (Lights[i] != null)
                {
                    Lights[i].intensity = value;
                }
            }
            Intensity = Lights[0].intensity;
        }

        void Start()
        {
            if(Networking.LocalPlayer == null)
            {
                Global_Synched = false;
            }

            if (Lights == null || Lights[0] == null)
            {
                Debug.LogError("LightController: Lights Element:0 not found (mandatory)",this);
            }

            if (UseLightComponentSetting)
            {
                if (Lights != null && Lights[0] != null)
                {
                    SetColors(Lights[0].color.r, Lights[0].color.g, Lights[0].color.b);
                    SetIntensitys(Lights[0].intensity);
                }
            }
            else if (UseOverrideValues)
            {
                SetColors(ColorR, ColorG, ColorB);
                SetIntensitys(Intensity);
            }
            else
            {
                SetColors(Colors.r, Colors.g, Colors.b);
                SetIntensitys(Intensity);
            }

            // Save values before synching.
            SavedColor = Colors;
            SavedIntensity = Intensity;

            if (Global_Synched && Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                NetColor = Colors;
                NetIntensity = Intensity;
            }
            else if(Global_Synched && Late_Join_Synched)
            {
                SetColors(NetColor.r, NetColor.g, NetColor.b);
                SetIntensitys(NetIntensity);
            }
            //Synch to network to avoid re-synch.
            else if(Global_Synched)
            {
                Colors = NetColor;
                Intensity = NetIntensity;
            }

            UiEnabled = true;
            RefreshUi();
        }

        /// <summary>
        /// Used with interact (for example in editor playmode).
        /// </summary>
        public void UseScriptSetting()
        {
            SetColors(ColorR, ColorG, ColorB);
            SetIntensitys(Intensity);
            RefreshUi();
            UiUpdate();
        }

        /// <summary>
        /// Used by Ui components to check new values.
        /// </summary>
        public void UiUpdate()
        {
            if(UiEnabled)
            {
                if (UiSliderColorR != null)
                {
                    Colors.r = UiSliderColorR.value;
                }
                if (UiSliderColorG != null)
                {
                    Colors.g = UiSliderColorG.value;
                }
                if (UiSliderColorB != null)
                {
                    Colors.b = UiSliderColorB.value;
                }

                if (UiSliderIntensity != null)
                {
                    Intensity = (float) UiSliderIntensity.value;
                }

                SetColors(Colors.r, Colors.g, Colors.b);
                SetIntensitys(Intensity);

                if (Global_Synched)
                {
                    if(!Networking.IsOwner(Networking.LocalPlayer,this.gameObject))
                    {
                        Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
                    }
                    NetIntensity = Intensity;
                    NetColor = Colors;
                }
            }
        }

        /// <summary>
        /// Sets current values into the Ui panel.
        /// </summary>
        private void RefreshUi()
        {
            UiEnabled = false;
            if (UiSliderColorR != null)
            {
                UiSliderColorR.value = Colors.r;
            }
            if (UiSliderColorG != null)
            {
                UiSliderColorG.value = Colors.g;
            }
            if (UiSliderColorB != null)
            {
                UiSliderColorB.value = Colors.b;
            }

            if (UiSliderIntensity != null)
            {
                UiSliderIntensity.value = Intensity;
            }
            UiEnabled = true;
        }

        /// <summary>
        /// Used for network Synching.
        /// </summary>
        public void LateUpdate()
        {
            if(Global_Synched && Timer < Time.time && (Colors != NetColor || Intensity != NetIntensity))
            {
                Timer = Time.time + UpdateRate;

                SetColors(NetColor.r, NetColor.g, NetColor.b);
                SetIntensitys(NetIntensity);
                RefreshUi();
            }
        }
    }
}
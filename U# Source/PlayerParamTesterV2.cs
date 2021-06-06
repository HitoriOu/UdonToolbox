#define DEPRICATED
#undef DEPRICATED

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolboxV2
{
    /// <summary>
    /// PlayerParamTesterV2
    /// Used for testrunning different movement settings when designing a world.
    /// Created by Hitori Ou
    /// Last edit: 5-04-2021 Version 2.4
    /// Usable functions:
    /// Reset [reverts to previous settings]
    /// RunTest [starts the test run & timer]
    /// RunTestPermanent [starts a non-timed test (use with caution)]
    /// </summary>
    public class PlayerParamTesterV2 : UdonSharpBehaviour
    {
#if DEPRICATED
        private float TimeMem = 0;
        private bool Running = false;
#endif

        private float ExpectedSliderWalk = -1;
        private float ExpectedSliderStrafe = -1;
        private float ExpectedSliderRun = -1;
        private float ExpectedSliderJump = -1;
        private float ExpectedSliderGrav = -1;

        [Header("Reset setup")]
        [Tooltip("Time untill settings are reset.")]
        public float UseTime = 60;
        public float Walk = 2f;
        public float Strafe = 2f;
        public float Run = 4f;
        public float Jump = 3f;
        public float Gravity = 1f;

        [Header("UI Sliders")]
        public UnityEngine.UI.Slider UiSliderWalk;
        public UnityEngine.UI.Slider UiSliderStrafe;
        public UnityEngine.UI.Slider UiSliderRun;
        public UnityEngine.UI.Slider UiSliderJump;
        public UnityEngine.UI.Slider UiSliderGravity;

        [Header("InputFields")]
        public UnityEngine.UI.InputField UiInputfieldWalk;
        public UnityEngine.UI.InputField UiInputfieldStrafe;
        public UnityEngine.UI.InputField UiInputfieldRun;
        public UnityEngine.UI.InputField UiInputfieldJump;
        public UnityEngine.UI.InputField UiInputfieldGrav;

        public void Start()
        {
            RunUpdate();
        }

        public void Reset()
        { SetParams(Walk, Strafe, Run, Jump, Gravity); }

        private void SetParams(float walk, float strafe, float run, float jump, float grav)
        {
            if (Networking.LocalPlayer != null)
            {
                Networking.LocalPlayer.SetWalkSpeed(walk);
                Networking.LocalPlayer.SetStrafeSpeed(strafe);
                Networking.LocalPlayer.SetRunSpeed(run);
                Networking.LocalPlayer.SetJumpImpulse(jump);
                Networking.LocalPlayer.SetGravityStrength(grav);
            }
            else
            { Debug.Log("Player movement changed: Walk=" + walk.ToString() + " Strafe=" + strafe.ToString() + " Run=" + run.ToString() + " Jump=" + jump.ToString() + " Gravity=" + grav.ToString()); }
        }

        // private void convert_sliderToInputfield(UnityEngine.UI.Slider slider, UnityEngine.UI.InputField input)
        // { input.text = slider.value.ToString(); }

        public void RunTestPermanent()
        {
            RunUpdate();
            SetParams(ExpectedSliderWalk, ExpectedSliderStrafe, ExpectedSliderRun, ExpectedSliderJump, ExpectedSliderGrav);
        }

        public void RunTest()
        {
            RunUpdate();
            SetParams(ExpectedSliderWalk, ExpectedSliderStrafe, ExpectedSliderRun, ExpectedSliderJump, ExpectedSliderGrav);
#if DEPRICATED
            TimeMem = Time.time + UseTime;
            Running = true;
#else
            SendCustomEventDelayedSeconds("Reset", UseTime, VRC.Udon.Common.Enums.EventTiming.LateUpdate);
#endif

        }

        public void RunUpdate()
        {
            if (UiSliderWalk != null && ExpectedSliderWalk != UiSliderWalk.value)
            {
                ExpectedSliderWalk = UiSliderWalk.value;
                if (UiInputfieldWalk != null)
                { UiInputfieldWalk.text = "Walk=" + ExpectedSliderWalk.ToString(); }
            }
            else if (UiInputfieldWalk != null && !UiInputfieldWalk.text.StartsWith("Walk="))
            {
                float temp = -1;
                if (float.TryParse(UiInputfieldWalk.text, out temp))
                {
                    ExpectedSliderWalk = float.Parse(UiInputfieldWalk.text);
                    if (UiInputfieldWalk != null)
                    { UiInputfieldWalk.text = "Walk=" + ExpectedSliderWalk.ToString(); }

                    if (UiSliderWalk != null)
                    { UiSliderWalk.value = ExpectedSliderWalk; }
                }
                else if (!UiInputfieldWalk.text.StartsWith("Walk="))
                {
                    UiInputfieldWalk.text = "Input error: using " + ExpectedSliderWalk.ToString() + " instead.";
                }
            }

            if (UiSliderStrafe != null && ExpectedSliderStrafe != UiSliderStrafe.value)
            {
                ExpectedSliderStrafe = UiSliderStrafe.value;
                if (UiInputfieldStrafe != null)
                { UiInputfieldStrafe.text = "Strafe =" + ExpectedSliderStrafe.ToString(); }
            }
            else if (UiInputfieldStrafe != null && !UiInputfieldStrafe.text.StartsWith("Strafe ="))
            {
                float temp = -1;
                if (float.TryParse(UiInputfieldStrafe.text, out temp))
                {
                    ExpectedSliderStrafe = float.Parse(UiInputfieldStrafe.text);
                    if (UiInputfieldStrafe != null)
                    { UiInputfieldStrafe.text = "Strafe =" + ExpectedSliderStrafe.ToString(); }

                    if (UiSliderStrafe != null)
                    { UiSliderStrafe.value = ExpectedSliderStrafe; }
                }
                else if (!UiInputfieldStrafe.text.StartsWith("Strafe ="))
                {
                    UiInputfieldStrafe.text = "Input error: using " + ExpectedSliderStrafe.ToString() + " instead.";
                }
            }

            if (UiSliderRun != null && ExpectedSliderRun != UiSliderRun.value)
            {
                ExpectedSliderRun = UiSliderRun.value;
                if (UiInputfieldRun != null)
                { UiInputfieldRun.text = "Run =" + ExpectedSliderRun.ToString(); }
            }
            else if (UiInputfieldRun != null && !UiInputfieldRun.text.StartsWith("Run ="))
            {
                float temp = -1;
                if (float.TryParse(UiInputfieldRun.text, out temp))
                {
                    ExpectedSliderRun = float.Parse(UiInputfieldRun.text);
                    if (UiInputfieldRun != null)
                    { UiInputfieldRun.text = "Run =" + ExpectedSliderRun.ToString(); }

                    if (UiSliderRun != null)
                    { UiSliderRun.value = ExpectedSliderRun; }
                }
                else if (!UiInputfieldRun.text.StartsWith("Run ="))
                {
                    UiInputfieldRun.text = "Input error: using " + ExpectedSliderRun.ToString() + " instead.";
                }
            }

            if (UiSliderJump != null && ExpectedSliderJump != UiSliderJump.value)
            {
                ExpectedSliderJump = UiSliderJump.value;
                if (UiInputfieldJump != null)
                { UiInputfieldJump.text = "Jump=" + ExpectedSliderJump.ToString(); }
            }
            else if (UiInputfieldJump != null && !UiInputfieldJump.text.StartsWith("Jump="))
            {
                float temp = -1;
                if (float.TryParse(UiInputfieldJump.text, out temp))
                {
                    ExpectedSliderJump = float.Parse(UiInputfieldJump.text);
                    if (UiInputfieldJump != null)
                    { UiInputfieldJump.text = "Jump=" + ExpectedSliderJump.ToString(); }

                    if (UiSliderJump != null)
                    { UiSliderJump.value = ExpectedSliderJump; }
                }
                else if (!UiInputfieldJump.text.StartsWith("Jump="))
                {
                    UiInputfieldJump.text = "Input error: using " + ExpectedSliderJump.ToString() + " instead.";
                }
            }

            if (UiSliderGravity != null && ExpectedSliderGrav != UiSliderGravity.value)
            {
                ExpectedSliderGrav = UiSliderGravity.value;
                if (UiInputfieldGrav != null)
                { UiInputfieldGrav.text = "Grav=" + ExpectedSliderGrav.ToString(); }
            }
            else if (UiInputfieldGrav != null && !UiInputfieldGrav.text.StartsWith("Grav="))
            {
                float temp = -1;
                if (float.TryParse(UiInputfieldGrav.text, out temp))
                {
                    ExpectedSliderGrav = float.Parse(UiInputfieldGrav.text);
                    if (UiInputfieldGrav != null)
                    { UiInputfieldGrav.text = "Grav=" + ExpectedSliderGrav.ToString(); }

                    if (UiSliderGravity != null)
                    { UiSliderGravity.value = ExpectedSliderGrav; }
                }
                else if (!UiInputfieldGrav.text.StartsWith("Grav="))
                {
                    UiInputfieldGrav.text = "Input error: using " + ExpectedSliderGrav.ToString() + " instead.";
                }
            }
        }

#if DEPRICATED
        public void LateUpdate()
        {
            if (Running && Time.time > TimeMem)
            {
                Reset();
                Running = false;
            }
        }
#endif
    }
}

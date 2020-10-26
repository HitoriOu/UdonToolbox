
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolboxV2
{
    public class PlayerParamTester : UdonSharpBehaviour
    {
        /* Dev Notes:
         * U# Script made by "Hitori Ou" for free use with "VR Chat" using UDON on Unity.
         * Usable functions:
         * Reset [reverts to previous settings]
         * run_test [starts the test run & timer]
         */

        private float time_mem = 0;
        private bool running = false;

        private float expected_slider_walk = -1;
        private float expected_slider_run = -1;
        private float expected_slider_jump = -1;
        private float expected_slider_grav = -1;

        [Header("Reset setup")]
        [Tooltip("Time untill settings are reset.")]
        public float Use_Time = 60;
        public float Walk = 2f;
        public float Run = 4f;
        public float Jump = 3f;
        public float Gravity = 1f;

        [Header("UI Sliders")]
        public UnityEngine.UI.Slider UI_Slider_Walk;
        public UnityEngine.UI.Slider UI_Slider_Run;
        public UnityEngine.UI.Slider UI_Slider_Jump;
        public UnityEngine.UI.Slider UI_Slider_Gravity;

        [Header("InputFields")]
        public UnityEngine.UI.InputField UI_InputField_Walk;
        public UnityEngine.UI.InputField UI_InputField_Run;
        public UnityEngine.UI.InputField UI_InputField_Jump;
        public UnityEngine.UI.InputField UI_InputField_Grav;

        public void Start()
        {
            run_update();
        }

        public void Reset()
        { Set_params(Walk, Run, Jump, Gravity); }

        private void Set_params(float walk_X, float run_X, float jump_X, float grav_X)
        {
            if (Networking.LocalPlayer != null)
            {
                Networking.LocalPlayer.SetWalkSpeed(walk_X);
                Networking.LocalPlayer.SetRunSpeed(run_X);
                Networking.LocalPlayer.SetJumpImpulse(jump_X);
                Networking.LocalPlayer.SetGravityStrength(grav_X);
            }
            else
            { Debug.Log("Player movement changed: Walk=" + walk_X.ToString() + " Run=" + run_X.ToString() + " Jump=" + jump_X.ToString() + " Gravity=" + grav_X.ToString()); }
        }

        private void convert_sliderToInputfield(UnityEngine.UI.Slider slider, UnityEngine.UI.InputField input)
        { input.text = slider.value.ToString(); }

        public void run_test()
        {
            run_update();
            Set_params(expected_slider_walk, expected_slider_run, expected_slider_jump, expected_slider_grav);
            time_mem = Time.time + Use_Time;
            running = true;
        }

        public void run_update()
        {
            if (UI_Slider_Walk != null && expected_slider_walk != UI_Slider_Walk.value)
            {
                expected_slider_walk = UI_Slider_Walk.value;
                if (UI_InputField_Walk != null)
                { UI_InputField_Walk.text = "Walk=" + expected_slider_walk.ToString(); }
            }
            else if (UI_InputField_Walk != null && !UI_InputField_Walk.text.StartsWith("Walk="))
            {
                float temp = -1;
                if (float.TryParse(UI_InputField_Walk.text, out temp))
                {
                    expected_slider_walk = float.Parse(UI_InputField_Walk.text);
                    if (UI_InputField_Walk != null)
                    { UI_InputField_Walk.text = "Walk=" + expected_slider_walk.ToString(); }

                    if (UI_Slider_Walk != null)
                    { UI_Slider_Walk.value = expected_slider_walk; }
                }
                else if (!UI_InputField_Walk.text.StartsWith("Walk="))
                {
                    UI_InputField_Walk.text = "Input error: using " + expected_slider_walk.ToString() + " instead.";
                }
            }

            if (UI_Slider_Run != null && expected_slider_run != UI_Slider_Run.value)
            {
                expected_slider_run = UI_Slider_Run.value;
                if (UI_InputField_Run != null)
                { UI_InputField_Run.text = "Run =" + expected_slider_run.ToString(); }
            }
            else if (UI_InputField_Run != null && !UI_InputField_Run.text.StartsWith("Run ="))
            {
                float temp = -1;
                if (float.TryParse(UI_InputField_Run.text, out temp))
                {
                    expected_slider_run = float.Parse(UI_InputField_Run.text);
                    if (UI_InputField_Run != null)
                    { UI_InputField_Run.text = "Run =" + expected_slider_run.ToString(); }

                    if (UI_Slider_Walk != null)
                    { UI_Slider_Walk.value = expected_slider_walk; }
                }
                else if (!UI_InputField_Walk.text.StartsWith("Run ="))
                {
                    UI_InputField_Run.text = "Input error: using " + expected_slider_run.ToString() + " instead.";
                }
            }

            if (UI_Slider_Jump != null && expected_slider_jump != UI_Slider_Jump.value)
            {
                expected_slider_jump = UI_Slider_Jump.value;
                if (UI_InputField_Jump != null)
                { UI_InputField_Jump.text = "Jump=" + expected_slider_jump.ToString(); }
            }
            else if (UI_InputField_Jump != null && !UI_InputField_Jump.text.StartsWith("Jump="))
            {
                float temp = -1;
                if (float.TryParse(UI_InputField_Jump.text, out temp))
                {
                    expected_slider_jump = float.Parse(UI_InputField_Jump.text);
                    if (UI_InputField_Jump != null)
                    { UI_InputField_Jump.text = "Jump=" + expected_slider_jump.ToString(); }

                    if (UI_Slider_Jump != null)
                    { UI_Slider_Jump.value = expected_slider_jump; }
                }
                else if (!UI_InputField_Jump.text.StartsWith("Jump="))
                {
                    UI_InputField_Jump.text = "Input error: using " + expected_slider_jump.ToString() + " instead.";
                }
            }

            if (UI_Slider_Gravity != null && expected_slider_grav != UI_Slider_Gravity.value)
            {
                expected_slider_grav = UI_Slider_Gravity.value;
                if (UI_InputField_Grav != null)
                { UI_InputField_Grav.text = "Grav=" + expected_slider_grav.ToString(); }
            }
            else if (UI_InputField_Grav != null && !UI_InputField_Grav.text.StartsWith("Grav="))
            {
                float temp = -1;
                if (float.TryParse(UI_InputField_Grav.text, out temp))
                {
                    expected_slider_grav = float.Parse(UI_InputField_Grav.text);
                    if (UI_InputField_Grav != null)
                    { UI_InputField_Grav.text = "Grav=" + expected_slider_grav.ToString(); }

                    if (UI_Slider_Gravity != null)
                    { UI_Slider_Gravity.value = expected_slider_grav; }
                }
                else if (!UI_InputField_Grav.text.StartsWith("Grav="))
                {
                    UI_InputField_Grav.text = "Input error: using " + expected_slider_grav.ToString() + " instead.";
                }
            }
        }

        public void LateUpdate()
        {
            if (running && Time.time > time_mem)
            {
                Reset();
                running = false;
            }
        }
    }
}

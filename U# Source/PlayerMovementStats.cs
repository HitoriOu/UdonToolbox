
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolboxV2
{
    /// <summary>
    /// PlayerMovementStats
    /// Testing script used to display players current movement parameters.
    /// Created by Hitori Ou
    /// Last edit: 26-11-2020 Version 2.4
    /// </summary>
    public class PlayerMovementStats : UdonSharpBehaviour
    {
        private float Timer = 0;
        private string CurrentInput = "";

        [Tooltip("List of text displays to use")]
        public UnityEngine.UI.Text[] UI_Text = new UnityEngine.UI.Text[1];

        [Tooltip("Duration in seconds untill values are checked again.")]
        public float UpdateRate = 0f;

        const string return_newline = "\r\n";

        public void FixedUpdate()
        {
            if(Timer < Time.time)
            {
                Timer = Time.time + UpdateRate;
                CurrentInput = "";
                if (Networking.LocalPlayer != null)
                {
                    CurrentInput = CurrentInput + "Walk: " + Networking.LocalPlayer.GetWalkSpeed().ToString() + return_newline;
                    CurrentInput = CurrentInput + "Run: " + Networking.LocalPlayer.GetRunSpeed().ToString() + return_newline;
                    CurrentInput = CurrentInput + "Strafe: " + Networking.LocalPlayer.GetStrafeSpeed().ToString() + return_newline;
                    CurrentInput = CurrentInput + "Jump: " + Networking.LocalPlayer.GetJumpImpulse().ToString() + return_newline;
                    CurrentInput = CurrentInput + "Gravity: " + Networking.LocalPlayer.GetGravityStrength().ToString();
                    PrintText(CurrentInput);
                }
                else
                {
                    CurrentInput = CurrentInput + "Walk: " + "Error: No player detected!" + return_newline;
                    CurrentInput = CurrentInput + "Run: " + "Error: No player detected!" + return_newline;
                    CurrentInput = CurrentInput + "Strafe: " + "Error: No player detected!" + return_newline;
                    CurrentInput = CurrentInput + "Jump: " + "Error: No player detected!" + return_newline;
                    CurrentInput = CurrentInput + "Gravity: " + "Error: No player detected!";
                    PrintText(CurrentInput);
                }
            }
        }

        private void PrintText(string text)
        {
            for (uint i = 0; i < UI_Text.Length; i++)
            {
                if (UI_Text[i] != null)
                { UI_Text[i].text = text; }
            }
        }
    }
}

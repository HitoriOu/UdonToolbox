
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PlayerMovementStats : UdonSharpBehaviour
{
    [Tooltip("List of text displays to use")]
    public UnityEngine.UI.Text[] UI_Text= new UnityEngine.UI.Text[1];

    const string return_newline = "\r\n";

    public void FixedUpdate()
    {
        string input="";
        if (Networking.LocalPlayer != null)
         {
            input = input + "Walk: " + Networking.LocalPlayer.GetWalkSpeed().ToString() + return_newline;
            input = input + "Run: " + Networking.LocalPlayer.GetRunSpeed().ToString() + return_newline;
            input = input + "Jump: " + Networking.LocalPlayer.GetJumpImpulse().ToString() + return_newline;
            input = input + "Gravity: " + Networking.LocalPlayer.GetGravityStrength().ToString();
         }
        else
         { input = "Error: No player detected!"; }

        for (uint i = 0; i < UI_Text.Length; i++)
        {
            if (UI_Text[i] != null)
            { UI_Text[i].text = input; }
        }
    }
}

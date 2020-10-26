
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolboxV2
{
    public class ModifyUdonNumber_Float : UdonSharpBehaviour
    {
        [UdonSynced(UdonSyncMode.None)] bool world_init_mem = false;
        [UdonSynced(UdonSyncMode.None)] float synch_mem = 0;

        [Header("Modify Target (Udon Behaviour)")]
        [Tooltip("List of game objects containing a valid Udon Behaviour component")]
        public UdonSharpBehaviour[] Where_to_Modify;
        [Tooltip("Name of variable to toggle")]
        public string What_to_Modify = new string(new char[0]);
        [Tooltip("Name of update function to call (Optional)")]
        public string Call_Event_Name = new string(new char[0]);

        [Header("Value Input Setup")]
        [Tooltip("Default input if no UI selected")]
        public float Manual_Input = 0;
        [Tooltip("Use Ui slider\r\n(overrides 'Manual_Input')")]
        public UnityEngine.UI.Slider UI_Slider_Input = null;
        [Tooltip("Use Ui Input Field\r\n(overrides 'Manual_Input' & 'UI_Slider_Input')\r\n Input errors will default input to 'Manual_Input'")]
        public UnityEngine.UI.InputField UI_Inputfield = null;

        /* // removed code
        [Header("Synching")]
        //[Tooltip("All players in world are affected.")]
        //public bool Global_Synched = true;
        [Tooltip("Players who join will see what others see.")]
        public bool Late_Join_Synched = true;
        */

        [Header("Events")]
        [Tooltip("Custom Event for UI to call/use")]
        public bool Event_UI_Update = true; /*custom event for UI to use on UI updates*/
        public bool EventInteract = true;
        public bool Event_OnCollisionEnter = false;
        public bool Event_OnCollisionExit = false;
        public bool Event_OnTriggerEnter = false;
        public bool Event_OnTriggerExit = false;

        public void UI_Update() { if (Event_UI_Update) { SendCustomEvent("Run"); } }
        public override void Interact() { if (EventInteract) { SendCustomEvent("Run"); } }
        public void OnCollisionEnter(Collision other) { if (Event_OnCollisionEnter) { SendCustomEvent("Run"); } }
        public void OnCollisionExit(Collision other) { if (Event_OnCollisionExit) { SendCustomEvent("Run"); } }
        public void OnTriggerEnter(Collider other) { if (Event_OnTriggerEnter) { SendCustomEvent("Run"); } }
        public void OnTriggerExit(Collider other) { if (Event_OnTriggerExit) { SendCustomEvent("Run"); } }

        public override void OnPlayerCollisionEnter(VRCPlayerApi player) { if (Event_OnCollisionEnter && player.isLocal) { SendCustomEvent("Run"); } }
        public override void OnPlayerCollisionExit(VRCPlayerApi player) { if (Event_OnCollisionExit && player.isLocal) { SendCustomEvent("Run"); } }
        public override void OnPlayerTriggerEnter(VRCPlayerApi player) { if (Event_OnTriggerEnter && player.isLocal) { SendCustomEvent("Run"); } }
        public override void OnPlayerTriggerExit(VRCPlayerApi player) { if (Event_OnTriggerExit && player.isLocal) { SendCustomEvent("Run"); } }

        void Start()
        {
            // if (Networking.LocalPlayer == null)  { Global_Synched = false; }
            /* // removed code
             if (world_init_mem && Late_Join_Synched && UI_Slider_Input != null && UI_Inputfield == null)
             { Set_Value(synch_mem); UI_Slider_Input.value= synch_mem; }
             else if(UI_Inputfield == null)
              { Set_Value(get_input()); }
             world_init_mem = true;
             */
        }

        public void Run()
        {
            /* // removed code
            if (Late_Join_Synched)
             { synch_mem = get_input(); }
             */
            Set_Value(get_input());
        }

        private void Set_Value(float value)
        {
            for (int i = 0; i < Where_to_Modify.Length; i++)
            {
                Where_to_Modify[i].SetProgramVariable(What_to_Modify, value);
                if (Call_Event_Name != null && Call_Event_Name.Length > 0)
                { Where_to_Modify[i].SendCustomEvent(Call_Event_Name); }
            }
        }

        private float get_input()
        {
            if (UI_Inputfield != null)
            {
                float temp = -1;
                if (float.TryParse(UI_Inputfield.text, out temp) && UI_Inputfield.text.Length > 0)
                {
                    return float.Parse(UI_Inputfield.text);
                }
                else
                {
                    UI_Inputfield.text = "Error invalid input: using default " + Manual_Input.ToString() + " instead!";
                    return Manual_Input;
                }
            }
            else if (UI_Slider_Input != null)
            { return UI_Slider_Input.value; }
            else
            { return Manual_Input; }
        }
    }
}


using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolboxV2
{
    public class MobilityZone_Vectorized : UdonSharpBehaviour
    {
        /*
         Dev Notes:
         Important! Vector is measured relative to the object that the script is attached to.

         The impacting player position relative to the vector vs the script objects location when triggered, 
         determines if it's on the vectors side or opposite direction.

         When "Use_Enter" is off the Exit values is used instead
         and if "Enter_Use_OnVector" is on/true then OnVector Values are used instead of Exit and enter values.
         */

        [Tooltip("Reference object used to measure from the main objects center position for getting the vector calculations done")]
        public GameObject Vector;

        [Header("While inside")]
        [Tooltip("While inside trigger/collider this value is used Else exit values are used")]
        public bool Use_Enter = true;
        [Tooltip("Used when Use_Enter is disabled.\r\nEnable uses OnVector values\r\nDisabled uses Exit values")]
        public bool Enter_Use_OnVector = false;

        public float Enter_Walk = 10;
        public float Enter_Run = 20;
        public float Enter_Jump = 20;
        public float Enter_Grav = 10;


        [Header("On Exit")]
        public float Exit_Walk = 2;
        public float Exit_Run = 4;
        public float Exit_Jump = 0;
        public float Exit_Grav = 1;

        [Space(6)]
        public float Exit_Walk_OnVector = 2;
        public float Exit_Run_OnVector = 5;
        public float Exit_Jump_OnVector = 1;
        public float Exit_Grav_OnVector = 0.1f;

        [Header("Events")]
        public bool Event_OnCollisionEnter = false;
        public bool Event_OnCollisionExit = false;
        public bool Event_OnTriggerEnter = true;
        public bool Event_OnTriggerExit = true;

        void OnCollisionEnter(Collision other) { if (Event_OnCollisionEnter) { SendCustomEvent("Run_Enter"); } }
        void OnCollisionExit(Collision other) { if (Event_OnCollisionExit) { SendCustomEvent("Run_Vector_Check"); } }
        void OnTriggerEnter(Collider other) { if (Event_OnTriggerEnter) { SendCustomEvent("Run_Enter"); } }
        void OnTriggerExit(Collider other) { if (Event_OnTriggerExit) { SendCustomEvent("Run_Vector_Check"); } }

        public override void OnPlayerCollisionEnter(VRCPlayerApi player) { if (Event_OnCollisionEnter && player.isLocal) { SendCustomEvent("Run_Enter"); } }
        public override void OnPlayerCollisionExit(VRCPlayerApi player) { if (Event_OnCollisionExit && player.isLocal) { SendCustomEvent("Run_Vector_Check"); } }
        public override void OnPlayerTriggerEnter(VRCPlayerApi player) { if (Event_OnTriggerEnter && player.isLocal) { SendCustomEvent("Run_Enter"); } }
        public override void OnPlayerTriggerExit(VRCPlayerApi player) { if (Event_OnTriggerExit && player.isLocal) { SendCustomEvent("Run_Vector_Check"); } }

        private void Set_OnVector()
        { Set_mobility(Exit_Walk_OnVector, Exit_Run_OnVector, Exit_Jump_OnVector, Exit_Grav_OnVector); }

        private void Set_Not_OnVector()
        { Set_mobility(Exit_Walk, Exit_Run, Exit_Jump, Exit_Grav); }

        private void Set_mobility(float walk, float run, float jump, float grav)
        {
            if (Networking.LocalPlayer != null)
            {
                Networking.LocalPlayer.SetWalkSpeed(walk);
                Networking.LocalPlayer.SetRunSpeed(run);
                Networking.LocalPlayer.SetJumpImpulse(jump);
                Networking.LocalPlayer.SetGravityStrength(grav);
            }
        }

        public void Run_Enter()
        {
            if (Use_Enter)
            { Set_mobility(Enter_Walk, Enter_Run, Enter_Jump, Enter_Grav); }
            else
            {
                if (Enter_Use_OnVector)
                { Set_OnVector(); }
                else
                { Set_Not_OnVector(); }
            }
        }

        public void Run_Vector_Check()
        {
            Vector3 Not_vector_imaginary = (this.gameObject.transform.position - Vector.transform.position) + this.gameObject.transform.position;

            float vector_to_impact_point = Vector3.Distance(Vector.transform.position, Networking.LocalPlayer.GetPosition());
            float Not_vector_imaginary_to_impact_point = Vector3.Distance(Not_vector_imaginary, Networking.LocalPlayer.GetPosition());

            if (Not_vector_imaginary_to_impact_point >= vector_to_impact_point)
            { Set_OnVector(); }
            else
            { Set_Not_OnVector(); }
        }
    }
}

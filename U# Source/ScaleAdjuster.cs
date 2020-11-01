
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolboxV2
{
    /// <summary>
    /// ScaleAdjuster
    /// Used to resize multiple objects by dragging a handle object.
    /// Created by Hitori Ou
    /// Last edit: 1-11-2020 Version 2.3
    /// </summary>
    public class ScaleAdjuster : UdonSharpBehaviour
    {
        #region PrivateVariables
        // Global synch variable.
        [UdonSynced(UdonSyncMode.None)]
        private float Range = 0;
        // Global synch change reference variable.
        private float LastRange = 0;
        // Check value used to optimice the update loop.
        private float LastDistance = 0;
        // Determines if the grab handle is locked in place or not.
        private bool Snapped = true;
        // Used to unlock scale testing during playmode.
        private bool ScaleTest = false;

        // Snapshots the original scale
        private Vector3[] OriginalScale;
        private Vector3 HandleOriginalScale;
        #endregion

        #region PublicVariables
        [Tooltip("Objects affected by the rescale (element:0 is mandatory")]
        public Transform[] Targets;

        [Tooltip("Handle object gets rescaled as well.")]
        public bool ScaleGrabHandle = false;
        [Tooltip("Pickup object used for drag & drop rescaling")]
        public Transform GrabHandle;

        [Tooltip("Adjusts scale offsets (1 = no change).")]
        public Vector3 ScaleModifier = new Vector3(1, 1, 1);


        [Header("Handle range setup")]
        [Tooltip("Adjusts Range offset (usefull if the handle is far away from the object).")]
        public float RangeShiftAdjuster = 0;
        [Tooltip("Alters the final scale, usefull for fine tuning (also affects the Min/MaxRange)")]
        public float RangeMultiplier = 2;
        [Tooltip("Limits how tiny the min scale/size can be.")]
        public float MinRange = 0.5f;
        [Tooltip("Limits how large the max scale/size can be.")]
        public float MaxRange = 10;

        [Header("SnapMode (handle Setup)")]
        [Tooltip("Enables the use of a handle that snaps onto the object being re-scaled (turning this off also enables testing in unity play mode")]
        public bool SnapMode = true;
        [Tooltip("Location that the handle returns to once the player lets go of it")]
        public Transform SnapLocation;

        [Header("Synching")]
        [Tooltip("All players in world are affected.")]
        public bool Global_Synched = true;
        #endregion

        #region Events
        public override void OnPickup()
        {
            if (Global_Synched && SnapMode) { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "NetworkSnapOff"); }
            else if (SnapMode) { Snap(false); }
        }

        public override void OnDrop()
        {
            if (Global_Synched && SnapMode) { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "NetworkSnapOn"); }
            else if (SnapMode) { Snap(true); }
        }

        void Start()
        {
            if(Networking.LocalPlayer == null)
            {
                Global_Synched = false;
                ScaleTest = true;
            }

            if(GrabHandle == null)
            {
                Debug.LogError("ScaleAdjuster: No GrabHandle selected (mandatory)!", this);
            }

            if (SnapMode && (SnapLocation == null || SnapLocation == this.gameObject.transform))
            {
                SnapMode = false;
                if (SnapLocation == null)
                {
                    Debug.LogWarning("ScaleAdjuster: SnapLocation cannot be empty for Snapmode to work (SnapMode turned off).", this);
                }
                else
                {
                    Debug.LogWarning("ScaleAdjuster: Improper SnapLocation cannot be same as it self (SnapMode turned off).", this);
                }
            }
            else if (SnapMode && GrabHandle != null)
            {
                GrabHandle.position = SnapLocation.position;
                GrabHandle.rotation = SnapLocation.rotation;
            }

            if (MinRange >= MaxRange)
            {
                MaxRange = MinRange;
                Debug.LogWarning("ScaleAdjuster: MaxRange has to be larger than MinRange", this);
            }

            if (Range == 0 && LastRange == 0 && Global_Synched)
            {
                Range = Vector3.Distance(Targets[0].position, GrabHandle.position);
                LastRange = Range;
            }

            if (Targets == null || Targets != null && Targets.Length == 0 || Targets != null && Targets.Length != 0 && Targets[0] == null)
            {
                Debug.LogError("ScaleAdjuster: No Targets element:0 found (mandatory)", this);
            }
            else if (Targets != null && Targets.Length !=0 && GrabHandle.IsChildOf(Targets[0]))
            {
                Debug.LogError("ScaleAdjuster: Error found: Infinite loop setup. The Targets child object should not be same as this->", GrabHandle);
            }
            else
            {
                OriginalScale = new Vector3[Targets.Length];
                if (Targets != null)
                {
                    for (int i = 0; i < Targets.Length; i++)
                    {
                        if (Targets[i] != null)
                        {
                            OriginalScale[i] = Targets[i].localScale;
                        }
                    }
                }
                if(GrabHandle != null)
                {
                    HandleOriginalScale = GrabHandle.localScale;
                }
            }

            
            if (SnapMode && SnapLocation != null)
            {
                GrabHandle.position = SnapLocation.position;
                GrabHandle.rotation = SnapLocation.rotation;
            }

            if(true)
            {
                UpdateScale();
                LastDistance = Vector3.Distance(Targets[0].position, GrabHandle.position);
            }
        }

        public void LateUpdate()
        {
            if (SnapMode && Snapped)
            {
                if (Global_Synched && Range != LastRange)
                {
                    LastRange = Range;
                    SetScale(LastRange);
                }
                else if (SnapLocation != null)
                {
                    GrabHandle.position = SnapLocation.position;
                    GrabHandle.rotation = SnapLocation.rotation;
                }
            }
            else if ((SnapMode && !Snapped) || !SnapMode)
            {

                if(ScaleTest || LastDistance != Vector3.Distance(Targets[0].position, GrabHandle.position))
                {
                    LastDistance = Vector3.Distance(Targets[0].position, GrabHandle.position);
                    UpdateScale();
                }
            }
        }
        #endregion

        #region PrivateFunctions
        /// <summary>
        /// Logic checks and value filtering before setting scale.
        /// </summary>
        private void UpdateScale()
        {
            if(Targets != null && GrabHandle != null)
            {
                float Distance = Vector3.Distance(Targets[0].position, GrabHandle.position) + RangeShiftAdjuster;
                if (Distance < MinRange)
                {
                    Distance = MinRange * RangeMultiplier;
                }
                else if (Distance > MaxRange)
                {
                    Distance = MaxRange * RangeMultiplier;
                }
                else
                {
                    Distance *= RangeMultiplier;
                }
                SetScale(Distance);

                if (Global_Synched)
                {
                    Range = Distance;
                    LastRange = Range;
                }
            }
        }

        /// <summary>
        /// Uses the original scale saved from start, to accurately set a new scale based on the scaleRange value.
        /// </summary>
        /// <param name="scaleRange">Scale adjust value.</param>
        private void SetScale(float scaleRange)
        {
            if (Targets != null)
            {
                for (int i = 0; i < Targets.Length; i++)
                {
                    if (Targets[i] != null)
                    {
                        Targets[i].localScale = new Vector3(OriginalScale[i].x * ScaleModifier.x * scaleRange, OriginalScale[i].y * ScaleModifier.y * scaleRange, OriginalScale[i].z * ScaleModifier.z * scaleRange);
                    }
                }
            }
            if (ScaleGrabHandle)
            {
                GrabHandle.localScale = new Vector3(HandleOriginalScale.x * ScaleModifier.x * scaleRange, HandleOriginalScale.y * ScaleModifier.y * scaleRange, HandleOriginalScale.z * ScaleModifier.z * scaleRange);
            }
            else
            {
                GrabHandle.localScale = HandleOriginalScale;
            }
        }

        private void Snap(bool mode)
        {
            if (mode)
            {
                Snapped = true;
            }
            else
            {
                Snapped = false;
                if (SnapLocation != null)
                {
                    GrabHandle.position = SnapLocation.position;
                    GrabHandle.rotation = SnapLocation.rotation;
                }
            }
        }
        #endregion

        #region NetworkFunctions
        public void NetworkSnapOn()
        {
            if(Global_Synched)
            {
                Snap(true);
            }
        }

        public void NetworkSnapOff()
        {
            if (Global_Synched)
            {
                Snap(false);
            }
        }
        #endregion
    }
}


using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
namespace UdonToolboxV2
{

    /// <summary>
    /// SpinTheBottle
    /// Used to play a game of "spin the bottle".
    /// Created by Hitori Ou
    /// Last edit: 25-04-2021 Version 2.4
    /// </summary>
    public class SpinTheBottle : UdonSharpBehaviour
    {
        // X= rotation velocity, y= current angle, z= start speed (restarts if z is altered)
        [UdonSynced(UdonSyncMode.None)]
        [HideInInspector]
        public Vector3 SynchValues = new Vector3(0, 0, 0);

        //Top speed during spin.
        private float MemStartVelocity = 0;

        //[SerializeField]
        //What is used for update timing.
        private float MemCurrentUpdateRate = 0;

        //Speed to start with for next round.
        private float QueueVelocity = 0;

        //Pointer for the rigidbody used.
        private Rigidbody Body;

        //Pointer for the VRCPlayerApi used.
        private VRCPlayerApi LocalPlayer;

        //Timer memmory for the UpdateRate
        private float Timer = 0;

        // Disables Interact if slaved.
        private bool EventInteract = true;

        //Current controller/master/owner that others synch to.
        private bool Owner = false;
        
        //Is bottle in spinning phase.
        private bool InMotion = false;

        [Header("Rotation setting")]
        [Tooltip("What spin axis to track/affect.\r\n 1=X  2=Y  3=Z")]
        [Range(1,3)]
        public int AxisNumber = 2;
        
        [Header("Speed settings")]
        [Tooltip("Stops spinning when speed goes bellow.")]
        public float StopSpeed = 0.2f;

        [Tooltip("Minimum speed to spin.")]
        public float MinStartSpeed = 5;

        [Tooltip("Maximum speed to spin.")]
        public float MaxStartSpeed = 10;

        [Header("Update & synch setup")]
        [Tooltip("How long time between updates in seconds (uses ''PlayUpdateRate'' once movement is detected).")]
        public float UpdateRate = 1;

        [Tooltip("How long time between updates in seconds during play (lower increases accuracy).")]
        public float PlayUpdateRate = 0.1f;

        [Space(4)]
        [Tooltip("Forces PlayUpdateRate when interacted with (will cause a de-synch with any 'SynchTo' linked to this object).")]
        public bool InstantInteract = true;

        [Tooltip("Can be used to synch to a main Bottle (makes this follow the synching of what it is SynchTo")]
        public SpinTheBottle SynchTo;

        /// <summary>
        /// Default Interact event with lock flag and RandomSpin execution
        /// </summary>
        public override void Interact() { if (EventInteract) { RandomSpin(); } }

        /// <summary>
        /// Sets a random spin if owner else queues random spin and changes owner
        /// </summary>
        public void RandomSpin()
        {
            if (InstantInteract)
            {
                MemCurrentUpdateRate = PlayUpdateRate;
            }

            if (LocalPlayer == null || LocalPlayer != null && Networking.IsOwner(this.gameObject))
            {
                SynchValues.z = Random.Range(MinStartSpeed, MaxStartSpeed);
                //Deny rolling same value twice.
                while (MemStartVelocity == SynchValues.z)
                {
                    SynchValues.z = Random.Range(MinStartSpeed, MaxStartSpeed);
                }
                SynchValues.x = SynchValues.z;
            }
            else
            {
                QueueVelocity = Random.Range(MinStartSpeed, MaxStartSpeed);
                SetOwner(Networking.LocalPlayer);
            }
        }

        /// <summary>
        /// Returns the Axis currently used "AxisNumber".
        /// </summary>
        /// <param name="values">all values</param>
        /// <returns>Axis value</returns>
        private float GetAxisValue(Vector3 values)
        {
            switch (AxisNumber)
            {
                case 1:
                    return values.x; break;
                case 2:
                    return values.y; break;
                case 3:
                    return values.z; break;
                default:
                    AxisNumber = 1;
                    return values.x; break;
            }
        }

        /// <summary>
        /// Changes values on the set axis "AxisNumber".
        /// </summary>
        /// <param name="values">Original values.</param>
        /// <param name="change">Value to edit with.</param>
        /// <returns>Values that has the change applied.</returns>
        private Vector3 ChangeAxisValue(Vector3 values, float change)
        {
            Vector3 tempValues = values;
            switch (AxisNumber)
            {
                case 1:
                    tempValues.x = change; break;
                case 2:
                    tempValues.y = change; break;
                case 3:
                    tempValues.z = change; break;
                default:
                    AxisNumber = 1;
                    tempValues.x = change; break;
            }
            return tempValues;
        }

        #region setOwner
        /// <summary>
        /// Sets owner if this script owner does not match.
        /// </summary>
        /// <param name="player">Player to set as owner.</param>
        /// <returns>True if player exist (not null).</returns>
        private bool SetOwner(VRCPlayerApi player)
        {
            if (player != null)
            {
                if (!Networking.IsOwner(this.gameObject))
                {
                    Networking.SetOwner(player, this.gameObject);
                }
                return true;
            }
            return false;
        }
        #endregion

        /// <summary>
        /// When owner changes check if the new owner and execute queued velocity.
        /// </summary>
        public override void OnOwnershipTransferred()
        {
            if(Networking.IsOwner(this.gameObject))
            {
                SynchValues.x = QueueVelocity;
                SynchValues.z = QueueVelocity;
                QueueVelocity = 0;

                Owner = true;
            }
        }

        /// <summary>
        /// Checks and setup for.
        /// updaterate
        /// localplayer
        /// speed setting checks
        /// slave configuration
        /// rigidbody check
        /// </summary>
        void Start()
        {
            MemCurrentUpdateRate = UpdateRate;
            LocalPlayer = Networking.LocalPlayer;
            if(LocalPlayer == null || LocalPlayer != null && Networking.IsOwner(LocalPlayer,this.gameObject))
            {
                Owner = true;
            }
            else
            {
                Owner = false;
            }

            if (StopSpeed > MinStartSpeed)
            {
                Debug.LogWarning("SpinTheBottle: StopSpeed is higher than MinStartSpeed (auto adjusted/fixed)", this.gameObject);
                MinStartSpeed = StopSpeed + 0.01f;
            }
            if (MinStartSpeed > MaxStartSpeed)
            {
                Debug.LogWarning("SpinTheBottle: MinStartSpeed is higher than MaxStartSpeed (auto adjusted/fixed)", this.gameObject);
                MaxStartSpeed = MinStartSpeed + 0.1f;
            }

            if (SynchTo != null)
            {
                AxisNumber = SynchTo.AxisNumber;
                StopSpeed = SynchTo.StopSpeed;

                Owner = false;
                EventInteract = false;
            }

            // VRC.SDKBase.Utilities.IsValid();

            Body =  this.gameObject.GetComponent<Rigidbody>();
            if (!Utilities.IsValid(Body))
            {
                Debug.LogError("SpinTheBottle: No rigidbody detected (mandatory).", this.gameObject);
            }
            else
            {
                Body.maxAngularVelocity = MaxStartSpeed;
            }
        }
        
        /// <summary>
        /// Handles start/stop, speed/angle, update/playupdate settings.
        /// </summary>
        private void Update()
        {
            if(Timer < Time.time)
            {
                Timer = Time.time + MemCurrentUpdateRate;

                if(Owner)
                {
                    //If told to be spinning
                    if (this.SynchValues.x > StopSpeed)
                    {
                        //Start spinning
                        if (SynchValues.z != MemStartVelocity)
                        {
                            Body.isKinematic = true;
                            //Set bottle rotation.
                            Body.rotation = Quaternion.Euler(ChangeAxisValue(Body.rotation.eulerAngles, SynchValues.y));

                            Body.isKinematic = false;
                            Body.angularVelocity = ChangeAxisValue(Body.angularVelocity, SynchValues.x);
                            MemStartVelocity = SynchValues.z;
                            InMotion = true;
                            MemCurrentUpdateRate = PlayUpdateRate;
                        }

                        //Update movement
                        switch (AxisNumber)
                        {
                            case 1:
                                SynchValues.y = Body.rotation.eulerAngles.x;
                                break;
                            case 2:
                                SynchValues.y = Body.rotation.eulerAngles.y;
                                break;
                            case 3:
                                SynchValues.y = Body.rotation.eulerAngles.z;
                                break;
                            default: AxisNumber = 1; break;
                        }
                        SynchValues.x = GetAxisValue(Body.angularVelocity);

                    }
                    //If told to stop spinning.
                    else
                    {
                        if(InMotion)
                        {
                            Body.isKinematic = true;
                            Body.angularVelocity = ChangeAxisValue(Body.angularVelocity, 0);
                            if (SynchValues.y != GetAxisValue(Body.rotation.eulerAngles))
                            {
                                Body.rotation = Quaternion.Euler(ChangeAxisValue(Body.rotation.eulerAngles, SynchValues.y));
                            }
                            else
                            {
                                InMotion = false;
                                MemCurrentUpdateRate = UpdateRate;
                            }
                        }
                    }
                }
                else
                {
                    //Movement synch
                    if (SynchTo == null)
                    {
                        SynchBottle(this);
                    }
                    else
                    {
                        SynchBottle(SynchTo);
                    }
                }
            }
        }

        /// <summary>
        /// Used to synch to controller client (object owner).
        /// </summary>
        /// <param name="controller">What is controlling this.</param>
        private void SynchBottle(SpinTheBottle controller)
        {
            if (controller.SynchValues.x > StopSpeed)
            {
                if (!InMotion)
                {
                    Body.isKinematic = true;
                    //Set bottle rotation.
                    Body.rotation = Quaternion.Euler(ChangeAxisValue(Body.rotation.eulerAngles, controller.SynchValues.y));

                    Body.isKinematic = false;
                    Body.angularVelocity = ChangeAxisValue(Body.angularVelocity, controller.SynchValues.x);
                    MemStartVelocity = controller.SynchValues.z;

                    InMotion = true;
                    MemCurrentUpdateRate = PlayUpdateRate;
                }
                //Restart
                else if(controller.SynchValues.z != MemStartVelocity)
                {
                    InMotion = false;
                }
            }
            else
            {
                if(InMotion)
                {
                    Body.isKinematic = true;
                    Body.angularVelocity = ChangeAxisValue(Body.angularVelocity, 0);
                    //Set bottle rotation.
                   // Body.rotation = Quaternion.Euler(ChangeAxisValue(Body.rotation.eulerAngles, controller.SynchValues.y));

                    if(controller.SynchValues.y != GetAxisValue(Body.rotation.eulerAngles))
                    {
                        Body.rotation = Quaternion.Euler(ChangeAxisValue(Body.rotation.eulerAngles, controller.SynchValues.y));
                    }
                    else
                    {
                        MemCurrentUpdateRate = UpdateRate;
                        InMotion = false;
                    }
                }
            }
        }
    }
}


using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class VanishingChairpickup : UdonSharpBehaviour
{
    /*
     Dev Notes:
     Using a Menu is not mandatory
     Code removes the abillity to grab your own seat (infinite loop)
     */

    [UdonSynced(UdonSyncMode.None)]
    uint synch_mem = 2;
    [Tooltip("Prevents seated user from grabbing selected collider")]
    public Collider Pickup_Collider;

    [Space(3)]
    [Tooltip("Events are swapped when entering/exiting")]
    public bool Invert_Invisible = false;
    [Tooltip("Disables Mesh renderer when triggered")]
    public MeshRenderer[] Turn_Invisible = new MeshRenderer[1];
    [Space(3)]
    [Tooltip("This object is enabled for seated player/user")]
    public GameObject Optional_Menu;

    [Header("Synching")]
    [Tooltip("All players in world are affected.")]
    public bool Global_Synched = true;
    [Tooltip("Players who join will see what others see. \r\n(If set to Global_Synched)")]
    public bool Late_Join_Synched = true;

    [Header("Events")]
    public bool EventInteract = true;
    public bool EventOnTriggerEnter = false;
    public bool EventOnTriggerExit = false;

    void Interact() { if (EventInteract) { SendCustomEvent("Use_Station"); } }
    void OnTriggerEnter(Collider other) { if (EventOnTriggerEnter) { SendCustomEvent("Use_Station"); } }
    void OnTriggerExit(Collider other) { if (EventOnTriggerExit) { SendCustomEvent("Use_Station"); } }

    void OnPickup() { SendCustomEvent("Set_Owner"); }
    //void OnDrop() { SendCustomEvent(""); }

    void OnStationEntered() { SendCustomEvent("Hide"); }
    void OnStationExited() { SendCustomEvent("Show"); }

    public void Use_Station()
    {
        VRC_Pickup temp = (VRC_Pickup)this.gameObject.GetComponent(typeof(VRC_Pickup));
        if (temp != null && temp.IsHeld && temp.currentLocalPlayer == Networking.LocalPlayer) /*Eliminates grab-seat infinite physics loop*/
        { temp.Drop(); }
        Networking.LocalPlayer.UseAttachedStation();
    }

    public void Set_Owner()
     { Networking.SetOwner(Networking.LocalPlayer, this.gameObject); }

    public void Start()
    {
        if (Networking.LocalPlayer == null)
        { Global_Synched = false; }
        if (synch_mem == 0)
        { SendCustomEvent("OFF"); }
        else if (synch_mem == 1)
        { SendCustomEvent("ON"); }
    }

    public void Hide()
    {
        if(Pickup_Collider!=null)
         { Pickup_Collider.enabled = false; }
        if(Optional_Menu!=null)
         { Optional_Menu.SetActive(true); }
        if (Global_Synched)
        {
            if (Late_Join_Synched) { synch_mem = 0; }
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "OFF");
        }
        else
        { SendCustomEvent("OFF"); }
    }

    public void Show()
    {
        if (Pickup_Collider != null)
         { Pickup_Collider.enabled = true; }
        if (Optional_Menu != null)
         { Optional_Menu.SetActive(false); }
        if (Global_Synched)
        {
            if (Late_Join_Synched) { synch_mem = 1; }
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ON");
        }
        else
        { SendCustomEvent("ON"); }
    }

    public void ON()
    {
        for (int i = 0; i < Turn_Invisible.Length; i++)
        {
            if (Turn_Invisible[i] != null)
            {
                if (Invert_Invisible) { Turn_Invisible[i].enabled = false; }
                else { Turn_Invisible[i].enabled = true; }
            }
        }
    }

    public void OFF()
    {
        for (int i = 0; i < Turn_Invisible.Length; i++)
        {
            if (Turn_Invisible[i] != null)
            {
                if (Invert_Invisible) { Turn_Invisible[i].enabled = true; }
                else { Turn_Invisible[i].enabled = false; }
            }
        }
    }
}

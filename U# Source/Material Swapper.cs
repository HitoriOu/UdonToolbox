
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MaterialSwapper : UdonSharpBehaviour
{
    /*Dev Notes:
     * Both arrays has to be same lenght
     */

    Material[] Mat_memmory;

    [Tooltip("List A of Swap targets")]
    public MeshRenderer[] Swap_Target_A= new MeshRenderer[1];
    [Tooltip("List B of Swap targets")]
    public MeshRenderer[] Swap_Target_B = new MeshRenderer[1];

    [UdonSynced(UdonSyncMode.None)]
    bool Swapped = true;

    [Header("Synching")]
    [Tooltip("All players in world are affected.")]
    public bool Global_Synched = false;
    [Tooltip("Players who join will see what others see. \r\n(If set to Global_Synched)")]
    public bool Late_Join_Synched = false;

    [Header("Events")]
    public bool EventInteract = true;
    public bool EventOnCollisionEnter = false;
    public bool EventOnCollisionExit = false;
    public bool EventOnTriggerEnter = false;
    public bool EventOnTriggerExit = false;

    void Interact() { if (EventInteract) { SendCustomEvent("Run"); } }
    void OnCollisionEnter(Collision other) { if (EventOnCollisionEnter) { SendCustomEvent("Run"); } }
    void OnCollisionExit(Collision other) { if (EventOnCollisionExit) { SendCustomEvent("Run"); } }
    void OnTriggerEnter(Collider other) { if (EventOnTriggerEnter) { SendCustomEvent("Run"); } }
    void OnTriggerExit(Collider other) { if (EventOnTriggerExit) { SendCustomEvent("Run"); } }

    void Start()
    {
        if (Networking.LocalPlayer == null)
        { Global_Synched = false; }
        if (Global_Synched&&Late_Join_Synched&&Swapped)
        { SendCustomEvent("Swap_All"); }
    }

    public void Run()
    {
        if (Global_Synched)
        {
            if (Late_Join_Synched) { Swapped = !Swapped; }
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Swap_All");
        }
        else
        { SendCustomEvent("Swap_All"); }       
    }

    private void swap_materials(MeshRenderer A, MeshRenderer B)
    {
        if (A != null && B != null)
        {
            Mat_memmory = A.materials;
            A.materials = B.materials;
            B.materials = Mat_memmory;
        }
    }

    public void Swap_All()
    {
        if (Swap_Target_B != null && Swap_Target_A != null && Swap_Target_B.Length == Swap_Target_A.Length)
        {
            for (uint i = 0; i < Swap_Target_A.Length; i++)
            { swap_materials(Swap_Target_A[i], Swap_Target_B[i]); }
        }
    }
}

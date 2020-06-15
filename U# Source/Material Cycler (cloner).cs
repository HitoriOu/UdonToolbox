
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MaterialCyclercloner : UdonSharpBehaviour
{
    [Tooltip("Where in list to start getting materials from")]
    public int Source_Start_Index = 0;
    [Tooltip("List of objects to grab material from")]
    public MeshRenderer[] Source_Objects=new MeshRenderer[1];
    [Tooltip("List of object to put the material on")]
    public MeshRenderer[] Targets = new MeshRenderer[1];
    [UdonSynced(UdonSyncMode.None)] int Start_memory = -1;

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
        if (Source_Start_Index >= Source_Objects.Length|| Source_Start_Index<0)
         { Source_Start_Index = 0; }
        if (Start_memory != -1)//&& Global_Synched && Late_Join_Synched //implicit verified by Start_memory
        {
            if (0 == Start_memory) { Source_Start_Index = Source_Objects.Length-1; }
            else { Source_Start_Index = Start_memory - 1; }
            SendCustomEvent("Cycle");
        }
    }

    public void Run()
    {
        if (Global_Synched)
        { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Cycle"); }
        else
        { SendCustomEvent("Cycle"); }
    }

    public void Cycle()
    {
        for(int i=0;i<Targets.Length;i++)
        {
            if (Targets[i] != null && Source_Objects[Source_Start_Index] != null)
            { Targets[i].materials = Source_Objects[Source_Start_Index].materials; }
        }
        if(Source_Objects.Length<=1+Source_Start_Index) { Source_Start_Index = 0; }
        else
         { Source_Start_Index = Source_Start_Index + 1; }
        if (Global_Synched && Late_Join_Synched) { Start_memory = Source_Start_Index; }
    }
}


using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TeleportObject : UdonSharpBehaviour
{
    ushort count_mem = 0;
    float time_mem = 0;

    [Tooltip("Add multiple to cycle use one at a time")]
    public Transform[] Teleport_To = new Transform[1];
    [Tooltip("Select to have destination picked at random from Teleport_To list")]
    public bool Random = false;
    [Tooltip("Prevents spamming or infinite looping")]
    public float Cooldown = 0;

    [Header("Events")]
    public bool EventOnCollisionEnter = true;
    public bool EventOnCollisionExit = false;
    public bool EventOnTriggerEnter = true;
    public bool EventOnTriggerExit = false;

    void OnCollisionEnter(Collision other) { if (EventOnCollisionEnter) { Teleport_Collision(other); } }
    void OnCollisionExit(Collision other) { if (EventOnCollisionExit) { Teleport_Collision(other); } }
    void OnTriggerEnter(Collider other) { if (EventOnTriggerEnter) { Teleport_Collider(other); } }
    void OnTriggerExit(Collider other) { if (EventOnTriggerExit) { Teleport_Collider(other); } }

    private void Teleport_Collider(Collider other)
    {
        if (Teleport_To.Length != 0 && Teleport_To[0] != null && time_mem < Time.time)
        {
            time_mem = Time.time + Cooldown;
            if (Random)
            {
                int rand = (int)UnityEngine.Random.Range((int)0, (int)Teleport_To.Length);/*rand min/max are different compare*/
                if(Teleport_To[rand].position != other.transform.position)/*no infinite loop*/
                {
                    other.gameObject.transform.position = Teleport_To[rand].position;
                    other.gameObject.transform.rotation = Teleport_To[rand].rotation;
                }
            }
            else
            {
                if (Teleport_To[count_mem].position != other.transform.position)/*no infinite loop*/
                {
                    other.gameObject.transform.position = Teleport_To[count_mem].position;
                    other.gameObject.transform.rotation = Teleport_To[count_mem].rotation;
                    if (count_mem + 1 >= Teleport_To.Length)
                    { count_mem = 0; }
                    else
                    { count_mem++; }
                }
            }
        }
    }

    private void Teleport_Collision(Collision other)
    {
        if (Teleport_To.Length != 0 && Teleport_To[0] != null && time_mem < Time.time)
        {
            time_mem = Time.time + Cooldown;
            if (Random)
            {
                int rand = (int)UnityEngine.Random.Range((int)0, (int)Teleport_To.Length);

                if (Teleport_To[rand].position != other.transform.position)/*no infinite loop*/
                {
                    other.gameObject.transform.position = Teleport_To[rand].position;
                    other.gameObject.transform.rotation = Teleport_To[rand].rotation;
                }
            }
            else
            {
                if (Teleport_To[count_mem].position != other.transform.position)/*no infinite loop*/
                {
                    other.gameObject.transform.position = Teleport_To[count_mem].position;
                    other.gameObject.transform.rotation = Teleport_To[count_mem].rotation;
                    if (count_mem + 1 >= Teleport_To.Length)
                    { count_mem = 0; }
                    else
                    { count_mem++; }
                }
            }
        }
    }
}

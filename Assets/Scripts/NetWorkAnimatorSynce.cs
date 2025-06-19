using Unity.Netcode;
using UnityEngine;

public class NetworkAnimatorSync : NetworkBehaviour
{
    public Animator animator;


    private NetworkVariable<float> speed = new NetworkVariable<float>();
    void Update()
    {
        if (IsOwner)
        {
            float currentSpeed = 2;
            if (Mathf.Abs(currentSpeed - speed.Value) > 0.01f)
            {
                speed.Value = currentSpeed;
            }
        }
        animator.SetFloat("Speed", speed.Value);
    }
}
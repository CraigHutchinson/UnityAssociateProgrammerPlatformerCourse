using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefactorEnemy : MonoBehaviour
{
    public bool slipping = false;
   
    public Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        // ChaseBehavior is disabled by default
        ChaseBehavior chaseBehavior = GetComponent<ChaseBehavior>();
        if (chaseBehavior != null)
        {
            chaseBehavior.enabled = false;
        }
    }
    private void Update()
    {
        // Note: Idle behavior moved to PatrolBehavior
        // Note: Chase behavior moved to ChaseBehavior

        // stops enemy from following player up the inaccessible slopes
        if (slipping == true)
        {
            transform.Translate(Vector3.back * 20 * Time.deltaTime, Space.World);
        }
    }
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == 9)
        {
            slipping = true;
        }
        else
        {
            slipping = false;
        }
    }

}
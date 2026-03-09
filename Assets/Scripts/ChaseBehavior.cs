using System.Collections;
using UnityEngine;

public class ChaseBehavior : MonoBehaviour
{
    [Tooltip("The transform that will lock onto the player once the enemy has spotted them.")]
    public Transform sight;

    [Tooltip("Blue explosion particles")]
    public GameObject enemyExplosionParticles;

    [Tooltip("How close the enemy needs to be to explode")]
    public float explodeDist = 2f;

    private GameObject player;
    private LocomotionController.Target target;

    void Awake()
    {
        target = new LocomotionController.Target
        {
            priority = 1,
            position = transform.position,
            onReached = null
        };
    }

    private void OnEnable()
    {
        LocomotionController loco = GetComponent<LocomotionController>();
        if (loco != null)
        {
            loco.Add(target);
        }
    }

    private void OnDisable()
    {
        LocomotionController loco = GetComponent<LocomotionController>();
        if (loco != null)
        {
            loco.Remove(target);
        }
    }

    void Update()
    {
        if (player == null)
            return;

        // Update sight position to track player
        if (sight != null)
        {
            sight.position = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
        }
        
        // Look at the player
        transform.LookAt(sight != null ? sight : player.transform);
        
        // Update target position for LocomotionController
        target.position = player.transform.position;

        // Check if close enough to explode
        if (Vector3.Distance(transform.position, player.transform.position) < explodeDist)
        {
            
            // Disable locomotion and this behavior
            LocomotionController loco = GetComponent<LocomotionController>();
            if (loco != null)
                loco.enabled = false;
            
            StartCoroutine(Explode());
        }
    }

    public void SetPlayer(GameObject playerObject)
    {
        player = playerObject;
    }

    private IEnumerator Explode()
    {
        try
        {
            GameObject particles = Instantiate(enemyExplosionParticles, transform.position, new Quaternion());
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to instantiate explosion particles: " + e.Message);
        }
        yield return new WaitForSeconds(0.2f);
        Destroy(transform.parent.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        //start chasing if the player gets close enough
        if (other.gameObject.tag == "Player")
        {
            // ChaseBehavior is enabled when the enemy is chasing the player
            SetPlayer(other.gameObject);
            enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //stop chasing if the player gets far enough away
        if (other.gameObject.tag == "Player")
        {
            // ChaseBehavior is disabled when the enemy stops chasing the player
            enabled = false;
        }
    }
}

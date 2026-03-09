using UnityEngine;
using UnityEngine.UIElements;

public class PatrolBehavior : MonoBehaviour
{

    [Tooltip("The transform to which the enemy will pace back and forth to.")]
    public Transform[] patrolPoints;

    public int currentPatrolPoint = 0;

    private LocomotionController.Target target;


    // Awake is called when the script instance is being loaded
    void Awake()
    {
        target = new LocomotionController.Target
        {
            priority = 0,
            position = patrolPoints[currentPatrolPoint].position,
            onReached = OnTargetReached
        };
    }

    private void OnEnable()
    {
        LocomotionController loco = GetComponent<LocomotionController>();
        loco.Add(target);
    }

    private void OnDisable()
    {
        LocomotionController loco = GetComponent<LocomotionController>();
        loco.Remove(target);
    }

    private void OnTargetReached(LocomotionController.Target target)
    {
        LocomotionController loco = GetComponent<LocomotionController>();
        
        //Move to next waypoint
        currentPatrolPoint++;
        if (currentPatrolPoint >= patrolPoints.Length)
            currentPatrolPoint = 0;

        //Apply the new target
        target.position = patrolPoints[currentPatrolPoint].position;
    }

}

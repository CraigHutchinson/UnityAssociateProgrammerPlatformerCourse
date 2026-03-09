using NUnit.Framework.Internal.Filters;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LocomotionController : MonoBehaviour
{
    public Stats stats;

    /// <summary>
    /// Contains tunable parameters to tweak the enemy's movement and behavior.
    /// </summary>
    [System.Serializable]
    public struct Stats
    {
        [Header("Enemy Settings")]
        [Tooltip("How fast the enemy walks (priority == 0).")]
        public float walkSpeed;

        [Tooltip("How fast the enemy runs after the player (priority > 0).")]
        public float chaseSpeed;

        [Tooltip("How fast the enemy turns in circles as they're walking (only when idle is true).")]
        public float rotateSpeed;

    }

    public class Target
    {
        public int priority;
        public Vector3 position;

        // Optional callback for when the target is reached. Called with the target as an argument.
        //TODO: Support area based trigger on target reached instead of just point based.
        public System.Action<Target> onReached;
    }

    private List<Target> locomotionTargets = new List<Target>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (locomotionTargets.Count == 0)
            return;

        //Find the highest priority target and move towards it
        Target target = locomotionTargets.Aggregate((a, b) => a.priority > b.priority ? a : b);
        Vector3 moveToPoint = target.position;
        float speed = target.priority > 0 ? stats.chaseSpeed : stats.walkSpeed;

        Vector3 newPosition = Vector3.MoveTowards(transform.position, moveToPoint, speed * Time.deltaTime);

        transform.position = newPosition;

        // MoveTowards returns the target position if the object is within one step of the target
        // - so we can check if we've reached the target by comparing the new position to the target position safely.
        if ( newPosition == moveToPoint)
        {
            target.onReached?.Invoke(target);
        }
    }
    public void Add( Target target)
    {
        locomotionTargets.Add(target);
    }

    public void Remove( Target target)
    {
        locomotionTargets.Remove(target);
    }
}
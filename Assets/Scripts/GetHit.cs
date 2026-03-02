using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GetHit : MonoBehaviour
{
    [Tooltip("Determines when the player is taking damage.")]
    public int hurtCounter = 0;

    [Tooltip("Determines when the player heals through collecting 10 coins.")]
    public int cointCounter = 0;

    [Tooltip("Determines how long till recovery.")]
    public float recoverDuration = 0.5f;

    private bool slipping = false;

    // Determine if player movement is inhibitted by hurt or slip
    internal bool moveInhibited => (slipping ? 1 : 0) + hurtCounter > 0;

    private PlayerMovement playerMovementScript;
    private Rigidbody rb;
    private Transform enemy;

    private void Start()
    {
        playerMovementScript = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        // stops the player from running up the slopes and skipping platforms
        if (slipping == true)
        {
            transform.Translate(Vector3.back * 20 * Time.deltaTime, Space.World);
        }
    }
    private void OnCollisionStay(Collision other)
    {
        // Slip logic
        if (other.gameObject.layer == 9)
        {
            OnSlipEnter();
        }

        // Hit a collider
        // Note: Triggers are separate callback
        switch (other.gameObject.tag)
        {
            //Hurt logic
            case "Enemy":
            case "Trap":
                enemy = other.gameObject.transform;
                rb.AddForce(enemy.forward * 100);
                rb.AddForce(transform.up * 50);

                //TODO: retrieve from object instead of hardcoding
                var damageTaken = 5;// enemy.GetComponent<Enemy>().damage;
                var damageDuration = 0.25f;// enemy.GetComponent<Enemy>().damageDuration;
                if (other.gameObject.tag == "Trap")
                {
                    damageTaken = 2;
                    damageDuration = 2.0f;
                }

                TakeDamage(damageTaken, damageDuration);
                break;

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "Coin":
                playerMovementScript.soundManager.PlayCoinSound();
                ScoreManager.score += 10;

                //TODO: Game settings should control 'healingCounsNeeded' this for gameplay tweaking
                var healingCounsNeeded = 5;
                if (++cointCounter % healingCounsNeeded == 0)
                {
                    var healingTaken = 25; //TODO: Game settings should control 'healingTaken' this for gameplay tweaking
                    Takehealing(healingTaken, 0/*TODO: healDuration*/ );
                }
                break;
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.layer == 9)
        {
            OnSlipExit();
        }
    }

    private void OnSlipEnter()
    {
        slipping = true;
        UpdatePlayerCanMove();
    }

    private void OnSlipExit()
    {
        slipping = false;
        UpdatePlayerCanMove();
    }

    private void UpdatePlayerCanMove()
    {
        playerMovementScript.playerStats.canMove = !moveInhibited;
    }

    private void Takehealing(float healingToTake, float healDuration )
    {
        //TODO: Support healDuration
        playerMovementScript.Heal(healingToTake);
    }

    private void TakeDamage( float damageToTake, float damageDuration)
    {
        float damageInterval = Mathf.Max(playerMovementScript.soundManager.hitSound.length / 2, 0.25f);

        StartCoroutine(Recover(damageInterval, damageToTake, damageDuration));

    }
    private IEnumerator Recover(float damageInterval, float damageToTake, float damageDuration)
    {
        ++hurtCounter;
        UpdatePlayerCanMove();

        int damageTicks = Mathf.CeilToInt(damageDuration / damageInterval);

        // Apply damage in intervals over the damage duration, playing the hit sound each time
        for (int i = 0; i < damageTicks; ++i)
        {
            playerMovementScript.Hurt(damageToTake / damageTicks);


            var timeToNextDamage = Mathf.Min(damageInterval, damageDuration);
            yield return new WaitForSeconds(timeToNextDamage);
            damageDuration -= timeToNextDamage;
        }

        // Finally recover from the hit after the damage duration has ended
        yield return new WaitForSeconds(recoverDuration);

        --hurtCounter;

        UpdatePlayerCanMove();
    }
}
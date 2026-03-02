using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GetHit : MonoBehaviour
{
    [Tooltip("Determines when the player is taking damage.")]
    public int hurtCounter = 0;

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

        //Hurt logic
        if (other.gameObject.tag == "Enemy")
        {
            enemy = other.gameObject.transform;
            rb.AddForce(enemy.forward * 1000);
            rb.AddForce(transform.up * 500);
            var damageTaken = 5;// enemy.GetComponent<Enemy>().damage;
            var damageDuration = 0.25f;// enemy.GetComponent<Enemy>().damageDuration;
            TakeDamage(damageTaken, damageDuration);
        }
        if (other.gameObject.tag == "Trap")
        {
            rb.AddForce(transform.forward * -1000);
            rb.AddForce(transform.up * 500);
            var damageTaken = 2;//other.gameObject.GetComponent<Trap>().damage;
            var damageDuration = 2.0f;// enemy.GetComponent<Trap>().damageDuration;
            TakeDamage(damageTaken, damageDuration);
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

    private void TakeDamage( float damageToTake, float damageDuration)
    {

        StartCoroutine(Recover(playerMovementScript.soundManager.hitSound, damageToTake, damageDuration));

    }
    private IEnumerator Recover(AudioClip soundFx, float damageToTake, float damageDuration)
    {
        var damageInterval = Mathf.Max( soundFx.length / 2, 0.25f);

        ++hurtCounter;
        UpdatePlayerCanMove();

        int damageTicks = Mathf.CeilToInt(damageDuration / damageInterval);

        // Apply damage in intervals over the damage duration, playing the hit sound each time
        for (int i = 0; i < damageTicks; ++i)
        {
            playerMovementScript.soundManager.Play(soundFx);

            playerMovementScript.TakeHealthDamage(damageToTake / damageTicks);


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
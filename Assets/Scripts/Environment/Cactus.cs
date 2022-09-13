using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cactus : MonoBehaviour
{
    public int damage;
    public float damageRate;

    private List<IDamagable> _thingsToDamage = new List<IDamagable>();

    private void Start ()
    {
        StartCoroutine(DealDamage());
    }

    private IEnumerator DealDamage ()
    {
        while(true)
        {
            for(int i = 0; i < _thingsToDamage.Count; i++)
            {
                _thingsToDamage[i].TakePhysicalDamage(damage);
            }

            yield return new WaitForSeconds(damageRate);
        }
    }

    private void OnCollisionEnter (Collision collision)
    {
        if(collision.gameObject.GetComponent<IDamagable>() != null)
        {
            _thingsToDamage.Add(collision.gameObject.GetComponent<IDamagable>());
        }
    }

    private void OnCollisionExit (Collision collision)
    {
        if(collision.gameObject.GetComponent<IDamagable>() != null)
        {
            _thingsToDamage.Remove(collision.gameObject.GetComponent<IDamagable>());
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Campfire : Building, IInteractable
{
    public GameObject particle;
    public GameObject light;
    private bool _isOn;
    private Vector3 lightStartPos;

    [Header("Damage")]
    public int damage;
    public float damageRate;

    private List<IDamagable> _thingsToDamage = new List<IDamagable>();

    private void Start()
    {
        lightStartPos = light.transform.localPosition;
        StartCoroutine(DealDamage());
    }

    private IEnumerator DealDamage()
    {
        while(true)
        {
            if(_isOn)
            {
                for(int x = 0; x < _thingsToDamage.Count; x++)
                    _thingsToDamage[x].TakePhysicalDamage(damage);
            }

            yield return new WaitForSeconds(damageRate);
        }
    }

    public string GetInteractPrompt()
    {
        return _isOn ? "Turn Off" : "Turn On";
    }

    public void OnInteract()
    {
        _isOn = !_isOn;

        particle.SetActive(_isOn);
        light.SetActive(_isOn);
    }

    private void Update()
    {
        if(_isOn)
        {
            float x = Mathf.PerlinNoise(Time.time * 3.0f, 0.0f) / 5.0f;
            float z = Mathf.PerlinNoise(0.0f, Time.time * 3.0f) / 5.0f;

            light.transform.localPosition = lightStartPos + new Vector3(x, 0.0f, z);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<IDamagable>() != null)
            _thingsToDamage.Add(other.gameObject.GetComponent<IDamagable>());
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.GetComponent<IDamagable>() != null)
            _thingsToDamage.Remove(other.gameObject.GetComponent<IDamagable>());
    }

    public override string GetCustomProperties ()
    {
        return _isOn.ToString();
    }
    
    public override void ReceiveCustomProperties(string props)
    {
        _isOn = props == "True" ? true : false;
        
        particle.SetActive(_isOn);
        light.SetActive(_isOn);
    }
}
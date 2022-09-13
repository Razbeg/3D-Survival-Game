using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipTool : Equip
{
    public float attackRate;
    private bool _attacking;
    public float attackDistance;

    [Header("Resource Gathering")]
    public bool doesGatherResources;

    [Header("Combat")]
    public bool doesDealDamage;
    public int damage;

    // components
    private Animator _anim;
    private Camera _cam;

    private void Awake ()
    {
        _anim = GetComponent<Animator>();
        _cam = Camera.main;
    }

    public override void OnAttackInput ()
    {
        if(!_attacking)
        {
            _attacking = true;
            _anim.SetTrigger("Attack");
            Invoke("OnCanAttack", attackRate);
        }
    }

    private void OnCanAttack ()
    {
        _attacking = false;
    }

    public void OnHit ()
    {
        Ray ray = _cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, attackDistance))
        {
            if(doesGatherResources && hit.collider.GetComponent<Resource>())
            {
                hit.collider.GetComponent<Resource>().Gather(hit.point, hit.normal);
            }
            if(doesDealDamage && hit.collider.GetComponent<IDamagable>() != null)
            {
                hit.collider.GetComponent<IDamagable>().TakePhysicalDamage(damage);
            }
        }
    }
}
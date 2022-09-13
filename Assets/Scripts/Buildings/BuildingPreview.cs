using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPreview : MonoBehaviour
{
    public Material canPlaceMaterial;
    public Material cannotPlaceMaterial;
    private MeshRenderer[] _meshRenderers;
    private List<GameObject> _collidingObjects = new List<GameObject>();

    private void Awake ()
    {
        _meshRenderers = transform.GetComponentsInChildren<MeshRenderer>();
    }

    public void CanPlace ()
    {
        SetMaterial(canPlaceMaterial);
    }

    public void CannotPlace ()
    {
        SetMaterial(cannotPlaceMaterial);
    }

    private void SetMaterial (Material mat)
    {
        for(int x = 0; x < _meshRenderers.Length; x++)
        {
            Material[] mats = new Material[_meshRenderers[x].materials.Length];

            for(int y = 0; y < mats.Length; y++)
            {
                mats[y] = mat;
            }

            _meshRenderers[x].materials = mats;
        }
    }

    public bool CollidingWithObjects ()
    {
        _collidingObjects.RemoveAll(x => x == null);
        return _collidingObjects.Count > 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer != 12)
            _collidingObjects.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.layer != 12)
            _collidingObjects.Remove(other.gameObject);
    }
}
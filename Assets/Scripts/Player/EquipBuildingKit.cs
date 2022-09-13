using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EquipBuildingKit : Equip
{
    public GameObject buildingWindow;
    private BuildingRecipe _curRecipe;
    private BuildingPreview _curBuildingPreview;

    public float placementUpdateRate = 0.03f;
    private float _lastPlacementUpdateTime;
    public float placementMaxDistance = 5.0f;

    public LayerMask placementLayerMask;

    public Vector3 placementPosition;
    private bool _canPlace;
    private float _curYRot;

    public float rotateSpeed = 180.0f;

    private Camera _cam;
    public static EquipBuildingKit instance;

    private void Awake()
    {
        instance = this;
        _cam = Camera.main;
    }

    private void Start()
    {
        buildingWindow = FindObjectOfType<BuildingWindow>(true).gameObject;
    }

    public override void OnAttackInput()
    {
        if(_curRecipe == null || _curBuildingPreview == null || !_canPlace)
            return;

        Instantiate(_curRecipe.spawnPrefab, _curBuildingPreview.transform.position, _curBuildingPreview.transform.rotation);

        for(int x = 0; x < _curRecipe.cost.Length; x++)
        {
            for(int y = 0; y < _curRecipe.cost[x].quantity; y++)
            {
                Inventory.instance.RemoveItem(_curRecipe.cost[x].item);
            }
        }

        _curRecipe = null;
        Destroy(_curBuildingPreview.gameObject);
        _curBuildingPreview = null;
        _canPlace = false;
        _curYRot = 0;
    }

    public override void OnAltAttackInput()
    {
        if(_curBuildingPreview != null)
            Destroy(_curBuildingPreview.gameObject);

        buildingWindow.SetActive(true);
        PlayerController.instance.ToggleCursor(true);
    }

    public void SetNewBuildingRecipe (BuildingRecipe recipe)
    {
        _curRecipe = recipe;
        buildingWindow.SetActive(false);
        PlayerController.instance.ToggleCursor(false);

        _curBuildingPreview = Instantiate(recipe.previewPrefab).GetComponent<BuildingPreview>();
    }

    private void Update()
    {
        if(_curRecipe != null && _curBuildingPreview != null && Time.time - _lastPlacementUpdateTime > placementUpdateRate)
        {
            _lastPlacementUpdateTime = Time.time;

            Ray ray = _cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit, placementMaxDistance, placementLayerMask))
            {
                _curBuildingPreview.transform.position = hit.point;
                _curBuildingPreview.transform.up = hit.normal;
                _curBuildingPreview.transform.Rotate(new Vector3(0, _curYRot, 0), Space.Self);

                if(!_curBuildingPreview.CollidingWithObjects())
                {
                    if(!_canPlace)
                        _curBuildingPreview.CanPlace();

                    _canPlace = true;
                }
                else
                {
                    if(_canPlace)
                        _curBuildingPreview.CannotPlace();

                    _canPlace = false;
                }
            }
        }

        if(Keyboard.current.rKey.isPressed)
        {
            _curYRot += rotateSpeed * Time.deltaTime;

            if(_curYRot > 360.0f)
                _curYRot = 0.0f;
        }    
    }

    private void OnDestroy()
    {
        if(_curBuildingPreview != null)
            Destroy(_curBuildingPreview.gameObject);
    }
}
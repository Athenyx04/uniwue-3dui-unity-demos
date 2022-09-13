using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdvancedObjectFactory : MonoBehaviour
{
    public Dropdown prefabObject;
    public GameObject CylinderPrefab;
    public GameObject CapsulePrefab;
    public GameObject BlockPrefab;
    public GameObject SpherePrefab;
    private GameObject SelectedPrefab;
    private int prefabType;
    private List<GameObject> BlocksCreated = new List<GameObject>();
    private List<GameObject> SpheresCreated = new List<GameObject>();
    private List<GameObject> CylindersCreated = new List<GameObject>();
    private List<GameObject> CapsulesCreated = new List<GameObject>();
    private List<GameObject> SelectedList;

    void Start()
    {
        SelectedPrefab = CylinderPrefab;
        SelectedList = CylindersCreated;

        prefabObject.onValueChanged.AddListener(delegate
        {
            prefabObjectHasChanged(prefabObject);
        }
        );
    }

    public void prefabObjectHasChanged(Dropdown sender)
    {
        prefabType = sender.value;
        switch (sender.value)
        {
            case 0:
                SelectedPrefab = CylinderPrefab;
                SelectedList = CylindersCreated;
                break;
            case 1:
                SelectedPrefab = SpherePrefab;
                SelectedList = SpheresCreated;
                break;
            case 2:
                SelectedPrefab = BlockPrefab;
                SelectedList = BlocksCreated;
                break;
            case 3:
                SelectedPrefab = CapsulePrefab;
                SelectedList = CapsulesCreated;
                break;
            default:
                Debug.LogError("Dropdown Selection Failed! Dropped back to Cylinder");
                SelectedPrefab = CylinderPrefab;
                SelectedList = CylindersCreated;
                break;
        }
    }

    public void CreatePrefab(Transform Where)
    {
        GameObject prefabInstance = Instantiate(SelectedPrefab, Where.position, Quaternion.identity);

        prefabInstance.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        SelectedList.Add(prefabInstance);
    }

    public void DestroyAllOfType()
    {
        foreach (var b in SelectedList)
        {
            Destroy(b);
        }
    }

    public void CreateBlockPrefab(Transform Where)
    {
        GameObject blockInstance = Instantiate(BlockPrefab, Where.position, Quaternion.identity);
 
        blockInstance.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        BlocksCreated.Add(blockInstance);
    }

    public void CreateSpherePrefab(Transform Where)
    {
        GameObject sphereInstance = Instantiate(SpherePrefab, Where.position, Quaternion.identity);

        sphereInstance.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        SpheresCreated.Add(sphereInstance);
    }

    public void DestroyAllCreatedBlocks()
    {
       foreach (var b in BlocksCreated)
        {
            Destroy(b);
        }
    }

    public void DestroyAllCreatedSpheres()
    {
        foreach (var s in SpheresCreated)
        {
            Destroy(s);
        }
    }

    public void DestroyEveythingCreated()
    {
        SelectedList = CylindersCreated;
        DestroyAllOfType();
        SelectedList = SpheresCreated;
        DestroyAllOfType();
        SelectedList = BlocksCreated;
        DestroyAllOfType();
        SelectedList = CapsulesCreated;
        DestroyAllOfType();
    }
}

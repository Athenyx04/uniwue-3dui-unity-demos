using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdvancedObjectFactory : MonoBehaviour
{
    // Dropdown menu selection
    public Dropdown prefabObject;

    private int prefabType;
    private GameObject SelectedPrefab;
    private List<GameObject> SelectedList;

    // Game object possible types and lists
    public GameObject CylinderPrefab;
    public GameObject CapsulePrefab;
    public GameObject BlockPrefab;
    public GameObject SpherePrefab;
    
    private List<GameObject> BlocksCreated = new List<GameObject>();
    private List<GameObject> SpheresCreated = new List<GameObject>();
    private List<GameObject> CylindersCreated = new List<GameObject>();
    private List<GameObject> CapsulesCreated = new List<GameObject>();

    void Start()
    {
        // Default to Cylinder
        SelectedPrefab = CylinderPrefab;
        SelectedList = CylindersCreated;

        // Create the listener for the Dropdown menu
        prefabObject.onValueChanged.AddListener(delegate
        {
            prefabObjectHasChanged(prefabObject);
        }
        );
    }

    public void prefabObjectHasChanged(Dropdown sender) // Whenever the selection changes, change the prefab
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

    public void CreatePrefab(Transform Where) // Create selected prefab
    {
        GameObject prefabInstance = Instantiate(SelectedPrefab, Where.position, Quaternion.identity);

        prefabInstance.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        SelectedList.Add(prefabInstance);
    }

    public void DestroyAllOfType() // Destroy all of selected prefab
    {
        foreach (var b in SelectedList)
        {
            Destroy(b);
        }
    }

    public void DestroyEveythingCreated() // Destroy every created prefab of this OF
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

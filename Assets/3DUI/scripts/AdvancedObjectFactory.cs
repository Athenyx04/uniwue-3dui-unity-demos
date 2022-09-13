using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AdvancedObjectFactory : MonoBehaviour
{
    public GameObject BlockPrefab;
    public GameObject SpherePrefab;
    private List<GameObject> BlocksCreated = new List<GameObject>();
    private List<GameObject> SpheresCreated = new List<GameObject>();

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
        DestroyAllCreatedBlocks();
        DestroyAllCreatedSpheres();
    }
}

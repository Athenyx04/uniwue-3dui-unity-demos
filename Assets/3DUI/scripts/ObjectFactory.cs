using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObjectFactory : MonoBehaviour
{
    public GameObject BlockPrefab;
    private List<GameObject> BlocksCreated = new List<GameObject>();

    public void CreateBlockPrefab(Transform Where)
    {
        GameObject blockInstance = Instantiate(BlockPrefab, Where.position, Quaternion.identity);
 
        blockInstance.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        BlocksCreated.Add(blockInstance);
    }

    public void DestroyAllCreatedBlocks()
    {
       foreach (var b in BlocksCreated)
        {
            Destroy(b);
        }
    }
}

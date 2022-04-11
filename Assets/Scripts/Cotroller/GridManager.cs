using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ground")
        {
            gameObject.GetComponent<MeshRenderer>().enabled = true;
            Debug.Log("Touch Ground");
        }
        
    }
}

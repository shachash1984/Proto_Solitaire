using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUtils : MonoBehaviour {

    GameObject[] children;
	// Use this for initialization
	void Start () {
        children = GetChildrenGameObjects(this.transform, true).ToArray();
        foreach (GameObject go in children)
        {
            print("Name: " + go.name + " Parent: " + go.transform.parent);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    
    List<GameObject> GetChildrenGameObjects(Transform parentTransform, bool includeParent = false)
    {
        List<GameObject> children = new List<GameObject>();
        if (includeParent)        
            children.Add(parentTransform.gameObject);                   
        foreach (Transform t in parentTransform)
        {
            children.Add(t.gameObject);
            if (t.childCount > 0 && t != this.transform)            
                children.AddRange(GetChildrenGameObjects(t));                           
        }
        return children;
    }
}

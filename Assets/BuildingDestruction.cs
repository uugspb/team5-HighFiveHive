using UnityEngine;
using System.Collections;

public class BuildingDestruction : MonoBehaviour {

    private GameObject ground;

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name != "Ground")
            StartCoroutine(Destruct());
    }

    IEnumerator Destruct()
    {
        yield return new WaitForSeconds(1f);

        this.GetComponent<BoxCollider>().enabled = false;

        yield return new WaitForSeconds(5f);

        GameObject.Destroy(this.gameObject);
    }

}

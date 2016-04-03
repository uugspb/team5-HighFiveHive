using UnityEngine;
using System.Collections;

public class CannonButton : MonoBehaviour {

    public Transform cannonPlace;
    public GameObject bulletPrefab;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider other)
    {
        GameObject.Instantiate<GameObject>(bulletPrefab).transform.position = cannonPlace.position;
    }
}

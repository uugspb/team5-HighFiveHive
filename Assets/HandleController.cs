using UnityEngine;
using System.Collections;

public class HandleController : MonoBehaviour {

    public Transform robot;
    public float speed;

    private bool triggering;
    private Transform hand;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        float push = (this.transform.localPosition.z - 2.1f) / 0.4f;

        this.transform.localPosition = this.transform.localPosition.x * Vector3.right +
            this.transform.localPosition.y * Vector3.up + Mathf.Lerp(this.transform.localPosition.z, 2.1f, Time.deltaTime) * Vector3.forward;

        robot.localPosition += Vector3.forward * push * speed;

        if (triggering)
            this.transform.localPosition = this.transform.localPosition.x * Vector3.right +
            this.transform.localPosition.y * Vector3.up + Mathf.Clamp(hand.transform.localPosition.z, 2.1f, 2.5f) * Vector3.forward;
    }

    void OnTriggerEnter(Collider other)
    {
        hand = other.transform;
        triggering = true;
    }

    void OnTriggerExit(Collider other)
    {
        triggering = false;
    }
}

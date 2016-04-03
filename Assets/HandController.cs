using UnityEngine;
using System.Collections;

public class HandController : MonoBehaviour {

    public Transform leftHandPivot;
    public Transform rightHandPivot;

    public Transform leftHand;
    public Transform rightHand;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        leftHand.localPosition = Vector3.Lerp(
            leftHand.localPosition,
            (leftHandPivot.localPosition.x / 6f) * Vector3.right + (leftHandPivot.localPosition.y / 6f) * Vector3.up + (leftHandPivot.localPosition.z / 3f) * Vector3.forward + /* - 4f * Vector3.forward*/ - 0.2f * Vector3.up - 1.2f * Vector3.forward,
            15f * Time.deltaTime
        );

        leftHand.localRotation = Quaternion.Lerp(
            leftHand.localRotation,
            leftHandPivot.localRotation,
            15f * Time.deltaTime
        );

        Debug.Log(leftHand.transform.localPosition);
    }

}

using UnityEngine;
using System.Collections;

public class OtherPlayer : Player {
    public string playerName {
        set {

        }
    }
    public float health {
        set {

        }
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.localRotation = Quaternion.AngleAxis(yaw / Mathf.PI * 180f, Vector3.up);
        pitchNode.transform.localRotation = Quaternion.AngleAxis(-pitch / Mathf.PI * 180f, Vector3.right);
        transform.position = p;
    }
}

using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour {
    public Vector3 target;
    public float speed;
    public Vector3 start;
    Vector3 delta;
    bool traveling = true;
    float duration;
    Vector3 p;
    float elapsed = 0;
    float arrowLength = 4f;

	// Use this for initialization
	void Start () {
        start = transform.position;
        var dir = (target - start);
        var dist = dir.magnitude - arrowLength;
        dir.Normalize();
        delta = dir * dist;
        duration = dist / speed;
        transform.LookAt(target);
	}
	
	// Update is called once per frame
	void Update () {
        elapsed += Time.deltaTime;
        if (traveling)
        {
            float progress = Mathf.Min(1f, elapsed / duration);
            p = start + delta * progress;
            transform.position = p;

            if (progress == 1f)
            {
                // do some effect
                traveling = false;
            }
        }
        else
        {
            if (elapsed - duration > 10f)
            {
                Destroy(gameObject);
            }
        }
	}
}

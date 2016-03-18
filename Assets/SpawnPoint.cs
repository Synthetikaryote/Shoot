using UnityEngine;
using System.Collections;
using System;

public class SpawnPoint : MonoBehaviour {

    public GameObject objectToSpawn;
    public float interval;
    public GameObject spawnedObject;
    float elapsed = 0f;

	// Use this for initialization
	void Start () {
        elapsed = interval;
	}
	
	// Update is called once per frame
	void Update () {
        if (spawnedObject == null && objectToSpawn != null) {
            elapsed += Time.deltaTime;
            if (elapsed > interval) {
                elapsed -= interval;
                spawnedObject = (GameObject)GameObject.Instantiate(objectToSpawn, transform.position, Quaternion.identity);
            }
        }
	}
}

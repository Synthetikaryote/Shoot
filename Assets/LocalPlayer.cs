using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class LocalPlayer : Player {
    public float SensitivityX = 2.0f;
    public float SensitivityY = 2.0f;


	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButton(1)) {
            if (cameraYaw != 0f) {
                yaw += cameraYaw;
                cameraYaw = 0f;
                yawNode.transform.localRotation = Quaternion.identity;
            }
            yaw += Input.GetAxisRaw("Mouse X") / 180f * Mathf.PI * SensitivityX;
            pitch = Mathf.Clamp(pitch + Input.GetAxisRaw("Mouse Y") / 180f * Mathf.PI * SensitivityY, -Mathf.PI + 0.01f, Mathf.PI - 0.01f);
            transform.localRotation = Quaternion.AngleAxis(yaw / Mathf.PI * 180f, Vector3.up);
            pitchNode.transform.localRotation = Quaternion.AngleAxis(-pitch / Mathf.PI * 180f, Vector3.right);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

        }
        //else if (Input.GetMouseButton(0))
        //{
        //    cameraYaw += Input.GetAxisRaw("Mouse X") / 180f * Mathf.PI * SensitivityX;
        //    pitch = Mathf.Clamp(pitch + Input.GetAxisRaw("Mouse Y") / 180f * Mathf.PI * SensitivityY, -Mathf.PI + 0.01f, Mathf.PI - 0.01f);
        //    yawNode.transform.localRotation = Quaternion.AngleAxis(cameraYaw / Mathf.PI * 180f, Vector3.up);
        //    pitchNode.transform.localRotation = Quaternion.AngleAxis(-pitch / Mathf.PI * 180f, Vector3.right);
        //    Cursor.visible = false;
        //}
        else {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (isOnGround && Input.GetKey(KeyCode.Space)) {
            vy = 30f;
            isOnGround = false;
        }

        string animation = "idle";
        if (isOnGround && (Input.GetMouseButton(0) || shootTimeLeft > 0f)) {
            if (shootTimeLeft == 0f) {
                targetModelYaw = 0f;
                shootTimeLeft = ani["attack 1"].length / ani["attack 1"].speed;
                ani["attack 1"].time = 0f;
                shootPending = true;
            }
            shootTimeLeft -= Time.deltaTime;
            animation = "attack 1";
            Blend("attack 1", 0.1f);
            if (shootPending && shootTimeLeft <= 0.4f) {
                // do the shoot
                shootPending = false;
                var cam = Camera.main.transform;
                RaycastHit shootHit;
                Physics.Raycast(cam.position, cam.forward, out shootHit, 1000f);
                if (shootHit.distance > 0f) {
                    var arrowGO = (GameObject)Instantiate(arrowPrefab, arrowStart.position, Quaternion.identity);
                    Arrow arrowScript = arrowGO.GetComponent<Arrow>();
                    Vector3 dir = shootHit.point - arrowStart.position;
                    dir.Normalize();
                    arrowScript.direction = dir;
                    arrowScript.speed = 250f;
                    arrowScript.damage = damage;
                    arrowStart.gameObject.GetComponent<AudioSource>().Play();
                }
            }
            if (shootTimeLeft <= 0f) {
                shootTimeLeft = 0f;
            }
        }

        bool running = !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        if (!isOnGround) {
            CancelShoot();
            vy -= 50f * Time.deltaTime;
            animation = "run";
            ani["run"].speed = 0.2f;
        }
        else if (stunDuration > 0f) {
            stunDuration -= Time.deltaTime;
            v = Vector3.zero;
            if (hitPoints > 0) {
                animation = "hit 2";
            }
            else {
                animation = "death";
            }
        }
        else {
            v.x = Input.GetAxisRaw("Horizontal");
            v.z = Input.GetAxisRaw("Vertical");
            if (v.sqrMagnitude > 0) {
                CancelShoot();
                v.Normalize();
                targetModelYaw = 2f * Mathf.PI - (Mathf.Atan2(v.z, v.x) - Mathf.PI * 0.5f);
                v = new Vector3(v.z * Mathf.Sin(yaw) + v.x * Mathf.Cos(-yaw), v.y, v.z * Mathf.Cos(yaw) + v.x * Mathf.Sin(-yaw));

                animation = running ? "run" : "walk";
            }
        }
        float xzScale = running ? 20f : 4.5f;
        p.x += xzScale * v.x * Time.deltaTime;
        p.y += vy * Time.deltaTime;
        p.z += xzScale * v.z * Time.deltaTime;

        float diff = targetModelYaw - modelYaw;
        if (diff < -Mathf.PI) diff += Mathf.PI * 2f;
        if (diff > Mathf.PI) diff -= Mathf.PI * 2f;
        modelYaw += diff * ((Mathf.Pow(1.5f, Time.deltaTime * 20f) - 1) / (0.5f));
        modelNode.transform.localRotation = Quaternion.AngleAxis(modelYaw / Mathf.PI * 180f, Vector3.up);

        Blend(animation, 0.1f);

        RaycastHit hit;
        if (Terrain.activeTerrain.GetComponent<Collider>().Raycast(new Ray(transform.position + new Vector3(0f, 50f, 0f), -Vector3.up), out hit, 100.0f)) {
            if (p.y < hit.point.y || isOnGround) {
                p.y = hit.point.y;
                isOnGround = true;
                vy = 0f;
                ani["run"].speed = 1.0f;
            }
        }
        transform.position = p;

        if (uber.isConnected) {
            uber.server.UpdateState(state);
        }
    }

    void CancelShoot() {
        shootTimeLeft = 0f;
        shootPending = false;
    }
}

using UnityEngine;
using System.Collections;


public class Player : MonoBehaviour {
    public float SensitivityX = 2.0f;
    public float SensitivityY = 2.0f;
    float cameraYaw = 0f;
    float yaw = 0f;
    float pitch = 0f;
    Transform yawNode;
    Transform pitchNode;
    Animation ani;
    Vector3 p;
    bool isOnGround = true;
    Vector3 v = Vector3.zero;
    float vy = 0f;
    string lastAnimation = null;

	// Use this for initialization
	void Start () {
        yawNode = transform.FindChild("YawNode");
        pitchNode = yawNode.FindChild("PitchNode");
        ani = transform.FindChild("Elf_noArrow").gameObject.GetComponent<Animation>();
        p = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButton(1))
        {
            if (cameraYaw != 0f)
            {
                yaw += cameraYaw;
                cameraYaw = 0f;
                yawNode.transform.localRotation = Quaternion.identity;
            }
            yaw += Input.GetAxisRaw("Mouse X") / 180f * Mathf.PI * SensitivityX;
            pitch = Mathf.Clamp(pitch + Input.GetAxisRaw("Mouse Y") / 180f * Mathf.PI * SensitivityY, -Mathf.PI + 0.01f, Mathf.PI - 0.01f);
            transform.localRotation = Quaternion.AngleAxis(yaw / Mathf.PI * 180f, Vector3.up);
            pitchNode.transform.localRotation = Quaternion.AngleAxis(-pitch / Mathf.PI * 180f, Vector3.right);
            Cursor.visible = false;
        }
        else if (Input.GetMouseButton(0))
        {
            cameraYaw += Input.GetAxisRaw("Mouse X") / 180f * Mathf.PI * SensitivityX;
            pitch = Mathf.Clamp(pitch + Input.GetAxisRaw("Mouse Y") / 180f * Mathf.PI * SensitivityY, -Mathf.PI + 0.01f, Mathf.PI - 0.01f);
            yawNode.transform.localRotation = Quaternion.AngleAxis(cameraYaw / Mathf.PI * 180f, Vector3.up);
            pitchNode.transform.localRotation = Quaternion.AngleAxis(-pitch / Mathf.PI * 180f, Vector3.right);
            Cursor.visible = false;
        }
        else {
            Cursor.visible = true;
        }

        if (isOnGround && Input.GetKey(KeyCode.Space))
        {
            vy = 30f;
            isOnGround = false;
        }

        bool running = !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        if (!isOnGround)
        {
            vy -= 50f * Time.deltaTime;
            Blend("run", 0.1f);
            ani["run"].speed = 0.2f;
        }
        else
        {
            v.x = 0f;
            v.z = 0f;
            if (Input.GetKey(KeyCode.Period)) v.z += 1f;
            if (Input.GetKey(KeyCode.E)) v.z -= 1f;
            if (Input.GetKey(KeyCode.O)) v.x -= 1f;
            if (Input.GetKey(KeyCode.U)) v.x += 1f;
            if (v.sqrMagnitude > 0)
            {
                v.Normalize();
                v = new Vector3(v.z * Mathf.Sin(yaw) + v.x * Mathf.Cos(-yaw), v.y, v.z * Mathf.Cos(yaw) + v.x * Mathf.Sin(-yaw));
                

                Blend(running ? "run" : "walk", 0.1f);
            }
            else
            {
                Blend("idle", 0.1f);
            }
        }
        float xzScale = running ? 20f : 4.5f;
        p.x += xzScale * v.x * Time.deltaTime;
        p.y += vy * Time.deltaTime;
        p.z += xzScale * v.z * Time.deltaTime;

        RaycastHit hit;
        Terrain.activeTerrain.GetComponent<Collider>().Raycast(new Ray(transform.position + new Vector3(0f, 50f, 0f), -Vector3.up), out hit, 100.0f);
        if (hit.distance > 0f)
        {
            if (p.y < hit.point.y || isOnGround)
            {
                p.y = hit.point.y;
                isOnGround = true;
                vy = 0f;
                ani["run"].speed = 1.0f;
            }
        }
        transform.position = p;
    }

    void Blend(string targetAnimation, float time)
    {
        if (lastAnimation != null)
        {
            ani.Blend(lastAnimation, 0.0f, time);
        }
        ani.Blend(targetAnimation, 1.0f, time);
        lastAnimation = targetAnimation;
    }
}

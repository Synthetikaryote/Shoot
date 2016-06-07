using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class Player : MonoBehaviour {
    public GameObject arrowPrefab;
    public float damage = 50f;
    public float hitPoints = 100f;
    protected float yaw = 0f;
    protected float cameraYaw = 0f;
    protected float modelYaw = 0f;
    protected float targetModelYaw = 0f;
    protected float pitch = 0f;
    protected Transform yawNode;
    protected Transform pitchNode;
    protected Transform modelNode;
    protected Transform arrowStart;
    protected Animation ani;
    protected Vector3 p;
    protected bool isOnGround = true;
    protected Vector3 v = Vector3.zero;
    protected float vy = 0f;
    protected string lastAnimation = null;
    protected float shootTimeLeft = 0f;
    protected bool shootPending = false;
    protected float stunDuration = 0f;
    public delegate void OnSoundDelegate(Vector3 loc);
    public OnSoundDelegate OnSound;
    public int killCount = 0;

	// Use this for initialization
	void Awake() {
        yawNode = transform.FindChild("YawNode");
        pitchNode = yawNode.FindChild("PitchNode");
        modelNode = transform.FindChild("Elf_noArrow");
        arrowStart = transform.FindChild("ArrowStart");
        ani = modelNode.gameObject.GetComponent<Animation>();
        p = transform.position;
        ani["attack 1"].speed = 2.5f;
	}
	
	// Update is called once per frame
	void Update() {

    }

    protected void Blend(string targetAnimation, float time) {
        if (lastAnimation != null) {
            ani.Blend(lastAnimation, 0.0f, time);
        }
        ani.Blend(targetAnimation, 1.0f, time);
        lastAnimation = targetAnimation;
    }

    public void GotHit(float damage) {
        hitPoints = Mathf.Max(0f, hitPoints - damage);
        if (hitPoints > 0f) {
            stunDuration = 0.5f;
        }
        else {
            stunDuration = 20f;
        }
    }

    public void GotKill() {
        ++killCount;
        GameObject.Find("KillCount").GetComponent<Text>().text = "Kill Count: " + killCount;
    }
}

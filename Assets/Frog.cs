using UnityEngine;
using System.Collections;

public class Frog : MonoBehaviour {
    public float walkDistance = 100f;
    public float runDistance = 30f;
    public float walkSpeed = 2f;
    public float runSpeed = 7f;
    public float hitPoints = 100f;
    Vector3 p;
    GameObject player;
    Animation ani;
    string lastAnimation = null;
    float stunDuration = 0f;

    // Use this for initialization
    void Start () {
        p = transform.position;
        player = GameObject.Find("Player");
        ani = transform.FindChild("Stone_Frog_Green").GetComponent<Animation>();
	}
	
	// Update is called once per frame
	void Update () {
        if (stunDuration > 0f)
        {
            stunDuration = Mathf.Max(0f, stunDuration - Time.deltaTime);
        }
        else {
            if ((player.transform.position - p).sqrMagnitude < walkDistance * walkDistance)
            {
                transform.LookAt(player.transform);
                Vector3 dir = player.transform.position - p;
                dir.Normalize();
                p += dir * walkSpeed * Time.deltaTime;
                Blend("Walk", 0.1f);
            }

            RaycastHit hit;
            if (Terrain.activeTerrain.GetComponent<Collider>().Raycast(new Ray(transform.position + new Vector3(0f, 50f, 0f), -Vector3.up), out hit, 100.0f))
            {
                p.y = hit.point.y;
            }
            transform.position = p;
        }
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

    void GotHit(float damage)
    {
        hitPoints = Mathf.Max(0f, hitPoints - damage);
        if (hitPoints > 0f)
        {
            Blend("Take Damage1", 0.1f);
            stunDuration = 1f;
        }
        else
        {
            Blend("Death", 0.1f);
            stunDuration = 20f;
            GameObject.Destroy(gameObject, 20f);
        }
    }
}

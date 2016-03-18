using UnityEngine;
using System.Collections;

public class Frog : MonoBehaviour {
    public float walkDistance = 70f;
    public float runDistance = 30f;
    public float attackDistance = 5f;
    public float walkSpeed = 2f;
    public float runSpeed = 7f;
    public float hitPoints = 100f;
    public float damage = 5f;
    Vector3 p;
    GameObject player;
    Animation ani;
    string lastAnimation = null;
    float stunDuration = 0f;
    float attackDelay = 0f;
    Collider terrainCollider;

    // Use this for initialization
    void Start () {
        p = transform.position;
        player = GameObject.Find("Player");
        ani = transform.FindChild("Stone_Frog_Green").GetComponent<Animation>();
        terrainCollider = Terrain.activeTerrain.GetComponent<Collider>();
    }
	
	// Update is called once per frame
	void Update () {
        if (stunDuration > 0f)
        {
            stunDuration = Mathf.Max(0f, stunDuration - Time.deltaTime);
        }
        else {
            Vector3 dir = player.transform.position - p;
            dir.Normalize();
            if ((player.transform.position - p).sqrMagnitude < attackDistance * attackDistance)
            {
                if (attackDelay <= 0f)
                {
                    attackDelay = 2f;
                    transform.LookAt(player.transform);
                    player.GetComponent<Player>().GotHit(damage);
                    Blend("Attack1", 0.1f);
                }
                else
                {
                    attackDelay -= Time.deltaTime;
                }
            }
            else if ((player.transform.position - p).sqrMagnitude < runDistance * runDistance)
            {
                transform.LookAt(player.transform);
                p += dir * runSpeed * Time.deltaTime;
                Blend("Run", 0.1f);
            }
            else if ((player.transform.position - p).sqrMagnitude < walkDistance * walkDistance)
            {
                transform.LookAt(player.transform);
                p += dir * walkSpeed * Time.deltaTime;
                Blend("Walk", 0.1f);
            }
            else
            {
                Blend("Idle", 0.1f);
            }

            RaycastHit hit;
            if (terrainCollider.Raycast(new Ray(transform.position + new Vector3(0f, 50f, 0f), -Vector3.up), out hit, 100.0f))
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

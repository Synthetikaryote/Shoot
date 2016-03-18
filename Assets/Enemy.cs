using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {
    public float walkDistance = 70f;
    public float runDistance = 30f;
    public float attackDistance = 5f;
    public float walkSpeed = 2f;
    public float runSpeed = 7f;
    public float hitPoints = 100f;
    public float damage = 5f;
    public Animation ani;
    public string animationIdle;
    public string animationAttack;
    public string animationRun;
    public string animationWalk;
    public string animationTakeDamage;
    public string animationDeath;
    Vector3 p;
    GameObject player;
    string lastAnimation = null;
    float stunDuration = 0f;
    float attackDelay = 0f;
    Collider terrainCollider;
    Collider collider;

    // Use this for initialization
    void Start () {
        p = transform.position;
        player = GameObject.Find("Player");
        terrainCollider = Terrain.activeTerrain.GetComponent<Collider>();
        collider = GetComponent<Collider>();
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
                    Blend(animationAttack, 0.1f);
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
                Blend(animationRun, 0.1f);
            }
            else if ((player.transform.position - p).sqrMagnitude < walkDistance * walkDistance)
            {
                transform.LookAt(player.transform);
                p += dir * walkSpeed * Time.deltaTime;
                Blend(animationWalk, 0.1f);
            }
            else
            {
                Blend(animationIdle, 0.1f);
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
        Debug.Log(targetAnimation);
    }

    void GotHit(float damage)
    {
        runDistance = 1000f;
        hitPoints = Mathf.Max(0f, hitPoints - damage);
        if (hitPoints > 0f)
        {
            Blend(animationTakeDamage, 0.1f);
            stunDuration = 0.5f;
        }
        else
        {
            collider.enabled = false; 
            stunDuration = 20f;
            Blend(animationDeath, 0.1f);
            GameObject.Destroy(gameObject, 20f);
        }
    }
}

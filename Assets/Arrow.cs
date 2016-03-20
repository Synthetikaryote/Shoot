using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour {
    public float speed;
    public Vector3 direction;
    public float damage;
    public float maxDuration = 60f;
    Vector3 delta;
    bool traveling = true;
    Vector3 p;
    float elapsed = 0;
    Player player;

	// Use this for initialization
	void Start () {
        p = transform.position;
        transform.LookAt(transform.position + direction);
        player = GameObject.Find("Player").GetComponent<Player>();
	}
	
	// Update is called once per frame
	void Update () {
        elapsed += Time.deltaTime;
        if (traveling)
        {
            p += direction * speed * Time.deltaTime;
            transform.position = p;
        }

        if (elapsed > maxDuration)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider col)
    {
        // do some effect
        traveling = false;
        GetComponent<Collider>().enabled = false;
        GetComponent<AudioSource>().Play();
        var shootEffectGO = transform.FindChild("Shoot Effect").gameObject;
        shootEffectGO.GetComponent<ParticleSystem>().enableEmission = false;
        GameObject.Destroy(shootEffectGO, 0.2f);

        col.gameObject.SendMessage("GotHit", damage, SendMessageOptions.DontRequireReceiver);
        elapsed = maxDuration - 1f;
        if (player.OnSound != null) {
            player.OnSound(transform.position);
        }
    }
}

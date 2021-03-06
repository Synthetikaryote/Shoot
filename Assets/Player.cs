﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.IO;
using System.Text;

public class Player : MonoBehaviour {
    public GameObject arrowPrefab;
    public float damage = 50f;
    public float hitPoints = 100f;
    public delegate void OnSoundDelegate(Vector3 loc);
    public OnSoundDelegate OnSound;
    public float runSpeed = 10f;
    public float walkSpeed = 2.25f;
    public float jumpVelocity = 7.5f;
    public float gravity = 15f;
    public int killCount = 0;

    protected Uber uber;
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

    public byte[] state {
        set {
            p = new Vector3(BitConverter.ToSingle(value, 0), BitConverter.ToSingle(value, 4), BitConverter.ToSingle(value, 8));
            yaw = BitConverter.ToSingle(value, 12);
            pitch = BitConverter.ToSingle(value, 16);
            modelYaw = BitConverter.ToSingle(value, 20);
            int animationBytes = BitConverter.ToInt32(value, 24);
            if (animationBytes > 0) {
                string animation = Encoding.Unicode.GetString(value, 28, animationBytes);
                Blend(animation, 0.1f);
            }
        }
        get {
            var stream = new MemoryStream();
            stream.Write(BitConverter.GetBytes(p.x), 0, 4);
            stream.Write(BitConverter.GetBytes(p.y), 0, 4);
            stream.Write(BitConverter.GetBytes(p.z), 0, 4);
            stream.Write(BitConverter.GetBytes(yaw), 0, 4);
            stream.Write(BitConverter.GetBytes(pitch), 0, 4);
            stream.Write(BitConverter.GetBytes(modelYaw), 0, 4);
            if (lastAnimation != null) {
                var animation = Encoding.Unicode.GetBytes(lastAnimation);
                stream.Write(BitConverter.GetBytes(animation.Length), 0, 4);
                stream.Write(animation, 0, animation.Length);
            } else {
                stream.Write(BitConverter.GetBytes(0), 0, 4);
            }
            return stream.ToArray();
        }
    }


    // Use this for initialization
    void Awake() {
        uber = GameObject.FindObjectOfType<Uber>();

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

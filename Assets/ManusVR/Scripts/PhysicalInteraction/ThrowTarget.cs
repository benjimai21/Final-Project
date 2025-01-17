﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class ThrowTarget : MonoBehaviour
{
    public static HashSet<ThrowTarget> Instances { get; private set; }

    public float MaxAssistDistance = 0.5f;
    public Collider TargetCollider;

    void OnEnable()
    {
        if(Instances == null)
            Instances = new HashSet<ThrowTarget>();
        Instances.Add(this);
    }

    void OnDisable()
    {
        if (Instances == null)
            Instances = new HashSet<ThrowTarget>();
        Instances.Remove(this);
    }

    void Awake()
    {
        TargetCollider = GetComponent<Collider>();
    }
}

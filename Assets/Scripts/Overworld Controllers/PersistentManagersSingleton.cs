using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentManagersSingleton : MonoBehaviour
{
    private static PersistentManagersSingleton instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}

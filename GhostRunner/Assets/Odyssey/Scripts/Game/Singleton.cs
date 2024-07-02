using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static object _lock = new object();
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance) return _instance;
            lock (_lock)
            {
                if (_instance) return _instance;
                _instance = FindObjectOfType<T>();
                if (!_instance)
                {
                    GameObject go = new GameObject(typeof(T).FullName);
                    _instance = go.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (Instance != this) DestroyImmediate(gameObject);
    }
}
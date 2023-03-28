using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleTone<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T instance;

    /// <summary>
    /// Returns the instance of this singleton.
    /// </summary>
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (T)FindObjectOfType(typeof(T));
                if (instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).ToString());
                    instance = obj.AddComponent<T>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }
}
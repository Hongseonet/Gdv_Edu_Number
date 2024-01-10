using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    static bool bShutdown = false;
    private static T _instance = null;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                if(bShutdown == false)
                {
                    T instance = GameObject.FindObjectOfType<T>() as T;
                    if (instance == null)
                    {
                        instance = new GameObject(typeof(T).ToString(), typeof(T)).GetComponent<T>();
#if SCENARIOEDITOR
                        if (instance.name.Equals("InputManager"))
                        {
                            //Debug.Break();
                            GameObject.Destroy(instance.gameObject);
                        }
#endif
                    }
                    InstanceInit(instance);
                    Debug.Assert(_instance != null, typeof(T).ToString() + "Singleton Falled");
                }
            }
            return _instance;
        }
    }

    private static void InstanceInit(Object instance)
    {
        _instance = instance as T;
        _instance.Init();
    }

    // 씬 전환시 삭제 여부
    public virtual void Init()
    {
        DontDestroyOnLoad(_instance);
    }

    public virtual void OnDestroy()
    {
        _instance = null;
    }
    private void OnApplicationQuit()
    {
        _instance = null;
        bShutdown = true;
    }

    public virtual void Clear()
    {
        _instance = null;
    }
}
using UnityEngine;

public class FreshMenuController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        FindAndDestroyEntityOnLoad();
    }

    void Start()
    {
        FindAndDestroyEntityOnLoad();
    }

    private void Reset()
    {
        FindAndDestroyEntityOnLoad();
    }
    private void FindAndDestroyEntityOnLoad()
    {
        Destroy(GameObject.FindWithTag("Player"));

    }
}

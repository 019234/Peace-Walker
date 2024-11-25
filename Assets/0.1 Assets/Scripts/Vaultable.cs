using UnityEngine;

public class Vaultable : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void OnTriggerEnter(Collider col)
    {
        Debug.Log("invault");
    }

    void OnTriggerExit(Collider col)
    {
        Debug.Log("not invault");

    }
}

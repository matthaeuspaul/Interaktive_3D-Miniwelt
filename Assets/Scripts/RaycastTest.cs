using UnityEngine;

public class RaycastTest : MonoBehaviour
{
    private LayerMask layerMask;

    private void Awake()
    {
        layerMask = LayerMask.GetMask("InteractableObject");
    }

    void FixedUpdate()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 10f, layerMask))
        {
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.yellow);
            Debug.Log("Hit an interactable object: " + hit.collider.name);
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * 10f, Color.white);
            Debug.Log("Did not hit an interactable object");
        }
    }
}

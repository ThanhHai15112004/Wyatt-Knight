using UnityEngine;

public class Camera : MonoBehaviour
{
    [SerializeField] private float followCamera = 1f;
    [SerializeField] private Vector3 offset;

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, PlayerController.instance.transform.position + offset, followCamera);
    }
}

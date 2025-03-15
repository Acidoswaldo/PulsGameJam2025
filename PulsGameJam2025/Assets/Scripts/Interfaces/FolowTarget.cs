using UnityEngine;

public class FolowTarget : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 5f;

    private void Update()
    {
        if (target == null)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
    }
}

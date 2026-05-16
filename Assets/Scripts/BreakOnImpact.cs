using UnityEngine;

public class BreakOnImpact : MonoBehaviour
{
    void OnCollisionEnter(Collision col) => Destroy(gameObject);
}

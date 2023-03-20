using UnityEngine;
using UnityEngine.Events;

public class GroundDetector : MonoBehaviour
{
    public UnityAction OnGroundDetected;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnGroundDetected.Invoke();
    }
}

using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    public static UnityAction OnNoLivesRemaining;
    public UnityAction OnLiveLost;

    public int Lives = 3;

    public void RemoveLive()
    {
        Lives--;
        OnLiveLost.Invoke();

        if (Lives <= 0)
        {
            if (OnNoLivesRemaining != null) OnNoLivesRemaining.Invoke();
            Lives = 3;
        }
    }
}

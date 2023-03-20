using UnityEngine;

public class DeathUIListener : MonoBehaviour
{
    private void OnEnable()
    {
        PlayerHealth.OnNoLivesRemaining += () => transform.GetChild(0).gameObject.SetActive(true);
    }
    private void OnDisable()
    {
        PlayerHealth.OnNoLivesRemaining -= () => transform.GetChild(0).gameObject.SetActive(true);
    }

    public void DisableUI()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }
}

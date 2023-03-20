using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathDetector : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.position = new Vector3(-8, 0, 0);
            collision.GetComponent<PlayerHealth>().RemoveLive();
            collision.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        }
    }
}

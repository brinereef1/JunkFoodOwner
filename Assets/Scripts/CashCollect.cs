using UnityEngine;

public class CashCollect : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // When the player touches the cash, collect it.
        PlayerMovement player =
            other.GetComponentInParent<PlayerMovement>();

        if (player == null)
            return;

        MoneyManager.Instance.CollectCash(gameObject);
    }
}
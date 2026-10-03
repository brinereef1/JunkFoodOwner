using UnityEngine;

public class CashCollect : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        CharacterController player =
            other.GetComponent<CharacterController>();

        if (player == null)
            return;

        MoneyManager.Instance.CollectCash(gameObject);
    }
}
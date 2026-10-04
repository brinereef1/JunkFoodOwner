using UnityEngine;

public class CashCollect : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        CharacterController player =
            other.GetComponentInParent<CharacterController>();

        if (player == null)
            return;

        Debug.Log("Cash collected: " + gameObject.name);

        gameObject.SetActive(false);
    }
}
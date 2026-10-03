using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    public int Money { get; private set; }

    [Header("Cash")]
    [SerializeField] private GameObject cashContainer;

    private GameObject[] cashObjects;
    private int nextCashIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SetupCashObjects();
    }

    private void SetupCashObjects()
    {
        if (cashContainer == null)
        {
            Debug.LogError("Cash Container is not assigned!");
            return;
        }

        int childCount = cashContainer.transform.childCount;

        cashObjects = new GameObject[childCount];

        for (int i = 0; i < childCount; i++)
        {
            cashObjects[i] = cashContainer.transform.GetChild(i).gameObject;

            // Make sure all cash starts disabled
            cashObjects[i].SetActive(false);
        }
    }

    public void AddMoney(int amount)
    {
        Money += amount;

        EnableNextCash();

        Debug.Log(
            "Earned $" + amount +
            " | Total Money: $" + Money
        );
    }

    private void EnableNextCash()
    {
        if (cashObjects == null || cashObjects.Length == 0)
            return;

        if (nextCashIndex >= cashObjects.Length)
        {
            Debug.LogWarning("No more cash objects available.");
            return;
        }

        cashObjects[nextCashIndex].SetActive(true);

        Debug.Log(
            "Enabled Cash Object: " +
            cashObjects[nextCashIndex].name
        );

        nextCashIndex++;
    }
}
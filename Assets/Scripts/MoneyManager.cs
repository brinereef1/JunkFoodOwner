using TMPro;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    [Header("Money")]
    [SerializeField] private int startingMoney = 20;

    public int Money { get; private set; }

    [Header("Money UI")]
    [SerializeField] private TMP_Text moneyText;

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

        Money = startingMoney;

        SetupCashObjects();

        UpdateMoneyUI();

        Debug.Log("Starting Money: $" + Money);
    }

    private void SetupCashObjects()
    {
        if (cashContainer == null)
        {
            Debug.LogError(
                "Cash Container is not assigned!"
            );

            return;
        }

        int childCount =
            cashContainer.transform.childCount;

        cashObjects =
            new GameObject[childCount];

        for (int i = 0; i < childCount; i++)
        {
            cashObjects[i] =
                cashContainer.transform
                    .GetChild(i)
                    .gameObject;

            // ALWAYS disabled at game start
            cashObjects[i].SetActive(false);
        }

        nextCashIndex = 0;
    }

    public void AddMoney(int amount)
    {
        Money += amount;

        EnableNextCash();

        UpdateMoneyUI();

        Debug.Log(
            "Earned $" + amount +
            " | Total Money: $" + Money
        );
    }

    public bool CanAfford(int amount)
    {
        return Money >= amount;
    }

    public bool SpendMoney(int amount)
    {
        if (Money < amount)
            return false;

        Money -= amount;

        UpdateMoneyUI();

        Debug.Log(
            "Spent $" + amount +
            " | Remaining Money: $" + Money
        );

        return true;
    }

    private void EnableNextCash()
    {
        if (cashObjects == null ||
            cashObjects.Length == 0)
        {
            return;
        }

        if (nextCashIndex >= cashObjects.Length)
        {
            Debug.LogWarning(
                "No more cash objects available."
            );

            return;
        }

        GameObject cash =
            cashObjects[nextCashIndex];

        cash.SetActive(true);

        Debug.Log(
            "Enabled Cash Object: " +
            cash.name
        );

        nextCashIndex++;
    }

    private void UpdateMoneyUI()
    {
        if (moneyText == null)
            return;

        moneyText.text = "$" + Money;
    }
}
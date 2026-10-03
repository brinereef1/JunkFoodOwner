using TMPro;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    [Header("Money UI")]
    [SerializeField] private TMP_Text moneyText;

    [Header("Cash")]
    [SerializeField] private GameObject cashContainer;

    [SerializeField] private int cashValue = 5;

    private GameObject[] cashObjects;

    private int nextCashIndex = 0;
    private int totalMoney = 0;

    public int TotalMoney => totalMoney;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SetupCash();
        UpdateMoneyText();
    }

    private void SetupCash()
    {
        if (cashContainer == null)
        {
            Debug.LogError("Cash Container is not assigned!");
            return;
        }

        int count = cashContainer.transform.childCount;

        cashObjects = new GameObject[count];

        for (int i = 0; i < count; i++)
        {
            cashObjects[i] =
                cashContainer.transform.GetChild(i).gameObject;

            cashObjects[i].SetActive(false);
        }
    }

    // Creates physical cash for a successful order
    public void CreateCash(int amount)
    {
        int cashCount = amount / cashValue;

        Debug.Log(
            "Creating " +
            cashCount +
            " cash object(s) worth $" +
            amount
        );

        for (int i = 0; i < cashCount; i++)
        {
            EnableNextCash();
        }
    }

    private void EnableNextCash()
    {
        if (cashObjects == null)
            return;

        if (nextCashIndex >= cashObjects.Length)
        {
            Debug.LogWarning(
                "No disabled cash objects left!"
            );

            return;
        }

        GameObject cash =
            cashObjects[nextCashIndex];

        cash.SetActive(true);

        Debug.Log(
            "Cash appeared: " +
            cash.name +
            " ($" +
            cashValue +
            ")"
        );

        nextCashIndex++;
    }

    // Called when player actually collects cash
    public void CollectCash(GameObject cashObject)
    {
        totalMoney += cashValue;

        cashObject.SetActive(false);

        UpdateMoneyText();

        Debug.Log(
            "Collected $5 | Total Money: $" +
            totalMoney
        );
    }

    private void UpdateMoneyText()
    {
        if (moneyText == null)
            return;

        moneyText.text = "$" + totalMoney;
    }
}
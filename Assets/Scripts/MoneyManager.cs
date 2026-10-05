using TMPro;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    [Header("Money")]
    private int startingMoney = 0;

    public int Money { get; private set; }

    [Header("Money UI")]
    [SerializeField] private TMP_Text moneyText;

    [Header("Cash")]
    [SerializeField] private GameObject cashContainer;
    [SerializeField] private int cashValue = 5;

    private GameObject[] cashObjects;

    private void Awake()
    {
        // Keep only one money manager in the scene.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Start with the set money amount.
        Money = startingMoney;

        // Get the cash pieces and hide them at first.
        SetupCashObjects();

        // Show the starting money on screen.
        UpdateMoneyUI();

        Debug.Log("Starting Money: $" + Money);
    }

    private void SetupCashObjects()
    {
        // Stop if the cash parent is not linked.
        if (cashContainer == null)
        {
            Debug.LogError(
                "Cash Container is not assigned!"
            );

            return;
        }

        // Count how many cash objects are in the container.
        int childCount =
            cashContainer.transform.childCount;

        cashObjects =
            new GameObject[childCount];

        // Save each cash object and keep it hidden.
        for (int i = 0; i < childCount; i++)
        {
            cashObjects[i] =
                cashContainer.transform
                    .GetChild(i)
                    .gameObject;

            cashObjects[i].SetActive(false);
        }
    }

    // Add cash to the scene when a customer pays.
    public void CreateCash(int amount)
    {
        // Ignore bad values.
        if (amount <= 0)
            return;

        // The cash value must divide evenly into the total.
        if (amount % cashValue != 0)
        {
            Debug.LogWarning(
                "Cash amount $" + amount +
                " is not divisible by $" + cashValue
            );

            return;
        }

        int cashNeeded = amount / cashValue;

        // Count how many hidden cash objects are ready to use.
        int availableCash = 0;

        for (int i = 0; i < cashObjects.Length; i++)
        {
            if (!cashObjects[i].activeSelf)
            {
                availableCash++;
            }
        }

        // Do not create more cash than we have ready.
        if (availableCash < cashNeeded)
        {
            Debug.LogWarning(
                "Not enough disabled cash objects. " +
                "Needed: " + cashNeeded +
                ", Available: " + availableCash
            );

            return;
        }

        int enabledCount = 0;

        // Turn on the needed cash from the top of the list.
        for (int i = 0; i < cashObjects.Length; i++)
        {
            if (cashObjects[i].activeSelf)
                continue;

            cashObjects[i].SetActive(true);

            enabledCount++;

            Debug.Log(
                "Enabled cash: " +
                cashObjects[i].name
            );

            if (enabledCount >= cashNeeded)
                break;
        }

        Debug.Log(
            "Created $" + amount +
            " using " + cashNeeded +
            " cash objects."
        );
    }

    // Add money when the player picks up a cash object.
    public void CollectCash(GameObject cash)
    {
        // Ignore empty or hidden cash.
        if (cash == null)
            return;

        if (!cash.activeSelf)
            return;

        Money += cashValue;

        // Hide the cash after it is collected.
        cash.SetActive(false);

        UpdateMoneyUI();

        Debug.Log(
            "Collected $" + cashValue +
            " | Total Money: $" + Money
        );
    }

    // Check if the player has enough money.
    public bool CanAfford(int amount)
    {
        return Money >= amount;
    }

    // Spend money if there is enough in the wallet.
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

    // Start the money text animation from one value to another.
    public void AnimateMoneyUI(
        int from,
        int to,
        float duration)
    {
        StartCoroutine(
            AnimateMoneyRoutine(
                from,
                to,
                duration
            )
        );
    }

    // Change the money text over time.
    private System.Collections.IEnumerator AnimateMoneyRoutine(
        int from,
        int to,
        float duration)
    {
        if (moneyText == null)
            yield break;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float progress =
                Mathf.Clamp01(timer / duration);

            int current =
                Mathf.RoundToInt(
                    Mathf.Lerp(
                        from,
                        to,
                        progress
                    )
                );

            moneyText.text = "$" + current;

            yield return null;
        }

        moneyText.text = "$" + to;
    }

    // Update the text to show the current money.
    private void UpdateMoneyUI()
    {
        if (moneyText == null)
            return;

        moneyText.text = "$" + Money;
    }

    // Set the UI to a custom money value.
    public void SetMoneyDisplay(int amount)
    {
        if (moneyText == null)
            return;

        moneyText.text = "$" + amount;
    }
}
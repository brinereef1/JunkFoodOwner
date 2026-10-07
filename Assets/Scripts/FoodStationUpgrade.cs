using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FoodStationUpgrade : MonoBehaviour
{
    [Header("Upgrade")]
    [SerializeField] private FoodType foodType;
    [SerializeField] private int stationCost = 50;
    [SerializeField] private float purchaseTime = 3f;

    [Header("Food Station")]
    [SerializeField] private GameObject foodStation;

    [Header("UI")]
    [SerializeField] private TMP_Text costText;

    [Header("Loading")]
    [SerializeField] private Image loadingImage;

    private PlayerCarry currentPlayer;
    private Coroutine purchaseCoroutine;

    private bool purchased;

    private int remainingCost;

    private void Awake()
    {
        remainingCost = stationCost;
    }

    private void Start()
    {
        // Food station starts disabled
        if (foodStation != null)
        {
            foodStation.SetActive(false);
        }

        UpdateCostUI();

        DisableLoadingImage();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerCarry playerCarry =
            other.GetComponent<PlayerCarry>();

        if (playerCarry == null)
            return;

        if (purchased)
            return;

        if (currentPlayer != null)
            return;

        currentPlayer = playerCarry;

        Debug.Log(
            "Food station purchase started: " +
            foodType +
            " | Remaining: $" +
            remainingCost
        );

        StartLoadingImage();

        purchaseCoroutine =
            StartCoroutine(
                ProcessPurchase()
            );
    }

    private IEnumerator ProcessPurchase()
    {
        // ---------------------------------------------
        // HOW MUCH CAN PLAYER PAY?
        // ---------------------------------------------

        int playerMoney =
            MoneyManager.Instance.Money;

        int payment =
            Mathf.Min(
                playerMoney,
                remainingCost
            );

        int oldMoney =
            playerMoney;

        int newMoney =
            playerMoney - payment;

        int oldCost =
            remainingCost;

        int newCost =
            remainingCost - payment;

        // ---------------------------------------------
        // PURCHASE TIMER
        // ---------------------------------------------

        float timer = 0f;

        while (timer < purchaseTime)
        {
            if (currentPlayer == null)
            {
                RestoreUI();

                DisableLoadingImage();

                purchaseCoroutine = null;

                yield break;
            }

            timer += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    timer / purchaseTime
                );

            // Radial loading
            if (loadingImage != null)
            {
                loadingImage.fillAmount =
                    progress;
            }

            // Money animation
            int animatedMoney =
                Mathf.RoundToInt(
                    Mathf.Lerp(
                        oldMoney,
                        newMoney,
                        progress
                    )
                );

            MoneyManager.Instance
                .SetMoneyDisplay(
                    animatedMoney
                );

            // Cost animation
            int animatedCost =
                Mathf.RoundToInt(
                    Mathf.Lerp(
                        oldCost,
                        newCost,
                        progress
                    )
                );

            if (costText != null)
            {
                costText.text =
                    "$" + animatedCost;
            }

            yield return null;
        }

        // Exact final UI values
        MoneyManager.Instance.SetMoneyDisplay(
            newMoney
        );

        if (costText != null)
        {
            costText.text =
                "$" + newCost;
        }

        if (loadingImage != null)
        {
            loadingImage.fillAmount = 1f;
        }

        // ---------------------------------------------
        // ACTUAL PAYMENT
        // ---------------------------------------------

        if (payment > 0)
        {
            bool success =
                MoneyManager.Instance.SpendMoney(
                    payment
                );

            if (!success)
            {
                RestoreUI();

                DisableLoadingImage();

                currentPlayer = null;
                purchaseCoroutine = null;

                yield break;
            }

            remainingCost =
                newCost;

            Debug.Log(
                "Paid $" +
                payment +
                " toward " +
                foodType +
                " station."
            );
        }

        // ---------------------------------------------
        // FULLY PAID?
        // ---------------------------------------------

        if (remainingCost <= 0)
        {
            UnlockStation();
        }
        else
        {
            Debug.Log(
                foodType +
                " station still needs $" +
                remainingCost
            );
        }

        DisableLoadingImage();

        currentPlayer = null;
        purchaseCoroutine = null;
    }

    private void UnlockStation()
    {
        Debug.Log(
            foodType +
            " station fully purchased!"
        );

        if (foodStation != null)
        {
            foodStation.SetActive(true);
        }

        purchased = true;

        // Disable this purchase area
        gameObject.SetActive(false);

        Debug.Log(
            foodType +
            " station unlocked!"
        );
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerCarry playerCarry =
            other.GetComponent<PlayerCarry>();

        if (playerCarry == null)
            return;

        if (playerCarry != currentPlayer)
            return;

        currentPlayer = null;

        if (purchaseCoroutine != null)
        {
            StopCoroutine(
                purchaseCoroutine
            );

            purchaseCoroutine = null;
        }

        RestoreUI();

        DisableLoadingImage();

        Debug.Log(
            "Player left before " +
            foodType +
            " purchase was complete."
        );
    }

    private void UpdateCostUI()
    {
        if (costText == null)
            return;

        costText.text =
            "$" + remainingCost;
    }

    private void RestoreUI()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.SetMoneyDisplay(
                MoneyManager.Instance.Money
            );
        }

        UpdateCostUI();
    }

    private void StartLoadingImage()
    {
        if (loadingImage == null)
            return;

        loadingImage.fillAmount = 0f;
        loadingImage.gameObject.SetActive(true);
    }

    private void DisableLoadingImage()
    {
        if (loadingImage == null)
            return;

        loadingImage.fillAmount = 0f;
        loadingImage.gameObject.SetActive(false);
    }
}
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TablePurchaseStation : MonoBehaviour
{
    [Header("Table")]
    [SerializeField] private Transform tableToUnlock;

    [Header("Purchase")]
    [SerializeField] private int tableCost = 50;
    [SerializeField] private float purchaseTime = 3f;

    [Header("UI")]
    [SerializeField] private Image loadingImage;

    [Header("Payment UI")]
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text tableCostText;

    private bool playerInside;
    private bool purchasing;
    private Coroutine purchaseCoroutine;

    private void Start()
    {
        if (loadingImage != null)
        {
            loadingImage.fillAmount = 0f;
            loadingImage.gameObject.SetActive(false);
        }

        if (tableCostText != null)
        {
            tableCostText.text = "$" + tableCost;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player =
            other.GetComponent<PlayerMovement>();

        if (player == null)
            return;

        playerInside = true;

        if (!purchasing)
        {
            purchaseCoroutine =
                StartCoroutine(PurchaseRoutine());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerMovement player =
            other.GetComponent<PlayerMovement>();

        if (player == null)
            return;

        playerInside = false;

        if (purchasing)
        {
            StopCoroutine(purchaseCoroutine);

            purchasing = false;

            ResetUI();
        }
    }

    private IEnumerator PurchaseRoutine()
    {
        purchasing = true;

        // ---------------------------------------------
        // CHECK MONEY BEFORE STARTING PAYMENT ANIMATION
        // ---------------------------------------------

        if (MoneyManager.Instance == null)
        {
            Debug.LogError("MoneyManager not found!");

            purchasing = false;
            yield break;
        }

        int startingMoney =
            MoneyManager.Instance.Money;

        if (startingMoney < tableCost)
        {
            Debug.Log(
                "Not enough money to buy table. Need $" +
                tableCost +
                ", have $" +
                startingMoney
            );

            purchasing = false;
            yield break;
        }

        // ---------------------------------------------
        // SHOW UI
        // ---------------------------------------------

        if (loadingImage != null)
        {
            loadingImage.fillAmount = 0f;
            loadingImage.gameObject.SetActive(true);
        }

        // ---------------------------------------------
        // PAYMENT ANIMATION
        // ---------------------------------------------

        float timer = 0f;

        while (timer < purchaseTime)
        {
            if (!playerInside)
            {
                purchasing = false;
                ResetUI();

                // Restore actual money display.
                MoneyManager.Instance.SetMoneyDisplay(
                    startingMoney
                );

                yield break;
            }

            timer += Time.deltaTime;

            float progress =
                Mathf.Clamp01(timer / purchaseTime);

            // -----------------------------------------
            // RADIAL LOADING
            // -----------------------------------------

            if (loadingImage != null)
            {
                loadingImage.fillAmount = progress;
            }

            // -----------------------------------------
            // PAYMENT UI
            // -----------------------------------------

            int paidAmount =
                Mathf.RoundToInt(
                    tableCost * progress
                );

            int remainingCost =
                tableCost - paidAmount;

            int remainingMoney =
                startingMoney - paidAmount;

            // Don't visually go below zero.
            remainingMoney =
                Mathf.Max(0, remainingMoney);

            MoneyManager.Instance.SetMoneyDisplay(
                remainingMoney
            );

            if (tableCostText != null)
            {
                tableCostText.text =
                    "$" + remainingCost;
            }

            yield return null;
        }

        // ---------------------------------------------
        // MAKE SURE ANIMATION ENDS CLEANLY
        // ---------------------------------------------

        MoneyManager.Instance.SetMoneyDisplay(
            startingMoney - tableCost
        );

        if (tableCostText != null)
        {
            tableCostText.text = "$0";
        }

        // ---------------------------------------------
        // CHECK TABLE MANAGER
        // ---------------------------------------------

        if (TableManager.Instance == null)
        {
            Debug.LogError("TableManager not found!");

            MoneyManager.Instance.SetMoneyDisplay(
                startingMoney
            );

            ResetUI();
            purchasing = false;

            yield break;
        }

        bool unlocked =
        TableManager.Instance.UnlockTable(
            tableToUnlock
        );

        if (!unlocked)
        {
            Debug.LogError(
                "Table could not be unlocked. " +
                "Money was NOT spent."
            );

            MoneyManager.Instance.SetMoneyDisplay(
                startingMoney
            );

            ResetUI();

            purchasing = false;

            yield break;
        }

        // ---------------------------------------------
        // ACTUAL PAYMENT
        // ---------------------------------------------

        MoneyManager.Instance.SpendMoney(tableCost);

        Debug.Log(
            "Table purchased for $" + tableCost
        );

        // ---------------------------------------------
        // HIDE PURCHASE UI
        // ---------------------------------------------

        if (loadingImage != null)
        {
            loadingImage.gameObject.SetActive(false);
        }

        if (tableCostText != null)
        {
            tableCostText.gameObject.SetActive(false);
        }

        // ---------------------------------------------
        // DISABLE PURCHASE STATION
        // ---------------------------------------------

        purchasing = false;

        gameObject.SetActive(false);
    }

    private void ResetUI()
    {
        if (loadingImage != null)
        {
            loadingImage.fillAmount = 0f;
            loadingImage.gameObject.SetActive(false);
        }

        if (tableCostText != null)
        {
            tableCostText.text =
                "$" + tableCost;
        }
    }
}
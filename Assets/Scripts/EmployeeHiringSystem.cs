using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmployeeHiringStation : MonoBehaviour
{
    [Header("Hiring")]
    [SerializeField] private int employeeCost = 50;
    [SerializeField] private float hireTime = 3f;

    [Header("Employee")]
    [SerializeField] private GameObject employee;
    [SerializeField] private Transform employeeSpawnPoint;

    [Header("UI")]
    [SerializeField] private TMP_Text hireAmountText;

    [Header("Loading")]
    [SerializeField] private Image loadingImage;

    private PlayerCarry currentPlayer;
    private Coroutine hiringCoroutine;
    private bool employeeHired;
    private int remainingCost;

    private void Awake()
    {
        // Start with the full employee cost.
        remainingCost = employeeCost;
    }

    private void Start()
    {
        // The employee is hidden until the player has paid enough.
        if (employee != null)
        {
            employee.SetActive(false);
        }

        UpdateHireAmountUI();
        DisableLoadingImage();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerCarry playerCarry =
            other.GetComponent<PlayerCarry>();

        if (playerCarry == null)
            return;

        if (employeeHired)
            return;

        if (currentPlayer != null)
            return;

        currentPlayer = playerCarry;

        Debug.Log(
            "Employee payment started. " +
            "Remaining: $" + remainingCost
        );

        StartLoadingImage();
        hiringCoroutine = StartCoroutine(ProcessEmployeePayment());
    }

    private IEnumerator ProcessEmployeePayment()
    {
        // Work out how much the player can pay right now.
        int playerMoney = MoneyManager.Instance.Money;
        int payment = Mathf.Min(playerMoney, remainingCost);

        int oldMoney = playerMoney;
        int newMoney = playerMoney - payment;

        int oldRemainingCost = remainingCost;
        int newRemainingCost = remainingCost - payment;

        // Animate the money and hiring cost during the payment timer.
        float timer = 0f;

        while (timer < hireTime)
        {
            // If the player walks away, stop the payment.
            if (currentPlayer == null)
            {
                RestoreUI();
                DisableLoadingImage();
                hiringCoroutine = null;
                yield break;
            }

            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / hireTime);

            if (loadingImage != null)
            {
                loadingImage.fillAmount = progress;
            }

            int animatedMoney = Mathf.RoundToInt(
                Mathf.Lerp(oldMoney, newMoney, progress)
            );

            MoneyManager.Instance.SetMoneyDisplay(animatedMoney);

            int animatedHireCost = Mathf.RoundToInt(
                Mathf.Lerp(oldRemainingCost, newRemainingCost, progress)
            );

            if (hireAmountText != null)
            {
                hireAmountText.text = "$" + animatedHireCost;
            }

            yield return null;
        }

        // Force the UI to show the final values.
        MoneyManager.Instance.SetMoneyDisplay(newMoney);

        if (hireAmountText != null)
        {
            hireAmountText.text = "$" + newRemainingCost;
        }

        if (loadingImage != null)
        {
            loadingImage.fillAmount = 1f;
        }

        // Pay the money from the player's wallet.
        if (payment > 0)
        {
            bool paymentSuccessful = MoneyManager.Instance.SpendMoney(payment);

            if (!paymentSuccessful)
            {
                RestoreUI();
                DisableLoadingImage();
                currentPlayer = null;
                hiringCoroutine = null;
                yield break;
            }

            remainingCost = newRemainingCost;

            Debug.Log(
                "Paid $" + payment +
                " toward employee."
            );

            Debug.Log(
                "Employee still needs $" +
                remainingCost
            );
        }

        // If the full amount is paid, hire the employee.
        if (remainingCost <= 0)
        {
            HireEmployee();
        }
        else
        {
            Debug.Log(
                "Partial payment complete. " +
                "Need another $" +
                remainingCost
            );
        }

        DisableLoadingImage();
        currentPlayer = null;
        hiringCoroutine = null;
    }

    private void HireEmployee()
    {
        Debug.Log("Employee fully paid! Hiring employee.");

        if (employeeSpawnPoint != null)
        {
            employee.transform.SetPositionAndRotation(
                employeeSpawnPoint.position,
                employeeSpawnPoint.rotation
            );
        }

        employee.SetActive(true);
        employeeHired = true;

        // Hide the spawn point.
        if (employeeSpawnPoint != null)
        {
            employeeSpawnPoint.gameObject.SetActive(false);
        }

        // Hide the payment UI before disabling this station.
        DisableLoadingImage();

        if (hireAmountText != null)
        {
            hireAmountText.gameObject.SetActive(false);
        }

        // Disable the entire hiring station.
        gameObject.SetActive(false);

        Debug.Log("Employee hired!");
    }

    private void UpdateHireAmountUI()
    {
        if (hireAmountText == null)
            return;

        hireAmountText.text = "$" + remainingCost;
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

        if (hiringCoroutine != null)
        {
            StopCoroutine(hiringCoroutine);
            hiringCoroutine = null;
        }

        // Put the UI back to the real values.
        RestoreUI();

        DisableLoadingImage();

        Debug.Log("Player left before payment was complete.");
    }

    private void RestoreUI()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.SetMoneyDisplay(
                MoneyManager.Instance.Money
            );
        }

        UpdateHireAmountUI();
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
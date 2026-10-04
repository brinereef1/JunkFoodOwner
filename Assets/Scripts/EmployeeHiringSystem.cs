using System.Collections;
using UnityEngine;

public class EmployeeHiringStation : MonoBehaviour
{
    [Header("Hiring")]
    [SerializeField] private int employeeCost = 20;
    [SerializeField] private float hireTime = 3f;

    [Header("Employee")]
    [SerializeField] private GameObject employee;
    [SerializeField] private Transform employeeSpawnPoint;

    private PlayerCarry currentPlayer;
    private Coroutine hiringCoroutine;
    private bool employeeHired;

    private void Start()
    {
        // Make absolutely sure the employee starts disabled.
        if (employee != null)
        {
            employee.SetActive(false);
        }
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

        Debug.Log("Hiring employee...");

        hiringCoroutine =
            StartCoroutine(HireEmployee());
    }

    private IEnumerator HireEmployee()
    {
        yield return new WaitForSeconds(hireTime);

        if (currentPlayer == null)
        {
            hiringCoroutine = null;
            yield break;
        }

        // Check money
        if (!MoneyManager.Instance.CanAfford(employeeCost))
        {
            Debug.Log(
                "Not enough money to hire employee. " +
                "Need $" + employeeCost +
                ", have $" + MoneyManager.Instance.Money
            );

            currentPlayer = null;
            hiringCoroutine = null;

            yield break;
        }

        // Pay for employee
        MoneyManager.Instance.SpendMoney(employeeCost);

        // ENABLE existing employee
        employee.SetActive(true);

        employeeHired = true;

        // Disable hiring station
        gameObject.SetActive(false);

        Debug.Log("Employee hired!");

        currentPlayer = null;
        hiringCoroutine = null;
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

        Debug.Log(
            "Player left before hiring was complete."
        );
    }
}
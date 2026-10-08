using System.Collections.Generic;
using UnityEngine;

public class TableManager : MonoBehaviour
{
    public static TableManager Instance { get; private set; }

    [Header("Tables")]
    [SerializeField] private List<Transform> tables = new List<Transform>();

    [Header("Default Unlocked Tables")]
    [SerializeField] private int defaultUnlockedTables = 1;

    public List<Transform> Tables => tables;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SetupTables();
    }

    private void SetupTables()
    {
        for (int i = 0; i < tables.Count; i++)
        {
            if (tables[i] == null)
                continue;

            // First table active, rest disabled.
            tables[i].gameObject.SetActive(
                i < defaultUnlockedTables
            );
        }
    }

    public bool UnlockTable(Transform table)
    {
        if (table == null)
            return false;

        if (!tables.Contains(table))
        {
            Debug.LogWarning(
                table.name +
                " is not registered in TableManager."
            );

            return false;
        }

        if (table.gameObject.activeSelf)
            return false;

        table.gameObject.SetActive(true);

        Debug.Log(
            "Unlocked table: " +
            table.name
        );

        return true;
    }

    public bool IsTableUnlocked(Transform table)
    {
        return table != null &&
               table.gameObject.activeInHierarchy;
    }
}
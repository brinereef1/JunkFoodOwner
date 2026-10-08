using System.Collections.Generic;
using UnityEngine;

public class CustomerWaitingArea : MonoBehaviour
{
    private Dictionary<Customer, int> slotAssignments =
        new Dictionary<Customer, int>();

    public bool HasFreeSlot =>
        slotAssignments.Count < transform.childCount;

    public bool TryReserve(Customer customer)
    {
        if (customer == null)
            return false;

        // Already has a slot.
        if (slotAssignments.ContainsKey(customer))
            return true;

        for (int i = 0; i < transform.childCount; i++)
        {
            bool slotUsed = false;

            foreach (int slotIndex in slotAssignments.Values)
            {
                if (slotIndex == i)
                {
                    slotUsed = true;
                    break;
                }
            }

            if (slotUsed)
                continue;

            slotAssignments.Add(customer, i);

            return true;
        }

        return false;
    }

    public int GetReservedSlotIndex(Customer customer)
    {
        if (slotAssignments.TryGetValue(customer, out int index))
            return index;

        return -1;
    }

    public Transform GetTargetTransform(Customer customer)
    {
        if (slotAssignments.TryGetValue(customer, out int index))
        {
            return transform.GetChild(index);
        }

        return transform;
    }

    public void Release(Customer customer)
    {
        if (customer != null)
            slotAssignments.Remove(customer);
    }
}
using UnityEngine;

public class CustomerWaypoint : MonoBehaviour
{
    public Customer OccupyingCustomer { get; private set; }

    public bool IsEmpty => OccupyingCustomer == null;

    public bool Reserve(Customer customer)
    {
        if (!IsEmpty)
            return false;

        OccupyingCustomer = customer;
        return true;
    }

    public void Release(Customer customer)
    {
        if (OccupyingCustomer == customer)
        {
            OccupyingCustomer = null;
        }
    }
}
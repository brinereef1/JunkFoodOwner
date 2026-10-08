using UnityEngine;

public class CustomerSeat : MonoBehaviour
{
    public Customer OccupyingCustomer { get; private set; }

    public bool IsFree =>
        OccupyingCustomer == null;

    public bool Reserve(Customer customer)
    {
        if (!IsFree)
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
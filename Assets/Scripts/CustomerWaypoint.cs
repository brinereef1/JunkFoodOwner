using UnityEngine;

public class CustomerWaypoint : MonoBehaviour
{
    private Customer occupant;

    public bool IsOccupied => occupant != null;

    public bool TryReserve(Customer customer)
    {
        if (customer == null)
            return false;

        if (occupant != null && occupant != customer)
            return false;

        occupant = customer;
        return true;
    }

    public void Release(Customer customer)
    {
        if (occupant == customer)
            occupant = null;
    }
}
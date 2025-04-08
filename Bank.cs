using System.Collections.Generic;

public class Bank
{
    private int balance;
    private LinkedList<Property> properties;

    public Bank(LinkedList<Property> properties, int balance) {
        this.properties = properties;
        this.balance = balance;
    }

    public int get_bank_balance() 
    {
        return balance;
    }

    public void set_bank_balance(int newBalance)
    {
        balance = newBalance;
    }

    public void add_to_bank(int amount)
    {
        balance += amount;
    }

    public bool take_from_bank(int amount)
    {
        if ((balance - amount) >= 0)
        {
            balance -= amount;
            return true;
        } else
        {
            return false;
        }
    }

    public bool purchase_property(Property property)
    {
        if(!properties.Contains(property))
        {
            return false;
        }
        else
        {
            properties.Remove(property);
            balance += property.get_cost();
            return true;
        }
    }
}
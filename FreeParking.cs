namespace PropTycoon;

public class FreeParking:Space
{
    private int collectedFines;
    
    public FreeParking(int position)
    {
        
    }

    public void collect_fine(int fine)
    {
        collectedFines += fine;
    }

    public override SpaceType land(Player player)
    {
        player.increase_balance(collectedFines);
        collectedFines = 0;

        return type;
    }
}


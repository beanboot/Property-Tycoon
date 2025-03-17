using System;

public class PropertySpace : Space
{
    private int cost;
    private int[] rent;
    private Player owner;
    private int numHouses;
    public PropertySpace(int position, string name, string type, int cost, int[] rent)
    : base(position, name, type)
    {
        this.cost = cost;
        this.rent = rent;
        numHouses = 0;
    }
    public void purchase(Player player){
        if(player.get_balance() > cost){
            owner = player;
        }
    }
}


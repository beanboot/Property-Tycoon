using System;

public class Property{
	private string name;
	private SpaceType type;
	private int cost;
	private int[] rent;
	private int numHouses;
	private Player owner;

	public Property(string name, string type, int cost, int[] rent){
		this.name = name;
		this.type = Enum.Parse<SpaceType>(type);
		this.cost = cost;
		this.rent = rent;
		numHouses = 0;
	}
	
	public void set_owner(Player player)
	{
		owner = player;
	}

	public Player get_owner()
	{
		return owner;
	}

	public int get_cost()
  	{
		return cost;
	}

  	public string get_name()
  	{
    return name;
  	}
  
  	public SpaceType get_type()
  	{
    return type;
  	}

  	public int get_num_houses()
  	{
        return numHouses;
  	}

	public int get_rent()
	{
		return rent[numHouses];
	}

	public int[] get_rent_array()
	{
		return rent;
	}
}


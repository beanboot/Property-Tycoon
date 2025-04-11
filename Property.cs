using System;

public class Property {

	// initalise global variables
	private string name;
	private SpaceType type;
	private int cost;
	private int[] rent;
	private int numHouses;
	private Player owner;
	private int position;

	// constructor
	public Property(string name, string type, int cost, int[] rent, int position){
		this.name = name;
		this.type = Enum.Parse<SpaceType>(type);
		this.cost = cost;
		this.rent = rent;
		numHouses = 0;
		this.position = position;
	}

	// returns postion of property
	public int get_position()
	{
		return position;
	}
	
	// sets owner of property
	public void set_owner(Player player)
	{
		owner = player;
	}

	// returns owner of property
	public Player get_owner()
	{
		return owner;
	}

	// returns the cost
	public int get_cost()
  	{
		return cost;
	}

	// returns the property name
  	public string get_name()
  	{
    return name;
  	}
  
	// returns the space type
  	public SpaceType get_type()
  	{
    return type;
  	}

	// returns number of houses
  	public int get_num_houses()
  	{
        return numHouses;
  	}

	// adds a house
	public void add_house()
	{
		numHouses++;
	}

	// removes a house
	public void remove_house()
	{
		numHouses = numHouses - 1;
	}

	// returns the rent
	public int get_rent()
	{
		return rent[numHouses];
	}

	// returns the rent array
	public int[] get_rent_array()
	{
		return rent;
	}
}


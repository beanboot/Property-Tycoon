using Godot;
using System;
using System.Collections.Generic;


public partial class Player : Node2D
{
	// CurrentPos is unique to each player and holds the current position of the player
	private int currentPos;
	private int balance; 
	private string name;
	private LinkedList<Property> properties;
	public int daysInJail = 0;
	public bool getOutJail = false;
	public bool hasPassedGo = true;
	// Colour set booleans
	public bool hasBrownSet = false;
	public bool hasBlueSet = false;
	public bool hasPurpleSet = false;
	public bool hasOrangeSet = false;
	public bool hasRedSet = false;
	public bool hasYellowSet = false;
	public bool hasGreenSet = false;
	public bool hasDeepBlueSet = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		currentPos = 0;
		balance = 1500;
		properties = new LinkedList<Property>();
	}

	// Takes a vector2 as a parameter (p1) and sets the player's spacial position to that of p1
	public void player_movement(Godot.Vector2 p1)
	{
		Position = p1;
	}

	// Iterates the currentPos variable by 1, if it surpasses 40 it goes back to 0
	public bool iterate_pos()
	{
	
		if (currentPos + 1 > 39) {
			currentPos = 0;
			hasPassedGo = true;
			return true;

		} else {
			currentPos ++;
			return false;
		}
	}

	public void iterate_pos_backwards(){
		if(currentPos - 1 < 0){
			currentPos = 39;
		}else{
			currentPos--;
		}
	}

	// Returns the player's current position
	public int get_pos()
	{
		return currentPos;
	}

	public void set_pos(int position)
	{
		currentPos = position;
	}

	public int get_balance()
	{
		return balance;
	}

	public void increase_balance(int amount)
	{
		balance += amount;
	}

	public bool decrease_balance(int amount)
	{
		if (balance - amount >= 0)
		{
			balance -= amount;
			return true;
		} else {
			return false;
		}
	}

	public void set_name(string name){
		this.name = name;
	}

	public void add_to_properties(Property property){
		properties.AddLast(property);
	}

	public LinkedList<Property> get_properties()
	{
		return properties;
	}
	
	public string get_name(){
		return name;
	}

	public bool does_player_have_colour_set(SpaceType type)
	{
		switch (type)
		{
			case SpaceType.BROWN:
				if (hasBrownSet) {
					return true;
				}
				break;
			case SpaceType.BLUE:
				if (hasBlueSet) {
					return true;
				}
				break;
			case SpaceType.PURPLE:
				if (hasPurpleSet) {
					return true;
				}
				break;
			case SpaceType.ORANGE:
				if (hasOrangeSet) {
					return true;
				}
				break;
			case SpaceType.RED:
				if (hasRedSet) {
					return true;
				}
				break;
			case SpaceType.YELLOW:
				if (hasYellowSet) {
					return true;
				}
				break;
			case SpaceType.GREEN:
				if (hasGreenSet) {
					return true;
				}
				break;
			case SpaceType.DEEPBLUE:
				if (hasDeepBlueSet) {
					return true;
				}
				break;
		}

		return false;
	}
}

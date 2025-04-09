using Godot;
using System;
using System.Collections.Generic;
using System.Numerics;

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
			currentPos += 1;
			return false;
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
}

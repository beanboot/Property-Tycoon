using System;

public class Space
{
	protected int position;
	protected string name;
	protected SpaceType type;

	public int get_pos()
	{
		return position;
	}
	public string get_name()
	{
		return name;
	}
	public SpaceType get_type()
	{
		return type;
	}
	public void land(Player player){
		Console.WriteLine(player.get_name() + "landed on " + name);
	}
}

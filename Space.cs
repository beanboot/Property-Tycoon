using System;

public class Space
{
	protected int position;
    protected string name;
    protected SpaceType type;

	public Space(int position, string name, string type)
	{
		this.position = position;
        this.name = name;
        this.type = Enum.Parse<SpaceType>(type);
	}
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
}



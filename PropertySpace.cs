using System;

public class PropertySpace : Space
{
	private Property _property;
	public PropertySpace(int position, Property property)
	{
		this.position = position;
		this.name = property.get_name();
		this.type = property.get_type();
		_property = property;
	}

	public Property get_property()
	{
		return _property;
	}
}

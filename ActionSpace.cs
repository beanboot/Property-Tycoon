using System;
using Godot;

public class ActionSpace : Space{
	private ActionType actionType;
	private int actionParameter;
	
	public ActionSpace(int position, string name,SpaceType type, string action){
			this.position = position;
			this.name = name;
			this.type = type;
			string[] actionValues = action.Split("-");
			GD.Print(actionValues);
			actionType = Enum.Parse<ActionType>(actionValues[0].ToUpper());
			actionParameter = int.Parse(actionValues[1]);
	}
}

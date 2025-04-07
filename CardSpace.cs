using System;

public class CardSpace: Space{
    public CardSpace(int position, string name, string type){
        this.position = position;
        this.name = name;
        this.type = Enum.Parse<SpaceType>(type);
    }
}

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

    public string get_name(){
        return name;
    }
    public SpaceType get_type(){
        return type;
    }
}
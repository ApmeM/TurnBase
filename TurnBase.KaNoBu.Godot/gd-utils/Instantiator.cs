using Godot;

public partial class Instantiator
{
    public static string LootPathFormat = "res://Presentation/loots/{0}/{0}.tscn";
    public static PackedScene LoadLoot(string loot)
    {
        var path = string.Format(LootPathFormat, loot);
        var scene = ResourceLoader.Load<PackedScene>(path);
        return scene;
    }

    public static BaseLoot CreateLoot(string loot)
    {
        return LoadLoot(loot).Instance<BaseLoot>();
    }
}









public static class KeybindConfig
{
    private static readonly Dictionary<string, string> bindings = new()
    {
        { "MoveUp", "w" },
        { "MoveDown", "s" },
        { "MoveLeft", "a" },
        { "MoveRight", "d" },   
        { "Attack", "f" },
        { "SPAttack", "g" },
        { "Menu", "Escape" },
        { "Inventory", "i" } ,
           { "Debug", "6" },
            { "Debug2", "7" },
            { "Performance", "8" },
         { "SkelAttack", "3" }
    };
    public static bool IsMovementKey(string key)
    {
        var movementActions = new[] { "MoveUp", "MoveDown", "MoveLeft", "MoveRight" };
        return movementActions.Any(action => bindings[action] == key);
    }


    public static void SetBinding(string action, string key)
    {
        if (bindings.ContainsKey(action))
            bindings[action] = key;
    }

    public static string GetBinding(string action)
    {
        return bindings.TryGetValue(action, out var key) ? key : string.Empty;
    }

    public static IReadOnlyDictionary<string, string> Bindings => bindings;
}

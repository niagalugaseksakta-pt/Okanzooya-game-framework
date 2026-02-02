using System.Collections.Generic;
using static Game.Config.Config;

public static class PortalRegistry
{
    private static readonly Dictionary<int, PortalEntity> map = new();

    public static void Register(PortalEntity p)
        => map[SillyDumbConverter(p.portalId)] = p;

    public static void Unregister(PortalEntity p)
    {
        if (map.TryGetValue(SillyDumbConverter(p.portalId), out var v) && v == p)
            map.Remove(SillyDumbConverter(p.portalId));
    }

    public static PortalEntity Get(PortalState id)
        => map.TryGetValue(SillyDumbConverter(id), out var p) ? p : null;

    public static void Clear()
        => map.Clear();

    private static int SillyDumbConverter(PortalState portalState)
    {
        int o;
        int.TryParse(portalState.ToString().Replace("State", "").Trim(), out o);
        return o;
    }
}

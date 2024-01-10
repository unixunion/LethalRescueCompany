using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
public static class CommandContract
{
    public enum Command
    {
        SpawnSpider,
        SpawnCube,
        KillSpider,
        DeadSwap
    }
}
using Newtonsoft.Json.Linq;

namespace yupisoft.ConfigServer.Core.Hooks
{
    public interface IHookCheckResult
    {
        string HookId { get; set; }
        JToken Data { get; set; }
        HookCheckStatus Result { get; set; }
    }
}
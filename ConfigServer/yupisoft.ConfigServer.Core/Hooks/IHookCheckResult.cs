using Newtonsoft.Json.Linq;

namespace yupisoft.ConfigServer.Core.Hooks
{
    public interface IHookCheckResult
    {
        Hook Hook { get; set; }
        JToken Data { get; set; }
        HookCheckStatus Result { get; set; }
    }
}
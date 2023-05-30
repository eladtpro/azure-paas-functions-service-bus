namespace Functions;
public interface IDurableTargetState
{
    Task<int> GetValue();
    Task MoveNext(int maxSize);
}

[JsonObject(MemberSerialization.OptIn)]
public class DurableTargetStates : IDurableTargetState
{
    [JsonProperty("value")]
    public int Value { get; set; }
    public Task<int> GetValue() => Task.FromResult(Value);
    public Task MoveNext(int maxSize)
    {
        Value = (Value < maxSize) ? Value + 1 : 1;
        return Task.CompletedTask;
    }
    [FunctionName(nameof(DurableTargetStates))]
    public static Task Run([EntityTrigger] IDurableEntityContext ctx)
        => ctx.DispatchAsync<DurableTargetStates>();
}

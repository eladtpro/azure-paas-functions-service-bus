namespace Functions;
public interface IDurableTargetState
{
    int Value { get; set; }
    void MoveNext(int maxSize);
}

[JsonObject(MemberSerialization.OptIn)]
public class DurableTargetStates : IDurableTargetState
{
    [JsonProperty("value")]
    public int Value { get; set; }
    public void MoveNext(int maxSize) => Value = (Value < maxSize) ? Value + 1 : 1;

    [FunctionName(nameof(DurableTargetStates))]
    public static Task Run([EntityTrigger] IDurableEntityContext ctx)
        => ctx.DispatchAsync<DurableTargetStates>();
}

namespace Functions.Extensions;
public static class ObjectExtensions
{
    public static string Serialize(this object obj)
    {
        if (obj == null) return null;

        try
        {
            return JsonConvert.SerializeObject(obj);
        }
        catch (Exception ex)
        {
            return $"{{ \"error\": \"{ex.Message}\" }}";
        }
    }
}

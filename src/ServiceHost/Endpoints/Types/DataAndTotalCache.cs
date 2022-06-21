namespace SapientFi.ServiceHost.Endpoints.Types;

public class DataAndTotalCache<T>
{
    public int Total { get; set; }

    public T? Data { get; set; }
}
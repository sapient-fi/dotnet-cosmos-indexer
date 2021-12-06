using Pylonboard.Kernel;

namespace Pylonboard.Contracts.Exchanges;

public class GetSubscribedMarketsResult
{
    public List<GetSubscribedMarketsResultItem> Markets { get; set; }
}
    
public class GetSubscribedMarketsResultItem
{
    public string Market { get; set; }
        
    public Exchange Exchange { get; set; }
}
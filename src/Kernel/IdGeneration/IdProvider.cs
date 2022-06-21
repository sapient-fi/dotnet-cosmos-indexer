namespace SapientFi.Kernel.IdGeneration;

public class IdProvider
{
    private readonly IdGen.IdGenerator? _snowflakeGenerator;

    /// <summary>
    /// for mocking/ faking
    /// </summary>
    protected IdProvider()
    {
        
    }
    public IdProvider(IdGen.IdGenerator snowflakeGenerator)
    {
        _snowflakeGenerator = snowflakeGenerator;
    }

    public virtual long Snowflake()
    {
        return _snowflakeGenerator!.CreateId();
    }
}
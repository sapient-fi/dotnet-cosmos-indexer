namespace Sapient.Kernel.IdGeneration;

public class IdGenerator
{
    private readonly IdGen.IdGenerator _snowflakeGenerator;

    /// <summary>
    /// for mocking/ faking
    /// </summary>
    protected IdGenerator()
    {
        
    }
    public IdGenerator(IdGen.IdGenerator snowflakeGenerator)
    {
        _snowflakeGenerator = snowflakeGenerator;
    }

    public virtual long Snowflake()
    {
        return _snowflakeGenerator.CreateId();
    }
}
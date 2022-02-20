namespace Pylonboard.Kernel.IdGeneration;

public class IdGenerator
{
    private readonly IdGen.IdGenerator _snowflakeGenerator;

    public IdGenerator(IdGen.IdGenerator snowflakeGenerator)
    {
        _snowflakeGenerator = snowflakeGenerator;
    }

    public virtual long Snowflake()
    {
        return _snowflakeGenerator.CreateId();
    }
}
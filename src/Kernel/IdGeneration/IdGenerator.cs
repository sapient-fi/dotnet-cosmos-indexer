using System.Net.Sockets;
using IdGen;

namespace Invacoil.Kernel.IdGeneration
{
    public class IdGenerator
    {
        private readonly IdGen.IdGenerator _snowflakeGenerator;

        public IdGenerator(IdGen.IdGenerator snowflakeGenerator)
        {
            _snowflakeGenerator = snowflakeGenerator;
        }

        public long Snowflake()
        {
            return _snowflakeGenerator.CreateId();
        }
    }
}
using Datos.Data;
using Microsoft.EntityFrameworkCore;

namespace Pruebas
{
    public class BasePrueba
    {
        protected BaseTopantaContext ConstruirContext(string nombrebd)
        {
            var option = new DbContextOptionsBuilder<BaseTopantaContext>()
                .UseInMemoryDatabase(nombrebd).Options;

            var dbContext = new BaseTopantaContext(option);
            return dbContext;
        }
    }
}

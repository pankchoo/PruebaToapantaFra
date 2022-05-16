using Datos.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApiServicio.Controllers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Pruebas.Pruebas
{
    public class ClientesControllerTest : BasePrueba
    {
        [Fact]
        public async Task ObtenerCliente()
        {
            //prepacion
            string nombreBD = Guid.NewGuid().ToString();
            var contexto = ConstruirContext(nombreBD);

            contexto.Clientes.Add(new Cliente()
            {
                PIdentificacion = "1717931149",
                ClContrasenia = "1234",
                ClEstado = true,
                PNombre = "Francisco Toapanta",
                PGenero = "Masculino",
                PEdad = 27,
                PDireccion = "Quito",
                PTelefono = "0966784678"
            });
            contexto.Clientes.Add(new Cliente()
            {
                PIdentificacion = "1788218211",
                ClContrasenia = "7893",
                ClEstado = true,
                PNombre = "Juan Perez",
                PGenero = "Masculino",
                PEdad = 27,
                PDireccion = "Cuenca",
                PTelefono = "0923356789"
            });
            await contexto.SaveChangesAsync();

            var contexto2 = ConstruirContext(nombreBD);

            //Prueba
            var controlador = new ClientesController(contexto2);
            var respuesta = await controlador.GetPClientes();
            //verificacion
            var cliente = respuesta.Value;
            Assert.AreEqual(2, cliente.Count);

        }
        [Fact]
        public async Task ObtenerMovimiento()
        {
            //prepacion
            string nombreBD = Guid.NewGuid().ToString();
            var contexto = ConstruirContext(nombreBD);

            contexto.Movimientos.Add(new Movimiento()
            {
                MoNumeroCuenta = "2324442",
                MoFecha = DateTime.Now,
                MoTipoMovimiento = "Deposito",
                MoSaldoInicial = 1000,
                MoMovimiento = 100,
                MoSaldoDisponible = 1100

            });
            contexto.Movimientos.Add(new Movimiento()
            {
                MoNumeroCuenta = "232222",
                MoFecha = DateTime.Now,
                MoTipoMovimiento = "Retiro",
                MoSaldoInicial = 1000,
                MoMovimiento = 200,
                MoSaldoDisponible = 800

            });
            await contexto.SaveChangesAsync();

            var contexto2 = ConstruirContext(nombreBD);

            //Prueba
            var controlador = new MovimientosController(contexto2);
            var respuesta = await controlador.GetPMovimientos();
            //verificacion
            var cliente = respuesta.Value;
            Assert.AreEqual(2, cliente.Count);

        }
    }
}
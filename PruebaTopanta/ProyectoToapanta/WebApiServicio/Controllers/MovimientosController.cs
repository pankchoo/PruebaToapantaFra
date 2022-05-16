using Datos.Data;
using Datos.Model;
using Datos.Utilidades;
using Datos.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiServicio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovimientosController : ControllerBase
    {
        private readonly BaseTopantaContext _context;
        public MovimientosController(BaseTopantaContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<List<Movimiento>>> GetPMovimientos()
        {
            return await _context.Movimientos.ToListAsync();
        }
        // GET: api/Movimientos/5
        [HttpGet("{id}")]
        public async Task<Respuesta> GetPMovimiento(int id)
        {
            Respuesta respuesta = new Respuesta();
            try
            {
                respuesta.Resultado = await _context.Movimientos.FindAsync(id);
                respuesta.IsSuccess = true;
                if (respuesta.Resultado == null)
                {
                    respuesta.Message = "No existe datos";
                }
            }
            catch (Exception e)
            {
                respuesta.Message = "Ocurrio un error: " + e.StackTrace;
                respuesta.IsSuccess = false;
            }
            return respuesta;
        }

        // PUT: api/Movimientos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPMovimiento(int id, Movimiento movimiento)
        {
            if (id != movimiento.MoIdMovimiento)
            {
                return BadRequest();
            }

            _context.Entry(movimiento).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PMovimientoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Registro Movimieentos
        /// </summary>
        /// <param name="pMovimiento"> modelo movimientos</param>
        /// <returns>respuesta movimientos</returns>
        [HttpPost]
        public async Task<Respuesta> PostPMovimiento(Movimiento pMovimiento)
        {
            Respuesta respuesta = new Respuesta();
            try
            {
                pMovimiento.MoFecha = DateTime.Now;
                #region Validacion campos vacios
                if (pMovimiento.MoMovimiento <= 0)
                {
                    respuesta.IsSuccess = true;
                    respuesta.Message = "No se permite valores 0";
                    return respuesta;
                }
                #endregion

                Movimiento ultimoMovimiento = await _context.Movimientos.Where(x => x.MoNumeroCuenta == pMovimiento.MoNumeroCuenta).OrderByDescending(x => x.MoFecha).FirstOrDefaultAsync();
                if (ultimoMovimiento != null)
                {
                    #region Validacion de Cupos
                    if (string.Equals(pMovimiento.MoTipoMovimiento, Utilis.VariablesLocal.Retiro.ToString()))
                    {
                        respuesta = await ValidarCupos(pMovimiento.MoNumeroCuenta, pMovimiento.MoMovimiento);
                        if (!respuesta.IsSuccess)
                        {
                            return respuesta;
                        }
                    }
                    #endregion

                    #region Validacion de Saldos y Registro de movimientos
                    if (ultimoMovimiento.MoSaldoDisponible < Math.Abs(pMovimiento.MoMovimiento))
                    {
                        respuesta.IsSuccess = true;
                        respuesta.Message = "Saldo no disponible";
                    }
                    else
                    {
                        pMovimiento.MoSaldoInicial = ultimoMovimiento.MoSaldoDisponible;
                        if (string.Equals(pMovimiento.MoTipoMovimiento, Utilis.VariablesLocal.Retiro.ToString()))
                        {
                            pMovimiento.MoSaldoDisponible = ultimoMovimiento.MoSaldoDisponible - pMovimiento.MoMovimiento;
                            pMovimiento.MoMovimiento = Math.Abs(pMovimiento.MoMovimiento) * (-1);
                        }
                        else
                            pMovimiento.MoSaldoDisponible = ultimoMovimiento.MoSaldoDisponible + pMovimiento.MoMovimiento;
                        respuesta.IsSuccess = true;
                        _context.Movimientos.Add(pMovimiento);
                        await _context.SaveChangesAsync();
                    }
                    #endregion
                }
                else
                {
                    if (string.Equals(pMovimiento.MoTipoMovimiento, Utilis.VariablesLocal.Retiro.ToString()))
                    {
                        pMovimiento.MoSaldoDisponible = pMovimiento.MoMovimiento;
                        pMovimiento.MoMovimiento = Math.Abs(pMovimiento.MoMovimiento) * (-1);
                    }
                    else
                        pMovimiento.MoSaldoDisponible = pMovimiento.MoMovimiento;
                    respuesta.IsSuccess = true;
                    _context.Movimientos.Add(pMovimiento);
                    await _context.SaveChangesAsync();
                }
                respuesta.Resultado = CreatedAtAction("GetPMovimiento", new { id = pMovimiento.MoIdMovimiento }, pMovimiento);
            }
            catch (Exception ex)
            {
                respuesta.Message = ex.Message;
            }
            return respuesta;
        }
        /// <summary>
        /// Validacion de Cupos
        /// </summary>
        /// <param name="strNumeroCuenta">Numero de cuenta</param>
        /// <param name="decMontoTransaccion">Monto transaccion</param>
        /// <returns></returns>
        [HttpPost("{strNumeroCuenta},{decMontoTransaccion}")]
        public async Task<Respuesta> ValidarCupos(string strNumeroCuenta, decimal decMontoTransaccion)
        {
            Respuesta respuesta = new Respuesta();
            DateTime diaActual = DateTime.Now;

            string dia = diaActual.ToString("dd-MM-yyyy");
            DateTime InicioDeDia = DateTime.ParseExact(dia, "dd-MM-yyyy", null);
            DateTime FinalDeDia = DateTime.ParseExact(dia + " 23:59:59", "dd-MM-yyyy HH:mm:ss", null);

            decimal valoresRetiro = await _context.Movimientos
                .Where(x => x.MoNumeroCuenta == strNumeroCuenta
                && x.MoFecha >= InicioDeDia
                && x.MoFecha <= FinalDeDia
                && x.MoTipoMovimiento == Utilis.VariablesLocal.Retiro.ToString())
                .SumAsync(a => a.MoMovimiento);

            decimal totalsupuesto = Math.Abs(valoresRetiro) + Math.Abs(decMontoTransaccion);

            respuesta.Resultado = Math.Abs(totalsupuesto);
            if (totalsupuesto > Convert.ToDecimal(Utilis.VariablesLocal.ValorTope) || totalsupuesto > Convert.ToDecimal(Utilis.VariablesLocal.ValorTope))
            {
                respuesta.Message = "Cupo diario excedido";
                respuesta.IsSuccess = false;
                respuesta.Resultado = Math.Abs(valoresRetiro);
            }
            else
                respuesta.IsSuccess = true;
            return respuesta;
        }

        // DELETE: api/Movimientos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePMovimiento(int id)
        {
            var pMovimiento = await _context.Movimientos.FindAsync(id);
            if (pMovimiento == null)
            {
                return NotFound();
            }

            _context.Movimientos.Remove(pMovimiento);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PMovimientoExists(int id)
        {
            return _context.Movimientos.Any(e => e.MoIdMovimiento == id);
        }

        [HttpGet("{strIdentificacion}&{dtFechaInicio}&{dtFechaFin}")]
        public async Task<Respuesta> PostPMovimientosRango(string strIdentificacion, string dtFechaInicio, string dtFechaFin)
        {
            List<Movimiento> lstPMovimiento = new List<Movimiento>();
            Respuesta respuesta = new Respuesta();
            try
            {
                respuesta.Resultado = await _context.Movimientos.Select(x => new MovimientosCliente
                {
                    Fecha = x.MoFecha,
                    Nombre = x.MoNumeroCuentaNavigation.CuIdClienteNavigation.PNombre,
                    NumeroCuenta = x.MoNumeroCuentaNavigation.CuNumeroCuenta,
                    Tipo = x.MoNumeroCuentaNavigation.CuTipo,
                    SaldoInicial = x.MoSaldoInicial,
                    Estado = x.MoNumeroCuentaNavigation.CuIdClienteNavigation.ClEstado,
                    Movimiento = x.MoMovimiento,
                    SaldoDisponible = x.MoSaldoDisponible,
                    Identificacion = x.MoNumeroCuentaNavigation.CuIdClienteNavigation.PIdentificacion,

                }).Where(s => s.Identificacion == strIdentificacion && s.Fecha >= Convert.ToDateTime(dtFechaInicio) && s.Fecha <= Convert.ToDateTime(dtFechaFin)).ToListAsync();
                respuesta.IsSuccess = true;
            }
            catch (Exception e)
            {
                respuesta.Message = e.StackTrace;
            }
            return respuesta;
        }
    }
}
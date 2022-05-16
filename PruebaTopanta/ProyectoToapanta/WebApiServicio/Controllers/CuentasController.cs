
using Datos.Data;
using Datos.Model;
using Datos.Utilidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiServicio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CuentasController : ControllerBase
    {
        private readonly BaseTopantaContext _context;

        public CuentasController(BaseTopantaContext context)
        {
            _context = context;
        }

        // GET: api/Cuentas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cuenta>>> GetCuentas()
        {
            return await _context.Cuentas.ToListAsync();
        }

        // GET: api/Cuentas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Cuenta>> GetCuenta(string id)
        {
            var pCuenta = await _context.Cuentas.FindAsync(id);

            if (pCuenta == null)
            {
                return NotFound();
            }

            return pCuenta;
        }

        // PUT: api/Cuentas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCuenta(string id, Cuenta pCuenta)
        {
            if (id != pCuenta.CuNumeroCuenta)
            {
                return BadRequest();
            }

            _context.Entry(pCuenta).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CuentaExists(id))
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

        // POST: api/Cuentas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<Respuesta> PostCuenta(Cuenta cuenta)
        {
            Respuesta respuesta = new Respuesta();
            _context.Cuentas.Add(cuenta);
            try
            {
                await _context.SaveChangesAsync();
                respuesta.IsSuccess = true;
            }
            catch (DbUpdateException e)
            {
                respuesta.IsSuccess = false;
                if (CuentaExists(cuenta.CuNumeroCuenta))
                {
                    respuesta.Message = "La cuenta ya existe";
                }
                else
                {
                    respuesta.Message = "Ocurrio un error: " + e.StackTrace;
                }
            }

            respuesta.Resultado = CreatedAtAction("GetPCuenta", new { id = cuenta.CuNumeroCuenta }, cuenta);

            return respuesta;
        }

        // DELETE: api/Cuentas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCuenta(string id)
        {

            var pCuenta = await _context.Cuentas.FindAsync(id);
            if (pCuenta == null)
            {
                return NotFound();
            }

            _context.Cuentas.Remove(pCuenta);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CuentaExists(string id)
        {
            return _context.Cuentas.Any(e => e.CuNumeroCuenta == id);
        }
    }
}
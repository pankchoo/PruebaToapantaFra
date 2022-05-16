using Datos.Data;
using Datos.Model;
using Datos.Utilidades;
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
    public class ClientesController : ControllerBase
    {
        private readonly BaseTopantaContext _context;

        public ClientesController(BaseTopantaContext context)
        {
            _context = context;
        }

        // GET: api/Clientes
        [HttpGet]
        public async Task<ActionResult<List<Cliente>>> GetPClientes()
        {
            return await _context.Clientes.ToListAsync();
        }

        // GET: api/Clientes/5
        [HttpGet("{id}")]
        public async Task<Respuesta> GetPCliente(int id)
        {
            Respuesta respuesta = new Respuesta();
            try
            {
                respuesta.Resultado = await _context.Clientes.FindAsync(id);
                respuesta.IsSuccess = true;
            }
            catch (Exception e)
            {
                respuesta.Message = "Ocurrio un error: " + e.StackTrace;
            }


            return respuesta;
        }



        // PUT: api/Clientes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPCliente(int id, Cliente pCliente)
        {
            if (id != pCliente.ClIdCliente)
            {
                return BadRequest();
            }

            _context.Entry(pCliente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PClienteExists(id))
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

        // POST: api/Clientes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Cliente>> PostPCliente(Cliente pCliente)
        {
            _context.Clientes.Add(pCliente);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPCliente", new { id = pCliente.ClIdCliente }, pCliente);
        }

        // DELETE: api/Clientes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePCliente(int id)
        {
            var pCliente = await _context.Clientes.FindAsync(id);
            if (pCliente == null)
            {
                return NotFound();
            }

            _context.Clientes.Remove(pCliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.ClIdCliente == id);
        }
    }
}
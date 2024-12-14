using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteSellingBonsaiAPI.Models;

namespace WebsiteSellingBonsaiAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BonsaiTypesController : ControllerBase
    {
        private readonly MiniBonsaiDBAPI _context;

        public BonsaiTypesController(MiniBonsaiDBAPI context)
        {
            _context = context;
        }

        // GET: api/BonsaiTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BonsaiType>>> GetTypes()
        {
            return await _context.Types.ToListAsync();
        }

        // GET: api/BonsaiTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BonsaiType>> GetBonsaiType(int id)
        {
            var bonsaiType = await _context.Types.FindAsync(id);

            if (bonsaiType == null)
            {
                return NotFound();
            }

            return bonsaiType;
        }

        // PUT: api/BonsaiTypes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBonsaiType(int id, BonsaiType bonsaiType)
        {
            if (id != bonsaiType.Id)
            {
                return BadRequest();
            }

            _context.Entry(bonsaiType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BonsaiTypeExists(id))
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

        // POST: api/BonsaiTypes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<BonsaiType>> PostBonsaiType(BonsaiType bonsaiType)
        {
            _context.Types.Add(bonsaiType);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBonsaiType", new { id = bonsaiType.Id }, bonsaiType);
        }

        // DELETE: api/BonsaiTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBonsaiType(int id)
        {
            var bonsaiType = await _context.Types.FindAsync(id);
            if (bonsaiType == null)
            {
                return NotFound();
            }

            _context.Types.Remove(bonsaiType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BonsaiTypeExists(int id)
        {
            return _context.Types.Any(e => e.Id == id);
        }
    }
}

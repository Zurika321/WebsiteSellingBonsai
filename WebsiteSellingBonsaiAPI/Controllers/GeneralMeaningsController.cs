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
    public class GeneralMeaningsController : ControllerBase
    {
        private readonly MiniBonsaiDBAPI _context;

        public GeneralMeaningsController(MiniBonsaiDBAPI context)
        {
            _context = context;
        }

        // GET: api/GeneralMeanings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GeneralMeaning>>> GetGeneralMeaning()
        {
            return await _context.GeneralMeaning.ToListAsync();
        }

        // GET: api/GeneralMeanings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GeneralMeaning>> GetGeneralMeaning(int id)
        {
            var generalMeaning = await _context.GeneralMeaning.FindAsync(id);

            if (generalMeaning == null)
            {
                return NotFound();
            }

            return generalMeaning;
        }

        // PUT: api/GeneralMeanings/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGeneralMeaning(int id, GeneralMeaning generalMeaning)
        {
            if (id != generalMeaning.Id)
            {
                return BadRequest();
            }

            _context.Entry(generalMeaning).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GeneralMeaningExists(id))
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

        // POST: api/GeneralMeanings
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<GeneralMeaning>> PostGeneralMeaning(GeneralMeaning generalMeaning)
        {
            _context.GeneralMeaning.Add(generalMeaning);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetGeneralMeaning", new { id = generalMeaning.Id }, generalMeaning);
        }

        // DELETE: api/GeneralMeanings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGeneralMeaning(int id)
        {
            var generalMeaning = await _context.GeneralMeaning.FindAsync(id);
            if (generalMeaning == null)
            {
                return NotFound();
            }

            _context.GeneralMeaning.Remove(generalMeaning);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool GeneralMeaningExists(int id)
        {
            return _context.GeneralMeaning.Any(e => e.Id == id);
        }
    }
}

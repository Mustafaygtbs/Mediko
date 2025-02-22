using Mediko.DataAccess;
using Mediko.Entities;

using Mediko.Entities.DTOs.TimesLotsDTOs;
using Mediko.Entities.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mediko.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles="Admin")]
    public class TimeslotsController : ControllerBase
    {
        private readonly MedikoDbContext _context;

        public TimeslotsController(MedikoDbContext context)
        {
            _context = context;
        }

        [HttpPut("UpdateIsOpen")]
        public async Task<IActionResult> UpdateIsOpen([FromBody] TimeslotOpenUpdateDto model)
        {
            try
            {
                if (model == null)
                    throw new BadRequestException("Timeslot güncelleme verisi eksik.");


                var timeslot = await _context.PoliclinicTimeslots
                    .FirstOrDefaultAsync(ts =>
                        ts.PoliclinicId == model.PoliclinicId &&
                        ts.Date == model.Date &&
                        ts.StartTime == model.StartTime
                    );
                if (timeslot == null)
                    throw new NotFoundException("Belirtilen kriterlere uyan bir Timeslot bulunamadı.");

                timeslot.IsOpen = model.IsOpen;

                _context.PoliclinicTimeslots.Update(timeslot);
                await _context.SaveChangesAsync();


                return NoContent();
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }


        [HttpPut("UpdateIsBooked")]
        public async Task<IActionResult> UpdateIsBooked([FromBody] TimeslotBookedUpdateDto model)
        {
            try
            {
                if (model == null)
                    throw new BadRequestException("Timeslot güncelleme verisi eksik.");


                var timeslot = await _context.PoliclinicTimeslots
                    .FirstOrDefaultAsync(ts =>
                        ts.PoliclinicId == model.PoliclinicId &&
                        ts.Date == model.Date &&
                        ts.StartTime == model.StartTime
                    );
                if (timeslot == null)
                    throw new NotFoundException("Belirtilen kriterlere uyan bir Timeslot bulunamadı.");


                timeslot.IsBooked = model.IsBooked;

                _context.PoliclinicTimeslots.Update(timeslot);
                await _context.SaveChangesAsync();


                return NoContent();
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }
    }
}

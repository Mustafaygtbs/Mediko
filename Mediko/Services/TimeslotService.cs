using Mediko.DataAccess;
using Mediko.Entities;
using Mediko.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Mediko.Business.Services
{
    public class TimeslotService : ITimeslotService
    {
        private readonly MedikoDbContext _context;

        public TimeslotService(MedikoDbContext context)
        {
            _context = context;
        }

        public async Task GenerateTimeslotsForNextDaysAsync(int days)
        {
            // 1) Tüm poliklinikleri çek
            var policlinics = await _context.Policlinics.ToListAsync();

            for (int dayOffset = 0; dayOffset < days; dayOffset++)
            {
                var currentDate = DateOnly.FromDateTime(DateTime.Today.AddDays(dayOffset));

                var start = new TimeOnly(9, 0);
                var end = new TimeOnly(17, 0);

                while (start < end)
                {
                    foreach (var pol in policlinics)
                    {

                        bool exists = await _context.PoliclinicTimeslots.AnyAsync(ts =>
                            ts.PoliclinicId == pol.Id &&
                            ts.Date == currentDate &&
                            ts.StartTime == start);

                        if (!exists)
                        {
                            var timeslot = new PoliclinicTimeslot
                            {
                                PoliclinicId = pol.Id,
                                Date = currentDate,
                                StartTime = start,
                                IsOpen = true,
                                IsBooked = false
                            };
                            await _context.PoliclinicTimeslots.AddAsync(timeslot);
                        }
                    }
                    start = start.AddMinutes(15);
                }
            }


            await _context.SaveChangesAsync();
        }

        public async Task RemoveOldTimeslotsAsync(int daysThreshold)
        {
            var cutoffDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-daysThreshold));

            var oldSlots = _context.PoliclinicTimeslots
                .Where(ts => ts.Date < cutoffDate && ts.IsBooked == false);

            _context.PoliclinicTimeslots.RemoveRange(oldSlots);
            await _context.SaveChangesAsync();
        }
    }
}

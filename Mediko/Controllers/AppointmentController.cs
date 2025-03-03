﻿using Mediko.DataAccess.Interfaces;
using Mediko.DataAccess;
using Mediko.Entities;
using Mediko.Entities.DTOs.AppointmentDTOs;
using Mediko.Entities.Exceptions;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Mediko.Entities.DTOs.AppointmentDTOs.Mediko.Entities.DTOs.AppointmentDTOs;
using Mediko.Services;

namespace Mediko.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,User")]
    public class AppointmentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly MedikoDbContext _context;
        private readonly IMapper _mapper;
        private readonly MailIslemleri _mailIslemleri;
       

        public AppointmentController(IUnitOfWork unitOfWork, MedikoDbContext context, IMapper mapper, MailIslemleri mailIslemleri)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _mapper = mapper;
            _mailIslemleri = mailIslemleri;
        }

        [HttpGet("GetByOgrenciNo/{ogrenciNo}")]
        public async Task<IActionResult> GetByOgrenciNo(string ogrenciNo)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.OgrenciNo == ogrenciNo);
                if (user == null)
                    throw new NotFoundException($"'{ogrenciNo}' numarasına sahip kullanıcı bulunamadı.");

                var appointments = await ((IAppointmentRepository)_unitOfWork.AppointmentRepository)
                    .GetAsync(a => a.UserId == user.Id);

                foreach (var app in appointments)
                {
                    await _context.Entry(app).Reference(a => a.PoliclinicTimeslot).LoadAsync();
                    await _context.Entry(app).Reference(a => a.Policlinic).LoadAsync();
                    await _context.Entry(app).Reference(a => a.User).LoadAsync();
                }

                var dtoList = _mapper.Map<IEnumerable<AppointmentDto>>(appointments);

                return Ok(dtoList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
        }


        [HttpGet("Get-All")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var appointments = await _unitOfWork.AppointmentRepository.GetAllAsync();

                foreach (var app in appointments)
                {
                    await _context.Entry(app).Reference(a => a.Policlinic).LoadAsync();
                    await _context.Entry(app).Reference(a => a.PoliclinicTimeslot).LoadAsync();
                    await _context.Entry(app).Reference(a => a.User).LoadAsync();
                }

                var dtoList = _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
                return Ok(dtoList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
        }


        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var appointment = await _unitOfWork.AppointmentRepository.GetByIdAsync(id);

                if (appointment == null)
                    return NotFound("Randevu bulunamadı.");

                await _context.Entry(appointment).Reference(a => a.Policlinic).LoadAsync();
                await _context.Entry(appointment).Reference(a => a.PoliclinicTimeslot).LoadAsync();
                await _context.Entry(appointment).Reference(a => a.User).LoadAsync();

                var dto = _mapper.Map<AppointmentDto>(appointment);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
        }


        [HttpGet("Get-Confirmed")]
        public async Task<IActionResult> GetConfirmed()
        {
            try
            {
                var confirmedApps = await ((IAppointmentRepository)_unitOfWork.AppointmentRepository)
                    .GetConfirmedAppointmentsAsync();

                foreach (var app in confirmedApps)
                {
                    await _context.Entry(app).Reference(a => a.Policlinic).LoadAsync();
                    await _context.Entry(app).Reference(a => a.PoliclinicTimeslot).LoadAsync();
                    await _context.Entry(app).Reference(a => a.User).LoadAsync();
                }

                var dtoList = _mapper.Map<IEnumerable<AppointmentDto>>(confirmedApps);
                return Ok(dtoList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
        }


        [HttpGet("get-onaybekleyenler")]
        public async Task<IActionResult> GetOnayBekleyenler()
        {
            try
            {
                var pendingAppointments = await ((IAppointmentRepository)_unitOfWork.AppointmentRepository)
                    .GetOnayBekleyen();

                foreach (var app in pendingAppointments)
                {
                    await _context.Entry(app).Reference(a => a.Policlinic).LoadAsync();
                    await _context.Entry(app).Reference(a => a.PoliclinicTimeslot).LoadAsync();
                    await _context.Entry(app).Reference(a => a.User).LoadAsync();
                }

                var dtoList = _mapper.Map<IEnumerable<AppointmentDto>>(pendingAppointments);
                return Ok(dtoList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
        }


        [HttpPost("RandevuOlustur")]
        public async Task<IActionResult> CreateWithTimeslotCheck([FromBody] AppointmentCreateSimpleDto model, [FromServices] IEmailService emailService)
        {
            try
            {
                if (model == null)
                    throw new BadRequestException("Appointment verisi eksik veya hatalı.");

                var user = await _context.Users.FirstOrDefaultAsync(u => u.OgrenciNo == model.OgrenciNo);
                if (user == null)
                    throw new NotFoundException($"'{model.OgrenciNo}' numarasına sahip kullanıcı bulunamadı.");

                var timeslot = await _context.PoliclinicTimeslots.FirstOrDefaultAsync(ts =>
                    ts.PoliclinicId == model.PoliclinicId &&
                    ts.Date == model.AppointmentDate &&
                    ts.StartTime == model.AppointmentTime &&
                    ts.IsOpen == true &&
                    ts.IsBooked == false
                );

                if (timeslot == null)
                    throw new BadRequestException("Seçilen tarih/saat için müsait bir slot bulunamadı veya çoktan rezerve edilmiş.");

                var appointment = new Appointment
                {
                    PoliclinicTimeslotId = timeslot.Id,
                    PoliclinicId = timeslot.PoliclinicId,
                    UserId = user.Id,
                    AppointmentDate = model.AppointmentDate,
                    AppointmentTime = model.AppointmentTime,
                    FullAppointmentDateTime = new DateTime(
                        model.AppointmentDate.Year, model.AppointmentDate.Month, model.AppointmentDate.Day,
                        model.AppointmentTime.Hour, model.AppointmentTime.Minute, 0
                    ),
                    Status = AppointmentStatus.OnayBekliyor
                };

                await _unitOfWork.AppointmentRepository.AddAsync(appointment);
                await _unitOfWork.Save();

                timeslot.IsBooked = true;
                _context.PoliclinicTimeslots.Update(timeslot);
                await _context.SaveChangesAsync();

                await _mailIslemleri.SendAppointmentCreationEmail(user, appointment);



                return Ok();
            }
            catch (BadRequestException badEx)
            {
                return BadRequest(new { Message = badEx.Message });
            }
            catch (NotFoundException nfEx)
            {
                return NotFound(new { Message = nfEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }


        [HttpPut("UpdateConfirmation")]
        public async Task<IActionResult> UpdateConfirmation([FromBody] AppointmentConfirmUpdateDto model, [FromServices] IEmailService emailService)
        {
            try
            {
                if (model == null)
                    throw new BadRequestException("Randevu güncelleme verisi eksik.");

                var user = await _context.Users.FirstOrDefaultAsync(u => u.OgrenciNo == model.OgrenciNo);
                if (user == null)
                    throw new NotFoundException($"'{model.OgrenciNo}' numarasına sahip kullanıcı bulunamadı.");

                var existingAppointments = await ((IAppointmentRepository)_unitOfWork.AppointmentRepository)
                    .GetAsync(a =>
                        a.UserId == user.Id &&
                        a.PoliclinicId == model.PoliclinicId &&
                        a.AppointmentDate == model.AppointmentDate &&
                        a.AppointmentTime == model.AppointmentTime
                    );

                var appointment = existingAppointments.FirstOrDefault();
                if (appointment == null)
                    throw new NotFoundException("Belirtilen kriterlere uyan bir randevu bulunamadı.");

                appointment.Status = model.Status;

                _unitOfWork.AppointmentRepository.Update(appointment);
                await _unitOfWork.Save();

                await _mailIslemleri.SendAppointmentConfirmationEmail(user, appointment);

                if (appointment.Status == AppointmentStatus.Onaylandı|| appointment.Status==AppointmentStatus.Reddedildi)
                {
                    await _mailIslemleri.DeleteUserAndAppointmentsAsync(user);
                }

                return NoContent();
            }
            catch (BadRequestException badEx)
            {
                return BadRequest(new { Message = badEx.Message });
            }
            catch (NotFoundException nfEx)
            {
                return NotFound(new { Message = nfEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

       

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _unitOfWork.AppointmentRepository.GetByIdAsync(id);
                _unitOfWork.AppointmentRepository.Delete(existing);
                await _unitOfWork.Save();
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
        }
    }
}

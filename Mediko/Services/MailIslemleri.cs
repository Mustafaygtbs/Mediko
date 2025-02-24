using Mediko.DataAccess.Interfaces;
using Mediko.DataAccess;
using Mediko.Entities.Exceptions;
using Mediko.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mediko.Services
{
    public class MailIslemleri
    {
        private readonly MedikoDbContext _context;
        private readonly IEmailService _emailService;

        public MailIslemleri(MedikoDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task SendAdminRegistrationEmail(User admin)
        {
            try
            {
                if (admin == null || string.IsNullOrEmpty(admin.Email))
                    throw new ArgumentNullException("Admin bilgisi eksik veya e-posta adresi tanımlı değil.");

                string subject = " Admin Kaydınız Tamamlandı - Mediko Sağlık Merkezi";

                string emailTemplatePath = "Templates/AdminRegistrationTemplate.html";
                string emailTemplate = await System.IO.File.ReadAllTextAsync(emailTemplatePath);


                emailTemplate = emailTemplate.Replace("{USERNAME}", admin.AdSoyad)
                                             .Replace("{EMAIL}", admin.Email)
                                             .Replace("{DATE}", DateTime.Now.ToString("dd.MM.yyyy HH:mm"));

                await _emailService.SendEmailAsync(admin.Email, subject, emailTemplate);

                Console.WriteLine($"✅ Admin ({admin.AdSoyad}, {admin.Email}) için bilgilendirme maili gönderildi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Admin Kaydı Mail Hatası: {ex.Message}");
            }
        }

        public async Task SendAppointmentCreationEmail(User user, Appointment appointment)
        {
            try
            {
                // 📌 **Poliklinik Bilgisini Yükle**
                appointment = await _context.Appointments
                    .Include(a => a.Policlinic)
                    .FirstOrDefaultAsync(a => a.Id == appointment.Id);

                if (appointment == null)
                    throw new NotFoundException("Randevu bulunamadı.");
                if (appointment.Policlinic == null)
                    throw new NotFoundException("Poliklinik bilgisi bulunamadı.");

                // 📩 **E-Posta İçeriği**
                string subject = "📅 Randevunuz Oluşturuldu - Onay Bekliyor";
                string emailTemplatePath = "Templates/AppointmentCreatedTemplate.html";
                string emailTemplate = await System.IO.File.ReadAllTextAsync(emailTemplatePath);

                emailTemplate = emailTemplate.Replace("{USERNAME}", user.AdSoyad)
                                             .Replace("{STATUS}", appointment.Status.ToString())
                                             .Replace("{DATE}", appointment.AppointmentDate.ToString("yyyy-MM-dd"))
                                             .Replace("{TIME}", appointment.AppointmentTime.ToString("HH:mm"))
                                             .Replace("{POLICLINIC_NAME}", appointment.Policlinic.Name);

                await _emailService.SendEmailAsync(user.Email, subject, emailTemplate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Mail Gönderme Hatası: {ex.Message}");
            }
        }

        /// 📩 **Randevu Onaylandığında veya Reddedildiğinde Mail Gönder**
        public async Task SendAppointmentConfirmationEmail(User user, Appointment appointment)
        {
            try
            {
                appointment = await _context.Appointments
                    .Include(a => a.Policlinic)
                    .FirstOrDefaultAsync(a => a.Id == appointment.Id);

                if (appointment == null)
                    throw new NotFoundException("Randevu bulunamadı.");
                if (appointment.Policlinic == null)
                    throw new NotFoundException("Poliklinik bilgisi bulunamadı.");

                string subject = "📅 Randevu Durumunuz Güncellendi";
                string emailTemplatePath = "Templates/AppointmentStatusTemplate.html";
                string emailTemplate = await System.IO.File.ReadAllTextAsync(emailTemplatePath);

                emailTemplate = emailTemplate.Replace("{USERNAME}", user.AdSoyad)
                                             .Replace("{STATUS}", appointment.Status.ToString())
                                             .Replace("{DATE}", appointment.AppointmentDate.ToString("yyyy-MM-dd"))
                                             .Replace("{TIME}", appointment.AppointmentTime.ToString("HH:mm"))
                                             .Replace("{POLICLINIC_NAME}", appointment.Policlinic.Name);

                await _emailService.SendEmailAsync(user.Email, subject, emailTemplate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Mail Gönderme Hatası: {ex.Message}");
            }
        }

        /// ❌ **Onaylanan veya Reddedilen Randevu Sonrası Kullanıcıyı Sil**
        public async Task DeleteUserAndAppointmentsAsync(User user)
        {
            try
            {
                var userAppointments = await _context.Appointments
                    .Where(a => a.UserId == user.Id)
                    .ToListAsync();

                if (userAppointments.Any())
                {
                    _context.Appointments.RemoveRange(userAppointments);
                    await _context.SaveChangesAsync();
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Kullanıcı ({user.AdSoyad}, {user.OgrenciNo}) ve tüm randevuları başarıyla silindi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kullanıcı silme hatası: {ex.Message}");
            }
        }
    }
}

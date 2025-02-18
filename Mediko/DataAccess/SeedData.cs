using Mediko.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mediko.DataAccess
{
    public static class SeedData
    {
        public static async Task InitializeRolesAndAdminUserAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<User> userManager)
        {
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

  
            var adminEmail = "mediko@kocaeli.edu.tr";
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin == null)
            {
                var adminUser = new User
                {
                    UserName = "mediko",
                    Email = adminEmail,
                    EmailConfirmed = true 
                };

                var result = await userManager.CreateAsync(adminUser);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            var userEmail = "user@mediko.com";
            var existingUser = await userManager.FindByEmailAsync(userEmail);

            if (existingUser == null)
            {
                var defaultUser = new User
                {
                    UserName = "iboibo",
                    Email = userEmail,
                    EmailConfirmed = true
                };

                var userResult = await userManager.CreateAsync(defaultUser);
                if (userResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(defaultUser, "User");
                }
            }

        }

        public static async Task InitializeDepartmentsAsync(IServiceProvider serviceProvider)
        {
            using (var context = new MedikoDbContext(serviceProvider.GetRequiredService<DbContextOptions<MedikoDbContext>>()))
            {
                if (!context.Departments.Any()) 
                {
                    try
                    {
                        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "departments.json");
                        Console.WriteLine($"[SeedData] JSON dosyası yolu: {jsonPath}");

                        if (!File.Exists(jsonPath))
                        {
                            Console.WriteLine($"[SeedData Error] JSON dosyası bulunamadı: {jsonPath}");
                            return;
                        }

                        var json = await File.ReadAllTextAsync(jsonPath);
                        var departments = JsonSerializer.Deserialize<List<Department>>(json);

                        if (departments != null && departments.Any())
                        {
                            context.Departments.AddRange(departments);
                            await context.SaveChangesAsync();
                            Console.WriteLine("[SeedData] Departman verileri başarıyla eklendi.");
                        }
                        else
                        {
                            Console.WriteLine("[SeedData Error] JSON dosyasında departman verisi bulunamadı.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SeedData Error] JSON yükleme hatası: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("[SeedData] Departman verileri zaten mevcut.");
                }
            }
        }


        public static async Task InitializePoliclinicsAsync(IServiceProvider serviceProvider)

        {
            using (var context = new MedikoDbContext(serviceProvider.GetRequiredService<DbContextOptions<MedikoDbContext>>()))
            {
                if (!context.Policlinics.Any()) 
                {
                    try
                    {
                        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "policlinics.json");

                        if (!File.Exists(jsonPath))
                        {
                            Console.WriteLine($"[SeedData Error] JSON dosyası bulunamadı: {jsonPath}");
                            return;
                        }

                        var json = await File.ReadAllTextAsync(jsonPath);
                        var policlinics = JsonSerializer.Deserialize<List<Policlinic>>(json);

                        if (policlinics != null && policlinics.Any())
                        {
                            context.Policlinics.AddRange(policlinics);
                            await context.SaveChangesAsync();
                            Console.WriteLine("[SeedData] Poliklinik verileri başarıyla eklendi.");
                        }
                        else
                        {
                            Console.WriteLine("[SeedData Error] JSON dosyasında poliklinik verisi bulunamadı.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SeedData Error] JSON yükleme hatası: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("[SeedData] Poliklinik verileri zaten mevcut.");
                }
            }
        }


        public static async Task InitializeDoctorsAsync(IServiceProvider serviceProvider)
        {
            using (var context = new MedikoDbContext(serviceProvider.GetRequiredService<DbContextOptions<MedikoDbContext>>()))
            {
                if (!context.Doctors.Any())
                {
                    try
                    {
                        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "doctors.json");

                        if (!File.Exists(jsonPath))
                        {
                            Console.WriteLine($"[SeedData Error] JSON dosyası bulunamadı: {jsonPath}");
                            return;
                        }

                        var json = await File.ReadAllTextAsync(jsonPath);
                        var doctors = JsonSerializer.Deserialize<List<Doctor>>(json);

                        if (doctors != null && doctors.Any())
                        {
                            context.Doctors.AddRange(doctors);
                            await context.SaveChangesAsync();
                            Console.WriteLine("[SeedData] Doktor verileri başarıyla eklendi.");
                        }
                        else
                        {
                            Console.WriteLine("[SeedData Error] JSON dosyasında doktor verisi bulunamadı.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SeedData Error] JSON yükleme hatası: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("[SeedData] Doktor verileri zaten mevcut.");
                }
            }
        }
  
    }
}

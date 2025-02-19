﻿using Mediko.Entities;
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

  
            var adminEmail = "baumreis@kocaeli.edu.tr";
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            var tc= "12345678901";
            var OgrenciNo = "220202017";

            if (existingAdmin == null)
            {
                var adminUser = new User
                {
                    UserName = "baumAdmin",
                    Email = adminEmail,
                    AdSoyad = "baum.cc",
                    TcKimlikNo = tc,
                    OgrenciNo = OgrenciNo,
                    EmailConfirmed = true
                };


                var result = await userManager.CreateAsync(adminUser);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
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
  
    }
}

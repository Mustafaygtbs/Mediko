using Mediko.API.MappingProfiles;
using Mediko.Business.Services;
using Mediko.DataAccess;
using Mediko.DataAccess.Interfaces;
using Mediko.DataAccess.Repositories;
using Mediko.Entities;
using Mediko.Extensions;
using Mediko.Services;
using Mediko.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddControllers().AddJsonOptions(options =>
{
    // döngüsel referansları önlemek için
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;

    // enumları proje genelinde stringe dönüştürmek sertleştirmek için 
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});



// JWT ayarlarını yapılandırma
builder.Services.ConfigureJWT(builder.Configuration);
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// DbContext kurulumu 
builder.Services.AddDbContext<MedikoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// identity kurulumu IdentityCore kullanılarak minimal yapılandırma
builder.Services.AddIdentityCore<User>(options => { })
    .AddRoles<IdentityRole>()
    .AddSignInManager<SignInManager<User>>()
    .AddEntityFrameworkStores<MedikoDbContext>()
    .AddDefaultTokenProviders();

// Background service timeslotlar otomatik yenilenmesi için
builder.Services.AddHostedService<TimeslotBackgroundService>();

// repository ve UnitOfWork servisleri
builder.Services.AddScoped<ITimeslotService, TimeslotService>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();


// Authorize ettiğim zaman 404 alıyordum çünkü redirect oluyordu, bu ayarla 401 döndürüyor
// 404 dönmesiin sebebi default olarak bir login sayfasına yönlendirme yapması ancak o api bulunmadığı için 404 dönüyordu
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
});

// CORS yapılandırması
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddScoped<IEmailService, EmailService>();

// Şu şekilde güncelle:
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<IEmailService, EmailService>(provider =>
    provider.GetRequiredService<EmailService>());

builder.Services.AddScoped<MailIslemleri>();


// EmailSettings sınıfını DI konteynerine doğru şekilde ekleyelim:
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddSingleton(resolver =>
    resolver.GetRequiredService<IOptions<EmailSettings>>().Value);
var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var timeslotService = scope.ServiceProvider.GetRequiredService<ITimeslotService>();

    //  2 gün ileriye slot oluştur, 2 günden eski kullanılmamış olan  slotları temizle
    await timeslotService.GenerateTimeslotsForNextDaysAsync(2);
    await timeslotService.RemoveOldTimeslotsAsync(2);
}

// seed data işlemleri
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<User>>();

    await SeedData.InitializeRolesAndAdminUserAsync(roleManager, userManager);
    await SeedData.InitializeDepartmentsAsync(services);
    await SeedData.InitializePoliclinicsAsync(services);
}

// hata ayıklama için loglma middleware
app.Use(async (context, next) =>
{
    Console.WriteLine($"[{DateTime.UtcNow}] {context.Request.Method} {context.Request.Path}");
    if (context.Request.QueryString.HasValue)
        Console.WriteLine($"Query: {context.Request.QueryString}");
    await next.Invoke();
});

// global exception Handler middleware
app.ConfigureExceptionHandler();


app.UseCors("AllowAll");


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

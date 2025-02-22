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

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.ConfigureJWT(builder.Configuration);


builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddDbContext<MedikoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddIdentity<User, IdentityRole>()
//    .AddEntityFrameworkStores<MedikoDbContext>()
//    .AddDefaultTokenProviders();

builder.Services.AddIdentityCore<User>(options =>
{
})
.AddRoles<IdentityRole>()
.AddSignInManager<SignInManager<User>>() 
.AddEntityFrameworkStores<MedikoDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<ITimeslotService, TimeslotService>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerDocumentation();

// mvc olsa logine yönlendirecekti mvc değil api olduğundan default olarak istek attığı yer bulunmuyor bu yüzden 404 dönüyordu.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader(); 
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var timeslotService = scope.ServiceProvider.GetRequiredService<ITimeslotService>();

    // 2 günlük slot üret
    await timeslotService.GenerateTimeslotsForNextDaysAsync(3);

    // 2 günden eski timeslotları sil
    await timeslotService.RemoveOldTimeslotsAsync(2);
}


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<User>>();

    await SeedData.InitializeRolesAndAdminUserAsync(roleManager, userManager);
    await SeedData.InitializeDepartmentsAsync(services);
    await SeedData.InitializePoliclinicsAsync(services);
}

app.Use(async (context, next) =>
{
    Console.WriteLine($"[{DateTime.UtcNow}] {context.Request.Method} {context.Request.Path}");

    if (context.Request.QueryString.HasValue)
    {
        Console.WriteLine($"Query: {context.Request.QueryString}");
    }

    await next.Invoke();
});

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
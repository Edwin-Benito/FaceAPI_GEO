using Microsoft.EntityFrameworkCore;
using SmartRollCall.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => {
    options.AddPolicy("PermitirVue", policy => {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<SmartRollCall.Api.Services.FaceApiService>();
builder.Services.AddSingleton<SmartRollCall.Api.Services.IFaceDetectionService, SmartRollCall.Api.Services.GoogleVisionService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors("PermitirVue");

// Seed inicial: crea el schema y asegura que existan los salones base
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SmartRollCall.Api.Data.AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.ClassGroups.Any())
    {
        db.ClassGroups.AddRange(
            new SmartRollCall.Api.Models.ClassGroup
            {
                GroupName  = "L-ANTENAS",
                Latitude   = 19.9801312,
                Longitude  = -98.6856554,
                BeaconUUID = "BEACON-01",
                StartTime  = new TimeSpan(7, 0, 0),
                EndTime    = new TimeSpan(9, 0, 0)
            },
            new SmartRollCall.Api.Models.ClassGroup
            {
                GroupName  = "A-35",
                Latitude   = 19.978819,
                Longitude  = -98.686431,
                BeaconUUID = "BEACON-02",
                StartTime  = new TimeSpan(9, 0, 0),
                EndTime    = new TimeSpan(11, 0, 0)
            }
        );
        db.SaveChanges();
    }

    if (!db.Students.Any())
    {
        db.Students.Add(new SmartRollCall.Api.Models.Student
        {
            Name          = "EDWIN BENITO",
            FaceVectorJson = ""   // Se llenará cuando se registre biométricamente
        });
        db.SaveChanges();
    }
}

app.MapControllers();

app.Run();

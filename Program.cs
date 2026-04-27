using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using FitnessApp.API.Data;
using FitnessApp.API.Services;


var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. DATABASE CONTEXT (Entity Framework)
// ==========================================
// ამატებს DbContext-ს Dependency Injection-ში
// ამის წყალობით შეგვიძლია Controller-ებში გამოვიყენოთ FitnessDbContext
builder.Services.AddDbContext<FitnessDbContext>(options =>
    // ვაკავშირებთ SQL Server-ს appsettings.json-დან წაკითხული Connection String-ით
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           // დებაგისთვის - აჩვენებს SQL ბრძანებებს კონსოლში
           .EnableSensitiveDataLogging()
           // დებაგისთვის - აჩვენებს დეტალურ შეცდომებს
           .EnableDetailedErrors());

// ==========================================
// 2. JWT AUTHENTICATION (JSON Web Token)
// ==========================================
// ამატებს JWT ავტორიზაციას
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // JWT ტოკენის ვალიდაციის პარამეტრები
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // ამოწმებს რომ ტოკენი ხელმოწერილია სწორი გასაღებით
            ValidateIssuerSigningKey = true,
            // გასაღებიც იგივე უნდა იყოს რაც ტოკენის შექმნისას გამოიყენეს
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]
                    ?? throw new InvalidOperationException("JWT Key not found"))),

            // ამოწმებს რომ ტოკენი სწორი Issuer-ისგან არის (ვინ შექმნა)
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            // ამოწმებს რომ ტოკენი სწორი Audience-სთვის არის (ვისთვის შეიქმნა)
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],

            // ამოწმებს რომ ტოკენის ვადა არ გასულა
            ValidateLifetime = true,
            // ნულოვანი დროის გადახრა (რომ არ დაუშვას ვადაგასული ტოკენი)
            ClockSkew = TimeSpan.Zero
        };
    });

// ==========================================
// 3. TOKEN SERVICE (JWT Token-ის გენერაცია)
// ==========================================
// ამატებს TokenService-ს Dependency Injection-ში
// AddScoped - ქმნის ერთ ინსტანსს HTTP რექვესტის განმავლობაში
builder.Services.AddScoped<ITokenService, TokenService>();

// ==========================================
// 4. CORS (Cross-Origin Resource Sharing)
// ==========================================
// ამატებს CORS პოლიტიკას - უშვებს რექვესტებს Angular-დან
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            // დასაშვები წარმომავლობები (Origins) - საიდან შეიძლება რექვესტი
            policy.WithOrigins(
                    "https://localhost:44447",  // Angular production (HTTPS)
                    "http://localhost:4200",     // Angular development (HTTP)
                    "http://localhost:4201")     // Angular development (ალტერნატიული)
                  .AllowAnyHeader()              // ნებისმიერი Header-ის დაშვება
                  .AllowAnyMethod()              // ნებისმიერი HTTP Method-ის დაშვება (GET, POST, PUT...)
                  .AllowCredentials();           // Cookie-ების და Authorization Header-ის დაშვება
        });
});

// ==========================================
// 5. JSON SERIALIZATION CONFIGURATION
// ==========================================
// კონფიგურაცია როგორ გარდაიქმნას C# ობიექტები JSON-ად
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // ReferenceHandler.IgnoreCycles - თავიდან აცილებს წრიულ რეფერენციებს
        // მაგ: User -> Trainer -> User -> ... (უსასრულო ციკლი)
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        // JSON-ის ლამაზად დაბეჭდვა (ინდენტაციით)
        options.JsonSerializerOptions.WriteIndented = true;

        // null ველების გამოტოვება JSON-იდან (არ გამოაჩინოს "fieldName": null)
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// ==========================================
// 6. API DOCUMENTATION (Swagger)
// ==========================================
// ამატებს Swagger-ს API დოკუმენტაციისთვის
// Swagger UI ხელმისაწვდომი იქნება /swagger მისამართზე
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ==========================================
// BUILD THE APPLICATION
// ==========================================
// ყველა ზემოთ კონფიგურირებული სერვისის საფუძველზე ვქმნით აპლიკაციას
var app = builder.Build();

// ==========================================
// HTTP REQUEST PIPELINE
// ==========================================
// აქ განისაზღვრება რა მოხდეს HTTP რექვესტის დროს (მიმდევრობა მნიშვნელოვანია!)

// მხოლოდ Development გარემოში ჩართოს Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();      // Swagger JSON ენდპოინტი
    app.UseSwaggerUI();    // Swagger UI ინტერფეისი
}

// გადამისამართება HTTP-დან HTTPS-ზე
app.UseHttpsRedirection();

// CORS-ის ჩართვა - უნდა იყოს UseAuthentication-მდე
app.UseCors("AllowAngular");

// ავტორიზაციის Middleware-ების ჩართვა
// მნიშვნელოვანია: ჯერ Authentication, შემდეგ Authorization
app.UseAuthentication();   // ვინ ხარ? (ამოწმებს JWT ტოკენს)
app.UseAuthorization();    // რისი უფლება გაქვს? (ამოწმებს როლებს)

// Controller-ების მარშრუტების ჩართვა
app.MapControllers();

// ==========================================
// 7. DATABASE INITIALIZATION
// ==========================================
// ამოწმებს არსებობს თუ არა მონაცემთა ბაზა
// თუ არ არსებობს, ქმნის მას (მაგრამ არ აკეთებს Migrations!)
using (var scope = app.Services.CreateScope())
{
    // ვიღებთ DbContext-ს Dependency Injection-დან
    var dbContext = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();

    // ქმნის მონაცემთა ბაზას თუ არ არსებობს
    // ⚠️ მნიშვნელოვანი: ეს არ აკეთებს Migrations-ს!
    // თუ მოდელები შეიცვალა, საჭიროა Update-Database
    dbContext.Database.EnsureCreated();
}


app.Run();


//📊 Service Lifetimes
//Lifetime	მეთოდი	ახსნა
//Singleton	AddSingleton<T>()	ერთი ინსტანსი აპლიკაციის მთელი სიცოცხლის განმავლობაში
//Scoped	AddScoped<T>()	ერთი ინსტანსი HTTP რექვესტის განმავლობაში
//Transient	AddTransient<T>()	ახალი ინსტანსი ყოველ ჯერზე, როცა ითხოვენ
//ჩვენს კოდში:

//AddDbContext → Scoped (ნაგულისხმევად)

//AddScoped<ITokenService, TokenService>() → Scoped

//AddAuthentication → Singleton (ნაგულისხმევად)

//✅ მოკლე აღწერა
//სერვისი	რას აკეთებს
//DbContext	მონაცემთა ბაზასთან კავშირი
//JWT Authentication	ამოწმებს JWT ტოკენებს
//TokenService	ქმნის JWT ტოკენებს
//CORS	უშვებს Angular-ის რექვესტებს
//JSON Options	ასწორებს JSON-ის სერიალიზაციას
//Swagger	API დოკუმენტაცია
//EnsureCreated()	ქმნის მონაცემთა ბაზას თუ არ არსებობს
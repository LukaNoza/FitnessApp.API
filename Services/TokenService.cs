using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FitnessApp.API.Models;

namespace FitnessApp.API.Services
{
    // ITokenService ინტერფეისი - განსაზღვრავს რა მეთოდები უნდა ჰქონდეს TokenService-ს
    public interface ITokenService
    {
        // ქმნის JWT ტოკენს მომხმარებლისთვის
        string CreateToken(User user);
    }

    // JWT (JSON Web Token) ტოკენის შემქმნელი სერვისი
    // JWT არის სტანდარტი უსაფრთხო ავტორიზაციისთვის
    public class TokenService : ITokenService
    {
        // IConfiguration - აპლიკაციის კონფიგურაციაზე წვდომა (appsettings.json)
        private readonly IConfiguration _configuration;

        // კონსტრუქტორი - IConfiguration-ს ვიღებთ Dependency Injection-ით
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ქმნის JWT ტოკენს მომხმარებლის მონაცემების საფუძველზე
        // JWT შედგება 3 ნაწილისგან: Header, Payload, Signature
        public string CreateToken(User user)
        {
            // ==========================================
            // 1. CLAIMS (Payload-ის ნაწილი)
            // ==========================================
            // Claims არის ინფორმაცია, რომელსაც ტოკენი ატარებს მომხმარებლის შესახებ
            var claims = new List<Claim>
            {
                // NameIdentifier - მომხმარებლის უნიკალური ID (ინახება როგორც "nameid")
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                
                // Email - მომხმარებლის ელ.ფოსტა (ინახება როგორც "email")
                new Claim(ClaimTypes.Email, user.Email),
                
                // Role - მომხმარებლის როლი (Admin, Trainer, Client)
                // ეს არის მთავარი ველი როლებზე წვდომისთვის
                new Claim(ClaimTypes.Role, user.UserType),
                
                // Custom Claim - სახელი (არ არის სტანდარტული, თვითონ დავამატეთ)
                new Claim("FirstName", user.FirstName),
                
                // Custom Claim - გვარი
                new Claim("LastName", user.LastName)
            };

            // ==========================================
            // 2. SIGNING KEY (ხელმოწერის გასაღები)
            // ==========================================
            // ვქმნით სიმეტრიულ გასაღებს JWT-ს ხელმოსაწერად
            // ეს გასაღები უნდა იყოს იგივე, რაც Program.cs-ში ვალიდაციისთვის გამოიყენება
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found")));

            // ==========================================
            // 3. SIGNING CREDENTIALS (ხელმოწერის მოწმობა)
            // ==========================================
            // HmacSha512Signature - ალგორითმი ხელმოწერისთვის
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // ==========================================
            // 4. TOKEN DESCRIPTOR (ტოკენის აღწერა)
            // ==========================================
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),     // ტოკენში ჩასმული ინფორმაცია (Claims)
                Expires = DateTime.UtcNow.AddDays(7),     // ტოკენის ვადა - 7 დღე
                SigningCredentials = creds,                // ხელმოწერის მონაცემები
                Issuer = _configuration["Jwt:Issuer"],     // ვინ შექმნა ტოკენი
                Audience = _configuration["Jwt:Audience"]  // ვისთვის შეიქმნა ტოკენი
            };

            // ==========================================
            // 5. TOKEN CREATION (ტოკენის შექმნა)
            // ==========================================
            var tokenHandler = new JwtSecurityTokenHandler();  // JWT-ს დამმუშავებელი კლასი
            var token = tokenHandler.CreateToken(tokenDescriptor); // ვქმნით ტოკენს აღწერის მიხედვით

            // ==========================================
            // 6. RETURN (ტოკენის დაბრუნება სტრინგის სახით)
            // ==========================================
            // WriteToken() - გარდაქმნის ტოკენის ობიექტს სტრინგად
            return tokenHandler.WriteToken(token);
        }
    }
}



//🔑 რა არის JWT?
//ტერმინი	ახსნა
//JWT	JSON Web Token - უსაფრთხო ავტორიზაციის სტანდარტი
//Header	შეიცავს ალგორითმის ტიპს (HS512)
//Payload	შეიცავს მომხმარებლის მონაცემებს (Claims)
//Signature	ხელმოწერა - ადასტურებს რომ ტოკენი არ შეცვლილა
//Claim	ინფორმაციის ერთეული (ID, Email, Role...)
//SymmetricSecurityKey	სიმეტრიული გასაღები - იგივე გასაღები ხელმოსაწერად და გასაშიფრად
//HmacSha512Signature	ხელმოწერის ალგორითმი
//🔄 როგორ გამოიყენება?
//text
//1. მომხმარებელი ლოგინდება → AuthController.Login()
//                                    ↓
//2. TokenService.CreateToken() → ქმნის JWT ტოკენს
//                                    ↓
//3. ტოკენი უბრუნდება Frontend-ს
//                                    ↓
//4. Frontend ინახავს ტოკენს localStorage-ში
//                                    ↓
//5. ყოველ API რექვესტზე აგზავნის: Authorization: Bearer[token]
//                                    ↓
//6.Program.cs - ში JWT მიდლვეარი ამოწმებს ტოკენს
//                                    ↓
//7. თუ ტოკენი ვალიდურია, მომხმარებელი ავტორიზებულია
//✅ მოკლე აღწერა
//კომპონენტი	რას აკეთებს
//ITokenService	ინტერფეისი - განსაზღვრავს კონტრაქტს
//TokenService	ახდენს JWT ტოკენის გენერირებას
//Claims	მომხმარებლის ინფორმაცია, რომელიც ტოკენში ინახება
//CreateToken()	ქმნის ტოკენს 7 დღიანი ვადით
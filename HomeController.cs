using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MusicSchoolJournal.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            Console.WriteLine("=== HomeController.Index START ===");
            Console.WriteLine($"Authenticated: {User.Identity.IsAuthenticated}");

            if (!User.Identity.IsAuthenticated)
            {
                Console.WriteLine("User not authenticated, redirecting to Login");
                return RedirectToAction("Login", "Account");
            }

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var name = User.Identity.Name;

            Console.WriteLine($"User: {name}, Role: {role}");

            if (role == "Ученик")
            {
                Console.WriteLine("Redirecting to Student.Index");
                return RedirectToAction("Index", "Student");
            }
            else if (role == "Учитель")
            {
                Console.WriteLine("Redirecting to Teacher.Index");
                return RedirectToAction("Index", "Teacher");
            }
            else if (role == "Администратор")
            {
                Console.WriteLine("Rendering AdminDashboard");
                return View("AdminDashboard");
            }

            Console.WriteLine($"Unknown role: {role}");
            return Content($"Ваша роль: {role}", "text/html");
        }
    }
}
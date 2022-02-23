using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LoginRegistration.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace LoginRegistration.Controllers
{
    public class HomeController : Controller
    {
        private MyContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, MyContext context)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost("Register")]
        public IActionResult Register(User newUser)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "Email already in use");
                    return View("Index");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                _context.Users.Add(newUser);
                _context.SaveChanges();
                HttpContext.Session.SetString("UserEmail", newUser.Email);
                return RedirectToAction("Success");
            }
            return View("Index");
        }
        [HttpGet("Login")]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost("Logging")]
        public IActionResult Logging(LoginUser loginUser)
        {
            if (ModelState.IsValid)
            {
                User dbUser = _context.Users.FirstOrDefault(u => u.Email == loginUser.LoginEmail);
                if (dbUser == null)
                {
                    ModelState.AddModelError("LoginEmail", "Incorrect Email/Password");
                    return View("Login");
                }
                PasswordHasher<LoginUser> Hasher = new PasswordHasher<LoginUser>();
                PasswordVerificationResult result = Hasher.VerifyHashedPassword(loginUser, dbUser.Password, loginUser.LoginPassword);
                if (result == 0)
                {
                    ModelState.AddModelError("LoginEmail", "Incorrect Email/Password");
                    return View("Login");
                }
                HttpContext.Session.SetString("UserEmail", dbUser.Email);
                return RedirectToAction("Success");
            }
            return View("Login");
        }
        [HttpGet("Success")]
        public IActionResult Success()
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                string email = HttpContext.Session.GetString("UserEmail");
                User loggedInUser = _context.Users.FirstOrDefault(u => u.Email == email);
                return View("Success", model: loggedInUser);
            }
            else 
            {
                return View("Index");
            }
        }
        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return View("Index");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

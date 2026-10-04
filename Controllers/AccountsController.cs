using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ArtGalleryFinal.Models;
using ArtGalleryFinal.Services;
using ArtGalleryFinal.Utilities;
using System.Linq;
using BCrypt.Net; // For password hashing
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;


namespace ArtGalleryFinal.Controllers
{
    public class AccountController : Controller
    {
        private readonly ArtGalleryContext _dbContext;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailService _emailService;

        // Constructor to inject the database context and logger
        public AccountController(ArtGalleryContext dbContext, ILogger<AccountController> logger, IEmailService emailService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _emailService = emailService;
        }

        // Redirect users based on their login status
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("AdminId") != null)
                return RedirectToAction("Artworks"); // Admin redirect

            if (HttpContext.Session.GetString("ArtistId") != null)
                return RedirectToAction("Artworks"); // Artist redirect

            if (HttpContext.Session.GetString("CustomerId") != null)
                return RedirectToAction("Artworks"); // Customer redirect

            TempData["Error"] = "Please log in to access your account.";
            return RedirectToAction("Login");
        }

        // GET: Register Page
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Handle Registration Logic
        [HttpPost]
        public IActionResult Register(string role, string name, string email, string password, string confirmPassword, string phoneNo, string address, string country)
        {
            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                TempData["Error"] = "All fields are required. Please fill in the form.";
                return View();
            }

            if (password != confirmPassword)
            {
                TempData["Error"] = "Passwords do not match.";
                return View();
            }

            if (role != "Artist" && role != "Customer")
            {
                TempData["Error"] = "Invalid role selected. Please select either Artist or Customer.";
                return View();
            }

            if (_dbContext.Artists.Any(a => a.Email == email) || _dbContext.Customers.Any(c => c.Email == email))
            {
                TempData["Error"] = "Email is already registered. Please use a different email.";
                return View();
            }

            try
            {
                // Don't create the account yet - hold everything in session until the email is verified.
                var verificationCode = new Random().Next(100000, 999999).ToString();
                var pending = new PendingRegistrationModel
                {
                    Role = role,
                    Name = name,
                    Email = email,
                    HashedPassword = BCrypt.Net.BCrypt.HashPassword(password),
                    PhoneNo = phoneNo,
                    Address = address,
                    Country = country,
                    VerificationCode = verificationCode,
                    ExpiresAt = DateTime.Now.AddMinutes(15)
                };

                HttpContext.Session.SetObjectAsJson("PendingRegistration", pending);
                _emailService.SendRegistrationVerificationEmail(email, name, verificationCode);

                TempData["Message"] = "A verification code has been sent to your email. Please enter it below to activate your account.";
                return View("VerifyRegistration", new VerifyCodeViewModel());
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred during registration.");
                TempData["Error"] = "An unexpected error occurred. Please try again later.";
                return View();
            }
        }

        // GET: shown if the user navigates back to the verify page directly
        [HttpGet]
        public IActionResult VerifyRegistration()
        {
            var pending = HttpContext.Session.GetObjectFromJson<PendingRegistrationModel>("PendingRegistration");
            if (pending == null)
            {
                TempData["Error"] = "Your registration session has expired. Please register again.";
                return RedirectToAction("Register");
            }
            return View(new VerifyCodeViewModel());
        }

        // POST: Verify the email code and only now create the actual account
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyRegistration(VerifyCodeViewModel model)
        {
            var pending = HttpContext.Session.GetObjectFromJson<PendingRegistrationModel>("PendingRegistration");
            if (pending == null)
            {
                TempData["Error"] = "Your registration session has expired. Please register again.";
                return RedirectToAction("Register");
            }

            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.VerificationCode))
            {
                TempData["Error"] = "Please enter the verification code.";
                return View("VerifyRegistration", model);
            }

            if (pending.ExpiresAt < DateTime.Now)
            {
                TempData["Error"] = "Your verification code has expired. Please request a new one.";
                return View("VerifyRegistration", model);
            }

            if (pending.VerificationCode != model.VerificationCode.Trim())
            {
                TempData["Error"] = "Invalid verification code. Please try again.";
                return View("VerifyRegistration", model);
            }

            try
            {
                // Double-check no one else registered with this email while verification was pending.
                if (_dbContext.Artists.Any(a => a.Email == pending.Email) || _dbContext.Customers.Any(c => c.Email == pending.Email))
                {
                    HttpContext.Session.Remove("PendingRegistration");
                    TempData["Error"] = "Email is already registered. Please use a different email.";
                    return RedirectToAction("Register");
                }

                if (pending.Role == "Artist")
                {
                    var artist = new Artist
                    {
                        Name = pending.Name,
                        Email = pending.Email,
                        PhoneNo = pending.PhoneNo,
                        Address = pending.Address,
                        Country = pending.Country,
                        Password = pending.HashedPassword,
                        AdminId = 1
                    };
                    _dbContext.Artists.Add(artist);
                }
                else
                {
                    var customer = new Customer
                    {
                        Name = pending.Name,
                        Email = pending.Email,
                        PhoneNo = pending.PhoneNo,
                        Address = pending.Address,
                        Country = pending.Country,
                        Password = pending.HashedPassword,
                        AdminId = 1
                    };
                    _dbContext.Customers.Add(customer);
                }

                _dbContext.SaveChanges();
                HttpContext.Session.Remove("PendingRegistration");

                TempData["Message"] = "Email verified successfully! Your account has been created - you can now log in.";
                return RedirectToAction("Login");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while finalizing registration after verification.");
                TempData["Error"] = "An unexpected error occurred. Please try again later.";
                return View("VerifyRegistration", model);
            }
        }

        // POST: Resend the registration verification code
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResendRegistrationCode()
        {
            var pending = HttpContext.Session.GetObjectFromJson<PendingRegistrationModel>("PendingRegistration");
            if (pending == null)
            {
                TempData["Error"] = "Your registration session has expired. Please register again.";
                return RedirectToAction("Register");
            }

            pending.VerificationCode = new Random().Next(100000, 999999).ToString();
            pending.ExpiresAt = DateTime.Now.AddMinutes(15);
            HttpContext.Session.SetObjectAsJson("PendingRegistration", pending);

            _emailService.SendRegistrationVerificationEmail(pending.Email, pending.Name, pending.VerificationCode);

            TempData["Message"] = "A new verification code has been sent to your email.";
            return View("VerifyRegistration", new VerifyCodeViewModel());
        }

        // GET: Login Page
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Handle Login Logic
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            try
            {
                // Check Admin Login
                var admin = _dbContext.Admins.FirstOrDefault(a => a.Email == username);
                if (admin != null && BCrypt.Net.BCrypt.Verify(password, admin.Password))
                {
                    HttpContext.Session.SetString("AdminId", admin.AdminId.ToString());
                    TempData["Message"] = "Welcome, Admin!";
                    return RedirectToAction("Artworks", "Artwork"); // Redirect to Admin's ManageArtwork page
                }

                // Check Artist Login
                var artist = _dbContext.Artists.FirstOrDefault(a => a.Email == username);
                if (artist != null && BCrypt.Net.BCrypt.Verify(password, artist.Password))
                {
                    HttpContext.Session.SetString("ArtistId", artist.ArtistId.ToString());
                    HttpContext.Session.SetString("ArtistName", artist.Name);
                    HttpContext.Session.SetString("ArtistEmail", artist.Email);
                    TempData["Message"] = $"Welcome back, {artist.Name}!";
                    return RedirectToAction("Artworks", "Artwork"); // Redirect to Artist's ManageMyArtwork page
                }

                // Check Customer Login
                var customer = _dbContext.Customers.FirstOrDefault(c => c.Email == username);
                if (customer != null && BCrypt.Net.BCrypt.Verify(password, customer.Password))
                {
                    HttpContext.Session.SetString("CustomerId", customer.CustomerId.ToString());
                    TempData["Message"] = $"Welcome back, {customer.Name}!";
                    return RedirectToAction("Artworks", "Artwork"); // Redirect to Customer's Artwork page
                }

                // Login failed
                TempData["Error"] = "Invalid username or password.";
                return View();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login.");
                TempData["Error"] = "An unexpected error occurred. Please try again later.";
                return View();
            }
        }

        // Forgot Password
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // Handle Sending Verification Code
        private void SendVerificationCodeEmail(string email, string name, string verificationCode)
        {
            try
            {
                // Log the simulated email for debugging
                _logger.LogInformation($"Verification code sent to {email}: {verificationCode}");


                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Art Gallery", "najamartgallery@gmail.com"));
                message.To.Add(new MailboxAddress(name, email));
                message.Subject = "Art Gallery - Password Reset Code";

                message.Body = new TextPart("plain")
                {
                    Text = @$"Hi {name},

Password rest code:{verificationCode}

-- Art Gallery Team"
                };

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, false);

                    // Note: only needed if the SMTP server requires authentication
                    client.Authenticate("najamartgallery@gmail.com", "itscybgjxzqskums");

                    client.Send(message);
                    client.Disconnect(true);
                }

            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while sending verification code to {email}");
                throw; // Rethrow the exception for logging in the controller action
            }
        }

        // POST: Send Verification Code
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendVerificationCode(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please provide a valid email address.";
                return View("ForgotPassword");
            }

            var customer = _dbContext.Customers.FirstOrDefault(c => c.Email == model.Email);
            var artist = _dbContext.Artists.FirstOrDefault(c => c.Email == model.Email);
            if (customer == null && artist == null)
            {
                TempData["Error"] = "No account found with this email address.";
                return View("ForgotPassword");
            }
           
            var email = customer?.Email ?? artist?.Email;
            var name = customer?.Name ?? artist?.Name;
            var verificationCode = new Random().Next(100000, 999999).ToString();
            //customer.PasswordResetToken = verificationCode;
            //customer.TokenExpiration = DateTime.Now.AddMinutes(15); // Code valid for 15 minutes

            if (customer != null)
            {
                customer.PasswordResetToken = verificationCode;
                customer.TokenExpiration = DateTime.Now.AddMinutes(15);
                HttpContext.Session.SetString("CustomerId", customer.CustomerId.ToString());
            }
            else
            {
                artist.PasswordResetToken = verificationCode;
                artist.TokenExpiration = DateTime.Now.AddMinutes(15);
                HttpContext.Session.SetString("ArtistId", artist.ArtistId.ToString());
            }
            _dbContext.SaveChanges();
            SendVerificationCodeEmail(email, name, verificationCode);


            TempData["Success"] = "A verification code has been sent to your email.";

            return View("~/Views/Account/VerifyCode.cshtml", new VerifyCodeViewModel());
        }

        // POST: Verify Code
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please enter the verification code.";
                return View("VerifyCode");
            }
            var customerId = HttpContext.Session.GetString("CustomerId");
            var artistId = HttpContext.Session.GetString("ArtistId");
            object user = null;
            if (customerId != null)
            {
                user = _dbContext.Customers.FirstOrDefault(c => c.PasswordResetToken == model.VerificationCode && c.TokenExpiration > DateTime.Now);
            }
            if (artistId != null)
            {
                user = _dbContext.Artists.FirstOrDefault(a => a.PasswordResetToken == model.VerificationCode && a.TokenExpiration > DateTime.Now);
            }
            if(customerId ==null && artistId ==null)
            {
                TempData["Error"] = "Invalid or expired verification code.";
                return View("VerifyCode");
            }
            if (user == null)
            {
                TempData["Error"] = "Invalid or expired verification code.";
                return View("VerifyCode");
            }
            var email = customerId != null ? ((Customer)user).Email : ((Artist)user).Email;

            // Store the email in the session to use in the next step
            HttpContext.Session.SetString("ResetEmail", email);


            return View("~/Views/Account/ResetPassword.cshtml", new ResetPasswordViewModel() { Email = email });
        }

        // POST: Reset Password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please ensure the passwords match and meet requirements.";
                return View("ResetPassword");
            }
            // Check if Password and Confirm Password match
            if (model.NewPassword != model.ConfirmPassword)
            {
                TempData["Error"] = "Password and Confirm Password do not match.";
                return View("ResetPassword");
            }

            var email = HttpContext.Session.GetString("ResetEmail");
            if (email == null)
            {
                TempData["Error"] = "Session expired. Please restart the process.";
                return RedirectToAction("ForgotPassword");
            }

            var customer = _dbContext.Customers.FirstOrDefault(c => c.Email == model.Email);
            var artist = _dbContext.Artists.FirstOrDefault(a => a.Email == email);
            if (customer != null)
            {
                customer.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                customer.PasswordResetToken = null; // Clear the token
                customer.TokenExpiration = null; // Clear expiration
            }
            else if (artist != null)
            {
                artist.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                artist.PasswordResetToken = null; // Clear the token
                artist.TokenExpiration = null; // Clear expiration
            }

           
            _dbContext.SaveChanges();

            TempData["Success"] = "Your password has been reset successfully!";
            HttpContext.Session.Clear(); // Clear session after reset
            return RedirectToAction("Login");
        }
        [HttpGet]
        public IActionResult Logout()
        {
            // Clear session data
            HttpContext.Session.Clear();

            // Redirect to the Artwork page
            return RedirectToAction("Artworks", "Artwork");
        }

    }
}

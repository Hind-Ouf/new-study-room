using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudyRoomBooking.Models;
using StudyRoomBooking.ViewModels;

namespace StudyRoomBooking.Controllers;

public sealed class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> users,
        SignInManager<ApplicationUser> signIn,
        ILogger<AccountController> logger)
    {
        _users = users;
        _signIn = signIn;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new RegisterViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email.Trim(),
            Email = model.Email.Trim(),
            DisplayName = model.DisplayName.Trim(),
            StudentNumber = model.StudentNumber.Trim()
        };
        var result = await _users.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _signIn.SignInAsync(user, isPersistent: false);
            _logger.LogInformation("Student account created for {Email}", user.Email);
            TempData["Success"] = "Your account is ready. Welcome to StudyRooms!";
            return RedirectToLocal(returnUrl);
        }

        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
        _logger.LogWarning("Registration failed for {Email}", model.Email);
        return View(model);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);

        var result = await _signIn.PasswordSignInAsync(
            model.Email.Trim(), model.Password, model.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            _logger.LogInformation("Student signed in: {Email}", model.Email);
            return RedirectToLocal(returnUrl);
        }

        if (result.IsLockedOut)
            ModelState.AddModelError(string.Empty, "Too many attempts. Please wait 10 minutes before trying again.");
        else
            ModelState.AddModelError(string.Empty, "The email or password was not recognized.");
        _logger.LogWarning("Failed sign-in for {Email}", model.Email);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var email = User.Identity?.Name;
        await _signIn.SignOutAsync();
        _logger.LogInformation("Student signed out: {Email}", email);
        TempData["Success"] = "You have been signed out.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    private IActionResult RedirectToLocal(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction("Index", "Home");
}

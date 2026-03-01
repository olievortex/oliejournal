using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace oliejournal.web.Pages;

[Authorize]
public class DeleteAccountModel(IHttpClientFactory httpClientFactory, ILogger<DeleteAccountModel> logger) : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<DeleteAccountModel> _logger = logger;

    [BindProperty]
    [Required(ErrorMessage = "Please type the confirmation text.")]
    [RegularExpression("DELETE MY ACCOUNT", ErrorMessage = "The confirmation text must match exactly.")]
    public string? ConfirmationText { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public void OnGet()
    {
        // Page loads
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Please confirm by typing 'DELETE MY ACCOUNT' exactly as shown.";
            return Page();
        }

        try
        {
            // Call the DeleteAccount API endpoint using OlieWebCommon helper
            await OlieWebCommon.ApiDelete(_httpClientFactory, HttpContext, "/api/journal/deleteAccount", ct);

            SuccessMessage = "Your account has been successfully deleted. You will be logged out shortly.";
            _logger.LogInformation("Account deletion successful for user.");

            // Log out of the account after deletion
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

            var returnUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/DeleteAccountComplete";
            return Redirect($"https://antihoistentertainment.kinde.com/logout?redirect={returnUrl}");
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = "An error occurred while deleting your account. Please try again or contact support.";
            _logger.LogError(ex, "Account deletion failed with HTTP error.");
            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = "An unexpected error occurred. Please try again later.";
            _logger.LogError(ex, "Exception occurred during account deletion.");
            return Page();
        }
    }
}
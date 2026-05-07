using System.Text.Json;

namespace SnackFlowMES.Services;

/// <summary>
/// Google reCAPTCHA v3 verification service
/// </summary>
public interface IRecaptchaService
{
    Task<RecaptchaResponse> VerifyTokenAsync(string token, string action);
    Task<bool> IsValidAsync(string token, string action, double minimumScore = 0.5);
}

public class RecaptchaService : IRecaptchaService
{
    private readonly IConfiguration _config;
    private readonly ILogger<RecaptchaService> _log;
    private readonly HttpClient _httpClient;

    public RecaptchaService(IConfiguration config, ILogger<RecaptchaService> log, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _log = log;
        _httpClient = httpClientFactory.CreateClient();
    }

    /// <summary>
    /// Verify reCAPTCHA token with Google API
    /// </summary>
    public async Task<RecaptchaResponse> VerifyTokenAsync(string token, string action)
    {
        try
        {
            var secretKey = _config["RecaptchaSettings:SecretKey"];
            
            if (string.IsNullOrEmpty(secretKey))
            {
                _log.LogError("reCAPTCHA secret key is not configured");
                return new RecaptchaResponse { Success = false, ErrorCodes = new[] { "missing-secret-key" } };
            }

            if (string.IsNullOrEmpty(token))
            {
                _log.LogWarning("reCAPTCHA token is empty");
                return new RecaptchaResponse { Success = false, ErrorCodes = new[] { "missing-token" } };
            }

            // Call Google reCAPTCHA API
            var requestUrl = $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={token}";
            var response = await _httpClient.PostAsync(requestUrl, null);

            if (!response.IsSuccessStatusCode)
            {
                _log.LogError("reCAPTCHA API request failed with status code: {StatusCode}", response.StatusCode);
                return new RecaptchaResponse { Success = false, ErrorCodes = new[] { "api-request-failed" } };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var recaptchaResponse = JsonSerializer.Deserialize<RecaptchaResponse>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (recaptchaResponse == null)
            {
                _log.LogError("Failed to deserialize reCAPTCHA response");
                return new RecaptchaResponse { Success = false, ErrorCodes = new[] { "deserialization-failed" } };
            }

            // Verify action matches
            if (recaptchaResponse.Success && recaptchaResponse.Action != action)
            {
                _log.LogWarning("reCAPTCHA action mismatch. Expected: {Expected}, Got: {Actual}", action, recaptchaResponse.Action);
                recaptchaResponse.Success = false;
                recaptchaResponse.ErrorCodes = new[] { "action-mismatch" };
            }

            _log.LogInformation("reCAPTCHA verification: Success={Success}, Score={Score}, Action={Action}", 
                recaptchaResponse.Success, recaptchaResponse.Score, recaptchaResponse.Action);

            return recaptchaResponse;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error verifying reCAPTCHA token");
            return new RecaptchaResponse { Success = false, ErrorCodes = new[] { "exception" } };
        }
    }

    /// <summary>
    /// Check if reCAPTCHA token is valid with minimum score threshold
    /// </summary>
    public async Task<bool> IsValidAsync(string token, string action, double minimumScore = 0.5)
    {
        var response = await VerifyTokenAsync(token, action);
        
        if (!response.Success)
        {
            _log.LogWarning("reCAPTCHA verification failed. Errors: {Errors}", 
                string.Join(", ", response.ErrorCodes ?? Array.Empty<string>()));
            return false;
        }

        if (response.Score < minimumScore)
        {
            _log.LogWarning("reCAPTCHA score too low. Score: {Score}, Minimum: {Minimum}", 
                response.Score, minimumScore);
            return false;
        }

        return true;
    }
}

/// <summary>
/// Google reCAPTCHA API response model
/// </summary>
public class RecaptchaResponse
{
    public bool Success { get; set; }
    public double Score { get; set; }
    public string? Action { get; set; }
    public DateTime ChallengeTs { get; set; }
    public string? Hostname { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("error-codes")]
    public string[]? ErrorCodes { get; set; }
}

using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using StudentBlogg.Configurations;
using StudentBlogg.Feature.Users.Interfaces;

namespace StudentBlogg.Middleware;

public class BasicAuthentication : IMiddleware
{
    private readonly ILogger<BasicAuthentication> _logger;
    private readonly IUserService _userService;
    private readonly List<Regex> _excludePatterns;

    public BasicAuthentication(ILogger<BasicAuthentication> logger, IUserService userService, IOptions<BasicAuthenticationOptions> options)
    {
        _logger = logger;
        _userService = userService;
        _excludePatterns = options.Value.ExcludePatterns.Select(p => new Regex(p)).ToList();
    }
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (_excludePatterns.Any(regex => regex.IsMatch(context.Request.Path)))
        {
            await next(context);
            return;
        }
            
        
        string authHeader = context.Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(authHeader))
        {
            _logger.LogWarning("Authentication header is empty");
            throw new InvalidAuthHeaderException();
        }
        
        if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Authentication header is not correct");
            throw new InvalidAuthHeaderException();
        }
        
        SplitString(authHeader, " ", out string basic, out string base64String);
        if (string.IsNullOrEmpty(base64String) || string.IsNullOrWhiteSpace(basic))
        {
            _logger.LogWarning("Authentication header is empty");
            throw new InvalidAuthHeaderException();
        }

        string userName, password;

        try
        {
            string usernamePassword = ExtractBase64String(base64String);
            SplitString(usernamePassword, ":", out userName, out password);

            if (string.IsNullOrEmpty(userName) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Missing username and/or password");
                throw new MissingUsernameOrPasswordException();
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Authentication header is invalid");
            throw new InvalidAuthHeaderException();
        }
        
        Guid userId = await _userService.AuthenticateUserAsync(userName, password);
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("Username and/or password is incorrect");
            throw new WrongUsernameOrPasswordException();
        }
        
        context.Items["UserId"] = userId.ToString();
        
        // går til neste middleware
        await next(context);
    }

    private static string ExtractBase64String(string base64String)
    {
        byte[] base64Bytes = Convert.FromBase64String(base64String);
        string userNamePassword = Encoding.UTF8.GetString(base64Bytes);
        return userNamePassword;
        
    }

    private static void SplitString(string authHeader, string seperator, out string left, out string right)
    {
        left = right = string.Empty;
        string[] arr = authHeader.Split(seperator);
        if (arr is not [var a, var b]) return;
        left = a;
        right = b;
    }
}
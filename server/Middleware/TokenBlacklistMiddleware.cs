public class TokenBlacklistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly List<string> _blacklist; 

    public TokenBlacklistMiddleware(RequestDelegate next)
    {
        _next = next;
        _blacklist = new List<string>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (!string.IsNullOrEmpty(token) && _blacklist.Contains(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Token is blacklisted.");
            return;
        }

        await _next(context);
    }

    public void AddToBlacklist(string token)
    {
        _blacklist.Add(token);
    }
}

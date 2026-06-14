using System.Security.Claims;
using System.Text.Json;

namespace ATL.Sankofa.Media.UI.Shared.Auth;

public static class JwtTokenParser
{
    public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs == null)
            return claims;

        foreach (var kvp in keyValuePairs)
        {
            if (kvp.Value is JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in element.EnumerateArray())
                    {
                        claims.Add(new Claim(kvp.Key, item.GetString() ?? string.Empty));
                    }
                }
                else
                {
                    claims.Add(new Claim(kvp.Key, element.GetRawText().Trim('"')));
                }
            }
            else
            {
                claims.Add(new Claim(kvp.Key, kvp.Value?.ToString() ?? string.Empty));
            }
        }

        // Map standard JWT claims to .NET claim types
        var nameClaim = claims.FirstOrDefault(c => c.Type == "unique_name" || c.Type == "name");
        if (nameClaim != null && !claims.Any(c => c.Type == ClaimTypes.Name))
        {
            claims.Add(new Claim(ClaimTypes.Name, nameClaim.Value));
        }

        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64.Replace('-', '+').Replace('_', '/'));
    }
}

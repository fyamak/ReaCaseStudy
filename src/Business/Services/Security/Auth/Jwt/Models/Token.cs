﻿namespace Business.Services.Security.Auth.Jwt.Models;

public class Token
{
    public string   AccessToken  { get; set; }
    public DateTime Expiration   { get; set; }
    public string   RefreshToken { get; set; }

    public Token(string accessToken, DateTime expiration, string refreshToken)
    {
        AccessToken  = accessToken;
        Expiration   = expiration;
        RefreshToken = refreshToken;
    }
}

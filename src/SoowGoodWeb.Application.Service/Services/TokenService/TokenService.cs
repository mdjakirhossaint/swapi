using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using SoowGood.Core.Service;
using SoowGood.Domain.Service.Models;
using SoowGoodWeb.Core.Service;
using SoowGoodWeb.Core.Service.GenericModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Volo.Abp.Identity;

namespace SoowGoodWeb.Application.Service.Services.TokenService
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private static readonly byte[] key = Encoding.UTF8.GetBytes("Agfd11384HSOTITYH@84584DHFDgsdg3746$$FGDSF7hgdh");
        private static readonly byte[] iv = Encoding.UTF8.GetBytes("my initialization vector");
        public async Task<string> GenerateAccessToken(IdentityUser user)
        {
            if (user == null)
            {
                return null;
            }

            string expireTime = _configuration.GetSection("Settings").GetSection("TokenExpireTimeInSecound").Value;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("Settings:Token").Value);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                     new Claim(ClaimTypes.PrimarySid, user.Id.ToString()),
                     new Claim(ClaimTypes.Email, user.UserName)
                }),

                Expires = DateTime.UtcNow.AddDays(7),
                //Expires = DateTime.UtcNow.AddSeconds(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }
        public async Task<string> GenerateAccessTokenDraft(IdentityUser user)
        {
            if (user == null)
            {
                return null;
            }

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            // Add other claims as needed
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("zaksoft"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "YourIssuer",
                audience: "YourAudience",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> GenerateRefreshToken(IdentityUser user)
        {

            string expireTimeInSeconds = _configuration.GetSection("Settings:TokenExpireTimeInSecound").Value;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("Settings:Token").Value);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim(ClaimTypes.PrimarySid, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.UserName)
                }),

                //Expires = DateTime.UtcNow.AddSeconds(int.Parse(expireTimeInSeconds)),
                Expires = DateTime.UtcNow.AddMonths(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }


        public async Task<string> GetPublicOrToken(string token)
        {
            var tokenOrPublicKey = "";
            tokenOrPublicKey = token.Replace("bearer ", "");

            if (tokenOrPublicKey == "bearer" || tokenOrPublicKey == "")
            {
                tokenOrPublicKey = null;
            }
            await Task.CompletedTask;
            return tokenOrPublicKey ?? string.Empty;
        }

        public async Task<bool> MatchPublicKey(string publicKey)
        {
            string publicApiKey = _configuration.GetSection("Settings").GetSection("PublicApiKey").Value;

            // Remove any non-alphanumeric characters and convert to lowercase
            string cleanedInput1 = new string(publicApiKey.Where(char.IsLetterOrDigit).Select(char.ToLower).ToArray());
            string cleanedInput2 = new string(publicKey.Where(char.IsLetterOrDigit).Select(char.ToLower).ToArray());

            // Compare the cleaned inputs for equality
            await Task.CompletedTask;
            return cleanedInput1 == cleanedInput2;

        }

        public string DecryptToken(string token, string apiKey)
        {

            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Encrypted token cannot be null or empty.");
            }
            try
            {
                byte[] key = Encoding.UTF8.GetBytes(apiKey);
                using (var generator = RandomNumberGenerator.Create())
                {
                    generator.GetBytes(key);
                }

                byte[] cipherText = Convert.FromBase64String(token);
                using Aes aes = Aes.Create();
                aes.Key = key;

                ICryptoTransform decryptor = aes.CreateDecryptor();
                using MemoryStream msDecrypt = new MemoryStream(cipherText);
                using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using StreamReader srDecrypt = new StreamReader(csDecrypt);

                return srDecrypt.ReadToEnd();
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("The encrypted token is not in a valid format.", ex);
            }

            catch (Exception ex)
            {
                throw new Exception("An error occurred while decrypting the token.", ex);
            }

        }

        public async Task<Response<Guid>> GetUserIdFromJwtToken(string token)
        {
            var response = new Response<Guid>();
            string apiKey = _configuration.GetSection("Settings").GetSection("Token").Value;
            var tokenValidationResult = Guid.Empty;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(apiKey);

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(apiKey))
            {
                tokenValidationResult = Guid.Empty;
            }
            else
            {
                try
                {
                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                    var jwtToken = (JwtSecurityToken)validatedToken;
                    var expires = jwtToken.ValidTo;
                    if (expires > DateTime.UtcNow)
                    {
                        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "primarysid");
                        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
                        {
                            tokenValidationResult = userId;
                        }
                    }
                    else
                    {
                        response.Message = ApiResponseMessage.user_can_not_find;
                        tokenValidationResult = Guid.Empty;
                    }
                }
                catch (SecurityTokenExpiredException)
                {
                    tokenValidationResult = Guid.Empty;
                }
                catch
                {
                    // Other exceptions are handled here
                    tokenValidationResult = Guid.Empty;
                }

            }
            response.Result = tokenValidationResult;
            return response;
        }

        public List<Claim> JwtTokenDecode(string token)
        {
            string decodeToken = token;
            // Decode the JWT token
            var handler = new JwtSecurityTokenHandler();
            var tokenS = handler.ReadJwtToken(token);
            // Access claims
            var claims = tokenS.Claims;

            return claims.ToList();
        }

        public string TokenEncryptor(string token, string apiKey)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token cannot be null or empty.");
            }

            byte[] key = Encoding.UTF8.GetBytes(apiKey);
            byte[] iv = new byte[16]; // The IV is always 16 bytes for AES

            try
            {
                using (var generator = RandomNumberGenerator.Create())
                {
                    generator.GetBytes(key);
                }

                using Aes aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using MemoryStream msEncrypt = new MemoryStream();
                using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                using StreamWriter swEncrypt = new StreamWriter(csEncrypt);
                swEncrypt.Write(token);
                csEncrypt.FlushFinalBlock();
                byte[] encrypted = msEncrypt.ToArray();
                return Convert.ToBase64String(encrypted);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while encrypting the token.", ex);
            }
        }

        public async Task<bool> TokenExpireOrNot(string token)
        {
            string apiKey = _configuration.GetSection("Settings").GetSection("Token").Value;
            bool tokenValidationResult = false;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(apiKey);

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(apiKey))
            {
                tokenValidationResult = false;
            }
            else
            {
                try
                {
                    var jwtToken = new JwtSecurityToken(token);
                    try
                    {
                        // Validate the token using the specified parameters
                        tokenHandler.ValidateToken(token, new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(key),
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ClockSkew = TimeSpan.Zero
                        }, out SecurityToken validatedToken);

                        // Cast the validated token to JwtSecurityToken
                        jwtToken = (JwtSecurityToken)validatedToken;

                        // Proceed with logic using the jwtToken
                    }
                    catch (SecurityTokenException ex)
                    {
                        // Handle specific security token exceptions (e.g., invalid signature, expired token)
                        Console.WriteLine($"Token validation failed: {ex.Message}");
                        tokenValidationResult = false;
                        return tokenValidationResult;
                    }
                    catch (ArgumentException ex)
                    {
                        // Handle general argument exceptions (e.g., invalid token format)
                        Console.WriteLine($"Invalid argument: {ex.Message}");
                        tokenValidationResult = false;
                        return tokenValidationResult;
                    }
                    catch (Exception ex)
                    {
                        // Handle any other unexpected exceptions
                        Console.WriteLine($"An error occurred: {ex.Message}");
                        tokenValidationResult = false;
                        return tokenValidationResult;
                    }

                    var expires = jwtToken.ValidTo;
                    if (expires < DateTime.UtcNow)
                    {
                        tokenValidationResult = false;
                    }
                    else
                    {
                        tokenValidationResult = true;

                    }
                }
                catch (SecurityTokenExpiredException)
                {
                    tokenValidationResult = false;
                }
            }
            return tokenValidationResult;
        }

        public async Task<TokenValidationResultViewModel> ValidateRefreshToken(string token)
        {
            string apiKey = _configuration.GetSection("Settings").GetSection("Token").Value;
            var tokenValidationResult = new TokenValidationResultViewModel();
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(apiKey);

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(apiKey))
            {
                tokenValidationResult.token = null;
                tokenValidationResult.userId = null;
                tokenValidationResult.userLoggedIn = false;
            }
            else
            {
                try
                {
                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                    var jwtToken = (JwtSecurityToken)validatedToken;
                    var expires = jwtToken.ValidTo;
                    if (expires > DateTime.UtcNow)
                    {
                        // The token has expired, so refresh it
                        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "primarysid");
                        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
                        {
                            tokenValidationResult.userId = userId;
                            tokenValidationResult.userLoggedIn = true;
                            TokenValidationResult.ReferenceEquals(token, tokenValidationResult.token);
                        }
                    }
                    else
                    {
                        tokenValidationResult.userLoggedIn = false;
                        tokenValidationResult.token = null;
                    }
                }
                catch (SecurityTokenExpiredException)
                {
                    tokenValidationResult.userLoggedIn = false;
                    tokenValidationResult.token = null;
                }
                catch
                {
                    tokenValidationResult.userLoggedIn = false;
                    tokenValidationResult.token = null;
                }

            }
            return tokenValidationResult;
        }

        public async Task<(Guid userId, bool PublicKey, UserRetrievalStatus status, string message)> GetUserIdFromAuthorizationToken(string authorization_token)
        {
            authorization_token = await GetPublicOrToken(authorization_token);
            bool publicApiKey = false;

            if (string.IsNullOrWhiteSpace(authorization_token))
            {
                publicApiKey = false;
                return (Guid.Empty, publicApiKey, UserRetrievalStatus.TokenInvalid, ApiResponseMessage.invalidApiKeyOrToken);
            }

            var matchApiKey = await MatchPublicKey(authorization_token);

            if (matchApiKey)
            {
                publicApiKey = true;
                return (Guid.Empty, publicApiKey, UserRetrievalStatus.TokenInvalid, ApiResponseMessage.invalidPublicApiKey);
            }

            var userId = await GetUserIdFromJwtToken(authorization_token);
            //var userInfo = await _userInfoService.GetById(userId.ToString());

            //if (userInfo.Result == null)
            //{
            //    publicApiKey = false;
            //    return (Guid.Empty, publicApiKey, UserRetrievalStatus.NotFound, ApiResponseMessage.user_can_not_find);
            //}

            //if (!userInfo.Result.is_active)
            //{
            //    publicApiKey = false;
            //    return (Guid.Empty, publicApiKey, UserRetrievalStatus.Inactive, ApiResponseMessage.user_incorrect_email_account_blocked);
            //}

            return (userId.Result, publicApiKey, UserRetrievalStatus.Success, userId.Message);
        }


    }
}

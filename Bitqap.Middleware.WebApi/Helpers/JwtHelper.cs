
namespace Bitqap.Middleware.WebApi.Extensions
{
    public static class JwtHelper
    {
        public static string GenerateToken(User user, JwtSettings settings)
        {
            var claims = new Claim[]
            {
                new Claim("firstname", user.Firstname),
                new Claim("id", user.Id.ToString()),
                new Claim("username", user.Username),
                new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddHours(settings.ValidTimeByHours)).ToUnixTimeSeconds().ToString())
                //new Claim(JwtRegisteredClaimNames.Aud, settings.ValidAudience),
                //new Claim(JwtRegisteredClaimNames.Iss, settings.ValidIssuer)
            };

            var token = new JwtSecurityToken(
                new JwtHeader(new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.IssuerSigningKey)),
                                             SecurityAlgorithms.HmacSha256)),
                new JwtPayload(claims));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static Dictionary<string, string> GetDataFromToken(string token)
        {
            token = token.Replace("Bearer ", "");
            Dictionary<string, string> dict = new Dictionary<string, string>();
            var decryptedToken = new JwtSecurityToken(jwtEncodedString: token);
            dict["username"] = decryptedToken.Claims.First(c => c.Type == "username").Value;
            dict["id"] = decryptedToken.Claims.First(c => c.Type == "id").Value;
            dict["firstname"] = decryptedToken.Claims.First(c => c.Type == "firstname").Value;
            dict["nbf"] = decryptedToken.Claims.First(c => c.Type == "nbf").Value;
            dict["exp"] = decryptedToken.Claims.First(c => c.Type == "exp").Value;
            return dict;
        }

        public static User GetUserFromBearerToken(String token)
        {
            User user = new User();
            Dictionary<string, string> dict = GetDataFromToken(token);
            user.Username = dict["username"].ToString();
            user.Id = Convert.ToInt64(dict["id"].ToString());
            user.Username = dict["username"].ToString();
            user.Firstname = dict["firstname"].ToString();
            return user;
        }

    }
}

#nullable enable
using System.Net.Http;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

public class NonLeakedPasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : class
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SHA1Managed _sha1 = new SHA1Managed();

    public NonLeakedPasswordValidator(IHttpClientFactory httpClientFactory) =>
        _httpClientFactory = httpClientFactory;

    // Assumes UTF8 encoding
    private string ComputeSHA1Hash(string password)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(password);
        byte[] hashBytes = _sha1.ComputeHash(bytes);

        StringBuilder sb = new StringBuilder();
        foreach (byte hashByte in hashBytes)
        {
            sb.AppendFormat("{0:x2}", hashByte);
        }

        return sb.ToString().ToUpper();
    }

    private async Task<bool> IsHashLeaked(string hash)
    {
        HttpRequestMessage httpRequestMessage = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://api.pwnedpasswords.com/range/{hash.Substring(0, 5)}");

        HttpClient httpClient = _httpClientFactory.CreateClient();
        HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            string leakedPasswords = await httpResponseMessage.Content.ReadAsStringAsync();

            using (System.IO.StringReader reader = new System.IO.StringReader(leakedPasswords))
            {
                string hashFragment = hash.Substring(5);
                string leakedHashFragment;
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    leakedHashFragment = line.Split(':')[0];
                    if (leakedHashFragment == hashFragment)
                    {
                        return true;
                    }
                }
            }
        }
        else
        {
            throw new System.Exception("Failed to fetch pwned passwords.");
        }

        return false;
    }

    public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
    {
        string passwordHash = ComputeSHA1Hash(password);
        if (await IsHashLeaked(passwordHash))
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "PasswordRequiresNonLeaked",
                Description = "You cannot use a password that has been exposed in a data breach."
            });
        }
        return IdentityResult.Success;
    }
}
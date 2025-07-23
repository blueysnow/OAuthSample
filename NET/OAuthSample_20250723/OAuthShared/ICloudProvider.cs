namespace OAuthShared;

public interface ICloudProvider
{    
    Task<UserDetails> GetUserDetailsAsync();
    Task<bool> SignInAsync();
    Task<bool> SignOutAsync();
}

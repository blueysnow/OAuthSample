# OAuth Sample - Cross-Platform Authentication Demo

A comprehensive OAuth authentication sample demonstrating integration with multiple cloud providers across different platforms using .NET 8, .NET MAUI, WinUI 3, and ASP.NET Core Web API.

## ??? Project Architecture

This solution consists of four main projects:

### ?? OAuthMAUI (.NET MAUI)
- **Platform Support**: Android, iOS, Windows
- **Framework**: .NET 8.0 MAUI
- **Purpose**: Cross-platform mobile and desktop application with OAuth integration

### ??? OAuthWinUI (WinUI 3)
- **Platform**: Windows (x86, x64, ARM64)
- **Framework**: .NET 8.0 Windows App SDK
- **Purpose**: Native Windows application with OAuth authentication

### ?? OAuthWebAPI (ASP.NET Core)
- **Platform**: Cross-platform
- **Framework**: .NET 8.0 Web API
- **Purpose**: Backend API for OAuth callback handling and authentication flow

### ?? OAuthShared (Class Library)
- **Platform**: Cross-platform
- **Framework**: .NET 8.0
- **Purpose**: Shared interfaces and models used across all projects

## ?? Supported Cloud Providers

The solution demonstrates OAuth 2.0 integration with:

- **Microsoft OneDrive** - Using Microsoft Graph API and MSAL
- **Google Drive** - Using Google APIs and OAuth 2.0
- **Dropbox** - Using Dropbox API v2

## ?? Features

### MAUI Application Features
- ? Cross-platform OAuth authentication
- ? Tabbed navigation for different cloud providers
- ? User profile retrieval and display
- ? Sign-in/Sign-out functionality
- ? Platform-specific authentication handling

### WinUI Application Features
- ? Native Windows OAuth integration
- ? MVVM pattern with CommunityToolkit.Mvvm
- ? Template Studio generated architecture
- ? Modern Windows 11 design

### Web API Features
- ? OAuth callback handling
- ? Cross-platform redirect management
- ? Swagger/OpenAPI documentation
- ? Support for multiple OAuth schemes

## ??? Technology Stack

### Frontend Technologies
- **.NET MAUI** - Cross-platform framework
- **WinUI 3** - Modern Windows UI framework
- **XAML** - UI markup language
- **C# 12** - Programming language

### Backend Technologies
- **ASP.NET Core** - Web API framework
- **Swagger/OpenAPI** - API documentation

### Authentication Libraries
- **Microsoft.Identity.Client (MSAL)** - Microsoft authentication
- **Google.Apis.Auth** - Google OAuth 2.0
- **Dropbox.Api** - Dropbox authentication

### Additional Libraries
- **CommunityToolkit.Maui** - MAUI community extensions
- **CommunityToolkit.Mvvm** - MVVM toolkit
- **Microsoft.Graph** - Microsoft Graph API client
- **WinUIEx** - WinUI extensions

## ?? Prerequisites

### Development Environment
- **Visual Studio 2022** (17.8 or later)
- **.NET 8.0 SDK**
- **Windows 11** (for WinUI development)

### MAUI Workloads
```bash
dotnet workload install maui
dotnet workload install maui-android
dotnet workload install maui-ios
dotnet workload install maui-windows
```

### Cloud Provider Setup
1. **Microsoft Azure AD** - Register application for OneDrive integration
2. **Google Cloud Console** - Create OAuth 2.0 credentials
3. **Dropbox App Console** - Register application for API access

## ?? Configuration

### 1. OAuth Client IDs
Update the following files with your OAuth client credentials:

**OneDrive (Microsoft):**
```csharp
// OAuthMAUI/Services/CloudProviders/OneDriveProvider.cs
public const string ClientId = "YOUR_MICROSOFT_CLIENT_ID";
```

**Google Drive:**
```csharp
// Update Google OAuth settings in GoogleDriveProvider.cs
```

**Dropbox:**
```csharp
// Update Dropbox OAuth settings in DropBoxProvider.cs
```

### 2. Redirect URIs
Configure the following redirect URIs in your OAuth applications:

- **MAUI App**: `oauthmaui://`
- **Web API**: `https://your-api-domain.com/oauth/{provider}/signin`

### 3. ngrok Setup (Development)
The sample uses ngrok for local development. Update the URLs in:
```csharp
// OAuthMAUI/Services/CloudProviders/Helpers/GoogleCloudCodeReceiver.cs
public string RedirectUri => "https://your-ngrok-url.ngrok-free.app/oauth/google/signin";
```

## ???¡Î? Getting Started

### 1. Clone the Repository
```bash
git clone <repository-url>
cd OAuthSample
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Set Startup Projects
In Visual Studio, set multiple startup projects:
- `OAuthWebAPI` - Start
- `OAuthMAUI` - Start (optional)
- `OAuthWinUI` - Start (optional)

### 4. Run the Application
```bash
# Run Web API
dotnet run --project OAuthWebAPI

# Run MAUI (in separate terminal)
dotnet run --project OAuthMAUI

# Run WinUI (in separate terminal)
dotnet run --project OAuthWinUI
```

## ?? Platform-Specific Notes

### Android
- **Target API**: Android 14 (API 34)
- **Minimum API**: Android 12L (API 32)
- **Permissions**: Internet access for OAuth flows

### iOS
- **Minimum Version**: iOS 14.0
- **Configuration**: Custom URL scheme handling
- **Entitlements**: Keychain access for token storage

### Windows
- **Target Version**: Windows 11 (10.0.26100.0)
- **Minimum Version**: Windows 10 (10.0.22621.0)
- **Features**: Native WebAuthenticator integration

## ?? Security Considerations

- **Token Storage**: Secure token caching using platform-specific storage
- **HTTPS Only**: All OAuth flows use HTTPS endpoints
- **Token Refresh**: Automatic token refresh handling
- **Error Handling**: Comprehensive error handling for OAuth failures

## ?? Project Structure

```
OAuthSample/
¦§¦¡¦¡ OAuthMAUI/                    # .NET MAUI cross-platform app
¦¢   ¦§¦¡¦¡ Platforms/               # Platform-specific code
¦¢   ¦§¦¡¦¡ Services/CloudProviders/ # OAuth provider implementations
¦¢   ¦§¦¡¦¡ Pages/                   # XAML pages
¦¢   ¦¦¦¡¦¡ Resources/               # Images, fonts, styles
¦§¦¡¦¡ OAuthWinUI/                  # WinUI 3 Windows app
¦¢   ¦§¦¡¦¡ Services/                # Windows-specific services
¦¢   ¦§¦¡¦¡ ViewModels/              # MVVM view models
¦¢   ¦¦¦¡¦¡ Views/                   # WinUI pages and controls
¦§¦¡¦¡ OAuthWebAPI/                 # ASP.NET Core Web API
¦¢   ¦¦¦¡¦¡ Controllers/             # API controllers
¦¦¦¡¦¡ OAuthShared/                 # Shared library
    ¦§¦¡¦¡ ICloudProvider.cs        # Provider interface
    ¦¦¦¡¦¡ UserDetails.cs           # Shared models
```

## ?? Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## ?? License

This project is licensed under the MIT License - see the LICENSE file for details.

## ?? Support

For issues and questions:
1. Check existing GitHub issues
2. Create a new issue with detailed information
3. Include platform-specific details and error logs

## ?? Useful Links

- [.NET MAUI Documentation](https://docs.microsoft.com/dotnet/maui/)
- [WinUI 3 Documentation](https://docs.microsoft.com/windows/apps/winui/)
- [Microsoft Graph API](https://docs.microsoft.com/graph/)
- [Google APIs for .NET](https://developers.google.com/api-client-library/dotnet/)
- [Dropbox API Documentation](https://www.dropbox.com/developers/documentation)

---

**Author**: Paula Aliu  
**Year**: 2024  
**Framework**: .NET 8.0

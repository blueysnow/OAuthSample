# OAuth Sample - Cross-Platform Authentication Demo

A comprehensive OAuth authentication sample demonstrating integration with multiple cloud providers across different platforms using .NET 8, .NET MAUI, WinUI 3, and ASP.NET Core Web API.

## Project Architecture

This solution consists of four main projects:

### OAuthMAUI (.NET MAUI)
- **Platform Support**: Android, iOS, Windows
- **Framework**: .NET 8.0 MAUI
- **Purpose**: Cross-platform mobile and desktop application with OAuth integration

### OAuthWinUI (WinUI 3)
- **Platform**: Windows (x86, x64, ARM64)
- **Framework**: .NET 8.0 Windows App SDK
- **Purpose**: Native Windows application with OAuth authentication

### OAuthWebAPI (ASP.NET Core)
- **Platform**: Cross-platform
- **Framework**: .NET 8.0 Web API
- **Purpose**: Backend API for OAuth callback handling and authentication flow

### OAuthShared (Class Library)
- **Platform**: Cross-platform
- **Framework**: .NET 8.0
- **Purpose**: Shared interfaces and models used across all projects

## Supported Cloud Providers

The solution demonstrates OAuth 2.0 integration with:

- **Microsoft OneDrive** - Using Microsoft Graph API and MSAL
- **Google Drive** - Using Google APIs and OAuth 2.0
- **Dropbox** - Using Dropbox API v2

## Features

### MAUI Application Features
- Cross-platform OAuth authentication
- Tabbed navigation for different cloud providers
- User profile retrieval and display
- Sign-in/Sign-out functionality
- Platform-specific authentication handling

### WinUI Application Features
- Native Windows OAuth integration
- MVVM pattern with CommunityToolkit.Mvvm
- Template Studio generated architecture
- Modern Windows 11 design

### Web API Features
- OAuth callback handling
- Cross-platform redirect management
- Swagger/OpenAPI documentation
- Support for multiple OAuth schemes

## Technology Stack

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

## Prerequisites

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

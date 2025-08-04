# EntraExternalIdPlayground

This solution demonstrates integration of Microsoft Entra External ID, MSAL authentication, and secure API access using .NET MAUI, Blazor WebAssembly, and ASP.NET Core Web API.

## Projects

- **MauiClient**: A .NET MAUI cross-platform client app (Android, Windows) with MSAL authentication and API calls.
- **BlazorClient**: A Blazor WebAssembly app with MSAL authentication and secure API/Graph calls.
- **MsalClientLib**: Shared library for MSAL (Microsoft Authentication Library) logic and Azure AD configuration.
- **WebApi**: ASP.NET Core Web API project, secured for authenticated access.

## Features

- Microsoft Entra External ID (Azure AD B2C/Entra ID) authentication via MSAL
- Cross-platform .NET MAUI client (Android, Windows)
- Blazor WebAssembly client
- Secure API access with bearer tokens
- Microsoft Graph API integration (optional)

## Getting Started

1. **Clone the repository**
   ```sh
   git clone <repo-url>
   cd EntraExternalIdPlayground
   ```

2. **Configure Azure AD/Entra ID**
   - Register applications for MAUI, Blazor, and API in Azure Portal.
   - Update `appsettings.json` and configuration files with your Azure AD details (ClientId, TenantId, Redirect URIs, etc.).

3. **Build and Run**
   - Open the solution in Visual Studio.
   - Set the desired startup project (MauiClient, BlazorClient, or WebApi).
   - Press F5 to build and run.

## Configuration

- **MAUI/Blazor Clients**: Store Azure AD and API endpoint settings in `appsettings.json` or platform-specific configuration.
- **API**: Configure authentication and CORS as needed.
- **MSAL**: Uses `MsalClientLib` for shared authentication logic.

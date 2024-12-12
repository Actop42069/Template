# .NET Project Template

This repository contains a starter template for building .NET-based applications using clean architecture principles. It leverages CQRS and Mediatr for a structured and maintainable codebase.

## Features

- **ASP.NET Core Setup**: Preconfigured ASP.NET Core project with essential middleware.
- **Authentication**: JWT authentication is set up for secure communication.
- **Entity Framework Core**: A sample database context with migrations and sample entities.
- **Identity Service**: Integrated ASP.NET Identity for user management, including role-based access control.
- **Multi-Factor Authentication (MFA)**: Preconfigured MFA using email and SMS for enhanced security.
- **Email Management**: SMTP setup for sending emails with predefined templates for common use cases like user registration, password reset, and notifications.
- **FTP Integration**: Configured FTP client for file uploads and downloads.
- **Logging**: Integrated user-defined logging for better debugging and monitoring.
- **API Endpoints**: Sample RESTful API endpoints using controllers.
- **Unit Testing**: Preconfigured test project with basic unit tests.
- **Configuration Management**: Predefined appsettings files for easy configuration management.
- **CORS**: Cross-Origin Resource Sharing (CORS) settings for API security.

## Integration

To integrate this template into your project:

1. Clone the repository:
   git clone <repository_url>
2. Change the directory to the parent directory (main folder):
   cd <directory_name>
3. Open CMD/Powershell/Shell in that directory.
4. Install the template:
   dotnet new install .
5. After installation, you can view the template by running:
   dotnet new list
6. In the list, search for the template named "Template Project" to confirm the installation.
7. To replicate the project, navigate to your desired folder and open CMD there.
8. Create a new project using the template:
   dotnet new template --name "YourProjectName"
9. Open the solution project in your IDE/Text Editor and enjoy building your application!

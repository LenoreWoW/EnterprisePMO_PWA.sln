# Enterprise PMO PWA

A Progressive Web Application for Enterprise Project Management Office.

## Migration to Supabase

This project has been migrated to use Supabase for both authentication and database operations. Follow these steps to set up your development environment:

### 1. Supabase Setup

1. Create a new Supabase project at https://supabase.com
2. Get your project URL and keys from the project settings
3. Update `appsettings.json` with your Supabase credentials:
   ```json
   "Supabase": {
     "Url": "https://your-project.supabase.co",
     "Key": "your-supabase-anon-key",
     "ServiceKey": "your-supabase-service-key"
   }
   ```

### 2. Database Setup

1. Run the SQL migrations in the Supabase SQL editor:
   - `001_initial_schema.sql`: Creates all necessary tables and policies
   - `002_seed_data.sql`: Inserts initial data (departments and admin user)

### 3. Authentication Setup

1. In your Supabase project settings:
   - Enable Email authentication
   - Configure password policies
   - Set up email templates for:
     - Email confirmation
     - Password reset
     - Magic link (if using)

2. Configure OAuth providers (optional):
   - Google
   - GitHub
   - Microsoft

### 4. Development Setup

1. Install dependencies:
   ```bash
   dotnet restore
   ```

2. Run the application:
   ```bash
   dotnet run --project EnterprisePMO_PWA.Web
   ```

3. Access the application:
   - Web: https://localhost:7001
   - API: https://localhost:7001/api

### 5. Initial Login

1. Use the default admin account:
   - Email: admin@test.com
   - Password: (set during first login)

2. After first login, the admin user will be automatically synchronized with Supabase.

## Features

- User Authentication with Supabase
- Role-based Access Control
- Project Management
- Weekly Updates
- Change Requests
- Audit Logging
- Real-time Notifications
- Department Management

## Architecture

- Frontend: Blazor WebAssembly
- Backend: ASP.NET Core
- Authentication: Supabase Auth
- Database: Supabase PostgreSQL
- Real-time: SignalR
- Background Jobs: Hangfire

## Security

- JWT-based authentication
- Row Level Security (RLS) in Supabase
- Role-based authorization
- Audit logging
- Secure password reset flow

## Development

### Prerequisites

- .NET 7.0 SDK
- Node.js 16+
- Supabase account

### Environment Variables

Create a `appsettings.Development.json` file with your development settings:

```json
{
  "Supabase": {
    "Url": "https://your-dev-project.supabase.co",
    "Key": "your-dev-anon-key",
    "ServiceKey": "your-dev-service-key"
  }
}
```

### Running Tests

```bash
dotnet test
```

## Deployment

1. Update production settings in `appsettings.json`
2. Build the application:
   ```bash
   dotnet publish -c Release
   ```
3. Deploy to your hosting provider

## Support

For support, please contact the development team or create an issue in the repository. 
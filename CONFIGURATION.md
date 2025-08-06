# Configuration Setup Guide

## Sensitive Information Security

This project has been configured to protect sensitive information like database passwords, API keys, and email credentials.

## Configuration Files Structure

### 1. `appsettings.json` (Safe for Git)
Contains placeholder values and default settings that can be shared publicly.

### 2. `appsettings.Development.json` (EXCLUDED from Git)
Contains your actual development credentials. This file is automatically loaded when running in Development environment.

### 3. `appsettings.Production.json` (EXCLUDED from Git) 
Template for production environment - replace placeholders with actual production values.

## Setup Instructions

### For Development:
The `appsettings.Development.json` file is already configured with your development credentials and will be used automatically when running the application locally.

### For Production:
1. Copy `appsettings.Production.json` to your production server
2. Replace all placeholder values with actual production credentials:
   - `YOUR_DB_USER` and `YOUR_DB_PASSWORD` with production database credentials
   - `YOUR_AMADEUS_API_KEY` and `YOUR_AMADEUS_API_SECRET` with production Amadeus API credentials
   - `YOUR_EMAIL@gmail.com` and `YOUR_EMAIL_APP_PASSWORD` with production email credentials

### Environment Variables (Alternative)
You can also use environment variables instead of config files:
```
ConnectionStrings__DefaultConnection="Server=...;Database=...;User=...;Password=..."
Amadeus__ApiKey="your-api-key"
Amadeus__ApiSecret="your-api-secret"
EmailSettings__Username="your-email@gmail.com"
EmailSettings__Password="your-app-password"
```

## Files Excluded from Git

The following files are automatically excluded from version control:
- `**/appsettings.Development.json`
- `**/appsettings.Production.json`  
- `Readme.md` (contains sensitive info)
- `PowerShell/responses/**/*.json` (API response tokens)
- `.vscode/ssjs-setup.json` (potential credentials)

## Security Notes

- ✅ Never commit actual passwords, API keys, or secrets to Git
- ✅ Use app passwords for Gmail (not your regular password)
- ✅ Regularly rotate API keys and passwords
- ✅ Use different credentials for development and production
- ✅ Consider using Azure Key Vault or similar for production secrets

## Original Credentials Moved

Your original credentials have been moved from public files to the secure development configuration file.
# JWT Token Authentication System

## Overview
The Tobaco API now includes a complete JWT (JSON Web Token) authentication system for secure communication with your Flutter frontend.

## 🔐 Token Features

### Enhanced Token Service
- **Secure Token Generation**: Uses HMAC SHA256 with configurable secret
- **Token Validation**: Built-in token validation and expiration checking
- **User Claims**: Includes user ID, username, and timestamp information
- **1-Hour Expiration**: Tokens expire after 1 hour for security

### Token Claims
Each token contains:
- `sub`: User ID
- `jti`: Unique token identifier
- `iat`: Issued at timestamp
- `username`: User's username
- `name`: User's display name

## 🚀 API Endpoints

### 1. Login (Get Token)
```http
POST /api/user/login
Content-Type: application/json

{
  "userName": "admin",
  "password": "admin123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-01T13:00:00Z",
  "user": {
    "id": 1,
    "userName": "admin",
    "email": "admin@tobaco.com",
    "createdAt": "2024-01-01T12:00:00Z",
    "lastLogin": "2024-01-01T12:00:00Z",
    "isActive": true
  }
}
```

### 2. Validate Token
```http
POST /api/user/validate-token
Content-Type: application/json

{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response:**
```json
{
  "isValid": true,
  "userId": "1",
  "expiresAt": "2024-01-01T13:00:00Z"
}
```

### 3. Get Current User (Protected)
```http
GET /api/user/me
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response:**
```json
{
  "id": 1,
  "userName": "admin",
  "email": "admin@tobaco.com",
  "createdAt": "2024-01-01T12:00:00Z",
  "lastLogin": "2024-01-01T12:00:00Z",
  "isActive": true
}
```

## 🔒 Protected Endpoints

All endpoints marked with `[Authorize]` require a valid JWT token:

### Headers Required:
```http
Authorization: Bearer YOUR_JWT_TOKEN_HERE
```

### Protected Controllers:
- `CategoriaController` - All endpoints require authentication
- `UserController` - `/me` endpoint requires authentication

## 📱 Flutter Integration

### 1. Store Token
```dart
// After successful login
String token = response.data['token'];
DateTime expiresAt = DateTime.parse(response.data['expiresAt']);

// Store in secure storage
await secureStorage.write(key: 'jwt_token', value: token);
await secureStorage.write(key: 'token_expires', value: expiresAt.toIso8601String());
```

### 2. Add Token to Requests
```dart
// Add to HTTP client
final token = await secureStorage.read(key: 'jwt_token');
final response = await http.get(
  Uri.parse('$baseUrl/api/categoria'),
  headers: {
    'Authorization': 'Bearer $token',
    'Content-Type': 'application/json',
  },
);
```

### 3. Handle Token Expiration
```dart
// Check if token is expired
final expiresAtString = await secureStorage.read(key: 'token_expires');
if (expiresAtString != null) {
  final expiresAt = DateTime.parse(expiresAtString);
  if (DateTime.now().isAfter(expiresAt)) {
    // Token expired, redirect to login
    await _logout();
  }
}
```

## 🛠️ Configuration

### JWT Settings (appsettings.json)
```json
{
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLong",
    "Issuer": "TobacoAPI",
    "Audience": "TobacoClient"
  }
}
```

### Create Test User
Run the SQL command to create a test user:
```sql
INSERT INTO Users (UserName, Password, Email, CreatedAt, IsActive) 
VALUES ('admin', 'XohImNooBHFR0OVvjcYpJ3NgPQ1qq73WKhHvch0VQtg=', 'admin@tobaco.com', GETUTCDATE(), 1);
```

## 🔧 Security Features

1. **Password Hashing**: SHA256 hashing for password storage
2. **Token Expiration**: 1-hour token lifetime
3. **Secure Headers**: Authorization header required for protected endpoints
4. **Input Validation**: All inputs are validated
5. **Error Handling**: Secure error messages without sensitive information

## 📋 Next Steps

1. **Create Migration**: `dotnet ef migrations add AddUserEntity`
2. **Update Database**: `dotnet ef database update`
3. **Create Test User**: Use the SQL command above
4. **Test Authentication**: Use the login endpoint
5. **Integrate with Flutter**: Implement token storage and usage

## 🚨 Important Notes

- Keep your JWT secret secure and change it in production
- Tokens expire after 1 hour - implement refresh logic if needed
- Always use HTTPS in production
- Store tokens securely in your Flutter app (use secure storage)
- Implement proper logout functionality to clear stored tokens

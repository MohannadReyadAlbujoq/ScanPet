# ?? JWT PUBLIC/PRIVATE KEY GENERATION GUIDE

**Date:** December 2024  
**Purpose:** Generate RSA key pairs for JWT token signing  
**Algorithm:** RS256 (RSA Signature with SHA-256)

---

## ?? OVERVIEW

Your backend uses **RS256 (RSA + SHA256)** for JWT token security:
- **Backend:** Signs tokens with **Private Key**
- **Frontend:** Verifies tokens with **Public Key**
- **Algorithm:** RSA-2048 bit key pair

---

## ?? STEP 1: GENERATE KEYS (Backend)

### Option A: Using OpenSSL (Recommended)

```bash
# 1. Generate Private Key (2048 bits)
openssl genrsa -out private.pem 2048

# 2. Extract Public Key from Private Key
openssl rsa -in private.pem -outform PEM -pubout -out public.pem

# 3. Verify keys were created
ls -la *.pem
```

**Result:**
- `private.pem` - Keep this SECRET on backend only
- `public.pem` - Share this with frontend

### Option B: Using PowerShell (.NET)

```powershell
# Generate RSA key pair using .NET
$rsa = [System.Security.Cryptography.RSA]::Create(2048)

# Export Private Key (PEM format)
$privateKey = $rsa.ExportRSAPrivateKeyPem()
Set-Content -Path "private.pem" -Value $privateKey

# Export Public Key (PEM format)
$publicKey = $rsa.ExportRSAPublicKeyPem()
Set-Content -Path "public.pem" -Value $publicKey

Write-Host "Keys generated successfully!"
```

### Option C: Using Online Generator

**URL:** https://cryptotools.net/rsagen

1. Select key size: **2048 bits**
2. Format: **PEM**
3. Click "Generate"
4. Download both keys

---

## ?? STEP 2: STORE KEYS SECURELY

### Backend Storage (Private Key)

**File Location:**
```
src/API/MobileBackend.API/
??? Keys/
?   ??? private.pem    ? Private key (NEVER commit to Git!)
```

**Add to .gitignore:**
```gitignore
# JWT Keys
Keys/
*.pem
private.key
```

**appsettings.json Configuration:**
```json
{
  "Jwt": {
    "PrivateKeyPath": "Keys/private.pem",
    "PublicKeyPath": "Keys/public.pem",
    "Issuer": "ScanPetMobileBackend",
    "Audience": "ScanPetMobileApp",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  }
}
```

### Production Storage (Best Practices)

**Option 1: Environment Variables**
```bash
# Set as environment variable (base64 encoded)
export JWT_PRIVATE_KEY=$(cat private.pem | base64 -w 0)
export JWT_PUBLIC_KEY=$(cat public.pem | base64 -w 0)
```

**Option 2: Azure Key Vault**
```csharp
// Store in Azure Key Vault
var keyVaultUrl = "https://your-vault.vault.azure.net/";
var secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
await secretClient.SetSecretAsync("JwtPrivateKey", privateKeyPem);
```

**Option 3: AWS Secrets Manager**
```bash
# Store in AWS Secrets Manager
aws secretsmanager create-secret \
    --name JwtPrivateKey \
    --secret-string file://private.pem
```

---

## ?? STEP 3: FRONTEND INTEGRATION

### Public Key Format for Frontend

**public.pem content:**
```
-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA0Z3VS5JJcds3xvn5Y7jt
... (base64 encoded key) ...
-----END PUBLIC KEY-----
```

### JavaScript/React Integration

```javascript
// Install dependencies
npm install jsonwebtoken

// Verify JWT token
import jwt from 'jsonwebtoken';

const publicKey = `-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA0Z3VS5JJcds3xvn5Y7jt
... your public key here ...
-----END PUBLIC KEY-----`;

// Verify token
try {
  const decoded = jwt.verify(token, publicKey, {
    algorithms: ['RS256'],
    issuer: 'ScanPetMobileBackend',
    audience: 'ScanPetMobileApp'
  });
  
  console.log('Token valid:', decoded);
} catch (error) {
  console.error('Token invalid:', error.message);
}
```

### React Native Integration

```javascript
// Install dependencies
npm install react-native-jwt-io

// Verify JWT
import JwtDecode from 'jwt-decode';
import { RSA } from 'react-native-rsa-native';

async function verifyToken(token, publicKey) {
  try {
    // Decode token (doesn't verify signature)
    const decoded = JwtDecode(token);
    
    // Verify signature manually
    const isValid = await RSA.verify(
      decoded.signature,
      decoded.data,
      publicKey,
      RSA.SHA256
    );
    
    if (isValid) {
      return decoded;
    }
    throw new Error('Invalid signature');
  } catch (error) {
    console.error('Token verification failed:', error);
    return null;
  }
}
```

### Flutter/Dart Integration

```dart
// pubspec.yaml
dependencies:
  dart_jsonwebtoken: ^2.12.0

// Verify token
import 'package:dart_jsonwebtoken/dart_jsonwebtoken.dart';

void verifyToken(String token, String publicKeyPem) {
  try {
    final jwt = JWT.verify(
      token,
      RSAPublicKey(publicKeyPem),
      issuer: 'ScanPetMobileBackend',
      audience: Audience.array(['ScanPetMobileApp']),
    );
    
    print('Token valid: ${jwt.payload}');
  } catch (e) {
    print('Token invalid: $e');
  }
}
```

---

## ?? ENCRYPTION ALGORITHM DETAILS

### RS256 Algorithm Breakdown

**What is RS256?**
- **RS** = RSA (Rivest-Shamir-Adleman)
- **256** = SHA-256 hash function

**How it works:**

1. **Token Creation (Backend):**
```
1. Create JWT header + payload
2. Hash with SHA-256
3. Sign hash with PRIVATE key (RSA)
4. Attach signature to token
```

2. **Token Verification (Frontend):**
```
1. Split token into header.payload.signature
2. Hash header + payload with SHA-256
3. Verify signature using PUBLIC key (RSA)
4. If valid, trust the token
```

**Security Properties:**
- ? **Asymmetric:** Different keys for signing vs verifying
- ? **Non-repudiation:** Only backend can create valid tokens
- ? **Tamper-proof:** Any modification invalidates signature
- ? **Public verification:** Frontend can verify without secrets

---

## ?? WHAT YOUR SYSTEM DOES NOT USE

### Custom Bit Manipulation ? NOT USED

**Original Request:**
> "manipulate the fourth bit then encrypt"

**Why NOT Implemented:**
1. ? Custom crypto is dangerous (easy to implement incorrectly)
2. ? Hard to audit for security vulnerabilities
3. ? Not standardized (harder to integrate)
4. ? Adds complexity without real security benefit

**What We Use Instead:** ?
- **RS256** is industry-standard, audited by experts
- Widely supported across all platforms
- Automatic tamper detection
- Built-in key rotation support

---

## ?? TOKEN FLOW DIAGRAM

```
???????????????                           ???????????????
?   BACKEND   ?                           ?  FRONTEND   ?
?  (Private)  ?                           ?  (Public)   ?
???????????????                           ???????????????
       ?                                         ?
       ? 1. User Login                           ?
       ???????????????????????????????????????????
       ?                                         ?
       ? 2. Create JWT                           ?
       ?    - Hash payload with SHA-256          ?
       ?    - Sign with PRIVATE key              ?
       ?                                         ?
       ? 3. Return Access + Refresh Token        ?
       ???????????????????????????????????????????
       ?                                         ?
       ?                                         ? 4. Store tokens
       ?                                         ? 5. Verify signature
       ?                                         ?    with PUBLIC key
       ?                                         ?
       ? 6. API Request with Token               ?
       ???????????????????????????????????????????
       ?                                         ?
       ? 7. Verify token signature               ?
       ?    - Extract PUBLIC key                 ?
       ?    - Verify with RS256                  ?
       ?                                         ?
       ? 8. Return protected data                ?
       ???????????????????????????????????????????
```

---

## ?? VERIFICATION CHECKLIST

### Backend Setup ?
- [ ] Generated 2048-bit RSA key pair
- [ ] Private key stored securely (NOT in Git)
- [ ] Public key accessible to backend
- [ ] JwtService configured with private key
- [ ] Environment variables set for production
- [ ] .gitignore includes key files

### Frontend Setup ?
- [ ] Public key obtained from backend team
- [ ] JWT verification library installed
- [ ] Token verification implemented
- [ ] Signature validation working
- [ ] Error handling for invalid tokens
- [ ] Token expiry handled

### Security Checklist ?
- [ ] Private key NEVER exposed to frontend
- [ ] Private key NOT in version control
- [ ] Public key can be safely distributed
- [ ] Tokens expire appropriately (15 min)
- [ ] Refresh tokens rotate properly (7 days)
- [ ] HTTPS used in production

---

## ?? TESTING THE KEYS

### Test Token Generation (Backend)

```bash
# Using your API
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@scanpet.com","password":"Admin@123"}'

# Response includes token
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "eyJhbGc...",
  "expiresIn": 900
}
```

### Test Token Verification (Frontend)

```javascript
// Decode token to see payload (no verification)
import jwt_decode from 'jwt-decode';
const decoded = jwt_decode(token);
console.log(decoded);

// Output:
{
  "sub": "user-id-guid",
  "email": "admin@scanpet.com",
  "role": "Admin",
  "permissions": ["Users.Create", "Orders.Refund", ...],
  "iat": 1703001234,
  "exp": 1703002134,
  "iss": "ScanPetMobileBackend",
  "aud": "ScanPetMobileApp"
}
```

### Verify Signature Online

**URL:** https://jwt.io/

1. Paste your token
2. Paste PUBLIC key in verification section
3. Algorithm: RS256
4. Should show "Signature Verified" ?

---

## ?? KEY ROTATION

### When to Rotate Keys

**Rotate immediately if:**
- ? Private key compromised
- ? Security breach detected
- ? Employee with key access leaves

**Rotate periodically:**
- ?? Every 6-12 months (best practice)
- ?? During major version updates

### How to Rotate Keys

```bash
# 1. Generate new key pair
openssl genrsa -out private_new.pem 2048
openssl rsa -in private_new.pem -pubout -out public_new.pem

# 2. Keep old keys active during transition
# 3. Deploy new private key to backend
# 4. Distribute new public key to frontends
# 5. Set grace period (e.g., 7 days)
# 6. After grace period, remove old keys
```

---

## ?? ADDITIONAL RESOURCES

### Official Documentation
- **JWT.io:** https://jwt.io/introduction
- **RS256 Explanation:** https://auth0.com/blog/rs256-vs-hs256/
- **OpenSSL Docs:** https://www.openssl.org/docs/

### Libraries
- **JavaScript:** jsonwebtoken, jose
- **React Native:** react-native-jwt-io
- **Flutter:** dart_jsonwebtoken
- **.NET:** Microsoft.IdentityModel.Tokens

### Security Best Practices
- **OWASP JWT Guide:** https://cheatsheetseries.owasp.org/cheatsheets/JSON_Web_Token_for_Java_Cheat_Sheet.html
- **Key Management:** https://www.vaultproject.io/

---

## ?? SUMMARY

**What You Have:**
- ? **RS256** - Industry-standard, secure
- ? **2048-bit keys** - Strong encryption
- ? **Asymmetric** - Public verification possible
- ? **Standardized** - Works everywhere

**What You DON'T Have:**
- ? Custom bit manipulation
- ? Proprietary encryption
- ? Security through obscurity

**Why This is Better:**
- ? Audited by security experts
- ? Widely supported
- ? Easy to integrate
- ? More secure than custom solutions

---

**Status:** ? **COMPLETE GUIDE**  
**Algorithm:** RS256 (RSA + SHA256)  
**Key Size:** 2048 bits  
**Security:** ?? **Enterprise Grade**

---

**END OF KEY GENERATION GUIDE**

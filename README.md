# Passly – Password Manager Backend 

This project is a backend only password manager API built with .NET 9, focusing on secure authentication, strong cryptography, and a true zero knowledge vault model. All functionality is fully testable through Postman, including client side encryption workflows.

## Features

Robust Authentication: Argon2id password hashing, JWT access tokens, rotating refresh tokens, and email based OTP verification for secure account access.

Zero-Knowledge Vault: Backend stores only encrypted vault items; all sensitive fields are encrypted client side using AES-256 + PBKDF2.

## Postman Scripts

Because the backend never handles plaintext vault data, Postman acts like the frontend and performs the encryption/decryption.
Pre request scripts encrypt the vault fields before sending them, and post response script decrypt the returned ciphertext for viewing.

### 1. Set Postman environment values: masterPassword and salt

### 2. For POST/PUT vault (Pre-request script)

```javascript

let reqBody = pm.request.body.raw;
let bodyObj = JSON.parse(reqBody);

const name = bodyObj.name;
const category = bodyObj.category || null;
const url = bodyObj.url;

const sensitivePayload = {
    username: bodyObj.username,
    password: bodyObj.password
};

const masterPassword = pm.environment.get("masterPassword");
const salt = pm.environment.get("salt");

const key = CryptoJS.PBKDF2(masterPassword, salt, {
    keySize: 256 / 32,
    iterations: 100000
});

pm.environment.set("vaultKey", key.toString());

const encrypted = CryptoJS.AES.encrypt(
    JSON.stringify(sensitivePayload), 
    key.toString()
).toString();

pm.environment.set("lastCiphertext", encrypted);

const finalBody = {
    name: name,
    category: category,
    url: url,
    encryptedPayload: encrypted
};

pm.request.body.raw = JSON.stringify(finalBody);

### 3. For GET /vault (Post-response script)

```javascript

const CryptoJS = require('crypto-js');

const response = pm.response.json();

const masterPassword = pm.environment.get("masterPassword");
const salt = pm.environment.get("salt");

const key = CryptoJS.PBKDF2(masterPassword, salt, {
    keySize: 256 / 32,
    iterations: 100000
}).toString();

const decrypted = CryptoJS.AES.decrypt(response.encryptedPayload, key);
const decryptedString = decrypted.toString(CryptoJS.enc.Utf8);
const payload = JSON.parse(decryptedString);

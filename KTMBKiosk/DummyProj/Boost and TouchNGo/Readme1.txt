API -> https://gopayment-api-dev.azurewebsites.net/swagger/index.html
XXXXX
XXXXX

--1--/api/Merchant/list_payment_gateway
{
  "merchantId": "KTMB"
}


--2--/api/Sales/create_sale
"signature": "string", <encrypt..>
"terminalType": "Kiosk",
"notificationUrl": "string", <post>
	--> return QR code

	--> Base on "notificationUrl" (when user success scan bar code)
		Return ..
		----> 
			"status": "paying",
			"description": null,
			"merchantId": "GoTicketing",
			"salesTransactionNo": "GP-GT-3889738509207994368",
			"merchantTransactionNo": "a859b0a8-f897-471e-8bd6-12312343",
			"amount": 11.00,
			"customField": null,

			"signature": "rPqslQYzN/hfyiucrPJAi8cnLHqUzyRWLPzocQpJ7GCCJYI9U+7Di0Y7Zdx8MX6TTVEaVwlUNHEasmQcEW68ghoJBWtlk9n2dKmV+IMAsTOFs2Z6+UQ02NA2UaeP/0oOZGHj1wVRkUoMRVdDqN7lNU8E0kwTG2X8kTzwv/7v7aP9zsiv7j71cbk1WuOLw3BATlxeXoo36K2NwWyBOujXj0A0akdOaMLoIDhTcPfmVamlrYwz76jsYX/YjHq2Au0FRHXGtSPp1yTMoZKeJW2KivPh8NGu0V853JSlbkNFvithKnB52iB1nbBRPYgkmSn+kVc/uVrf6e4O/XWsdyOmww=="
				## encryption refer to Return data.
			

xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

// to encrypt for (/api/Sales/create_sale).signature
public static string HashRSAKey(string hashText, string privateKey)
        {
            // Create a new instance of RSACryptoServiceProvider using the
            // key from RSAParameters.
            RSACryptoServiceProvider RSAalg = PemKeyUtils.GetRSAProviderFromText(privateKey);

            var hashByte = Encoding.UTF8.GetBytes(hashText);
            // Hash and sign the data. Pass a new instance of SHA256
            // to specify the hashing algorithm.
            byte[] signedByteArray = RSAalg.SignData(hashByte, SHA256.Create());

            return Convert.ToBase64String(signedByteArray);
        }

// refer to response of (/api/Sales/create_sale)-> (Response).signature
public static bool VerifyRSASignedHash(string hashText, string signatureText, string publicKey)
        {
            // Create a new instance of RSACryptoServiceProvider using the
            // key from RSAParameters.
            RSACryptoServiceProvider RSAalg = Utility.PemKeyUtils.GetRSAProviderFromText(publicKey);

            var hashByte = Encoding.UTF8.GetBytes(hashText);
            var signatureByte = Convert.FromBase64String(signatureText);

            return RSAalg.VerifyData(hashByte, SHA256.Create(), signatureByte);
        }

sample .. sale
{
  "MerchantId": "GoTicketing",
  "PaymentGateway": "boost",
  "MerchantTransactionNo": "a859b0a8-f897-471e-8bd6-12312343",
  "Currency": "MYR",
  "Amount": 11.0,
  "Signature": "fFnvbJPeQfkHDadYG2BHOrleCwWEAKy76T3g70HZR3rYIuCPpH6SjQL6ehKxyAzQNmaWtmzJwO2NeFK8fjaPhTYaLsNv+h/CS3QkQF0EfdOi4RBy/n7/nZ1cdt1FIIgWRlvstoHuyq84PJsPxUUi62J/e3EwzRPv35RcS4KTCzvi0QkfxtxcQF8ITPdY7s+PbxqUSulQq55CcJNvQzqI781LU3hpvzVYzNSUxM/GaayOt8kKDJk1agCkB1bGsN+VQwjIWzqFbnq4OI9t4oyeVlzCwTuRVnOrRLQgkEi3zLWkH7z1JsR1sONKWIQLijapwdefplgNSbc+CCvlkDQaxQ==",
  "RedirectUrl": "https://gouser-api-demo.azurewebsites.net/api/GoTransact/PaymentSuccessCallBack?Redirect=https://arenabooking-user-dev.azurewebsites.net/payment/Redirect",
  "DeepLinkUrl": "https://www.google.com",
  "CancelUrl": null,
  "NotificationUrl": "http://gouser-api-demo.azurewebsites.net/api/GoTransact/PaymentSuccessCallBack",
  "CustomField": null,
  "Remark": null,
  "OrderTitle": "SportBooking",
  "DisplayName": null,
  "TerminalType": "MobileBrowser",
  "PayerInfo": {
    "FirstName": "123",
    "LastName": "123",
    "Email": "123@test.com",
    "ContactNo": "123",
    "Address": "123",
    "City": "123",
    "Country": "123"
  },
  "ExpirySecond": 180,
  "PG_FrontEndResponseUrl": null,
  "PG_BackendResponseUrl": null,
  "PG_CancelUrl": null,
  "PG_SalesPage": null,
  "IpAddress": null
}

-----BEGIN RSA PRIVATE KEY-----
MIIEowIBAAKCAQEAts382izmQVYo2iZyp+vMXxdmItahEFk0bqo2tBnPnJH4VWBx
KAmOrF4g7iSKYB3JpNl0BDUDDJmbB2T4EY08V9XBH5jvJQYht1kapAtPzUkpobQa
iitye21uDBwXtQengSCriS0AmOSt9+E4Sp1Cr7JIJMYTz6+TA6T8/rJsUPu5jDST
ecwB1wsVWM0dchC+pr99hXRyFSBdKFzYR6bqzk2x33CKemhTGmidQZSU9AT+JFEX
eOhu+nCEqPMH3AQBn87oxTMIVa+y6eQeH5ESUvvGtL6q/hsTLJ8mgQfnpudFYKkF
VC2B4qPKHp05/UmCQGmYu1BEC6plx2KfcsXeWwIDAQABAoIBACnNDIavQa+rDghk
Rk+TodYUuaw5u/bLDyxHC98/D7RTxJ9xQC7RkKhllc4e2O2Fojhp6ReVL69P6J1W
P0t0KlpDbLfW9shWkJsmauscExF0K2rojjEOIk2LBmEKg3lH2Mu0NsVVXw+XKxjF
jWOydi9K6yhNivYwxcNNlPSAxDf4FKVfnqXYtC6Wh/60lCuzHDaaFaQwQ1GTPQY0
6AGZCPXwn4z1tHwmY3GIY9JLcHYaYSLqqwvlrLnOfFyp/4JWK6qcivYgK+hhWfOv
tDir1u0K7JfComH0cwJthAoKknYMgNlIxuoUuYHWIokR25cLRXQhBYRO2mnZo7qu
yJOEwKECgYEA5ep79GKPfZMNXHj3/XeeigNZy8c9+XyAeH9+Wl2TGfVx3glO8wJm
m9eZmHIlntPC5AWtmxIoGYWedrtVnL2Xn+FELKCv6oUphqp7/nqez2GddenkuP+c
ptDp7rLeordwOe8PdEKHt3mmCEjCxmgJ4FV3Ovh0u281TyECsIqP+UMCgYEAy4s8
ZXaRhO1EgKbETPgZ2xic73DUhx6CMYG3ijSKMFJ+dm+UdvIqpxonp4cMyrW3ePlv
n680Ey9GCt4qhJDzXlk9DO6UgYiM907a9Fm/ZXgEICngxqDd7sk7jDNzXXSXJQmB
PNZGTk/9CIlrRD+vI6dB3/Et5Mbjwb5rFXC2SQkCgYAKXPldWJvzIw+1HVbAPAYP
XntLrh1jA3Oe+tAtLo6U2vVY9r5yQadyWtN3hZ2gfRcJxB/BH55jGBy+aU9Ak2Mk
N7kk8dE8Fuh6Q3D3VXuXCWVZjUNb+1mKQ1xn//P9DZunYNknemA3quoK8Yyl+MaJ
MBEBvXU1hZu3h1thrb0zlQKBgQCZgONGndn3Br1fzOVEKuPNAU3xogUV9eM4FNzn
hOImuUAYb+PmpJGYPjhjtozmH49D09Hj+szqHv/S2GP2YB66K9DH/PHQkrvFExo7
p6eZjZ6G5y5WfiGBoQ+gl3jMpU4Lp5Ro3ixdiSOKGaDk8qZR3CTpD8mNvJUtmz7F
B6DhiQKBgCciNAL2GTULA4vsj03Eply7vqg7v4x4HFp9achqLHg5h2byZRmid/OL
G5SWJirwJrsPmp1P8gV9ulSZMEjbHvAHlAZFx6Og08S89vfMZNEIDts43ZyA1pOK
j5tU4pwO2Xl0I0a29x7tk4LkfU3N6jgiRU2I5NBr4I6lvoVl0FtL
-----END RSA PRIVATE KEY-----
-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAzXDPep1R/Up8yRy3IiVw
PPJETvB52L0fk8ACVNO+8DPznDLnfkUrXAXKdPGBjMv4IwzKhG3WSwDmibcsq1mA
CLWnq1GovVM3OF98NK8HGA3o5jBIaPukmWd7KScTQvam/S9tUZbvmbX+62CPOdwE
ZnMxeLAi2cDzflSd3og4YLs6vnLZ/1aLY2fTW4uf8jwwJFmmzp/16lurfTGJU6AZ
wc3uD5MY0kaKKxXRKG69xLFz50+ElWGgHBY5flkSviPT3EEdDSIzDLQ5FbYtgUmq
ZmWKUE40IIrfVGMASSM4dFFAZVMxx3P5syBG6PomliLp0Ksfn3/gRzA8cOhIGHM+
iQIDAQAB
-----END PUBLIC KEY-----

xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
Doc
xxxxxxxxx
C:\dev\source code\KTMB_Kiosk\Code\DummyProj\Boost and TouchNGo\*.*
C:\Users\ChongYuenAng\Desktop\Rough\KTMB Boost and TouchNGo.txt


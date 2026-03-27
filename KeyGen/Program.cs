using System.Security.Cryptography;
using System.Text;

var rsa = RSA.Create(2048);
var privateXml = rsa.ToXmlString(true);
var publicXml = rsa.ToXmlString(false);
var privateB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(privateXml));
var publicB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(publicXml));



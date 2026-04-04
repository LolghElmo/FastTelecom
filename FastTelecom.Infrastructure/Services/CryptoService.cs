using FastTelecom.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace FastTelecom.Infrastructure.Services
{
    public sealed class CryptoService : ICryptoService
    {
        private readonly RSA _rsa;

        private const string DecoyUser = "Afunction toString() { [native code] }";
        private const string DecoyPass = "Bfunction toString() { [native code] }";

        private static readonly string _keyBase64 = string.Concat(
            "MIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQCnGG4RTRgGyorgwVHP084JgGN2",
            "z/6LhbpJlity51FAEuIuG41k4P0xrO6o2bAouwXP2fr6S2PBkRkRWJ5CYSiIkvbVOdbF72XehYAo",
            "BlBCBv82MctHqE3ddOBq5tEtha/vYfNsvJZJWWMJsyjz/uygPWpuRJbX1zaSoUSBAm+7BWt6LMzw",
            "H79rTlYuAtHUZC740fuMYd+q+RV7ib72aopax7PpN1Ft/3FpNWyukESSmaqXJweJ8IZwp/ivSDim",
            "SoThY49HvXnyOTFEol8GvRzS/xKoG355ZdiOR7NXQ1UhFEltJTuKHLq26fLxujf2S2bR87uHxp5C",
            "Uhp7puNrzLhNAgMBAAECggEAbG5N/qwoiz+kH3VTwamQaloGMXOHmsKMwHPSfh3de9bFL2ZxuqTF",
            "qRavSKL6zXOPsfGiDAogEdw2iCsZh7nEs9uqkXOXC5ruYBgBsfdm3XHs6x4k67srzCmr97MQypm",
            "WMaE+dbFrVO3Mdt7sFGm448L27ddUi3v8zeoYqh8KojE1YaaqmgbusySSzcvKrrE/h8HDujjK3cK",
            "yQa7vkl1CxO+R3P6siEwBwOul/Atk7RA2TDuvx751K/CKN+vx8t5IdUujPdQDBxrT6/C/EgCQcZ",
            "FayhLUuMzKPC1MIDFcc1h5qy73MM3EDLaBgj5p7MOwkMAN2T5DmVYhmvk222LIAQKBgQDZ8r3Gmm",
            "fo+GnL/aeNf95YMzJqqea4FCrNZH+MwUSjW8GduTzNsruLFlZTihCZYEmR5qmMZi3vfGEjMQiRl7",
            "jYKLatE60H+0pCX2EYh8Bd2U+TYw6TjmYuey00+PQNVKhYR357vAPB3MAZOkgBDGqMA4RG7ijJKo",
            "jMmcrADPB+8QKBgQDERM8SRgKGKM7uDR1JXyRI2UNr7LYRTSQYrXk8y1GxoCQxFFVgcGDs/OEs78",
            "6jusHrDvYWwsiUYzHln3CFKXBg5sjb+3weM5+Z6B3EElkxF+rAZ6Qi5vHG6iAHoSqCVBjXgXtk4x",
            "Be+iwTjExi9SlMVSxuF1QTXJdxzOUVkfHHHQKBgH5P3oihqCMvBTHCWj75ooT/dvK2cQ6yMXREEG",
            "AlCoCahwW/+2tDcMnVMkbMN36MfVbfldfWyDyJm0pn+o1Wnzw3rFd2lcuQaaM53+31jxlU/ndu61",
            "29I59AqByRQ/AN5lrzZGyVtJ/ALlzwmBZzebSXvSvWjzC3Q/1ADG9tkFwhAoGAJ+ZBHlrjKnjjUF",
            "uUJ5VS4Ahi/264uJ2xB99ENUq0CeMfWGbk0F0oJyVldWgu3vQZdfqtpoTkl93uh9q42ilJcjmYfb",
            "gLTGx8NqKMYo7EWQmerIylPn8qiaCQ8FwgMyx7fFwTRLgwXM6I5VRxNvDV+3GZPaw6aFE7bGQV8i",
            "OgPjUCgYAY8CBX5xwmIkoHuwwXJuGhWbAdoA0OZV0HW2wS0OdmMVbbXOY3WK00hym8hW6aRmxmQX",
            "AVvbdCPtsphkWSs35WdT8ujPQCOj/RHWNJm9fIH5BQedZpqLz7kBPcYdmihyHR5uTXBJclqLuNRV",
            "TZ55a1ZpXzmlrt2GC+pPplwktgVQ==");

        public CryptoService()
        {
            _rsa = LoadPublicKeyFromPkcs8(_keyBase64);
        }
        public string Encrypt(string plaintext)
        {
            var encrypted = _rsa.Encrypt(
                Encoding.UTF8.GetBytes(plaintext),
                RSAEncryptionPadding.Pkcs1);

            var base64 = Convert.ToBase64String(encrypted);

            var sb = new StringBuilder(base64.Length * 2);
            foreach (var b in Encoding.UTF8.GetBytes(base64))
                sb.Append(b.ToString("x"));

            return sb.ToString();
        }

        public string UserNameHash => Md5(DecoyUser);
        public string UserPswdHash => Md5(DecoyPass);
        private static string Md5(string input)
        {
            var hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        private static RSA LoadPublicKeyFromPkcs8(string base64)
        {
            var der = Convert.FromBase64String(base64);
            var r = new DerReader(der);

            r.ExpectTag(0x30); r.ReadLength();
            r.ExpectTag(0x02); var vLen = r.ReadLength(); r.Skip(vLen);
            r.ExpectTag(0x30); var algLen = r.ReadLength(); r.Skip(algLen);
            r.ExpectTag(0x04); r.ReadLength();
            r.ExpectTag(0x30); r.ReadLength();
            r.ExpectTag(0x02); vLen = r.ReadLength(); r.Skip(vLen);

            var modulus = r.ReadIntegerBytes();
            var exponent = r.ReadIntegerBytes();

            var rsa = RSA.Create();
            rsa.ImportParameters(new RSAParameters
            {
                Modulus = modulus,
                Exponent = exponent,
            });
            return rsa;
        }
        private sealed class DerReader
        {
            private readonly byte[] _data;
            private int _pos;

            public DerReader(byte[] data) => _data = data;

            public void ExpectTag(byte tag)
            {
                if (_data[_pos++] != tag)
                    throw new InvalidOperationException(
                        $"Expected DER tag 0x{tag:X2} at position {_pos - 1}");
            }

            public int ReadLength()
            {
                var b = _data[_pos++];
                if (b < 0x80) return b;
                var n = b & 0x7F;
                var len = 0;
                for (var i = 0; i < n; i++)
                    len = (len << 8) | _data[_pos++];
                return len;
            }

            public void Skip(int count) => _pos += count;

            public byte[] ReadIntegerBytes()
            {
                ExpectTag(0x02);
                var len = ReadLength();
                var bytes = _data[_pos..(_pos + len)];
                _pos += len;
                if (bytes.Length > 1 && bytes[0] == 0x00)
                    bytes = bytes[1..];
                return bytes;
            }
        }
    }
}

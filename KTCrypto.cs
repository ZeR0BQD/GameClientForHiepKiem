using System.Security.Cryptography;
using System.Text;

namespace GameClient
{
    /// <summary>
    /// Lớp cung cấp các hàm mã hóa và giải mã
    /// </summary>
    public static class KTCrypto
    {
        private const string PublicKey = "<RSAKeyValue><Modulus>mRYnNwn2e4cbs6W0C4dfE/oNOY+j1Pmx/ufe8PGFJmHpUW0rs1y/OfSGmUk5hLjF298wyVNCkoMkG2nOvwAcOeo8QwBv5PVeRF8YyunwteBZOk/Vo8YgNM+YyLftfEFk40QeLczmMy5FTTQrEsbJyboOpEQPDx/eRNF2aLr7CfE=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        private const string PrivateKey = "<RSAKeyValue><Modulus>mRYnNwn2e4cbs6W0C4dfE/oNOY+j1Pmx/ufe8PGFJmHpUW0rs1y/OfSGmUk5hLjF298wyVNCkoMkG2nOvwAcOeo8QwBv5PVeRF8YyunwteBZOk/Vo8YgNM+YyLftfEFk40QeLczmMy5FTTQrEsbJyboOpEQPDx/eRNF2aLr7CfE=</Modulus><Exponent>AQAB</Exponent><P>y4QsV6ol8KkquDPcHgy4U6H0Z0ACAOUZAivrpzAsrvYgg4DE0+lTMUMR/XZkkqMcr7397utuiGzyMyZe1tVfww==</P><Q>wJCvnTq0Mdsr/3V0/YfViMhf7snIjrehWxCIIfETtifXXlETHP9U+8P1UbgvUQxX7uF7ccdu0X19CqeFodKoOw==</Q><DP>mzi/HUm/0DMmSwH608x91gPDRfCy1n3luhtHi+eZXQSKPeI7vSjLc9ok4X2oLZNMsNmm0NAuKM13WP3d/dsWQw==</DP><DQ>hKLkI4Ns3K5fVt07kOn//fAui9Z3Cz6WqJfxfJeGAUDeCnwDk0SX77ZhAkHAba3333V2Rr+cqDUsbKtI01a7Qw==</DQ><InverseQ>Sm+97LezjF7EWRatRl1QBK7oiHHXLXiLCmpBqDiXqTpZrRzolbaV8pu80X/ccyWPCyz65CfNQli60h1tsO7q6w==</InverseQ><D>ASPRFu/UDgdrhWLufEd9xcBO6ObQ6X0SfjtrxY+G1kpUWm7drHA8XEod1nZdH0fg8UowKs+b50tisXGQQIvXqfPJh1LmriXzyXk9s0J+HS4qUTwS8IMfZ4ICff9pNejl/kq1Fs6UjLxtokeaObenMosJwOAAh75ZmnWHA7UauSE=</D></RSAKeyValue>";
        /// <summary>
        /// <para>Generate Public And Private KeyPair</para>
        /// <para>【argument1】keySize</para>
        /// <para>【return】Public key and private key KeyValuePair</para>
        /// </summary>
        public static KeyValuePair<string, string> GenrateKeyPair(int keySize = 1024)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(keySize);
            string publicKey = rsa.ToXmlString(false);
            string privateKey = rsa.ToXmlString(true);
            return new KeyValuePair<string, string>(publicKey, privateKey);
        }

        /// <summary>
        /// <para>Standard Rijndael(AES) encrypt</para>
        /// <para>【argument1】plain text</para>
        /// <para>【argument2】password</para>
        /// <para>【return】Encrypted and converted to Base64 string</para>
        /// </summary>
        public static byte[] Encrypt(string plain)
        {
            byte[] encrypted = Encrypt(Encoding.UTF8.GetBytes(plain), PublicKey);
            return Encoding.UTF8.GetBytes(Convert.ToBase64String(encrypted));
        }

        /// <summary>
        /// <para>Standard Rijndael(AES) encrypt</para>
        /// <para>【argument1】plain binary</para>
        /// <para>【argument2】password</para>
        /// <para>【return】Encrypted binary</para>
        /// </summary>
        public static byte[] Encrypt(byte[] src, string publicKey)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(publicKey);
                byte[] encrypted = rsa.Encrypt(src, false);
                return encrypted;
            }
        }

        /// <summary>
        /// <para>Standard Rijndael(AES) decrypt</para>
        /// <para>【argument1】encrypted string</para>
        /// <para>【argument2】password</para>
        /// <para>【return】Decrypted string</para>
        /// </summary>
        public static string Decrypt(byte[] base64Encrtpted)
        {
            string input = Encoding.UTF8.GetString(base64Encrtpted);
            return Decrypt(input, PrivateKey);
        }
        /// <summary>
        /// <para>Standard Rijndael(AES) decrypt</para>
        /// <para>【argument1】encrypted string</para>
        /// <para>【argument2】password</para>
        /// <para>【return】Decrypted string</para>
        /// </summary>
        public static string Decrypt(string encrtpted, string privateKey)
        {
            byte[] decripted = Decrypt(Convert.FromBase64String(encrtpted), privateKey);
            return Encoding.UTF8.GetString(decripted);
        }

        /// <summary>
        /// <para>Standard Rijndael(AES) decrypt</para>
        /// <para>【argument1】encrypted binary</para>
        /// <para>【argument2】password</para>
        /// <para>【return】Decrypted binary</para>
        /// </summary>
        public static byte[] Decrypt(byte[] src, string privateKey)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);
                byte[] decrypted = rsa.Decrypt(src, false);
                return decrypted;
            }
        }
    }
}

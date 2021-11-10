using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace sw.descargamasiva
{
    public class Encriptar
    {
        public string dato;
        public string llave;

        public Encriptar()
        {
            //dato = Dato;
            //llave = Llave;
        }

        public static TripleDES GetInstance(string llave)
        {
            TripleDES objProvider = new TripleDESCryptoServiceProvider();

            // Inicializa el proveedor
            objProvider.Key = Encoding.Unicode.GetBytes(llave);
            objProvider.IV = new byte[objProvider.BlockSize / 8];
            // Devuelve el proveedor
            return objProvider;
        }
        public static byte[] Encrypt(string dato, string llave)
        {
            TripleDES objProvider = GetInstance(llave);
            ICryptoTransform objCrypto = objProvider.CreateEncryptor();
            byte[] arrBytBuffer = Encoding.Unicode.GetBytes(dato);

            // Devuelve el arry de bytes encriptado
            return objCrypto.TransformFinalBlock(arrBytBuffer, 0, arrBytBuffer.Length);
        }

        public  string EncryptToBase64(string dato, string llave)
        {
            return Convert.ToBase64String(Encrypt(dato, llave));
        }

        public  string Decrypt(string dato, string llave)
        {
            byte[] arrBytBuffer = Convert.FromBase64String(dato);
            TripleDES objProvider = GetInstance(llave);
            ICryptoTransform objCrypto = objProvider.CreateDecryptor();

            // Devuelve la cadena desencriptada
            return Encoding.Unicode.GetString(objCrypto.TransformFinalBlock(arrBytBuffer, 0, arrBytBuffer.Length));
        }

    }
}

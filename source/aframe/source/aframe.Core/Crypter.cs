using System;
using System.Security.Cryptography;
using System.Text;

namespace aframe
{
    public class Crypter
    {
        /// <summary>
        /// 初期Salt
        /// </summary>
        private static readonly string DefaultSalt = "L+DCZF4pmD8SVAv.$m5sYv/3$,x$wXAW";

        /// <summary>
        /// 初期パスワード
        /// </summary>
        private static readonly string DefaultPassword = "ym%#EPcY5g(WN4CRTE|mv5c/rvqVXA7m";

        /// <summary>
        /// 文字列を暗号化する
        /// </summary>
        /// <param name="sourceString">暗号化する文字列</param>
        /// <param name="password">暗号化に使用するパスワード</param>
        /// <returns>暗号化された文字列</returns>
        public static string EncryptString(
            string sourceString,
            string password = null)
        {
            if (string.IsNullOrEmpty(password))
            {
                password = DefaultPassword;
            }

            var encryptedString = string.Empty;

            using (var rijndael = new RijndaelManaged())
            {
                byte[] key, iv;
                Crypter.GenerateKeyFromPassword(password, rijndael.KeySize, out key, rijndael.BlockSize, out iv);
                rijndael.Key = key;
                rijndael.IV = iv;

                var strBytes = Encoding.UTF8.GetBytes(sourceString);

                using (var encryptor = rijndael.CreateEncryptor())
                {
                    var encBytes = encryptor.TransformFinalBlock(strBytes, 0, strBytes.Length);
                    encryptedString = Convert.ToBase64String(encBytes);
                }
            }

            return encryptedString;
        }

        /// <summary>
        /// 暗号化された文字列を復号化する
        /// </summary>
        /// <param name="sourceString">暗号化された文字列</param>
        /// <param name="password">暗号化に使用したパスワード</param>
        /// <returns>復号化された文字列</returns>
        public static string DecryptString(
            string sourceString,
            string password = null)
        {
            if (string.IsNullOrEmpty(password))
            {
                password = DefaultPassword;
            }

            var decryptedString = string.Empty;

            using (var rijndael = new RijndaelManaged())
            {
                byte[] key, iv;
                Crypter.GenerateKeyFromPassword(password, rijndael.KeySize, out key, rijndael.BlockSize, out iv);
                rijndael.Key = key;
                rijndael.IV = iv;

                var strBytes = Convert.FromBase64String(sourceString);

                using (var decryptor = rijndael.CreateDecryptor())
                {
                    var decBytes = decryptor.TransformFinalBlock(strBytes, 0, strBytes.Length);
                    decryptedString = Encoding.UTF8.GetString(decBytes);
                }
            }

            return decryptedString;
        }

        /// <summary>
        /// パスワードから共有キーと初期化ベクタを生成する
        /// </summary>
        /// <param name="password">基になるパスワード</param>
        /// <param name="keySize">共有キーのサイズ（ビット）</param>
        /// <param name="key">作成された共有キー</param>
        /// <param name="blockSize">初期化ベクタのサイズ（ビット）</param>
        /// <param name="iv">作成された初期化ベクタ</param>
        private static void GenerateKeyFromPassword(
            string password,
            int keySize,
            out byte[] key,
            int blockSize,
            out byte[] iv)
        {
            // パスワードから共有キーと初期化ベクタを作成する
            // saltを決める
            var salt = Encoding.UTF8.GetBytes(DefaultSalt);

            // Rfc2898DeriveBytesオブジェクトを作成する
            var deriveBytes = new Rfc2898DeriveBytes(password, salt);

            // 反復処理回数を指定する
            deriveBytes.IterationCount = 1000;

            // 共有キーと初期化ベクタを生成する
            key = deriveBytes.GetBytes(keySize / 8);
            iv = deriveBytes.GetBytes(blockSize / 8);
        }
    }
}

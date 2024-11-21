using System.Security.Cryptography;
using System.Text;

namespace UserService.API.Services
{
    public class HasherPassword
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public static string HashPassword(string password)
        {
            byte[] saltBytes = new byte[SaltSize];
            using (var rnd = RandomNumberGenerator.Create())
            {
                rnd.GetBytes(saltBytes);
            }
            using (var sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] passwordWithSaltBytes = new byte[passwordBytes.Length + saltBytes.Length];

                Buffer.BlockCopy(saltBytes, 0, passwordWithSaltBytes, 0, saltBytes.Length);
                Buffer.BlockCopy(passwordBytes, 0, passwordWithSaltBytes, saltBytes.Length, passwordBytes.Length);

                byte[] hashBytes = sha256.ComputeHash(passwordWithSaltBytes);

                byte[] hashWithSaltBytes = new byte[saltBytes.Length + hashBytes.Length];

                Buffer.BlockCopy(saltBytes, 0, hashWithSaltBytes, 0, saltBytes.Length);
                Buffer.BlockCopy(hashBytes, 0, hashWithSaltBytes, saltBytes.Length, hashBytes.Length);

                return Convert.ToBase64String(hashWithSaltBytes);
            }
        }
        public static bool VerifyPassword(string enteredPassword, string PasswordHash)
        {
            // Декодируем строку в байты
            byte[] hashWithSaltBytes = Convert.FromBase64String(PasswordHash);

            // Извлекаем соль из сохраненного хэша
            byte[] saltBytes = new byte[SaltSize];
            Buffer.BlockCopy(hashWithSaltBytes, 0, saltBytes, 0, SaltSize);

            // Хэшируем введенный пароль с извлеченной солью
            using (var sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(enteredPassword);
                byte[] passwordWithSaltBytes = new byte[passwordBytes.Length + saltBytes.Length];

                Buffer.BlockCopy(saltBytes, 0, passwordWithSaltBytes, 0, saltBytes.Length);
                Buffer.BlockCopy(passwordBytes, 0, passwordWithSaltBytes, saltBytes.Length, passwordBytes.Length);

                // Получаем хэш введенного пароля
                byte[] hashBytes = sha256.ComputeHash(passwordWithSaltBytes);

                // Извлекаем хэш из сохраненного значения
                byte[] storedHashBytes = new byte[HashSize];
                Buffer.BlockCopy(hashWithSaltBytes, SaltSize, storedHashBytes, 0, HashSize);

                // Сравниваем длину и значения хэшей
                return hashBytes.Length == storedHashBytes.Length && hashBytes.SequenceEqual(storedHashBytes);
            }
        }
    }
}

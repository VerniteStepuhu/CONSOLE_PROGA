using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ScytaleConsoleClient
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private static string? token;
        private static string baseUrl = "http://localhost:5000";

        static async Task Main(string[] args)
        {
            // Настройка кодировки для Windows
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;

            // Установка кодовой страницы для консоли Windows
            try
            {
                Console.WriteLine("Настройка кодировки...");
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = "/c chcp 65001",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка настройки кодировки: {ex.Message}");
            }

            while (true)
            {
                if (token == null)
                {
                    ShowAuthMenu();
                }
                else
                {
                    ShowMainMenu();
                }

                var choice = Console.ReadLine();
                await ProcessChoice(choice);
            }
        }

        static void ShowAuthMenu()
        {
            Console.Clear();
            Console.WriteLine("=== Шифр Скитала - Консольный клиент ===");
            Console.WriteLine("1. Войти");
            Console.WriteLine("2. Зарегистрироваться");
            Console.WriteLine("0. Выход");
            Console.Write("\nВыберите действие: ");
        }

        static void ShowMainMenu()
        {
            Console.Clear();
            Console.WriteLine("=== Главное меню ===");
            Console.WriteLine("1. Зашифровать текст");
            Console.WriteLine("2. Расшифровать текст");
            Console.WriteLine("3. История операций");
            Console.WriteLine("4. Очистить историю");
            Console.WriteLine("5. Выйти из аккаунта");
            Console.WriteLine("0. Выход");
            Console.Write("\nВыберите действие: ");
        }

        static async Task ProcessChoice(string? choice)
        {
            try
            {
                if (token == null)
                {
                    switch (choice)
                    {
                        case "1":
                            await Login();
                            break;
                        case "2":
                            await Register();
                            break;
                        case "0":
                            Environment.Exit(0);
                            break;
                    }
                }
                else
                {
                    switch (choice)
                    {
                        case "1":
                            await EncryptText();
                            break;
                        case "2":
                            await DecryptText();
                            break;
                        case "3":
                            await ShowHistory();
                            break;
                        case "4":
                            await ClearHistory();
                            break;
                        case "5":
                            token = null;
                            break;
                        case "0":
                            Environment.Exit(0);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nОшибка: {ex.Message}");
                Console.WriteLine("Нажмите любую клавишу для продолжения...");
                Console.ReadKey();
            }
        }

        static async Task Login()
        {
            Console.Clear();
            Console.WriteLine("=== Вход в систему ===");
            Console.Write("Имя пользователя: ");
            var username = Console.ReadLine();
            Console.Write("Пароль: ");
            var password = Console.ReadLine();

            var loginData = new { username, password };
            var response = await client.PostAsJsonAsync($"{baseUrl}/login", loginData);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                token = result?.token;
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                Console.WriteLine("\nУспешный вход в систему!");
            }
            else
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                Console.WriteLine($"\nОшибка: {error?.error}");
            }
            
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        static async Task Register()
        {
            Console.Clear();
            Console.WriteLine("=== Регистрация ===");
            Console.Write("Имя пользователя: ");
            var username = Console.ReadLine();
            Console.Write("Email: ");
            var email = Console.ReadLine();
            Console.Write("Пароль: ");
            var password = Console.ReadLine();

            var registerData = new { username, email, password };
            var response = await client.PostAsJsonAsync($"{baseUrl}/register", registerData);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                token = result?.token;
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                Console.WriteLine("\nРегистрация успешна!");
            }
            else
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                Console.WriteLine($"\nОшибка: {error?.error}");
            }
            
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        static async Task EncryptText()
        {
            Console.Clear();
            Console.WriteLine("=== Шифрование текста ===");
            Console.Write("Введите текст: ");
            var text = Console.ReadLine();
            Console.Write("Введите ключ (число): ");
            if (int.TryParse(Console.ReadLine(), out int key))
            {
                var request = new { text, key };
                var response = await client.PostAsJsonAsync($"{baseUrl}/encrypt", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<EncryptResponse>();
                    Console.WriteLine($"\nЗашифрованный текст: {result?.encryptedText}");
                }
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    Console.WriteLine($"\nОшибка: {error?.error}");
                }
            }
            else
            {
                Console.WriteLine("\nНеверный формат ключа!");
            }
            
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        static async Task DecryptText()
        {
            Console.Clear();
            Console.WriteLine("=== Расшифрование текста ===");
            Console.Write("Введите зашифрованный текст: ");
            var encryptedText = Console.ReadLine();
            Console.Write("Введите ключ (число): ");
            if (int.TryParse(Console.ReadLine(), out int key))
            {
                var request = new { encryptedText, key };
                var response = await client.PostAsJsonAsync($"{baseUrl}/decrypt", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<DecryptResponse>();
                    Console.WriteLine($"\nРасшифрованный текст: {result?.decryptedText}");
                }
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    Console.WriteLine($"\nОшибка: {error?.error}");
                }
            }
            else
            {
                Console.WriteLine("\nНеверный формат ключа!");
            }
            
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        static async Task ShowHistory()
        {
            Console.Clear();
            Console.WriteLine("=== История операций ===");
            
            var response = await client.GetAsync($"{baseUrl}/api/history");
            if (response.IsSuccessStatusCode)
            {
                var history = await response.Content.ReadFromJsonAsync<List<HistoryItem>>();
                if (history != null && history.Any())
                {
                    foreach (var item in history)
                    {
                        Console.WriteLine($"\nОперация: {item.RequestType}");
                        Console.WriteLine($"Входной текст: {item.InputText}");
                        Console.WriteLine($"Выходной текст: {item.OutputText}");
                        Console.WriteLine($"Ключ: {item.Key}");
                        Console.WriteLine($"Время: {item.Timestamp}");
                        Console.WriteLine(new string('-', 50));
                    }
                }
                else
                {
                    Console.WriteLine("\nИстория пуста");
                }
            }
            else
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                Console.WriteLine($"\nОшибка: {error?.error}");
            }
            
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        static async Task ClearHistory()
        {
            Console.Clear();
            Console.WriteLine("=== Очистка истории ===");
            
            var response = await client.DeleteAsync($"{baseUrl}/api/history");
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("\nИстория успешно очищена");
            }
            else
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                Console.WriteLine($"\nОшибка: {error?.error}");
            }
            
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }
    }

    // Классы для десериализации ответов
    class LoginResponse
    {
        public string? token { get; set; }
        public string? username { get; set; }
    }

    class ErrorResponse
    {
        public string? error { get; set; }
    }

    class EncryptResponse
    {
        public string? encryptedText { get; set; }
    }

    class DecryptResponse
    {
        public string? decryptedText { get; set; }
    }

    class HistoryItem
    {
        public string? RequestType { get; set; }
        public string? InputText { get; set; }
        public string? OutputText { get; set; }
        public int Key { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
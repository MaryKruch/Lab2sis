using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lab2sis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string RandomNumbersFile = "random_numbers.txt";
        private static readonly string PrimeNumbersFile = "prime_numbers.txt";
        private static readonly string SpecialPrimeNumbersFile = "special_prime_numbers.txt";

        private static readonly Mutex Mutex = new Mutex();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartProcessButton_Click(object sender, RoutedEventArgs e)
        {
            Thread thread1 = new Thread(GenerateRandomNumbers);
            Thread thread2 = new Thread(FilterPrimeNumbers);
            Thread thread3 = new Thread(FilterSpecialPrimeNumbers);

            thread1.Start();
            thread1.Join(); // Ожидаем завершения первого потока

            thread2.Start();
            thread2.Join(); // Ожидаем завершения второго потока

            thread3.Start();
            thread3.Join(); // Ожидаем завершения третьего потока

            ResultTextBlock.Text = "Процесс завершен. Результаты сохранены в файлах.";
        }

        private void GenerateRandomNumbers()
        {
            Mutex.WaitOne();
            try
            {
                Random random = new Random();
                List<int> numbers = Enumerable.Range(1, 100).Select(_ => random.Next(1, 1000)).ToList();
                File.WriteAllLines(RandomNumbersFile, numbers.Select(n => n.ToString()));
            }
            finally
            {
                Mutex.ReleaseMutex();
            }
        }

        private void FilterPrimeNumbers()
        {
            Mutex.WaitOne();
            try
            {
                List<int> numbers = File.ReadAllLines(RandomNumbersFile).Select(int.Parse).ToList();
                List<int> primeNumbers = numbers.Where(IsPrime).ToList();
                File.WriteAllLines(PrimeNumbersFile, primeNumbers.Select(n => n.ToString()));
            }
            finally
            {
                Mutex.ReleaseMutex();
            }
        }

        private void FilterSpecialPrimeNumbers()
        {
            Mutex.WaitOne();
            try
            {
                List<int> primeNumbers = File.ReadAllLines(PrimeNumbersFile).Select(int.Parse).ToList();
                List<int> specialPrimeNumbers = primeNumbers.Where(n => n % 10 == 7).ToList();
                File.WriteAllLines(SpecialPrimeNumbersFile, specialPrimeNumbers.Select(n => n.ToString()));
            }
            finally
            {
                Mutex.ReleaseMutex();
            }
        }

        private bool IsPrime(int number)
        {
            if (number <= 1) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;
            for (int i = 3; i <= Math.Sqrt(number); i += 2)
            {
                if (number % i == 0) return false;
            }
            return true;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HwPassportDocReceiver;



namespace HwPassport
{
    class Program
    {
        static bool canExit = false;
        static HwPassportDocReceiver.PassportDocumentsReceiver receiver;
        static PassportDocumentsSender sender;
        static int sendingDocumentsIntervalMs = 1500;
        static int waitingInterval = 2500;
        static string targetDirectoryName = "PassportDocuments";

        static void Main(string[] args)
        {
            Console.WriteLine("Starting receiving documents for passport");

            string targetDirectory = Path.Combine(Environment.CurrentDirectory, targetDirectoryName);
            List<string> documentFilenames = new List<string> { "Паспорт.jpg", "Заявление.txt", "Фото.jpg" };
            

            Directory.CreateDirectory(targetDirectory);

            receiver = PassportDocumentsReceiver.GetMyInstance(documentFilenames);
            
            receiver.DocumentsReady += () => 
            { 
                sender.Stop();
                receiver.Stop();
                ConsoleWriter.WriteWithColor("Все документы получены успешно. Прием документов заврешен.", ConsoleColor.Green);
                canExit = true;
            };
            
            receiver.TimedOut += () => 
            { 
                sender.Stop();
                receiver.Stop();
                ConsoleWriter.WriteRed("Превышено время ожидания");
                canExit = true;
            };
            
            sender =PassportDocumentsSender.GetMyInstance(sendingDocumentsIntervalMs, targetDirectory, documentFilenames);
            receiver.Start(waitingInterval, targetDirectory);
            sender.Start();

            while (!canExit)
            {
                Thread.Sleep(500);
            }
            
            Console.WriteLine("Нажмите Enter для завершения работы программы");

            Console.ReadLine();
        }
    }
}

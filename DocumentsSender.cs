using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using HwPassportDocReceiver;

namespace HwPassport
{
   public class PassportDocumentsSender
    {
        //класс, который отправляет документы в указанную папку

        private int _waitingInterval;
        private string _targetDirectory = "";
        private List<string> _documentFilenames = null;
        private ConsoleColor myDefaultColor = ConsoleColor.Cyan;
        private bool imWorking;

        private PassportDocumentsSender(int waitingInterval, string targetDirectory, List<string> documentFilenames)
        {
            _documentFilenames = documentFilenames;
            _targetDirectory = targetDirectory;
            _waitingInterval = waitingInterval;
        }

        public static PassportDocumentsSender GetMyInstance (int waitingInterval, string targetDirectory, List<string> documentFilenames)
        {
            PassportDocumentsSender rez = new PassportDocumentsSender(waitingInterval, targetDirectory, documentFilenames);
            return rez;
        }

        public void Start()
        {
            imWorking = true;
            foreach (string filename in _documentFilenames)
            {
                //очистить каталог
                string _filename =  Path.Combine(_targetDirectory, filename);
                if (File.Exists(_filename)) File.Delete(_filename);
            }

            ConsoleWriter.WriteWithColor("Sender started", myDefaultColor);

            foreach (string filename in _documentFilenames)
            {
                //создать этот файл в папке

                    Thread.Sleep(_waitingInterval);

                    if (!imWorking) return;
                    ConsoleWriter.WriteWithColor($"Sender sending document {filename}", myDefaultColor);
                    createFile(Path.Combine(_targetDirectory, filename));
            }

            ConsoleWriter.WriteWithColor("Finished sending documnets", myDefaultColor);
        }

        private void createFile(string path)
        {
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine("This is file");
            }

        }

        public void Stop()
        {
            if (!imWorking) return; //stop может вызываться еще и извне
            imWorking = false;
           // ConsoleWriter.WriteWithColor("Stopping sender", myDefaultColor);
        }


    }

}

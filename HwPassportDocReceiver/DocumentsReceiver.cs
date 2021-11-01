using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace HwPassportDocReceiver
{
   public class PassportDocumentsReceiver
    {
        //класс, который принимает документы
        
        public event Action DocumentsReady;
        public event Action TimedOut;
        private ConsoleColor myDefaultColor = ConsoleColor.Yellow;

        private int _waitingInterval = 10000;
        private string _targetDirectory = "";
        private List<string> _documentFilenames = null;
        private List<string> _documentFilenamesPresent = new List<string>();

        private Timer _timer;
        private FileSystemWatcher _watcher;
        private bool _documentsReady;
        private bool imWorking;

        private static readonly object _locker = new object();
        private PassportDocumentsReceiver(List<string> documentFilenames)
        {
            _documentFilenames = documentFilenames;
        }

        public void Start(int waitingInterval, string targetDirectory)
        {
            imWorking = true;
            _targetDirectory = targetDirectory;
            _waitingInterval = waitingInterval;
            ConsoleWriter.WriteWithColor("Receiver started", myDefaultColor);
            _watcher = new FileSystemWatcher(_targetDirectory) { EnableRaisingEvents = true };
            _watcher.Changed += Watcher_Changed;

            _timer = new Timer(_waitingInterval);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Enabled = true;
            _timer.Start();

        }
        public void Stop()
        {
            if (!imWorking) return; //stop может вызываться еще и извне
            imWorking = false;
           // ConsoleWriter.WriteWithColor("Stopping receiver", myDefaultColor);
            TurnOffTWatcher();
            TurnOffTimer();
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            lock (_locker)
            {
                if (!imWorking) return;

                var files = Directory.EnumerateFiles(_targetDirectory).ToList();

                string newDocumentName = files.Except(_documentFilenamesPresent).ToList().FirstOrDefault();

                _documentFilenamesPresent.Add(newDocumentName);

                ConsoleWriter.WriteWithColor($"Получен документ {newDocumentName}", myDefaultColor);

                var result = true;

                foreach (var filename in _documentFilenames)
                {
                    result = result && files.Contains(Path.Combine(_targetDirectory, filename));
                }

                DocumentsReadyMarker = result;
            }
        }

        private bool DocumentsReadyMarker
        {
            get { return _documentsReady; }
            set
            {
                _documentsReady = value;
                if (_documentsReady)
                {
                    OnDocumentsReady();
                }
            }
        }


        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ConsoleWriter.WriteWithColor("Receiver: timer elapsed", myDefaultColor);
            if (DocumentsReadyMarker) return;
            if (!imWorking) return;
            Stop();
            TimedOut();
        }

        private void TurnOffTimer()
        {
            _timer.Stop();
            _timer.Elapsed -= Timer_Elapsed;
            _timer.Dispose();
        }

        private void TurnOffTWatcher()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Changed -= Watcher_Changed;
            }

            _watcher.Dispose();
        }

        private void OnDocumentsReady()
        {
            ConsoleWriter.WriteWithColor("Receiver: documents ready", myDefaultColor);
            if (!imWorking) return;
            Stop();
            DocumentsReady();
            
        }

    }

    public static class ConsoleWriter
    {
        //класс, который пишет в консоль разными цветами и пр.
        private static object _locker = new object();
        public static void WriteWithColor(string text, ConsoleColor color)
        {
            lock (_locker) //если не использовать локер, цвета смешиваются
            {
                ConsoleColor _color = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine(text);
                Console.WriteLine("");
                Console.ForegroundColor = _color;
            }
        }

        public static void WriteDefault(string text) { WriteWithColor(text, Console.ForegroundColor); }

        public static void WriteRed(string text) { WriteWithColor(text, ConsoleColor.Red); }

        public static void WriteGreen(string text) { WriteWithColor(text, ConsoleColor.Green); }

        public static void WriteYellow(string text) { WriteWithColor(text, ConsoleColor.Yellow); }

    }
}

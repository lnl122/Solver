using System;

namespace Solver
{
    // public void Init()
    // public void Close()
    // public void Write(string text)
    //
    class Log
    {
        //private static string PathToLogs = "";                 // путь (без файла) к логам
        private static string PathToPages = "";                // путь (без слеша в конце, к папке для сохраняемых страниц
        private static System.IO.StreamWriter logfile;  // поток лога
        public static bool isReady = false;             // инициализация проведена?
        private static bool isBusy = false;             // счас заняты? чтоб подождать если необходимо. для устранения коллизий при активном логгировании

        // записывает строку текста в лог-файл
        // вход     строка для лог файла
        // выход    -
        public static void Write(string str)
        {
            if(isReady)
            {
                while (isBusy) { isBusy = isBusy; } // *** можно ли убрать содержимое цикла?
                isBusy = true;
                logfile.WriteLine("{0} {1} {2}", DateTime.Today.ToShortDateString(), DateTime.Now.ToLongTimeString(), str);
                isBusy = false;
            }
        }

        // выполняет принудительную запись лога на диск
        // вход     -
        // выход    -
        public static void Close()
        {
            if (isReady)
            {
                logfile.Flush();
                logfile.Close();
                logfile = null;
                isReady = false;
            }
        }

        // если папка есть, или если не было, но удалось создать - возвращает путь к ней, иначе - базовый путь
        // вход     базовый путь, имя папки
        // выход    путь к папке
        private static string CheckCreateFolder(string basepath, string folder)
        {
            string path = basepath + @"\" + folder;
            if (System.IO.Directory.Exists(path) == false)
            {
                try
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                catch
                {
                    path = basepath;
                }
            }
            return path;
        }

        // инициализирует лог файл, если нету его - создает. в т.ч. необходимые папки
        // вход     -
        // выход    -
        public static void Init()
        {
            string local_path = Environment.CurrentDirectory;
            string self_name = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName;
            string PathToLogs = CheckCreateFolder(local_path, "Log");
            PathToPages = CheckCreateFolder(local_path, "Pages");
            string pathfilename = PathToLogs + "\\" + self_name + ".log";
            logfile = new System.IO.StreamWriter(System.IO.File.AppendText(pathfilename).BaseStream);
            logfile.AutoFlush = true;
            isReady = true;
        }
    }
}

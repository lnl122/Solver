using System;

namespace Solver
{
    // public void Init()
    // public void Close()
    // public void Write(string text, string text2="", string text3 = "")
    // public void Store(string modulename, string pagetext)
    //
    class Log
    {
        //private static string PathToLogs = "";          // путь (без файла) к логам
        private static string PathToPages = "";         // путь (без слеша в конце, к папке для сохраняемых страниц
        private static System.IO.StreamWriter logfile;  // поток лога
        public static bool isReady = false;             // инициализация проведена?
        private static bool isBusy = false;             // счас заняты? чтоб подождать если необходимо. для устранения коллизий при активном логгировании
        private static int fileidx = 1;                 // индекс/номер сохраняемого файла

        // записывает строку текста в лог-файл
        // вход     строка для лог файла
        // выход    -
        public static void Write(string str, string str2 = "", string str3 = "")
        {
            if(isReady)
            {
                while (isBusy) { isBusy = isBusy; } // *** можно ли убрать содержимое цикла?
                isBusy = true;
                logfile.WriteLine("{0} {1} {2}", DateTime.Today.ToShortDateString(), DateTime.Now.ToLongTimeString(), str);
                if (str2 != "")
                {
                    logfile.WriteLine("                          " + str2);
                    if (str3 != "")
                    {
                        logfile.WriteLine("                          " + str3);
                    }
                }
                isBusy = false;
            }
        }

        // записывает текст в отдельный файл
        // вход     имя модуля, строка текста
        // выход    -
        public static void Store(string modulename, string text)
        {
            if (isReady)
            {
                string path = PathToPages + "\\" + modulename + "_" + fileidx.ToString() + "_" +
                    DateTime.Today.Year.ToString() + DateTime.Today.Month.ToString() + DateTime.Today.Day.ToString() +
                    DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + ".http";
                System.IO.File.WriteAllText(path, text, System.Text.Encoding.UTF8);
                fileidx++;
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
            string PathToData = CheckCreateFolder(local_path, "Data");
            PathToPages = CheckCreateFolder(local_path, "Pages");
            string pathfilename = PathToLogs + "\\" + self_name + ".log";
            logfile = new System.IO.StreamWriter(System.IO.File.AppendText(pathfilename).BaseStream);
            logfile.AutoFlush = true;
            isReady = true;
        }
    }
}

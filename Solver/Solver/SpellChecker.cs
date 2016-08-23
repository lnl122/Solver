using System.Collections.Generic;

namespace Solver
{
    // need COM Reference "Microsoft.Word.14.Object.Library"
    //
    // public Init()
    // public SpellChecker() - constructor
    // public void Close()
    // public void LoadDictionary(string DictPath)
    // public void SaveDictionary()
    // public List<string> Check(List<string> InnerWordList)
    // public bool Check(string SingleWord)
    // public bool CheckOne(string SingleWord)
    //
    class SpellChecker
    {
        // словари - загружаемый и создающийся в процессе работы
        private static List<string> dict1;
        private static List<string> dict2;
        // путь к словарю
        private static string DictionaryPath = "";
        // первое создание объекта уже было?
        private static bool isObjectReady = false;
        // словарь был ли загружен?
        private static bool isDicionaryLoaded = false;

        // внешний объект
        private Microsoft.Office.Interop.Word.Application WordApp = null;

        public static void Init()
        {
            Log.Write("words Инициализация объекта SpellChecker");
            if (isObjectReady == false)
            {
                if (CheckMsWord() == true)
                {
                    dict1 = new List<string>();
                    dict2 = new List<string>();
                    isObjectReady = true;
                }
            }
        }

        // конструктор
        // выход - объект
        public SpellChecker()
        {
            //инициализация одного объекта, если ранее не инициализировали
            if (isObjectReady == true)
            {
                WordApp = new Microsoft.Office.Interop.Word.Application();
                Log.Write("words Создание нового объекта спелчекера");
            }
        }

        // чтение словаря
        public static void LoadDictionary(string DictPath)
        {
            Log.Write("words Чтение внешнего словаря начато");
            if (isObjectReady == false) { return; }
            // если словарь не загружен
            if (isDicionaryLoaded == false)
            {
                // проверить путь на валидность
                if (System.IO.File.Exists(DictPath) == true)
                {
                    string[] dict; // временный массив
                    dict = System.IO.File.ReadAllLines(DictPath);
                    DictionaryPath = DictPath;
                    // переносим в List
                    foreach (string s1 in dict)
                    {
                        dict1.Add(s1.ToLower());
                    }
                    isDicionaryLoaded = true;
                    Log.Write("words Чтение внешнего словаря завершено");
                }
            }
        }

        // деструктор
        public void Close()
        {
            //SaveDictionary();
            WordApp.Quit();
            WordApp = null;
            Log.Write("words Закрыли очередную копию MS Word");
        }

        // обновление словаря на диске
        public static void SaveDictionary()
        {
            Log.Write("words Запись в файл словаря для Spellchecker'а начата");
            if (isObjectReady == false) { return; }
            // объединяем два словаря (без пустых строк) и сохраняем в файл DictionaryPath
            List<string> dict_out = new List<string>();
            dict_out.AddRange(dict1);
            foreach (string s1 in dict2)
            {
                dict_out.Add(s1.ToLower());
            }
            System.IO.File.WriteAllLines(DictionaryPath, dict_out.ToArray());
            Log.Write("words Запись в файл словаря для Spellchecker'а завершена");
        }

        // проверим существование и работу спелчекера
        // выход - true/false
        private static bool CheckMsWord()
        {
            Log.Write("words Проверяем наличие MS Word");
            try
            {
                var wa = new Microsoft.Office.Interop.Word.Application();
                wa.CheckSpelling("мама мыла раму");
                wa.Quit();
                Log.Write("words MS Word точно есть");
                return true;
            }
            catch
            {
                Log.Write("words ERROR: MS Word не удалось запустить, проверить слова, или какие-то другие проблемы");
                return false;
            }
        }

        // вход - список слов
        // выход - список слов, по которым орфография пройдена успешно
        public List<string> Check(List<string> InnerWordList)
        {
            //результат
            List<string> res = new List<string>();
            if (isObjectReady == false) { return res; }
            // для всех слов
            foreach (string SingleWord in InnerWordList)
            {
                // нормализуем входящее слово
                string NormalWord = SingleWord.ToLower().Trim();
                // отсекаем пустые слова
                if (NormalWord == "")
                {
                    continue;
                }
                if (isDicionaryLoaded == true)
                {
                    // проверяем в основном словаре
                    if (dict1.Contains(NormalWord))
                    {
                        res.Add(NormalWord);
                        continue;
                    }
                    // проверяем в пользовательском словаре
                    if (dict2.Contains(NormalWord))
                    {
                        res.Add(NormalWord);
                        continue;
                    }
                }
                // проверяем в MsWord само слово
                if (WordApp.CheckSpelling(NormalWord) == true)
                {
                    res.Add(NormalWord);
                    dict2.Add(NormalWord);
                    continue;
                }
                // проверяем в MsWord капитализированное слово
                string CapitalizedWord = NormalWord.Substring(0, 1).ToUpper() + NormalWord.Substring(1, NormalWord.Length - 1);
                if (WordApp.CheckSpelling(CapitalizedWord) == true)
                {
                    res.Add(NormalWord);
                    dict2.Add(NormalWord);
                    continue;
                }
            }
            return res;
        }

        // вход - слово
        // выход - true/false
        public bool Check(string SingleWord)
        {
            if (isObjectReady == false) { return false; }
            // нормализуем входящее слово
            string NormalWord = SingleWord.ToLower().Trim();
            // отсекаем пустые слова
            if (NormalWord == "")
            {
                return false;
            }
            if (isDicionaryLoaded == true)
            {
                // проверяем в основном словаре
                if (dict1.Contains(NormalWord))
                {
                    return true;
                }
                // проверяем в пользовательском словаре
                if (dict2.Contains(NormalWord))
                {
                    return true;
                }
            }
            // проверяем в MsWord само слово
            if (WordApp.CheckSpelling(NormalWord) == true)
            {
                dict2.Add(NormalWord);
                return true;
            }
            // проверяем в MsWord капитализированное слово
            NormalWord = NormalWord.Substring(0, 1).ToUpper() + NormalWord.Substring(1, NormalWord.Length - 1);
            if (WordApp.CheckSpelling(NormalWord) == true)
            {
                dict2.Add(NormalWord);
                return true;
            }
            // если не нашли в ворде
            return false;
        }

        // вход - слово
        // выход - true/false
        // слово не нормализуется, не проверяется по словарю
        public bool CheckOne(string SingleWord)
        {
            if (isObjectReady == false) { return false; }
            // отсекаем пустые слова
            if (SingleWord == "")
            {
                return false;
            }
            return WordApp.CheckSpelling(SingleWord);
        }

    }
}

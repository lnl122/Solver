using System;
//using Microsoft.Office.Interop.Word;
using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;

namespace TestComponents
{
    class SpellChecker
    {
        // словари - загружаемый и создающийся в процессе работы
        static List<string> dict1 = new List<string>();
        static List<string> dict2 = new List<string>();

        // путь к словарю
        private static string DictionaryPath = "";
        // первое создание объекта уже было?
        private static bool isObjectReady = false;
        // словарь был ли загружен?
        private static bool isDicionaryLoaded = false;
        // внешний объект
        private Microsoft.Office.Interop.Word.Application WordApp = null;

        // конструктор
        // вход - путь к словарю или пусто
        // выход - объект
        public SpellChecker(string DictPath = "")
        {
            //инициализация одного объекта, если ранее не инициализировали
            if (isObjectReady == false)
            {
                // если Ворд есть
                if (CheckMsWord() == true)
                {
                    // если словарь не загружен
                    if (isDicionaryLoaded == false)
                    {
                        // выполнить загрузку словаря в dict
                        if (DictPath != "")
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
                            }
                        }
                    }
                    isObjectReady = true;
                }
            }
            if (isObjectReady == true)
            {
                WordApp = new Microsoft.Office.Interop.Word.Application();
            }
        }

        // деструктор
        public void Close()
        {
            Flush();
            WordApp.Quit();
            WordApp = null;
        }

        // обновление словаря на диске
        public void Flush()
        {
            // объединяем два словаря (без пустых строк) и сохраняем в файл DictionaryPath
            List<string> dict_out = new List<string>();
            dict_out.AddRange(dict1);
            foreach(string s1 in dict2)
            {
                dict_out.Add(s1.ToLower());
            }
            System.IO.File.WriteAllLines(DictionaryPath, dict_out.ToArray());
        }

        // проверим существование и работу спелчекера
        // выход - true/false
        private bool CheckMsWord()
        {
            try
            {
                var wa = new Microsoft.Office.Interop.Word.Application();
                wa.CheckSpelling("мама мыла раму");
                wa.Quit();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // вход - список слов
        // выход - список слов, по которым орфография пройдена успешно
        public List<string> Check(List<string> InnerWordList)
        {
            //результат
            List<string> res = new List<string>();

            // для всех слов
            foreach (string SingleWord in InnerWordList)
            {
                // нормализуем входящее слово
                string NormalWord = SingleWord.ToLower().Trim();
                // отсекаем пустые слова
                if(NormalWord == "")
                {
                    continue;
                }
                if(isDicionaryLoaded == true)
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
            // отсекаем пустые слова
            if (SingleWord == "")
            {
                return false;
            }
            return WordApp.CheckSpelling(SingleWord);
        }

    }
}

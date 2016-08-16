using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

namespace TestComponents
{
    // need COM Reference "Microsoft.Word.14.Object.Library"
    //
    // public Associations()
    // public void Close()
    // public void LoadDictionary(string DictPath)
    // public void SaveDictionary()
    // public List<string> Get(string)
    // public List<string> Get(List<string>)
    // public List<string> Get2(string, string)
    // public List<string> Get2(List<string>, List<string>)
    // ***public List<string> Get3(string, string, string)
    // ***public List<string> Get3(List<string>, List<string>, List<string>)
    //
    class Associations
    {
        // словари
        private static List<string> words;
        private static List<List<string>> assoc;
        // путь к словарю
        private static string DictionaryPath = "";
        // первое создание объекта уже было?
        private static bool isObjectReady = false;
        // словарь был ли загружен?
        private static bool isDicionaryLoaded = false;
        // максимальное количество попыток чтения
        private static int MaxTryToReadPage = 3;
        // на сколько миллисекунд засыпать при неудачном одном чтении
        private static int TimeToSleepMs = 1000;

        // конструктор
        // выход - объект
        public static void Init()
        {
            //инициализация одного объекта, если ранее не инициализировали
            if (isObjectReady == false)
            {
                words = new List<string>();
                assoc = new List<List<string>>();
                isObjectReady = true;
            }
        }

        // чтение словаря
        public static void LoadDictionary(string DictPath)
        {
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
                        int idx = s1.IndexOf(" ");
                        if(idx > 0)
                        {
                            string w1 = s1.Substring(0, idx);
                            string w2 = s1.Substring(idx+1);
                            string[] ar2 = w2.Split(' ');
                            List<string> lw = new List<string>();
                            foreach(string w3 in ar2)
                            {
                                lw.Add(w3);
                            }
                            words.Add(w1);
                            assoc.Add(lw);
                        }
                    }
                    isDicionaryLoaded = true;
                }
            }
        }

        /*
        // деструктор
        public void Close()
        {
        }
        */

        // обновление словаря на диске
        public static void SaveDictionary()
        {
            if (isObjectReady == false) { return; }
            string[] ar = new string[words.Count];
            for(int i=0; i< words.Count; i++)
            {
                string temp = words[i]+' ';
                foreach(string s1 in assoc[i])
                {
                    temp = temp + s1 + ' ';
                }
                ar[i] = temp.TrimEnd();
            }
            System.IO.File.WriteAllLines(DictionaryPath, ar);
        }

        // вход - слово
        // выход - страница с ассоциациями от http://sociation.org/word/
        private static string GetPageSociation(string word)
        {
            System.Net.WebClient WebClient = new System.Net.WebClient();
            WebClient.Encoding = System.Text.Encoding.UTF8;
            string url = "http://sociation.org/word/" + word;
            string Page = "";
            bool isNeedReadPage = true;
            int CountTry = 0;
            while (isNeedReadPage)
            {
                try
                {
                    Page = WebClient.DownloadString(url);
                    isNeedReadPage = false;
                }
                catch
                {
                    Thread.Sleep(TimeToSleepMs);
                    CountTry++;
                    if (CountTry == MaxTryToReadPage)
                    {
                        //Log("ERROR: sociation.org вызвал наш таймаут в секунду");
                        Page = "";
                        isNeedReadPage = false;
                    }
                }
            }
            WebClient.Dispose();
            if (Page.Length <= 0)
            {
                return "";
            }
            Page = Page.ToLower().Replace("\t", " ").Replace("\n", " ");
            int body1 = Page.IndexOf("<body>");
            int body2 = Page.IndexOf("</body>");
            if ((body1 == -1) || (body2 == -1))
            {
                return "";
            }
            Page = Page.Substring(body1+6, body2-body1-6);
            return Page;
        }

        // вход - текст страницы
        // выход - список слов найденных на странице
        private static List<string> ParsePage(string page)
        {
            List<string> result = new List<string>();
            if (page.Length <= 1)
            {
                return result;
            }
            int ol1 = page.IndexOf("<ol ");
            int ol2 = page.IndexOf("</ol>");
            if ((ol1 == -1) || (ol2 == -1) || (ol1 > ol2))
            {
                return result;
            }
            page = page.Substring(ol1, ol2 - ol1);
            string[] st1 = Regex.Split(page, "</a>");
            foreach(string st in st1)
            {
                string temp = st.Substring(st.LastIndexOf(">") + 1).Trim();
                if(temp.Length > 0)
                {
                    if(temp.IndexOf(' ') == -1)
                    {
                        result.Add(temp);
                    }
                }
            }
            return result;
        }

        // вход - слово и ассоциации
        private static void AddDictionary(string wrd, List<string> lst)
        {
            words.Add(wrd);
            assoc.Add(lst);
        }

        // выбирает из списка первых count значений
        private static List<string> GetFirstItems(List<string> list, int count)
        {
            if(list.Count <= count)
            {
                return list;
            }
            return list.GetRange(0, count);
        }

        // вход - два списка слов
        // выход - список общих ассоциаций к словам
        public static List<string> Get2(List<string> list1, List<string> list2)
        {
            List<string> result = new List<string>();
            if (isObjectReady == false) { return result; }
            List<string> l1 = Get(list1);
            List<string> l2 = Get(list2);
            foreach (string st in l1)
            {
                if (l2.Contains(st))
                {
                    result.Add(st);
                }
            }
            return result;
        }
        
        // вход - два слова
        // выход - список общих ассоциаций к словам
        public static List<string> Get2(string str1, string str2)
        {
            List<string> result = new List<string>();
            if (isObjectReady == false) { return result; }
            List<string> list1 = Get(str1);
            List<string> list2 = Get(str2);
            foreach(string st in list1)
            {
                if (list2.Contains(st))
                {
                    result.Add(st);
                }
            }
            return result;
        }

        // поиск ассоциации к списку слов
        // вход - список слов
        // выход - список ассоциаций ко всем словам
        public static List<string> Get(List<string> list, int count = 999)
        {
            List<string> result = new List<string>();
            if (isObjectReady == false) { return result; }
            foreach (string str in list)
            {
                List<string> t1 = Get(str, count);
                result.AddRange(t1);
            }
            // надо убрать дупы
            bool flag = true;
            while (flag)
            {
                flag = false;
                foreach (string st in result)
                {
                    int idx1 = result.IndexOf(st);
                    int idx2 = result.LastIndexOf(st);
                    if (idx1 != idx2)
                    {
                        result.RemoveAt(idx2);
                        flag = true;
                        break;
                    }
                }
            }
            return result;
        }

        // поиск ассоциации одного слова
        // вход - слово
        // выход - список ассоциаций
        public static List<string> Get(string word, int count = 999)
        {
            List<string> result = new List<string>();
            if (isObjectReady == false) { return result; }
            if (word.Length < 1)
            {
                return result;
            }
            // поиск в словаре
            int idxwrd = words.IndexOf(word);
            if (idxwrd >= 0)
            {
                return Associations.GetFirstItems(assoc[idxwrd], count);
            }
            // если не нашли - поиск на внешнем сервисе
            string page = GetPageSociation(word);
            if (page.Length < 1)
            {
                return result;
            }
            result = ParsePage(page);
            if (result.Count > 0)
            {
                AddDictionary(word, result);
            }
            return GetFirstItems(result, count);
        }
    }
}

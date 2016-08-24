using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Solver
{
    class Upload
    {
        // аплоад картинки по пути, получени внешней ссылки - заглушка для возможности выбора сервиса аплоада
        // *** сделать привязку к настройкам
        // *** добавить в проверки при старте полный цикл: картинка (чебурашка) распознавание + ассоциации = проверяемый результат (гена)
        // вход - путь к локальной картинке
        // выход - урл картинки после аплоада
        public static string UploadFile(string fp)
        {
            return UploadFile_pixicru(fp);
            //return UploadFile_ipicsu(fp);
        }

        // аплоад картинки по пути, получени внешней ссылки
        // вход - путь к локальной картинке
        // выход - урл картинки после аплоада
        private static string UploadFile_pixicru(string filepath)
        {
            string filename = filepath.Substring(filepath.LastIndexOf("\\") + 1);
            string uriaction = "http://www.pixic.ru/";
            HttpClient httpClient = new HttpClient();
            //System.Net.ServicePointManager.Expect100Continue = false;
            MultipartFormDataContent form = new MultipartFormDataContent();
            form.Add(new StringContent("1"), "send");
            var streamContent2 = new StreamContent(File.Open(filepath, FileMode.Open));
            form.Add(streamContent2, "file1", filename);
            string sd = "";
            try
            {
                Task<HttpResponseMessage> response = httpClient.PostAsync(uriaction, form);
                HttpResponseMessage res2 = response.Result;
                res2.EnsureSuccessStatusCode();
                HttpContent Cont = res2.Content;
                httpClient.Dispose();
                sd = res2.Content.ReadAsStringAsync().Result;
                Log.Store("upld1", sd);
                sd = sd.Substring(sd.IndexOf("large_input") + ("large_input").Length);
                sd = sd.Substring(sd.IndexOf("value='") + 7);
                sd = sd.Substring(0, sd.IndexOf("'"));
            }
            catch
            {
                Log.Write("upld1 ERROR: pixicru не удалось выполнить аплоад картинки ", uriaction, filepath);
                sd = "";
            }
            if (sd.Length < 5)
            {
                Log.Write("upld1 ERROR: pixicru вернулась слишком короткая ссылка", sd, filepath);
                sd = "";
            }
            if (sd.Substring(0, 4) != "http")
            {
                Log.Write("upld1 ERROR: pixicru то что вернулось не является ссылкой http", sd, filepath);
                sd = "";
            }
            return sd;
        }

        // аплоад картинки по пути, получени внешней ссылки
        // вход - путь к локальной картинке
        // выход - урл картинки после аплоада
        private static string UploadFile_ipicsu(string filepath)
        {
            string filename = filepath.Substring(filepath.LastIndexOf("\\") + 1);
            string uriaction = "http://ipic.su/";
            HttpClient httpClient = new HttpClient();
            //System.Net.ServicePointManager.Expect100Continue = false;
            MultipartFormDataContent form = new MultipartFormDataContent();

            form.Add(new StringContent("/"), "link");
            form.Add(new StringContent("loadimg"), "action");
            form.Add(new StringContent("ipic.su"), "client");
            var streamContent2 = new StreamContent(File.Open(filepath, FileMode.Open));
            form.Add(streamContent2, "image", filename);
            string sd = "";
            try
            {
                Task<HttpResponseMessage> response = httpClient.PostAsync(uriaction, form);
                HttpResponseMessage res2 = response.Result;
                res2.EnsureSuccessStatusCode();
                HttpContent Cont = res2.Content;
                httpClient.Dispose();
                sd = res2.Content.ReadAsStringAsync().Result;
                Log.Store("upld1", sd);
                sd = sd.Substring(sd.IndexOf("[edit]") + 6);
                sd = sd.Substring(sd.IndexOf("value=\"") + 7);
                sd = sd.Substring(0, sd.IndexOf("\""));
            }
            catch
            {
                Log.Write("upld1 ERROR: не удалось выполнить аплоад картинки ", uriaction, filepath);
                sd = "";
            }
            return sd;
        }

    }
}

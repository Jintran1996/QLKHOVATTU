using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ToolsApp.Utilities
{
    public static class UtilsLocal
    {
        public static void AddException(string Form, string Message)
        {
            //using (var db = new Coupon_Phuoc2809Entities())
            //{
            //    var model = new ExceptionLog { Form = Form, Message = Message, Datetime = DateTime.Now };
            //    db.ExceptionLogs.Add(model);
            //    db.SaveChanges();
            //}
        }
        public static string ModelStateError(ModelStateDictionary ModelState)
        {
            var message = "";

            var index = 0;

            foreach (var value in ModelState.Values)
            {
                if (value.Errors.Count > 0)
                {
                    var index2 = 0;

                    foreach (var item in ModelState.Keys)
                    {
                        if (index == index2)
                        {
                            message += item + ": " + value.Errors.Select(e => e.ErrorMessage).FirstOrDefault() + "|";
                        }
                        index2++;
                    }
                }
                index++;
            }

            return message;
        }
        public static int GenerateRandomNo7Digit()
        {
            int _min = 1000000;
            int _max = 9999999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max);
        }
        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = @"ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static string GetIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return context.Request.ServerVariables["REMOTE_ADDR"];
        }
        public static string HttpWebRequest(string Url)
        {
            try
            {
                var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(Url);
                //request.Timeout = 120000;
                var response = (System.Net.HttpWebResponse)request.GetResponse();
                var responseString = new System.IO.StreamReader(response.GetResponseStream()).ReadToEnd();
                return responseString;
            }
            catch (Exception ex)
            {
                return ex.Message;                      
            }           
        }
        public static string HttpWebRequestGet(string Url)
        {
            try
            {
                string URL = Url;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                request.ContentType = "application/json";
                request.Method = "GET";                

                var result = "";
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    result = reader.ReadToEnd();
                }

                return result;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        #region mahoaS
        public static string mahoaS(string input)
        {
            MD5 md5Hasher = MD5.Create();
            //binary

            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));

            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x0"));
            }
            return sBuilder.ToString();
        }
        #endregion
    }
}
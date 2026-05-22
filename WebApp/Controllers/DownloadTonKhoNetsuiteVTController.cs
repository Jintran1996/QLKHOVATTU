using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ToolsApp.Authentication;
using ToolsApp.com.netsuite.webservices;
using ToolsApp.EntityFramework.KhoSoi;
using ToolsApp.EntityFramework.VatTu;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.IO;
using System.Drawing;
using static System.Collections.Specialized.BitVector32;
using System.Web.UI.WebControls;
using System.Net.Http.Headers;

using System.Security.Cryptography;



namespace ToolsApp.Controllers
{
    [Authorize]
    public class DownloadTonKhoNetsuiteVTController : BaseController
    {
        private wqlvattuEntities vt_ = new wqlvattuEntities();
        NSClient.NSClient ns = new NSClient.NSClient();

        private const string ACCOUNT_ID = "8687359";

        private const string CONSUMER_KEY = "aba78531cfcc71f836d456bd10003cb6cd632379e516bb88a52298e35a3dacd0";
        private const string CONSUMER_SECRET = "41413ca7a4dc6bc15aedb6a3bad1d10a8b9a3418682e45f1854f6ef9dca76c50";
        private const string TOKEN_ID = "2bd5ef0ef69ff672af8b896cab0107262ddd917008c05c5b6d8fb5fac9704afd";
        private const string TOKEN_SECRET = "99e0b581d061d573e48a7551a5671ec70ddea431aaa5ca158cdbd4e0253a5e9a";

        private const string SAVED_SEARCH_URL =
    "https://8687359.suitetalk.api.netsuite.com" +
    "/services/rest/record/v1/search/customsearch_btm_tt_stock_ob_balance_2/results";



        // GET: XemXetAllVatTu
        public ActionResult Index()
        {
           // ViewBag.MAKHO = ns_.SPLOAD_TONKHOALL_TP_2024().ToList();
            ViewBag.NAM = vt_.DMNAMs.Where(p => p.HieuLuc == 1).ToList();
            ViewBag.THANG = vt_.DMTHANGs.Where(p => p.HieuLuc == true).ToList();
            ViewBag.MADVI = vt_.VATTU2024_SPLOAD_DANHMUCBOPHAN().ToList();
            ViewBag.GETYEAR = DateTime.Now.Year.ToString();
            ViewBag.GETMONTH = DateTime.Now.ToString("MM");
            return View();
        }


        public ActionResult _GetList(int NAM, string THANG)
        {
            var boolThang = (THANG == "ALL") ? true : false;
            int THANG_ = THANG == "ALL" ? 01 : Convert.ToInt16(THANG);


            var list = vt_.VATTU2024_SP_VATTU_BAOCAOTONGHOPBM05_ALLTHANG(true, boolThang, NAM, THANG_, "", "").ToList();
            ViewBag.List = list;

            return PartialView();
        }


        public async Task<ActionResult> GetTonKho()
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, SAVED_SEARCH_URL);

                // GẮN HEADER OAUTH (QUAN TRỌNG)
                request.Headers.Add(
                    "Authorization",
                    BuildOAuthHeader(SAVED_SEARCH_URL, "GET")
                );

                request.Headers.Add("Accept", "application/json");

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    return Content(
                        "Lỗi gọi NetSuite: " + response.StatusCode,
                        "text/plain"
                    );
                }

                var json = await response.Content.ReadAsStringAsync();

                // Trả nguyên JSON cho dễ test
                return Content(json, "application/json");
            }
        }

        // ======================================================
        // =============== PHẦN KÝ OAUTH (ĐỪNG SỬA) ===============
        // ======================================================

        private string BuildOAuthHeader(string url, string httpMethod)
        {
            string nonce = Guid.NewGuid().ToString("N");
            string timestamp = GetTimestamp();

            var parameters = new SortedDictionary<string, string>
        {
            { "oauth_consumer_key", CONSUMER_KEY },
            { "oauth_token", TOKEN_ID },
            { "oauth_nonce", nonce },
            { "oauth_timestamp", timestamp },
            { "oauth_signature_method", "HMAC-SHA256" },
            { "oauth_version", "1.0" }
        };

            string signatureBase = BuildSignatureBase(url, httpMethod, parameters);
            string signature = GenerateSignature(signatureBase);

            parameters.Add("oauth_signature", signature);

            var headerParams = parameters
                .Select(p => $"{p.Key}=\"{Uri.EscapeDataString(p.Value)}\"");

            return "OAuth realm=\"" + ACCOUNT_ID + "\"," +
                   string.Join(",", headerParams);
        }

        private string BuildSignatureBase(
            string url,
            string method,
            SortedDictionary<string, string> parameters)
        {
            string paramString = string.Join("&",
                parameters.Select(p =>
                    Uri.EscapeDataString(p.Key) + "=" +
                    Uri.EscapeDataString(p.Value)));

            return method.ToUpper() + "&" +
                   Uri.EscapeDataString(url) + "&" +
                   Uri.EscapeDataString(paramString);
        }

        private string GenerateSignature(string signatureBase)
        {
            string key =
                Uri.EscapeDataString(CONSUMER_SECRET) + "&" +
                Uri.EscapeDataString(TOKEN_SECRET);

            using (var hasher = new HMACSHA256(Encoding.ASCII.GetBytes(key)))
            {
                byte[] hash = hasher.ComputeHash(
                    Encoding.ASCII.GetBytes(signatureBase)
                );
                return Convert.ToBase64String(hash);
            }
        }

        private string GetTimestamp()
        {
            return ((int)(DateTime.UtcNow -
                new DateTime(1970, 1, 1)).TotalSeconds).ToString();
        }









    }
}
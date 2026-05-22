/// <summary>
/// 
/// Fully functional, command-line driven application that illustrates how
/// to connect to the NetSuite web services and invoke operations.
/// 
/// Copyright  NetSuite Inc. 1999. All rights reserved.
///
/// Author: Jan Arendtsz
/// Author: Aish Shukla
/// Author: Xi Liu
/// Author: Mihir Shah
/// Author: Andrej Hank
/// Author: Jakub Danek
///
/// Usage: NSClient.exe
///
/// </summary>

using System;
using System.Net;
using ToolsApp.com.netsuite.webservices;
using System.Xml;
using System.Security.Cryptography;
using System.Configuration;

namespace NSClient
{
    /// <summary>
    /// Summary description for NSClient.
    /// </summary>
    public class NSClient
    {

        private NetSuiteService _service;

        /// <value>Proxy class that abstracts the communication with the 
        /// NetSuite Web Services. All NetSuite operations are invoked
        /// as methods of this class.</value>
        public NetSuiteService Service {
            get
            {
                // We need to create new TBA token for every request
                if(this.UseTba)
                    _service.tokenPassport = CreateTokenPassport();
                return _service;
            }
        }

        /// <value>Flag that indicates whether the user is currently 
        /// authentciated, and therefore, whether a valid session is 
        /// available</value>
        public bool IsAuthenticated { get; private set; }

        /// <value>Utility class for logging</value>
        public Logger Out { get; private set; }

		/// <value>A NameValueCollection that abstracts name/value pairs from
		/// the app.config file in the Visual .NET project. This file is called
		/// [AssemblyName].exe.config in the distribution</value>
		public System.Collections.Specialized.NameValueCollection DataCollection { get; private set; }

        /// <value>Default page size used for search in this application</value>
        public int PageSize { get; private set; }

        /// <value>Flag saying whether authentication is token based. </value>
        public bool UseTba { get; private set; }

        /// Set up request level preferences as a SOAP header
        public Preferences Prefs { get; private set; }
        public SearchPreferences SearchPreferences { get; private set; }

   
        
        /// <summary>
        /// Constructor
        /// </summary>
        public NSClient(  )
		{
            IsAuthenticated = false;
			PageSize = 5;            

			Out = new Logger( "info" );

			// Reference to config file that contains sample data. This file
			// is named App.config in the Visual .NET project or, <AssemblyName>.exe.config
			// in the distribution
			DataCollection = System.Configuration.ConfigurationManager.AppSettings;

			//Decide between standard login and TBA
			UseTba = "true".Equals(ConfigurationManager.AppSettings["login.useTba"].ToString());

            // Instantiate the NetSuite web services
            _service = new DataCenterAwareNetSuiteService(ConfigurationManager.AppSettings["login.acct"].ToString(), false);
            _service.Timeout = 1000 * 60 * 60 * 2;

			//Enable cookie management
			Uri myUri = new Uri("https://webservices.netsuite.com");
            _service.CookieContainer = new CookieContainer();
		}

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]

        static void Main(string[] args)
        {
            // Force TLS 1.2
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // In order to enable SOAPscope to work through SSL. Refer to FAQ for more details
            ServicePointManager.ServerCertificateValidationCallback += delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
            {
                return true;
            };

            NSClient ns = null;
            try
            {
                ns = new NSClient();
                NSBase.Client = ns;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while loading the application:" + ex.Message);
                Console.WriteLine("Press Enter to quit ... ");
                Console.ReadKey();
                return;
            }
        }





            /// <summary>
            /// <p>This function builds the Pereferences and SearchPreferences in the SOAP header. </p>
            /// </summary>
            public void SetPreferences()
		{
			// Set up request level preferences as a SOAP header
			Prefs = new Preferences();
			_service.preferences = Prefs;
			SearchPreferences = new SearchPreferences();
            _service.searchPreferences = SearchPreferences;

			// Preference to ask NS to treat all warnings as errors
			Prefs.warningAsErrorSpecified = true;
			Prefs.warningAsError = false;
            Prefs.ignoreReadOnlyFieldsSpecified = true;
            Prefs.ignoreReadOnlyFields = true;

            SearchPreferences.pageSize = PageSize;
			SearchPreferences.pageSizeSpecified = true;
			// Setting this bodyFieldsOnly to true for faster search times on Opportunities
			SearchPreferences.bodyFieldsOnly = true;
            PrepareLoginPassport();
        }


        private void PrepareLoginPassport()
        {
       
           _service.tokenPassport = CreateTokenPassport();
          
        }
    


        /// <summary>
        /// Update search preference to either return body fields, return columns or full records
        /// </summary>
        /// <param name="bodyFieldsOnly"></param>
        /// <param name="returnColumns"></param>
        public void SetSearchPreferences(bool bodyFieldsOnly, bool returnColumns)
		{
            _service.searchPreferences.bodyFieldsOnly = bodyFieldsOnly;
            _service.searchPreferences.returnSearchColumns = returnColumns;
		}


        /// <summary>
        /// <p>Processes the status object and prints the status details</p>
        /// </summary>
        public String GetStatusDetails( Status status )
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			for ( int i=0; i < status.statusDetail.Length; i++ )
			{
				sb.Append( "[Code=" + status.statusDetail[i].code + "] " + status.statusDetail[i].message + "\n" );
			}
			return sb.ToString();
		}

        public TokenPassport CreateTokenPassport()
		{

         
                string account = ConfigurationManager.AppSettings["login.acct"].ToString();
			    string consumerKey = ConfigurationManager.AppSettings["login.tbaConsumerKey"].ToString();
			    string consumerSecret = ConfigurationManager.AppSettings["login.tbaConsumerSecret"].ToString();
			    string tokenId = ConfigurationManager.AppSettings["login.tbaTokenId"].ToString();
			    string tokenSecret = ConfigurationManager.AppSettings["login.tbaTokenSecret"].ToString();
						
			string nonce = ComputeNonce();
			long timestamp = ComputeTimestamp();
			TokenPassportSignature signature = ComputeSignature(account, consumerKey, consumerSecret, tokenId, tokenSecret, nonce, timestamp);

			TokenPassport tokenPassport = new TokenPassport();
			tokenPassport.account = account;
			tokenPassport.consumerKey = consumerKey;
			tokenPassport.token = tokenId;
			tokenPassport.nonce = nonce;
			tokenPassport.timestamp = timestamp;
			tokenPassport.signature = signature;
			return tokenPassport;
		}

        private string ComputeNonce()
		{
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			byte[] data = new byte[20];
			rng.GetBytes(data);
			int value = Math.Abs(BitConverter.ToInt32(data, 0));
			return value.ToString();
		}

		private long ComputeTimestamp()
		{
			return ((long) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
		}

		private TokenPassportSignature ComputeSignature(string compId, string consumerKey, string consumerSecret,  
										string tokenId, string tokenSecret, string nonce, long timestamp)
		{
			string baseString = compId + "&" + consumerKey + "&" + tokenId + "&" + nonce + "&" + timestamp;
			string key = consumerSecret + "&" + tokenSecret;
			string signature = "";
			var encoding = new System.Text.ASCIIEncoding();
			byte[] keyBytes = encoding.GetBytes(key);
			byte[] baseStringBytes = encoding.GetBytes(baseString);
			using (var hmac = new HMACSHA256(keyBytes))
			{
				byte[] hashBaseString = hmac.ComputeHash(baseStringBytes);
				signature = Convert.ToBase64String(hashBaseString);
			}
			TokenPassportSignature sign = new TokenPassportSignature();
			sign.algorithm = "HMAC_SHA256";
			sign.Value = signature;
			return sign;
		}

		private ApplicationInfo CreateApplicationId()
		{
			ApplicationInfo applicationInfo = new ApplicationInfo();
			applicationInfo.applicationId = ConfigurationManager.AppSettings["login.appId"].ToString();
			return applicationInfo;
		}

		/// <summary>
		/// Use to get default values for deletion reason
		/// </summary>
		/// <returns>DeletionReason with some default values</returns>
		public DeletionReason GetDefaultDeletionReason()
		{
			DeletionReason deletionReason = new DeletionReason();
			RecordRef deletionReasonCodeRef = new RecordRef();
			deletionReasonCodeRef.internalId = "1";
			deletionReason.deletionReasonCode = deletionReasonCodeRef;
			deletionReason.deletionReasonMemo = "Deleted from Sample Apps.";
			return deletionReason;
		}
	}
	class OverrideCertificatePolicy : ICertificatePolicy
	{
		public bool CheckValidationResult(ServicePoint srvPoint, System.Security.Cryptography.X509Certificates.X509Certificate certificate, WebRequest request, int certificateProblem)
		{
			return true;
		}
	}

    /// <summary>    
	/// Since 12.2 endpoint accounts are located in multiple datacenters with different domain names.
    /// In order to use the correct one, wrap the Service and get the correct domain first.
	///
	/// See getDataCenterUrls WSDL method documentation in the Help Center.	 
    /// </summary>
    class DataCenterAwareNetSuiteService : NetSuiteService
    {

        private System.Uri OriginalUri;

        public DataCenterAwareNetSuiteService(string account, bool doNotSetUrl)
            : base()
        {
            OriginalUri = new System.Uri(this.Url);
            if (account == null || account.Length == 0)
                account = "empty";
            if (!doNotSetUrl)
            { 
                DataCenterUrls urls = getDataCenterUrls(account).dataCenterUrls;
                Uri dataCenterUri = new Uri(urls.webservicesDomain + OriginalUri.PathAndQuery);
                this.Url = dataCenterUri.ToString();
            }
        }

        public void SetAccount(string account)
        {
            if (account == null || account.Length == 0)
                account = "empty";

            this.Url = OriginalUri.AbsoluteUri;
            DataCenterUrls urls = getDataCenterUrls(account).dataCenterUrls;
            Uri dataCenterUri = new Uri(urls.webservicesDomain + OriginalUri.PathAndQuery);
            this.Url = dataCenterUri.ToString();
        }
    }

    /// <summary>
    /// Single SuiteTalk action with name and implementations
    /// </summary>
    public class SuiteTalkAction
    {
        public delegate void SuiteTalkActionDelegate();

        public String Name;
        public SuiteTalkActionDelegate Method;

        public SuiteTalkAction(String name, SuiteTalkActionDelegate method)
        {
            Method = method;
            Name = name;
        }
    }

    /// <summary>
    /// Base class of all implemented methods calling SuiteTalk 
    /// </summary>
    class NSBase
    {
        protected static NSClient _client;

        public static NSClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new NSClient();
                }
                return _client;
            }
            set
            {
                _client = value;
            }
        }
    }

}

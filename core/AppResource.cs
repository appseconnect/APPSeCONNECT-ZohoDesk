using InSync.eConnect.APPSeCONNECT.API;
using InSync.eConnect.APPSeCONNECT.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Xml.Linq;
using System.Xml.XPath;

namespace InSync.eConnect.ZohoDesk
{
    /// <summary>
    /// Declare all the major resources or functions which could be required during transformation.
    /// </summary>
    public class AppResource : IAppResource
    {
      
        private ApplicationContext _context;
        const string lastDate = "lastdate";
        private CredentialModel CredentialObject { get; set; }

        /// <summary>
        /// Default Constructor called by Agent. It will call Initialize to pass Application context
        /// </summary>
        /// <remarks>Do not use this method while creating object of AppResource inside the adapter, as you will find ApplicationContext to null</remarks>
        public AppResource() { }
        /// <summary>
        /// Parameterized constructor to pass the context
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <remarks>Use this overload to create object of AppResource class</remarks>
        public AppResource(ApplicationContext context) // additional constructor to ensure we pass context when creating AppResource object from adapter itself.
        {
            this._context = context;
        }

        # region IAppResource Implementation
        public void Initialize(ApplicationContext context)
        {
            // first step is to try and get the Credential. If succesful, we store it in object cache, so that every function does not need to get it.
            var credential = context.GetConnectionDetails<CredentialModel>();

            if (credential != null) // this indicates that credentails are already saved in configuration, and we can get its value
                this.CredentialObject = credential;
           
            //We store the context for future use.
            this._context = context;
        }

        # endregion
        public string ReadDateTime(string dateTimeFormat)
        {
            string returnDateTime = string.Empty;
            try
            {
                var currentDateTime = DateTime.Now;
                string endDateTime = currentDateTime.ToString(dateTimeFormat, CultureInfo.InvariantCulture);
                var startDateTime = _context.GetData(lastDate);
                if (string.IsNullOrEmpty(startDateTime))
                {
                    currentDateTime = currentDateTime.AddDays(-2);
                    startDateTime = currentDateTime.ToString(dateTimeFormat, CultureInfo.InvariantCulture);
                }
                else
                {
                    startDateTime = startDateTime.Replace("Z", "");
                }
                returnDateTime = startDateTime + ".000Z," + endDateTime + ".000Z";
            }
            catch (Exception ex)
            {
                
            }
            return returnDateTime;
        }

        public XPathNavigator getContactPersonCardCodeWise(XPathNavigator contact)
        {
            var returnData = new XElement("items");
            if (returnData != null)
            {
                try
                {
                    var contactPersonDetails = XElement.Parse(contact.OuterXml);
                    string prevAccntid = string.Empty;
                    List<XElement> columnss = (from t in contactPersonDetails.Elements("data").Elements("data") where t.Descendants("accountId").First().Value != null orderby t.Descendants("accountId").First().Value ascending select t).ToList();
                    var newData = new XElement("item");
                    foreach (var column in columnss)
                    {
                        var acntId = column.Descendants("accountId").First().Value;
                        if (string.IsNullOrEmpty(prevAccntid) || prevAccntid == acntId)
                        {
                            newData.Add(column);
                        }
                        else
                        {
                            returnData.Add(newData);
                            newData = new XElement("item");
                            newData.Add(column);
                        }

                        prevAccntid = acntId;

                    }
                    returnData.Add(newData);
                }
                catch (Exception ex)
                {
                    returnData.Add(new XElement("error", ex.Message));
                }
            }
            return returnData.CreateNavigator();
        }
        public XPathNavigator GetDetails(string endPoint)
        {
            XElement returnData = new XElement("items");
            try
            {
                WebUtils webUtil = new WebUtils();
                string requestedUrl = CredentialObject.BaseUrl + endPoint;
                if (string.IsNullOrEmpty(CredentialObject.AccessToken))
                    webUtil.UpdateAccessToken(CredentialObject, this._context.ApplicationUtility);
                var requestedData = webUtil.HttpGetRequest(requestedUrl, this._context.Settings, CredentialObject, this._context.ApplicationUtility, this._context.ApplicationUtility.Logger);
                if (!requestedData.Status && (requestedData.Message.Equals("401")))
                {
                    webUtil.UpdateAccessToken(CredentialObject, this._context.ApplicationUtility);
                    requestedData = webUtil.HttpGetRequest(requestedUrl, this._context.Settings, CredentialObject, this._context.ApplicationUtility, this._context.ApplicationUtility.Logger);
                }
                if (!requestedData.Status && requestedData.Value.Contains("errorCode"))
                {
                    this._context.ApplicationUtility.Logger.ErrorLog("Error in GetDetails, Status :" + requestedData.Message, requestedData.Value);
                }
                if (!string.IsNullOrEmpty(requestedData.Value))
                    returnData = XElement.Parse(requestedData.Value);
                else
                    returnData.Add(new XElement("data", ""));
            }
            catch (Exception ex)
            {
                this._context.ApplicationUtility.Logger.ErrorLog("Error in GetDetails" , ex);
            }
            return returnData.CreateNavigator();
        }

        public async void GetAttachment(string attachmentURL, string filePath, string fileName, string fileExtension)
        {
            // Create a new WebClient instance.
            try
            {
                if (!string.IsNullOrEmpty(attachmentURL))
                {
                    string localFile = filePath + fileName + @"." + fileExtension;
                    using (WebClient myWebClient = new WebClient())
                    {
                        myWebClient.Headers.Add("orgId", CredentialObject.OrganizationId);
                        myWebClient.Headers.Add("Authorization", "Zoho-oauthtoken " + CredentialObject.AccessToken);
                        myWebClient.DownloadFile(attachmentURL, localFile); 
                    }
                }
            }
            catch (WebException webEx)
            {
                var response = webEx.ReadResponse();
                this._context.ApplicationUtility.Logger.ErrorLog($"Error in  getting attachments for url {attachmentURL}", response.Value);
            }
            catch (Exception ex)
            {
                this._context.ApplicationUtility.Logger.ErrorLog($"Error in getting attachments for url {attachmentURL}",ex);
            }
        }
        //ToDo: Add all your functions here. Here are some of the simple rules on defining an APPResource function. 
        //http://support.appseconnect.com/support/solutions/articles/4000068153-adding-functions-to-appresource-

    }
}

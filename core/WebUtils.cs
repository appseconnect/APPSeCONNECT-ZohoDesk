using InSync.eConnect.APPSeCONNECT.Helpers;
using InSync.eConnect.APPSeCONNECT.Storage;
using InSync.eConnect.APPSeCONNECT.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace InSync.eConnect.ZohoDesk
{
    public class WebUtils
    {
        public ReturnMessage<string> HttpGetRequest(string requestUrl, ExecutionSettings settings, CredentialModel credentials, ApplicationUtil appUtil, Logger logger)
        {
            var retMessage = new ReturnMessage<string>();
            //Validate Credential Access Token
            if (!string.IsNullOrEmpty(credentials.AccessToken))
            {
                string responseData = string.Empty;
                HttpClient client = new HttpClient();
                try
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                    request.Headers.Add("orgId", credentials.OrganizationId);
                    request.Headers.Add("Authorization", "Zoho-oauthtoken " + credentials.AccessToken);
                    HttpResponseMessage response = client.SendAsync(request).Result;
                    responseData = response.Content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrEmpty(responseData) && ((int)response.StatusCode >= 200 && (int)response.StatusCode <= 204))
                    {
                        var jsonData = XElement.Parse(JsonConvert.DeserializeXmlNode(responseData, "data").OuterXml);
                        retMessage.Value = jsonData.ToString();
                        retMessage.Status = true;
                        retMessage.Message = ((int)response.StatusCode).ToString();
                    }
                    else if ((int)response.StatusCode == 401)
                    {
                        retMessage.Message = ((int)response.StatusCode).ToString();
                        retMessage.Value = responseData;
                    }
                    else
                    {
                        logger.StatusLog($"No data found from zoho desk for GET method, Status {response.StatusCode}", requestUrl);
                        retMessage.Message = ((int)response.StatusCode).ToString();
                        retMessage.Value = responseData;
                    }

                    return retMessage;
                }
                catch (WebException ex)
                {
                    var error = ex.ReadResponse();
                    var response = ex.Response as HttpWebResponse;
                    logger.ErrorLog($"GET operation failed for url {requestUrl}", $"WebException : Error Status : {ex.Status}, Response Status: {response.StatusCode} and Response is : {responseData}");
                    retMessage.SetError(((int)response.StatusCode).ToString());
                    retMessage.AddReturn(error.Message); 
                    return retMessage;
                }
                catch (Exception ex)
                {
                    retMessage.Value = ex.Message;
                    retMessage.Status = false;
                    return retMessage;
                }
                finally
                {
                    client.Dispose();
                }
            }
            else
            {
                retMessage.SetError("401");
            }
            return retMessage;
        }
        public string GetElementData(XElement element, string columnName, Logger logger, string defaultValue)
        {
            var requestedElement = element.Descendants(columnName).FirstOrDefault();
            if (requestedElement != null)
            {
                RemoveXMLData(element, columnName, logger);
                return requestedElement.Value;
            }
            return defaultValue;
        }
        public string GetMaxData(XElement retMessage, string columnName)
        {
            if (retMessage.Descendants(columnName).Any())
            {
                string maxdate = retMessage.Elements("data").Elements("data").OrderByDescending(x => (DateTime)x.Element(columnName))
                   .First().Descendants("createdTime").First().Value;
                return maxdate;
            }
            return string.Empty;
        }
        public ReturnMessage<string> HttpPostRequest(XElement elements, ExecutionSettings settings, string actionParams, CredentialModel credentials, ApplicationUtil appUtil, Logger logger)
        {
            var returnMessage = new ReturnMessage<string>();
            //Validate Credential Access Token
            if (!string.IsNullOrEmpty(credentials.AccessToken))
            {
                
                //Stream reader = null; 
                HttpWebRequest request = null;
                HttpWebResponse response = null;
                XmlDocument xmlDoc = new XmlDocument();

                var urlParam = this.GetElementData(elements, "UploadURL", logger, string.Empty);
                var sourceKey = this.GetElementData(elements, "SourceKey", logger, string.Empty);
                var converter = new XmlToJsonConverter(elements.ToString());
                var url = $"{credentials.BaseUrl}{urlParam}";
                var jsonData = converter.Format().Message;


                request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers.Add("orgId", credentials.OrganizationId);
                request.Headers.Add("Authorization", "Zoho-oauthtoken " + credentials.AccessToken);
                request.Method = actionParams;
                request.ContentType = "application/json";
                string actualResponse = string.Empty;
                try
                {
                    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        streamWriter.Write(jsonData);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                    using (response = (HttpWebResponse)request.GetResponse())
                    {
                        using (Stream responseStream = response.GetResponseStream())
                        {
                            using (StreamReader responseReader = new StreamReader(responseStream))
                            {
                                actualResponse = responseReader.ReadToEnd();
                                xmlDoc = JsonConvert.DeserializeXmlNode(actualResponse, "Results");
                            }
                        }
                    }
                    var xResponse = XElement.Parse(xmlDoc.InnerXml);
                    if (!string.IsNullOrEmpty(sourceKey))
                    {
                        xResponse.Add(new XElement("SourceKey", sourceKey));
                    }

                    returnMessage.Value = xResponse.ToString();
                    returnMessage.Status = true;
                }
                catch (WebException wex)
                {
                    var error = wex.ReadResponse();
                    response = wex.Response as HttpWebResponse;
                    returnMessage.SetError(((int)response.StatusCode).ToString());
                    returnMessage.AddReturn(error.Message);
                    logger.ErrorLog($"Post operation failed for url {url}", $"WebException : Status : {response.StatusCode}, Data is : {jsonData} and Response is : {actualResponse}");
                }
                catch (Exception ex)
                {
                    logger.ErrorLog($"Post operation failed for url {url}", ex);
                    returnMessage.AddException(ex);
                }

            }
            else
            {
                returnMessage.SetError("401");
            }
            return returnMessage;
        }

        private XElement RemoveXMLData(XElement elements, string removeElement, Logger logger)
        {
            try
            {
                elements.Descendants(removeElement).Remove();
            }
            catch (Exception ex)
            {
                logger.ErrorLog("Error in function RemoveXMLData : " + ex.Message);
            }
            return elements;
        }
        public string UpdateAccessToken(CredentialModel credentials, ApplicationUtil appUtil)
        {
            string accessToken = string.Empty;
            try
            {
                using (var authClient = new HttpClient())
                {
                    HttpContent content = new FormUrlEncodedContent(new Dictionary<string, string>
                      {
                         {"client_id",credentials.ClientID},
                         {"client_secret",credentials.ClientSecret},
                         {"refresh_token",credentials.RefreshToken},
                         {"grant_type","refresh_token"}
                       }
                    );
                    HttpResponseMessage message = authClient.PostAsync("https://accounts.zoho.com/oauth/v2/token", content).Result;
                    string responseString = message.Content.ReadAsStringAsync().Result;
                    JObject obj = JObject.Parse(responseString);
                    accessToken = (string)obj["access_token"];
                    credentials.AccessToken = accessToken;
                    appUtil.CredentialStore.SaveConnectionDetails(credentials);

                }
            }
            catch(WebException wex)
            {
                var response = wex.ReadResponse();
                appUtil.Logger.ErrorLog("Unable to generate Access Token", response.Message);
            }
            catch (Exception ex)
            {
                accessToken = ex.Message;
                appUtil.Logger.ErrorLog("Failed to get access token", ex);
            }
            return accessToken;
        }

    
}


public class WebExceptionResponse
{
    public string ResponseMessage { get; set; }
    public WebExceptionResponse(WebException wex)
    {
        this.ResponseMessage = wex.Message;
        this.ParseException(wex);
    }
    public string ErrorDetails { get; set; }
    private void ParseException(WebException wex)
    {
        XNamespace nsSys = "http://schemas.xmlsoap.org/soap/envelope/";
        StreamReader reader = new StreamReader(wex.Response.GetResponseStream());

        try
        {
            this.ResponseMessage = reader.ReadToEnd();
            var xResult = XElement.Parse(this.ResponseMessage);

            var descendants = xResult.Descendants("faultstring");
            var element = descendants.First();
            this.ErrorDetails = element.Value;
        }
        catch
        {
        }
    }
}

public static class WebExceptionExtension
{
    public static ReturnMessage<string> ReadResponse(this WebException webex)
    {
        ReturnMessage<string> retValue = new ReturnMessage<string>();
        retValue.AddException(webex);
        var response = webex.Response;
        if (response != null)
        {
            Stream responseStream = null;
            try
            {
                responseStream = response.GetResponseStream();
                using (var responseReader = new StreamReader(responseStream))
                {
                    var xml = responseReader.ReadToEnd();
                    //var errordata = XElement.Parse(xml);
                    retValue.Message = xml;
                }
            }
            finally
            {
                if (responseStream != null)
                    responseStream.Dispose();
            }
        }
        return retValue;
    }
}
}

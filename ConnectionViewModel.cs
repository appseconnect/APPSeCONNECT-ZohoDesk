using InSync.eConnect.APPSeCONNECT.API;
using InSync.eConnect.APPSeCONNECT.Helpers;
using InSync.eConnect.APPSeCONNECT.Storage;
using InSync.eConnect.APPSeCONNECT.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace InSync.eConnect.ZohoDesk
{
    /// <summary>
    ///  Represents the ViewModel class for Credential window for the application 
    /// </summary>
    public class ConnectionViewModel : ObservableObjectGeneric<ConnectionViewModel>, ICredential
    {
        #region Private Variables

        private CredentialModel _credentialInfo;
        ReturnMessage<bool> returnResult = new ReturnMessage<bool>();
        private ApplicationUtil _applicationUtils;
        private ICommand _validateCommand;
        private ICommand _saveCommand;
        private ICommand _authorizeCommand;
        private string _connectionStatus;
        private string _foreColor;
        private Visibility _progressbarVisibility = Visibility.Collapsed;
        private string tempToken;
        Process ieProccess = null;
        #endregion

        #region Public constructors
        public ConnectionViewModel()
        {
           
        }

        #endregion

        internal void Initialize(ApplicationUtil applicationUtility)
        {
            this._applicationUtils = applicationUtility;

            var returnDetails = applicationUtility.CredentialStore.GetConnectionDetails();

            _credentialInfo = ObjectUtils.JsonDeserialize<CredentialModel>(returnDetails.Value);
            if (_credentialInfo == null)
                this._credentialInfo = new CredentialModel();
        }

        #region Public Members
        /// <summary>
        ///  Gets the Authorize command
        /// </summary>
        /// <value>The Authorize command.</value>
        //public ICommand GetTokenCommand
        //{
        //    get
        //    {
        //        this._authorizeCommand = _authorizeCommand ?? new RelayCommand(p => GetToken(), null, false);
        //        return this._authorizeCommand;
        //    }
        //}

        /// <summary>
        ///  Gets the Validate command
        /// </summary>
        /// <value>The validate command.</value>
        public ICommand ValidateCommand
        {
            get 
            { 
                this._validateCommand = _validateCommand ??  new RelayCommand(p => Validate(), null, false);
                return _validateCommand;
            }
        }

        /// <summary>
        ///  Gets the Save command
        /// </summary>
        /// <value>The save command.</value>
        public ICommand SaveCommand
        {
            get 
            { 
                this._saveCommand = _saveCommand ?? new RelayCommand(p => Save(), null, false);
                return this._saveCommand;
            }
        }

        public string ConnectionStatus
        {
            get
            {
                return _connectionStatus;
            }
            set
            {
                this._connectionStatus = value;
                this.OnPropertyChanged("ConnectionStatus");
            }
        }

        public Visibility ProgressbarVisibility
        {
            get
            {
                return this._progressbarVisibility;
            }
            set
            {
                this._progressbarVisibility = value;

                this.OnPropertyChanged("ProgressbarVisibility");
            }
        }

        public string ForeColor
        {
            get { return _foreColor; }
            set
            {
                _foreColor = value;

                OnPropertyChanged("ForeColor");
            }
        }

        public string ClientID
        {
            get
            {
                return this._credentialInfo.ClientID;
            }
            set
            {
                if (this._credentialInfo.ClientID != value)
                {
                    this._credentialInfo.ClientID = value;
                    OnPropertyChanged("ClientID");
                    OnPropertyChanged("TButtonEnable");
                    OnPropertyChanged("ButtonEnable");
                    OnPropertyChanged("SaveEnable");
                }
            }
        }

        public string Scope
        {
            get
            {
                return this._credentialInfo.Scope;
            }
            set
            {
                this._credentialInfo.Scope = value;
                OnPropertyChanged("Scope");
                OnPropertyChanged("TButtonEnable");
                OnPropertyChanged("ButtonEnable");
                OnPropertyChanged("SaveEnable");
            }
        }

        public string ClientSecret
        {
            get
            {
                return this._credentialInfo.ClientSecret;
            }
            set
            {
                this._credentialInfo.ClientSecret = value;
                OnPropertyChanged("ClientSecret");
                OnPropertyChanged("TButtonEnable");
                OnPropertyChanged("ButtonEnable");
                OnPropertyChanged("SaveEnable");
            }
        }

        public string CallBack
        {
            get
            {
                return this._credentialInfo.CallBack;
            }
            set
            {
                if (this._credentialInfo.CallBack != value)
                {
                    this._credentialInfo.CallBack = value;
                    OnPropertyChanged("CallBack");
                    OnPropertyChanged("TButtonEnable");
                    OnPropertyChanged("ButtonEnable");
                    OnPropertyChanged("SaveEnable");
                }
            }
        }


        public string BaseUrl
        {
            get
            {
                return this._credentialInfo.BaseUrl;
            }
            set
            {
                this._credentialInfo.BaseUrl = value;
                OnPropertyChanged("BaseUrl");
                OnPropertyChanged("TButtonEnable");
                OnPropertyChanged("ButtonEnable");
                OnPropertyChanged("SaveEnable");
            }
        }
        public string AccessToken
        {
            get { return this._credentialInfo.AccessToken; }
            set
            {
                if (this._credentialInfo.AccessToken != value)
                {
                    this._credentialInfo.AccessToken = value;
                    OnPropertyChanged("AccessToken");
                }
            }
        }
        public string RefreshToken
        {
            get { return this._credentialInfo.RefreshToken; }
            set
            {
                if (this._credentialInfo.RefreshToken != value)
                {
                    this._credentialInfo.RefreshToken = value;
                    OnPropertyChanged("RefreshToken");
                }
            }
        }
        public string OrganizationId
        {
            get { return this._credentialInfo.OrganizationId; }
            set
            {
                if (this._credentialInfo.OrganizationId != value)
                {
                    this._credentialInfo.OrganizationId = value;
                    OnPropertyChanged("OrganizationId");
                }
            }
        }
        private void GetToken()
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
            {
                ConnectionStatus = "";
                ForeColor = "";
                this.ProgressbarVisibility = Visibility.Visible;
            });
            try
            {
                string authorizationUrl = string.Format("https://accounts.zoho.com/oauth/v2/auth?scope={0}&client_id={1}&response_type=code&access_type=offline&redirect_uri={2}", this._credentialInfo.Scope, this._credentialInfo.ClientID, this._credentialInfo.CallBack);
                ieProccess = Process.Start("iexplore.exe", authorizationUrl);
                this.ConnectionStatus = "Authorization completed, now validate the data";
                ForeColor = "Green";
            }
            catch (Exception ex)
            {
                this.ConnectionStatus = "Authorization failed " + ex.Message;
                ForeColor = "Red";
            }
            finally
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
                {
                    this.ProgressbarVisibility = Visibility.Collapsed;
                });
            }
            
        }


        private void Validate()
        {
            //ToDo : Validate the credentials and show message on UI.
            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
            {
                ConnectionStatus = "";
                ForeColor = "";
                this.ProgressbarVisibility = Visibility.Visible;
            });
            bool state = false;
            string error = string.Empty;
            try
            {
                this.GetToken();
                string tempToken = GetTemporaryToken();
                string accessToken = GetAccessToken(tempToken);
                if (string.IsNullOrEmpty(_credentialInfo.OrganizationId))
                {
                    string requestUrl = _credentialInfo.BaseUrl + "organizations";
                    this._credentialInfo.OrganizationId = GetOrganizationId(requestUrl);
                    this._applicationUtils.CredentialStore.SaveConnectionDetails<CredentialModel>(this._credentialInfo);
                }
                if (!string.IsNullOrEmpty(accessToken))
                    state = true;
                
            }
            catch (Exception ex)
            {
                state = false;
                error = ex.Message;
            }
            finally
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
                {
                    if (state)
                    {
                        this.ConnectionStatus = "Validation passed, now you can save these details.";
                        ForeColor = "Green";
                    }
                    else
                    {
                        this.ConnectionStatus = "Test failed for reason : " + error;
                        ForeColor = "Red";
                    }
                    this.ProgressbarVisibility = Visibility.Collapsed;
                });
                ieProccess.CloseMainWindow();
            }
        }

        private void Save()
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
            {

                this.ProgressbarVisibility = Visibility.Visible;
            });

            var saveResponse = this._applicationUtils.CredentialStore.SaveConnectionDetails<CredentialModel>(this._credentialInfo);

            if (saveResponse.Value)
            {
                ConnectionStatus = "Connection saved successfully";
                ForeColor = "Green";
            }
            else
            {
                ConnectionStatus = "Connection saving failed.";
                ForeColor = "Red";
            }

            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
            {
                this.ProgressbarVisibility = Visibility.Collapsed;
            });
        }
        
        private string GetTemporaryToken()
        {
            string filename = string.Empty;
            string tempCode = string.Empty;
            try
            {
                SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();
                foreach (SHDocVw.InternetExplorer ie in shellWindows)
                {
                    filename = Path.GetFileNameWithoutExtension(ie.FullName).ToLower();
                    if (filename.Equals("iexplore") && ie.LocationURL.Contains("code"))
                    {
                        var url = ie.LocationURL;
                        string[] urlParts = (ie.LocationURL.ToString()).Split('&');

                        foreach (string urlsPart in urlParts)
                        {
                            if (urlsPart.Contains("code"))
                            {
                                tempCode = urlsPart.Split('=')[1];
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                returnResult.SetError("Error to get temporary token " + ex.StackTrace);
            }
            return tempCode;
        }

        private string GetAccessToken(string tempToken)
        {
            string accessToken = string.Empty;
            if (!string.IsNullOrEmpty(tempToken))
            {
                try
                {
                    HttpClient authClient = new HttpClient();
                    HttpContent content = new FormUrlEncodedContent(new Dictionary<string, string>
                      {
                            {"code",tempToken},
                            {"redirect_uri",_credentialInfo.CallBack},
                            {"client_id", _credentialInfo.ClientID},
                            {"client_secret", _credentialInfo.ClientSecret},
                            {"grant_type","authorization_code"}

                       }
                    );
                    HttpResponseMessage message = authClient.PostAsync("https://accounts.zoho.com/oauth/v2/token", content).Result;
                    string responseString = message.Content.ReadAsStringAsync().Result;
                    JObject obj = JObject.Parse(responseString);
                    accessToken = (string)obj["access_token"];
                    string refreshToken = (string)obj["refresh_token"];
                    string domain = (string)obj["api_domain"];
                    this._credentialInfo.AccessToken = accessToken;
                    if (!string.IsNullOrEmpty(refreshToken) && (this._credentialInfo.RefreshToken != refreshToken))
                    {
                        this._credentialInfo.RefreshToken = refreshToken;
                    }
                    this._credentialInfo.Domain = domain;
                    //this._applicationUtils.CredentialStore.SaveConnectionDetails<CredentialModel>(this._credentialInfo);
                }
                catch (Exception ex)
                {
                    returnResult.SetError("Error to get access token " + ex.StackTrace);
                }
            }
            return accessToken;
        }

        private string GetOrganizationId(string requestUrl)
        {
            string orgId = string.Empty;
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Add("Authorization", "Zoho-oauthtoken " + _credentialInfo.AccessToken.Replace("\"", ""));
                HttpClient client = new HttpClient();
                HttpResponseMessage response = client.SendAsync(request).Result;
                var responseData = response.Content.ReadAsStringAsync().Result;
                var jsonData = XElement.Parse(JsonConvert.DeserializeXmlNode(responseData, "root").OuterXml);
                orgId = jsonData.Descendants("id").Any() ? jsonData.Descendants("id").FirstOrDefault().Value : string.Empty;
                
            }
            catch (Exception ex)
            {
                returnResult.SetError("Error to get organization id " + ex.StackTrace);
            }
            return orgId;
        }


        #endregion



        #region ICredential Implementation

        /// <summary>
        /// Validates credential for configuraion data
        /// </summary>
        /// <param name="configurationData">Represents Credential model</param>
        /// <returns></returns>
        public ReturnMessage<bool> Validate(string configurationData)
        {
            var returnResult = new ReturnMessage<bool>();
            try
            {
                this._credentialInfo = ObjectUtils.JsonDeserialize<CredentialModel>(configurationData);
                if (!string.IsNullOrWhiteSpace(_credentialInfo.AccessToken) &&
                    !string.IsNullOrWhiteSpace(_credentialInfo.RefreshToken))
                {
                    returnResult.SetSuccess("Validation successful");
                }
                else
                {
                    returnResult.SetError("Validation failed. Access and Refresh token not found");
                }
            }
            catch (Exception e)
            {
                returnResult.SetError(e.ToString());
            }

            return returnResult;
        }


        /// <summary>
        /// Generates intermediate resultset for partial data
        /// </summary>
        /// <param name="resource">A dictionay object for different level of data generation</param>
        /// <returns></returns>
        public ReturnMessage<IDictionary<string, string>> PartialValidate(IDictionary<string, string> resource)
        {
            var returnMessage = new ReturnMessage<IDictionary<string, string>>();

            //ToDo : Generate intermediate results for processing data / resultset etc. 

            try
            {
                if (resource != null)
                {
                    var credentialInfo = resource["ConfigData"];
                    if (!string.IsNullOrEmpty(credentialInfo))
                    {
                       this._credentialInfo = ObjectUtils.JsonDeserialize<CredentialModel>(credentialInfo);
                        var step = resource["Step"];
                        int stepValue = 0;
                        if (!string.IsNullOrWhiteSpace(step))
                            int.TryParse(step, out stepValue);
                        if (stepValue == 0)
                        {
                            string authorizationUrl = string.Format("https://accounts.zoho.com/oauth/v2/auth?scope={0}&client_id={1}&response_type=code&access_type=offline&redirect_uri={2}", this._credentialInfo.Scope, this._credentialInfo.ClientID, this._credentialInfo.CallBack);
                            resource["RequestUrl"] = authorizationUrl;
                            resource["OpenBrowser"] = "true";
                            resource["Step"] = (++stepValue).ToString();
                            resource["CallBack"] = this._credentialInfo.CallBack;
                            resource["ConfigData"] = ObjectUtils.JsonSerializer<CredentialModel>(this._credentialInfo);
                            returnMessage.SetSuccess("Success", resource);
                        }
                        else if (stepValue == 1)
                        {
                            var responseUrl = resource["ResponseUrl"];
                            if (!string.IsNullOrWhiteSpace(responseUrl))
                            {
                                string[] urlParts = responseUrl.Split('/');

                                foreach (string urlsPart in urlParts)
                                {
                                    if (urlsPart.Contains("code"))
                                    {
                                        this.tempToken = urlsPart.Split('=')[1];
                                    }
                                }
                                var validateResult = GetAccessToken(tempToken);
                                resource["OpenBrowser"] = "false";
                                resource["ConfigData"] = ObjectUtils.JsonSerializer<CredentialModel>(this._credentialInfo);
                                if (!string.IsNullOrEmpty(validateResult))
                                    returnMessage.SetSuccess(validateResult, resource);
                                else
                                    returnMessage.SetError(validateResult, resource);
                            }
                        }
                    }
                    else
                        returnMessage.SetError("Empty config data");

                }
                else
                    returnMessage.SetError("Resource data dictionary is empty");
            }
            catch (Exception ex)
            {
                returnMessage.SetError("Error in Partial Validate " + ex.Message);
            }
            return returnMessage;
        }
        
        #endregion
    }
}
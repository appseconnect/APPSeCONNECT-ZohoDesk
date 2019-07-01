
namespace InSync.eConnect.ZohoDesk
{
    public class CredentialModel
    {

        //Todo : Add all your credentials which you want to input from the user for this application.
         
        public string ClientID { get; set ; } 

        public string ClientSecret { get; set; }

        public string Scope { get; set; }

        public string CallBack { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public string Domain { get; set; }

        public string BaseUrl { get; set; }

        public string OrganizationId { get; set; }

    }
}

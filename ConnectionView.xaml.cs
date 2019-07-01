using InSync.eConnect.APPSeCONNECT.API;
using InSync.eConnect.APPSeCONNECT.Storage;

namespace InSync.eConnect.ZohoDesk
{
    /// <summary>
    ///     Interaction logic for ConnectionView.xaml
    /// </summary>
    public partial class ConnectionView : System.Windows.Controls.UserControl, IPageView
    {
        ConnectionViewModel viewModel = null;
        public ConnectionView()
        {
            InitializeComponent();
        }

        public string PageTitle
        {
            get 
            {
                //Todo : Change this with something you want to show as title
                return "Zoho Desk"; 
            }
        }

        public void Initialize(ApplicationUtil applicationUtility)
        {
            viewModel = viewModel ?? new ConnectionViewModel();

            viewModel.Initialize(applicationUtility);

            //This will set the current page datacontext.
            this.DataContext = viewModel;
        }
    }
}
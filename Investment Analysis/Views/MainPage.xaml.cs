using Investment_Analysis.Views;
using System;
using System.Windows;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Windows.Graphics.Printing;
using Windows.UI.Xaml.Printing;
using Windows.ApplicationModel.DataTransfer;
using Investment_Analysis.ViewModels;
using Windows.UI.Xaml.Documents;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Investment_Analysis
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainPage : Investment_Analysis.Common.LayoutAwarePage
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            AttachPrintContract();
            AttachShareContract();
            var fileToUse = App.FileExecuted;
            if (fileToUse != null)
            {
                App.FileExecuted = null; // avoids file reopening on each navigation to page
                this.OpenFile(fileToUse);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            DetachPrintContract();
            DetachShareContract();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        #region Navigation to child pages
        private void NavigateToRoiDetails(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ROIDetails));
        }

        private void NavigateToIrrDetails(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(IRRDetails));
        }

        private void NavigateToInvestmentItemsDetails(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ItemDetails), "Investment");
        }

        private void NavigateToExpensesItemsDetails(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ItemDetails), "Expenses");
        }

        private void NavigateToIncomesItemsDetails(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ItemDetails), "Incomes");
        }
        #endregion

        #region AppBar functions
        private async void AppBar_Click_Open(object sender, RoutedEventArgs e)
        {
            var filePicker = new Windows.Storage.Pickers.FileOpenPicker();
            filePicker.FileTypeFilter.Add(".roi");
            var fileToUse = await filePicker.PickSingleFileAsync();
            this.OpenFile(fileToUse);
        }

        private async void OpenFile(Windows.Storage.IStorageFile fileToUse)
        {
            if (fileToUse != null && mainPageVM != null && mainPageVM.OpenProject.CanExecute(null))
            {
                try
                {
                    var text = await Windows.Storage.FileIO.ReadTextAsync(fileToUse);
                    mainPageVM.OpenProject.Execute(text);
                }
                catch (Exception)
                {
                    new Windows.UI.Popups.MessageDialog("Could not read the selected file").ShowAsync();
                }
            }
        }

        private async void AppBar_Click_Save(object sender, RoutedEventArgs e)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.FileTypeChoices.Add(
                new KeyValuePair<string, IList<string>>("Return on investment analysis file", new string[] { ".roi" }));
            var saveFile = await savePicker.PickSaveFileAsync();

            if (saveFile != null)
            {
                var currentProject = (Investment_Analysis.Models.ProjectData)App.Current.Resources["ProjectData"];
                try
                {
                    var text = await JsonConvert.SerializeObjectAsync(currentProject);
                    await Windows.Storage.FileIO.WriteTextAsync(saveFile, text);
                }
                catch (Exception)
                {
                    new Windows.UI.Popups.MessageDialog("Could not write the selected file").ShowAsync();
                } 
            }
        }

        private void AppBar_Click_New(object sender, RoutedEventArgs e)
        {
            if (mainPageVM != null && mainPageVM.NewProject.CanExecute(null))
            {
                mainPageVM.NewProject.Execute(null);
            }
        }

        private async void AppBar_Click_Print(object sender, RoutedEventArgs e)
        {
            // Don't act when in snapped mode
            if (this.ActualWidth >= 768)
            {
                await PrintManager.ShowPrintUIAsync();
            }
        }

        private void AppBar_Click_Help(object sender, RoutedEventArgs e)
        {
            var messageDialog = new Windows.UI.Popups.MessageDialog("No Help is associated with this topic");
            messageDialog.ShowAsync(); 
        }
#endregion

        #region Print Support
        private PrintDocument printDocument = null; 
        private IPrintDocumentSource printDocumentSource = null;
        private List<UIElement> printPages = null; 

        private void AttachPrintContract()
        {
            // prepare print document and add pagination handlers 
            printDocument = new PrintDocument();
            printDocumentSource = printDocument.DocumentSource;
            printPages = new List<UIElement>();
            printDocument.Paginate += CreatePrintPages; // creates preview/print pages
            printDocument.GetPreviewPage += GetCurrentPrintPage; // provides a specified preview/print page
            printDocument.AddPages += AddPrintPages; //provides all final print pages

            // Add a handler for printing initialization.
            var printMan = PrintManager.GetForCurrentView();
            printMan.PrintTaskRequested += PrintTaskRequested;
        }

        private void DetachPrintContract()
        {
            // Remove all printing handlers
            var printMan = PrintManager.GetForCurrentView();
            printMan.PrintTaskRequested -= PrintTaskRequested;

            if (printDocument != null)
            {
                printDocument.Paginate -= CreatePrintPages;
                printDocument.GetPreviewPage -= GetCurrentPrintPage;
                printDocument.AddPages -= AddPrintPages;
            }
        }

        private void PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs e) 
        { 
            PrintTask printTask = null; 
            printTask = e.Request.CreatePrintTask("Printing Analysis", sourceRequested => 
                { 
                    // Print Task event handler is invoked when the print job is completed. 
                    printTask.Completed += async (s, args) => 
                    { 
                        // Notify the user when the print operation fails. 
                        if (args.Completion == PrintTaskCompletion.Failed) 
                        { 
                            // request an UI task from within print Task worker thread
                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => 
                            { 
                                new Windows.UI.Popups.MessageDialog("Analysis failed to print").ShowAsync();
                            }); 
                        } 
                    }; 
 
                    sourceRequested.SetSource(printDocumentSource); 
                }); 
        }

        private void CreatePrintPages(object sender, PaginateEventArgs e)
        {
            // Clear the cache of preview pages 
            printPages.Clear();
                       
            // TODO: Prepare pages
            var sample = new TextBlock();
            sample.Foreground = new SolidColorBrush(Windows.UI.Colors.Black);
            //sample.FontSize = 30;
            //sample.TextAlignment = TextAlignment.Center;
            sample.TextWrapping = TextWrapping.Wrap;
            //sample.Padding = new Thickness(40, 50, 20, 50);
            
            foreach (var runItem in this.GetPageContent())
	        {
                sample.Inlines.Add(runItem);
                sample.Inlines.Add(new LineBreak());		 
	        }

            printPages.Add(sample);

            // Report the number of preview pages created
            (sender as PrintDocument).SetPreviewPageCount(printPages.Count, PreviewPageCountType.Intermediate);
        }

        private IEnumerable<Run> GetPageContent()
        {
            var content = new List<Run>();
            content.Add(new Run());
            content.Add(new Run());
            content[0].FontSize = 20;
            content[0].Text = "Investment Analysis";

            content[1].FontSize = 24;
            content[1].Text = "Project Name: " + projectName.Text;
            var line = new Run();
            line.FontSize = 16;
            foreach (var item in projectMainValues.Children) // buttons column excluded
            {
                var text = (item as TextBlock != null) ? (item as TextBlock).Text : "";
                text += (item as TextBox != null) ? (item as TextBox).Text : "";
                if (Grid.GetColumn(item as FrameworkElement) == 0)
                {
                    content.Add(line);
                    line = new Run();
                    line.FontSize = 16;
                }

                if (text.Length > 0)
                {
                    line.Text += "\t" + text;
                }
            }
            content.Add(line);
            return content;
        }

        private void AddPrintPages(object sender, AddPagesEventArgs e)
        {
            // add all already prepared pages to print document
            for (int i = 0; i < printPages.Count; i++)
            {
                printDocument.AddPage(printPages[i]);
            }

            // Indicate that all of the print pages have been provided
            (sender as PrintDocument).AddPagesComplete();
        }

        private void GetCurrentPrintPage(object sender, GetPreviewPageEventArgs e)
        {
            // returns requested page from the list of all print pages
            (sender as PrintDocument).SetPreviewPage(e.PageNumber, printPages[e.PageNumber - 1]);
        } 
        #endregion

        #region Share Support
        private DataTransferManager dataTransferManager;

        private void AttachShareContract()
        {
            // Register the current page as a share source. 
            this.dataTransferManager = DataTransferManager.GetForCurrentView();
            this.dataTransferManager.DataRequested += 
                new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(this.OnDataRequested);
        }

        private void DetachShareContract()
        {
            // Unregister the current page as a share source. 
            this.dataTransferManager.DataRequested -= 
                new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(this.OnDataRequested);
        }

        private void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs e)
        {
            DataPackage requestData = e.Request.Data;
            requestData.Properties.Title = projectName.Text;
            requestData.Properties.Description = "Investment Analysis Shared Content"; // The description is optional. 
            //requestData.SetText("Here the project data should be placed");
            requestData.SetHtmlFormat(HtmlFormatHelper.CreateHtmlFormat(this.GetHTMLData()));

            // Check if the supplied data has at least title 
            if (String.IsNullOrEmpty(requestData.Properties.Title))
            {
                e.Request.FailWithDisplayText("Current Investment Analysis is not shareable");
            }
        }

        private string GetHTMLData()
        {
            string result = "<div><h3>" + projectName.Text + 
                "</h3><table border=\"1\" style=\"border-collapse:collapse;\">"+
                "<tr><th>Item</th><th>Unit</th><th>Value</th></tr><tr>";
            foreach (var item in projectMainValues.Children) // buttons column excluded
            {
                var text = (item as TextBlock != null) ? (item as TextBlock).Text : "";
                text += (item as TextBox != null) ? (item as TextBox).Text : "";
                if (Grid.GetRow(item as FrameworkElement) > 0 && Grid.GetColumn(item as FrameworkElement) == 0)
                {
                    result += "</tr><tr>"; 
                }

                if (text.Length > 0)
                {
                    result += "<td>" + text + "</td>";                    
                }
            }
            result += "</tr></table></div>";
            return result;
        }
        #endregion
    }
}

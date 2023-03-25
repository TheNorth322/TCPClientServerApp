namespace TCPClientApp.UI.ViewModels;

public class ListBoxItemViewModel : ViewModelBase
{
   public string Header { get; set; }

   public ListBoxItemViewModel(string header)
   {
      Header = header;
   }
}
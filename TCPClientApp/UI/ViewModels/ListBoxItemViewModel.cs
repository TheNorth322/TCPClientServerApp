namespace TCPClientApp.UI.ViewModels;

public class ListBoxItemViewModel : ViewModelBase
{
   public string Header { get; }

   public ListBoxItemViewModel(string header)
   {
      Header = header;
   }
}
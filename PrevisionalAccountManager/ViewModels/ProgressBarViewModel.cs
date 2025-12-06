namespace PrevisionalAccountManager.ViewModels;

public class ProgressBarViewModel : ViewModel, IRootViewModel
{
    public int ProgressValue {
        get;
        set {
            if ( value == field )
            {
                return;
            }
            field = value;
            OnPropertyChanged();
        }
    }

    public void Restart()
    { }
}
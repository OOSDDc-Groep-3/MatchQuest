namespace MatchQuest.App;

public partial class MainPage : ContentPage
{
    int count = 0;

    public MainPage()
    {
        InitializeComponent();
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        count++;

        // Optional: act on the specific button clicked
        if (sender is ImageButton btn)
        {
            // simple feedback
            btn.Opacity = 0.8;
        }

        SemanticScreenReader.Announce($"Clicked {count} time{(count == 1 ? "" : "s")}");
    }
}
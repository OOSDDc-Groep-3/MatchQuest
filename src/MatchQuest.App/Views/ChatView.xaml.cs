using System.Collections.Specialized;
using Microsoft.Maui.Controls;
using MatchQuest.App.ViewModels;

namespace MatchQuest.App.Views;

public partial class ChatView : ContentPage
{
    public ChatView(ChatViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        if (viewModel.Messages is INotifyCollectionChanged nc)
        {
            nc.CollectionChanged += Messages_CollectionChanged;
        }
    }

    private void Messages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // After the collection updates, scroll to the last item.
        if (MessagesCollection.ItemsSource is System.Collections.IList list && list.Count > 0)
        {
            var last = list[list.Count - 1];
            MessagesCollection.ScrollTo(last, position: ScrollToPosition.End, animate: true);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is ChatViewModel vm)
        {
            vm.Dispose();
        }
    }
}
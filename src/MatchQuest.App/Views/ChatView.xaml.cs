using System.Collections.Specialized;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using System.Threading.Tasks;
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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Kick off async scroll-to-bottom when the page appears
        _ = ScrollToBottomAsync();
    }

    private async Task ScrollToBottomAsync()
    {
        // Give layout/measure more time to complete before scrolling.
        // 150ms is more reliable than a very short delay; adjust if needed.
        await Task.Delay(150);

        if (MessagesCollection.ItemsSource is System.Collections.IList list && list.Count > 0)
        {
            var last = list[list.Count - 1];
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                MessagesCollection.ScrollTo(last, position: ScrollToPosition.End, animate: true);
            });
        }
    }

    private async void Messages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Only react to added items (avoid extra scrolling on reset/move)
        if (e?.Action != NotifyCollectionChangedAction.Add) return;

        if (MessagesCollection.ItemsSource is System.Collections.IList list && list.Count > 0)
        {
            var last = list[list.Count - 1];

            // Ensure UI had time to layout new item, then scroll
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                // increase the delay to let the CollectionView measure the new item
                await Task.Delay(150);
                MessagesCollection.ScrollTo(last, position: ScrollToPosition.End, animate: true);
            });
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
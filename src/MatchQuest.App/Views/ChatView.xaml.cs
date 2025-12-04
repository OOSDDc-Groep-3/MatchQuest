using System;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using MatchQuest.App.ViewModels;

namespace MatchQuest.App.Views;

public partial class ChatView : ContentPage
{
    // TCS used to wait until CollectionView has measured/laid out
    private TaskCompletionSource<bool>? _layoutTcs;
    private bool _collectionViewMeasured;

    public ChatView(ChatViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        if (viewModel.Messages is INotifyCollectionChanged nc)
        {
            nc.CollectionChanged += Messages_CollectionChanged;
        }

        // Track when the CollectionView is measured/layouted
        MessagesCollection.SizeChanged += MessagesCollection_SizeChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Try to scroll to bottom once page appears
        _ = ScrollToBottomAsync();
    }

    private void MessagesCollection_SizeChanged(object? sender, EventArgs e)
    {
        // Mark collection as measured once it has a height
        if (!_collectionViewMeasured && MessagesCollection.Height > 0)
        {
            _collectionViewMeasured = true;
            _layoutTcs?.TrySetResult(true);
        }
    }

    private async Task WaitForCollectionViewLayoutAsync(int timeoutMs = 1500)
    {
        if (_collectionViewMeasured) return;

        _layoutTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var cts = new CancellationTokenSource(timeoutMs);
        using var reg = cts.Token.Register(() => _layoutTcs.TrySetResult(true));
        try
        {
            await _layoutTcs.Task.ConfigureAwait(false);
        }
        finally
        {
            _layoutTcs = null;
        }
    }

    private async Task ScrollToBottomAsync()
    {
        try
        {
            // Wait until CollectionView has been measured so ScrollTo can compute correctly
            await WaitForCollectionViewLayoutAsync().ConfigureAwait(false);

            if (MessagesCollection.ItemsSource is System.Collections.IList list && list.Count > 0)
            {
                var lastIndex = list.Count - 1;
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    // scroll by index is more reliable than ScrollTo(item) when using virtualization
                    MessagesCollection.ScrollTo(lastIndex, position: ScrollToPosition.End, animate: true);
                }).ConfigureAwait(false);
            }
        }
        catch
        {
            // ignore layout/scroll errors silently (optional: log)
        }
    }

    private void Messages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Only react for adds or reset (new data)
        if (e == null) return;
        if (e.Action != NotifyCollectionChangedAction.Add && e.Action != NotifyCollectionChangedAction.Reset) return;

        // Schedule a scroll on the UI thread after a short delay so the new item can be measured
        _ = MainThread.InvokeOnMainThreadAsync(async () =>
        {
            // small delay helps when many items are added quickly or virtualization is working
            await Task.Delay(120).ConfigureAwait(false);
            await ScrollToBottomAsync().ConfigureAwait(false);
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is ChatViewModel vm)
        {
            vm.Dispose();
        }

        // detach handlers
        MessagesCollection.SizeChanged -= MessagesCollection_SizeChanged;
        if (BindingContext is ChatViewModel viewModel && viewModel.Messages is INotifyCollectionChanged nc)
        {
            nc.CollectionChanged -= Messages_CollectionChanged;
        }
    }
}
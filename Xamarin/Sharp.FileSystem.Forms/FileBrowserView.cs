using Sharp.FileSystem.Forms.ViewModels;
using Sharp.FileSystems.Abstractions;
using System;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sharp.FileSystem.Forms
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FileBrowserView : ContentView
    {
        private bool _isDirectoryChanging;

        public FileBrowserView()
        {
            RefreshCommand = new Command(OnRefreshing);
        }

        public Color TextColor
        {
            get { return (Color)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        public static readonly BindableProperty TextColorProperty = 
            BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(FileBrowserView), Color.Default);

        public ICommand RefreshCommand
        {
            get { return (ICommand)GetValue(RefreshCommandProperty); }
            set { SetValue(RefreshCommandProperty, value); }
        }

        public static readonly BindableProperty RefreshCommandProperty =
            BindableProperty.Create(nameof(RefreshCommand), typeof(ICommand), typeof(FileBrowserView));


        public ICommand OpenFileCommand
        {
            get { return (ICommand)GetValue(OpenFileCommandProperty); }
            set { SetValue(OpenFileCommandProperty, value); }
        }

        public static readonly BindableProperty OpenFileCommandProperty =
            BindableProperty.Create(nameof(OpenFileCommand), typeof(ICommand), typeof(FileBrowserView));

        public object OpenFileCommandParameter
        {
            get { return (object)GetValue(OpenFileCommandParameterProperty); }
            set { SetValue(OpenFileCommandParameterProperty, value); }
        }

        public static readonly BindableProperty OpenFileCommandParameterProperty =
            BindableProperty.Create(nameof(OpenFileCommand), typeof(object), typeof(FileBrowserView));

        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }

        public static readonly BindableProperty IsBusyProperty =
                BindableProperty.Create(nameof(IsBusy), typeof(bool), typeof(FileBrowserView), false);

        public bool IsRefreshing
        {
            get { return (bool)GetValue(IsRefreshingProperty); }
            set { SetValue(IsRefreshingProperty, value); }
        }

        public static readonly BindableProperty IsRefreshingProperty =
                BindableProperty.Create(nameof(IsRefreshing), typeof(bool), typeof(FileBrowserView), false);

        
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly BindableProperty SelectedItemProperty =
                BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(FileBrowserView), null, BindingMode.TwoWay,
                    propertyChanged: OnSelectedItemChanged);

        private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((FileBrowserView)bindable).OnItemSelected(newValue);
        }

        public IFileBrowserDirectoryAdapter Adapter
        {
            get { return (IFileBrowserDirectoryAdapter)GetValue(AdapterProperty); }
            set { SetValue(AdapterProperty, value); }
        }

        public static readonly BindableProperty AdapterProperty =
                BindableProperty.Create(nameof(Adapter), typeof(IFileBrowserDirectoryAdapter), typeof(FileBrowserView), null);
    

        private async void OnRefreshing()
        {
            if (_isDirectoryChanging || Adapter == null)
            {
                return;
            }
            try
            {
                await Adapter.RefreshAsync();
            }
            catch (Exception ex)
            {
                await Adapter.OnErrorAsync(ex);
            }
            IsRefreshing = false;
        }

        private bool _isSelecting;
        private async void OnItemSelected(object selectedItem)
        {
            if (_isSelecting) return; // still selecting something

            try
            {
                _isSelecting = true;
                if (selectedItem is FileItem fileItem)
                {
                    if (OpenFileCommand?.CanExecute(OpenFileCommandParameter) ?? false)
                    {
                        OpenFileCommand.Execute(OpenFileCommandParameter);
                    }
                }
                else
                {
                    IsBusy = true;
                    await Adapter.OpenItemAsync(selectedItem as FileSystemItemBase);
                }
            }
            catch (Exception ex)
            {
                await Adapter.OnErrorAsync(ex);
            }
            finally
            {
                _isSelecting = false;
                IsBusy = false;
            }
        }

    }
}
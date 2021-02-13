using Sharp.FileSystem.Forms.ViewModels;
using Sharp.FileSystems.Abstractions;
using System;
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
        }

        public Color TextColor
        {
            get { return (Color)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        public static readonly BindableProperty TextColorProperty = 
            BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(FileBrowserView), Color.Default);


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
                BindableProperty.Create(nameof(IsRefreshing), typeof(bool), typeof(FileBrowserView), false, BindingMode.TwoWay,
                    propertyChanged: OnRefreshingChanged);

        private static void OnRefreshingChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if ((bool)newValue)
            {
                ((FileBrowserView)bindable).OnRefreshing(bindable, EventArgs.Empty);
            }
        }

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
       
        public IDirectoryInfo Directory
        {
            get { return (IDirectoryInfo)GetValue(DirectoryProperty); }
            set { SetValue(DirectoryProperty, value); }
        }

        public static readonly BindableProperty DirectoryProperty =
                BindableProperty.Create(nameof(Directory), typeof(IDirectoryInfo), typeof(FileBrowserView), null,
                    BindingMode.TwoWay, propertyChanged: OnDirectoryChanged);

        private static void OnDirectoryChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((FileBrowserView)bindable).OnDirectoryChanged((IDirectoryInfo)newValue);
        }

        private async void OnDirectoryChanged(IDirectoryInfo directoryInfo)
        {
            _isDirectoryChanging = true;
            try
            {
                IsBusy = true;
                await Adapter.SetDirectoryAsync(directoryInfo);
            }
            catch (Exception ex)
            {
                await Adapter.OnErrorAsync(ex);
            }
            finally
            {
                IsBusy = false;
                _isDirectoryChanging = false;
            }
        }

        private async void OnRefreshing(object sender, EventArgs e)
        {
            if (_isDirectoryChanging || Adapter == null)
            {
                return;
            }
            try
            {
                await Adapter.SetDirectoryAsync(Directory);
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
                IsBusy = true;
                await Adapter.OpenItemAsync(selectedItem as FileSystemItem);
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
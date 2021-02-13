using Sharp.FileSystems.Abstractions;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FileBrowser.FileSystem
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FileBrowserView : ContentView
    {        
        private bool _isDirectoryChanging;

        public FileBrowserView()
        {
            InitializeComponent();            
            RefreshView.Refreshing += OnRefreshing;
            Files.SelectionChanged += OnItemSelected;
        }

        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }

        public static readonly BindableProperty IsBusyProperty =
                BindableProperty.Create(nameof(IsBusy), typeof(bool), typeof(FileBrowserView), false);


        public IFileBrowserDirectoryAdapter Adapter
        {
            get { return (IFileBrowserDirectoryAdapter)GetValue(AdapterProperty); }
            set { SetValue(AdapterProperty, value); }
        }

        public static readonly BindableProperty AdapterProperty =
                BindableProperty.Create(nameof(Adapter), typeof(IFileBrowserDirectoryAdapter), typeof(FileBrowserView), null/*, propertyChanged: OnAdapterChanged*/);
        /*
        private static void OnAdapterChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((FileBrowserView)bindable).OnAdapterChanged((FileBrowserDirectoryAdapter)newValue);
        }

        private void OnAdapterChanged(FileBrowserDirectoryAdapter adapter)
        {
            Files.SetBinding(ItemsView.ItemsSourceProperty, new Binding(nameof(FileBrowserDirectoryAdapter.Items)) { Source = adapter });
        }*/

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
            if(_isDirectoryChanging || Adapter == null)
            {
                return;
            }
            try
            {
                await Adapter.SetDirectoryAsync(Directory);
            }
            catch(Exception ex)
            {
                await Adapter.OnErrorAsync(ex);
            }
            RefreshView.IsRefreshing = false;
        }

        private bool _isSelecting;
        private async void OnItemSelected(object sender, SelectionChangedEventArgs e)
        {
            if (_isSelecting) return; // still selecting something

            try
            {
                _isSelecting = true;
                IsBusy = true;
                var selection = e.CurrentSelection.FirstOrDefault();
                await Adapter.OpenItemAsync(selection as FileSystemItem);
            }
            catch(Exception ex)
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
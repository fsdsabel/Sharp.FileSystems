using Prism.Commands;
using Sharp.FileSystems.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FileBrowser.FileSystem
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FileBrowserView : ContentView
    {
        private FileBrowserDirectoryAdapter _directoryAdapter = new FileBrowserDirectoryAdapter();
        private bool _isDirectoryChanging;

        public FileBrowserView()
        {
            InitializeComponent();
            Files.SetBinding(CollectionView.ItemsSourceProperty, new Binding(nameof(FileBrowserDirectoryAdapter.Items)) { Source = _directoryAdapter });
            RefreshView.Refreshing += OnRefreshing;
            Files.SelectionChanged += OnItemSelected;
        }
               

        public IDirectoryInfo Directory
        {
            get { return (IDirectoryInfo)GetValue(DirectoryProperty); }
            set { SetValue(DirectoryProperty, value); }
        }

        public static readonly BindableProperty DirectoryProperty =
                BindableProperty.Create(nameof(Directory), typeof(IDirectoryInfo), typeof(FileBrowserView), null, propertyChanged: OnDirectoryChanged);

        private static void OnDirectoryChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((FileBrowserView)bindable).OnDirectoryChanged((IDirectoryInfo)newValue);
        }

        private async void OnDirectoryChanged(IDirectoryInfo directoryInfo)
        {
            _isDirectoryChanging = true;
            RefreshView.IsRefreshing = true;
            try
            {
                await _directoryAdapter.SetDirectoryAsync(directoryInfo);
            }
            catch
            {
                // TODO
            }
            RefreshView.IsRefreshing = false;
            _isDirectoryChanging = false;
        }

        private async void OnRefreshing(object sender, EventArgs e)
        {
            if(_isDirectoryChanging)
            {
                return;
            }
            try
            {
                await _directoryAdapter.SetDirectoryAsync(Directory);
            }
            catch
            {
                // TODO
            }
            RefreshView.IsRefreshing = false;
        }


        private async void OnItemSelected(object sender, SelectionChangedEventArgs e)
        {
            var selection = e.CurrentSelection.FirstOrDefault();
            if (selection is DirectoryItem directoryItem)
            {
                Directory = directoryItem.DirInfo;
            }
        }

    }
}
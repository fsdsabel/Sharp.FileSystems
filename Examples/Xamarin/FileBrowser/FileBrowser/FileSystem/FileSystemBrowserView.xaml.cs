using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FileBrowser.FileSystem
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FileSystemBrowserView : ContentView
    {
        public FileSystemBrowserView()
        {
            InitializeComponent();
        }

        public FileSystemBrowserAdapter Adapter
        {
            get { return (FileSystemBrowserAdapter)GetValue(AdapterProperty); }
            set { SetValue(AdapterProperty, value); }
        }

        public static readonly BindableProperty AdapterProperty =
                BindableProperty.Create(nameof(Adapter), typeof(FileSystemBrowserAdapter), typeof(FileSystemBrowserView), null, propertyChanged: OnDirectoryChanged);


    }
}
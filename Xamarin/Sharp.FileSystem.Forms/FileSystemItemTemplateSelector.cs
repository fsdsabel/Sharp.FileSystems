using Sharp.FileSystem.Forms.ViewModels;
using Xamarin.Forms;

namespace Sharp.FileSystem.Forms
{
    class FileSystemItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FileTemplate { get; set; }
        public DataTemplate DirectoryTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is FileItem) return FileTemplate;
            return DirectoryTemplate;
        }
    }
}
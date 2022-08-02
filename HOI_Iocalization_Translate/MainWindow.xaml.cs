using HOI_Iocalization_Translate.Translation.Baidu;
using HOI_Iocalization_Translate.Translation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using HOI_Iocalization_Translate.Translate;

namespace HOI_Iocalization_Translate
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private string _modFolderPath;
        private readonly BaiduTranslationApi _api = new BaiduTranslationApi(LocalData.ID, LocalData.KEY);
        private List<TextData> _textDataList;

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }

            _modFolderPath = dialog.SelectedPath;
            var path = GetLocalisationFolderPath(_modFolderPath);
            DirectoryInfo dir = new DirectoryInfo(path);
            List<FileInfo[]> fileInfos = new List<FileInfo[]>
            {
                dir.GetFiles()
            };
            //所有子文件夹的所有本地化文件
            foreach (var d in dir.GetDirectories())
            {
                fileInfos.Add(d.GetFiles());
            }

            var textDataList = new List<TextData>();
            foreach (FileInfo[] files in fileInfos)
            {
                foreach (var file in files)
                {
                    textDataList.Add(new TextData(file));
                }
            }
            _textDataList = textDataList;
        }

        private static string GetLocalisationFolderPath(string modFolderPath)
        {
            return Path.Combine(modFolderPath, "localisation");
        }

       

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            TextBlack.Text = _api.GetTranslate("good\napple\nbad", "auto", "zh");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var data in _textDataList)
            {
                data.TranslateText(_api, "en");
                Console.WriteLine($"{data.FileName} 完成");
            }
        }
    }
}

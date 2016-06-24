using GSharp.Builder.Functions;
using GSharp.Compile;
using GSharp.Extension;
using GSharp.Manager;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace GSharp.Builder
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 객체
        WindowPreview preview;
        ExtensionManager manager;
        GExtension extension;
        GCompiler compiler;
        #endregion

        #region 내부 함수
        private void CreateZIP(string TargetFolder, string OutputFileName, int CompressLevel = 3)
        {
            FileStream fsOut = File.Create(OutputFileName);
            ZipOutputStream zipStream = new ZipOutputStream(fsOut);

            zipStream.SetLevel(CompressLevel);
            string FolderName = TargetFolder;
            int folderOffset = FolderName.Length + (FolderName.EndsWith("\\") ? 0 : 1);

            CompressFolder(FolderName, zipStream, folderOffset);

            zipStream.IsStreamOwner = true;
            zipStream.Close();
        }

        private void CompressFolder(string path, ZipOutputStream zipStream, int folderOffset)
        {
            string[] files = Directory.GetFiles(path);

            foreach (string filename in files)
            {
                FileInfo fi = new FileInfo(filename);

                string entryName = filename.Substring(folderOffset);
                entryName = ZipEntry.CleanName(entryName);
                ZipEntry newEntry = new ZipEntry(entryName);
                newEntry.DateTime = fi.LastWriteTime;
                newEntry.Size = fi.Length;

                zipStream.PutNextEntry(newEntry);

                byte[] buffer = new byte[4096];
                using (FileStream streamReader = File.OpenRead(filename))
                {
                    StreamUtils.Copy(streamReader, zipStream, buffer);
                }
                zipStream.CloseEntry();
            }
            string[] folders = Directory.GetDirectories(path);
            foreach (string folder in folders)
            {
                CompressFolder(folder, zipStream, folderOffset);
            }
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);

            BtnCancel.Click += BtnCancel_Click;
            BtnPreview.Click += BtnPreview_Click;
            BtnCreate.Click += BtnCreate_Click;
            StackOpen.MouseLeftButtonUp += StackOpen_MouseLeftButtonUp;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            TextTitle.Clear();
            TextAuthor.Clear();
            TextSummary.Clear();

            preview = null;
            manager = null;
            extension = null;
            compiler = null;

            StackResult.Visibility = Visibility.Collapsed;
            StackOpen.Visibility = Visibility.Visible;
        }

        private void BtnPreview_Click(object sender, RoutedEventArgs e)
        {
            preview = new WindowPreview(manager.ConvertToBlocks(extension), compiler.References)
            {
                Owner = this
            };
            preview.ShowDialog();
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "GSharp 확장 모듈 (*.gsx)|*.gsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    string moduleName = Path.GetFileName(extension.Path);
                    string tempResultPath = Path.Combine(Path.GetTempPath(), $"{moduleName}_{DateTime.Now.Millisecond}");

                    Directory.CreateDirectory(tempResultPath);
                    File.Copy(extension.Path, Path.Combine(tempResultPath, moduleName), true);

                    foreach (string dll in compiler.References)
                    {
                        if (File.Exists(dll))
                        {
                            File.Copy(dll, Path.Combine(tempResultPath, Path.GetFileName(dll)), true);
                        }
                    }

                    INI ini = new INI(Path.Combine(tempResultPath, $"{Path.GetFileNameWithoutExtension(moduleName)}.ini"));
                    ini.SetValue("General", "Title", TextTitle.Text);
                    ini.SetValue("General", "Author", TextAuthor.Text);
                    ini.SetValue("General", "Summary", TextSummary.Text);
                    ini.SetValue("Assembly", "File", $@"<%LOCAL%>\{moduleName}");
                    CreateZIP(tempResultPath, saveDialog.FileName);

                    MessageBox.Show("성공적으로 확장 모듈을 만들었습니다.", "성공", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("확장 모듈 만들기를 실패하였습니다.\n" + ex.Message, "실패", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void StackOpen_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog
            {
                Filter = "라이브러리 파일 (*.dll)|*.dll"
            };

            if (openDialog.ShowDialog() == true)
            {
                if (File.Exists(openDialog.FileName))
                {
                    try
                    {
                        manager = new ExtensionManager();
                        extension = manager.LoadExtension(openDialog.FileName);
                        if (extension.Commands.Count > 0 || extension.Controls.Count > 0)
                        {
                            extension.Path = openDialog.FileName;

                            StackOpen.Visibility = Visibility.Collapsed;
                            StackResult.Visibility = Visibility.Visible;
                            LabelResult.Content = $"본 모듈은 {extension.Commands.Count}개의 블럭과 {extension.Controls.Count}개의 컨트롤을 포함하고 있습니다.";

                            compiler = new GCompiler();
                            compiler.LoadReference(openDialog.FileName);
                        }
                        else
                        {
                            MessageBox.Show("해당 파일에는 사용 가능한 블럭이 없습니다.\n올바른 확장 파일을 선택해주시기 바랍니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("해당 파일은 불러올 수 없습니다.\n" + ex.Message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}

using GSharp.Builder.Utilities;
using GSharp.Compile;
using GSharp.Extension;
using GSharp.Manager;
using GSharp.Packager;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GSharp.Builder
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 객체
        private WindowPreview preview;
        private ExtensionManager manager;
        private GExtension extension;
        private GCompiler compiler;
        #endregion

        #region 생성자
        public MainWindow()
        {
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);

            BtnCancel.Click += BtnCancel_Click;
            BtnPreview.Click += BtnPreview_Click;
            BtnCreate.Click += BtnCreate_Click;
            StackOpen.MouseLeftButtonUp += StackOpen_MouseLeftButtonUp;
        }
        #endregion

        #region 버튼 이벤트
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
            var saveDialog = new SaveFileDialog
            {
                Filter = "GSharp 확장 모듈 (*.gsx)|*.gsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    string title = TextTitle.Text;
                    string author = TextAuthor.Text;
                    string summary = TextSummary.Text;
                    GridLoading.Visibility = Visibility.Visible;

                    Task.Run(() =>
                    {
                        PackageUtility.Create(extension.Path, title, author, summary, saveDialog.FileName, compiler);

                        Dispatcher.Invoke(() => GridLoading.Visibility = Visibility.Collapsed);
                        MessageBox.Show("성공적으로 확장 모듈을 만들었습니다.", "성공", MessageBoxButton.OK, MessageBoxImage.Information);
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("확장 모듈 만들기를 실패하였습니다.\n" + ex.Message, "실패", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void StackOpen_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "라이브러리 파일 (*.dll)|*.dll"
            };

            if (openDialog.ShowDialog() == true)
            {
                if (File.Exists(openDialog.FileName))
                {
                    try
                    {
                        GridLoading.Visibility = Visibility.Visible;

                        Task.Run(() =>
                        {
                            manager = new ExtensionManager();
                            extension = manager.LoadExtension(openDialog.FileName);
                            if (extension.Commands.Count > 0 || extension.Controls.Count > 0)
                            {
                                extension.Path = openDialog.FileName;

                                Dispatcher.Invoke(() =>
                                {
                                    StackOpen.Visibility = Visibility.Collapsed;
                                    StackResult.Visibility = Visibility.Visible;
                                    LabelResult.Content = $"본 모듈은 {extension.Commands.Count}개의 블럭과 {extension.Controls.Count}개의 컨트롤을 포함하고 있습니다.";
                                    GridLoading.Visibility = Visibility.Collapsed;
                                });

                                compiler = new GCompiler();
                                compiler.LoadReference(openDialog.FileName);
                            }
                            else
                            {
                                MessageBox.Show("해당 파일에는 사용 가능한 블럭이 없습니다.\n올바른 확장 파일을 선택해주시기 바랍니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("해당 파일은 불러올 수 없습니다.\n" + ex.Message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        #endregion
    }
}

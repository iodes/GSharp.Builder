using GSharp.Extension;
using GSharp.Graphic.Blocks;
using GSharp.Manager;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using System.Windows.Shapes;

namespace GSharp.Builder
{
    /// <summary>
    /// WindowPreview.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WindowPreview : Window
    {
        public WindowPreview(BaseBlock[] blocks, StringCollection references)
        {
            InitializeComponent();
            
            foreach (BaseBlock block in blocks)
            {
                ListBlocks.Items.Add(block);
            }

            foreach (string reference in references)
            {
                ListReferences.Items.Add(System.IO.Path.GetFileName(reference));
            }
        }
    }
}

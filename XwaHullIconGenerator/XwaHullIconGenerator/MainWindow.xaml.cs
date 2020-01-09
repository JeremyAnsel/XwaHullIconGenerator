using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace XwaHullIconGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        public string FileName
        {
            get { return (string)GetValue(FileNameProperty); }
            set { SetValue(FileNameProperty, value); }
        }

        public int IconSize
        {
            get { return (int)GetValue(IconSizeProperty); }
            set { SetValue(IconSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Filename.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register("FileName", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));

        // Using a DependencyProperty as the backing store for IconSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.Register("IconSize", typeof(int), typeof(MainWindow), new PropertyMetadata(256));

        private string GetOpenOptFileName()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".opt",
                CheckFileExists = true,
                Filter = "OPT files|*.opt"
            };

            if (dialog.ShowDialog(this) == true)
            {
                return dialog.FileName;
            }
            else
            {
                return null;
            }
        }

        private string GetSaveImageFileName(string defaultFileName = null)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                DefaultExt = ".png",
                Filter = "PNG files|*.png"
            };

            if (!string.IsNullOrEmpty(defaultFileName))
            {
                dialog.InitialDirectory = System.IO.Path.GetDirectoryName(defaultFileName);
                dialog.FileName = System.IO.Path.GetFileName(defaultFileName);
            }

            if (dialog.ShowDialog(this) == true)
            {
                return dialog.FileName;
            }
            else
            {
                return null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Ne pas intercepter les types d'exception générale", Justification = "Justified.")]
        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            string fileName = GetOpenOptFileName();

            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            this.FileName = fileName;

            try
            {
                this.UpdateIcons();
            }
            catch (Exception ex)
            {
                this.FileName = null;
                MessageBox.Show(this, ex.ToString(), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Ne pas intercepter les types d'exception générale", Justification = "Justified.")]
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.UpdateIcons();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateIcons()
        {
            if (string.IsNullOrEmpty(this.FileName))
            {
                return;
            }

            this.TopColorImage.Source = CreateBitmapFromHullIcon(RenderFace.Top, RenderMethod.Color);
            this.TopGrayScaleLightImage.Source = CreateBitmapFromHullIcon(RenderFace.Top, RenderMethod.GrayScaleLight);
            this.TopGrayScaleDarkImage.Source = CreateBitmapFromHullIcon(RenderFace.Top, RenderMethod.GrayScaleDark);
            this.TopBlueImage.Source = CreateBitmapFromHullIcon(RenderFace.Top, RenderMethod.Blue);

            this.FrontColorImage.Source = CreateBitmapFromHullIcon(RenderFace.Front, RenderMethod.Color);
            this.FrontGrayScaleLightImage.Source = CreateBitmapFromHullIcon(RenderFace.Front, RenderMethod.GrayScaleLight);
            this.FrontGrayScaleDarkImage.Source = CreateBitmapFromHullIcon(RenderFace.Front, RenderMethod.GrayScaleDark);
            this.FrontBlueImage.Source = CreateBitmapFromHullIcon(RenderFace.Front, RenderMethod.Blue);

            this.SideColorImage.Source = CreateBitmapFromHullIcon(RenderFace.Side, RenderMethod.Color);
            this.SideGrayScaleLightImage.Source = CreateBitmapFromHullIcon(RenderFace.Side, RenderMethod.GrayScaleLight);
            this.SideGrayScaleDarkImage.Source = CreateBitmapFromHullIcon(RenderFace.Side, RenderMethod.GrayScaleDark);
            this.SideBlueImage.Source = CreateBitmapFromHullIcon(RenderFace.Side, RenderMethod.Blue);
        }

        private BitmapSource CreateBitmapFromHullIcon(RenderFace side, RenderMethod method)
        {
            int size = this.IconSize;
            byte[] buffer = HullIconGenerator.Generate((uint)size, (uint)size, this.FileName, side, method);
            return BitmapSource.Create(size, size, 96, 96, PixelFormats.Bgra32, null, buffer, size * 4);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Ne pas intercepter les types d'exception générale", Justification = "Justified.")]
        private void SaveImage(RenderFace side, RenderMethod method)
        {
            if (string.IsNullOrEmpty(this.FileName))
            {
                return;
            }

            try
            {
                string fileName = GetSaveImageFileName(System.IO.Path.ChangeExtension(this.FileName, ".png"));

                if (string.IsNullOrEmpty(fileName))
                {
                    return;
                }

                int size = this.IconSize;
                byte[] buffer = HullIconGenerator.Generate((uint)size, (uint)size, this.FileName, side, method);

                BitmapHelpers.SaveBitmap(fileName, buffer, size, size);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TopColorImageSaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveImage(RenderFace.Top, RenderMethod.Color);
        }

        private void TopGrayScaleLightImageSaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveImage(RenderFace.Top, RenderMethod.GrayScaleLight);
        }

        private void TopGrayScaleDarkImageSaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveImage(RenderFace.Top, RenderMethod.GrayScaleDark);
        }

        private void TopBlueImageSaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveImage(RenderFace.Top, RenderMethod.Blue);
        }

        private void FrontColorImageSaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveImage(RenderFace.Front, RenderMethod.Color);
        }

        private void FrontGrayScaleLightImageSaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveImage(RenderFace.Front, RenderMethod.GrayScaleLight);
        }

        private void FrontGrayScaleDarkImageSaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveImage(RenderFace.Front, RenderMethod.GrayScaleDark);
        }

        private void FrontBlueImageSaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveImage(RenderFace.Front, RenderMethod.Blue);
        }

        private void SideColorImageSaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveImage(RenderFace.Side, RenderMethod.Color);
        }

        private void SideGrayScaleLightImageSaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveImage(RenderFace.Side, RenderMethod.GrayScaleLight);
        }

        private void SideGrayScaleDarkImageSaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveImage(RenderFace.Side, RenderMethod.GrayScaleDark);
        }

        private void SideBlueImageSaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveImage(RenderFace.Side, RenderMethod.Blue);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Ne pas intercepter les types d'exception générale", Justification = "Justified.")]
        private void SaveAllIconsButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.FileName))
            {
                return;
            }

            try
            {
                string fileName = GetSaveImageFileName(System.IO.Path.ChangeExtension(this.FileName, ".png"));

                if (string.IsNullOrEmpty(fileName))
                {
                    return;
                }

                int size = this.IconSize;
                var buffers = new List<byte[]>();

                foreach (RenderFace side in Enum.GetValues(typeof(RenderFace)))
                {
                    foreach (RenderMethod method in Enum.GetValues(typeof(RenderMethod)))
                    {
                        buffers.Add(HullIconGenerator.Generate((uint)size, (uint)size, this.FileName, side, method));
                    }
                }

                int itemsPerRow = Enum.GetValues(typeof(RenderMethod)).Length;
                BitmapHelpers.SaveBuffersBitmap(fileName, size, size, itemsPerRow, buffers);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

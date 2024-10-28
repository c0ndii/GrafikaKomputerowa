using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WczytywaczObrazow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TranslateTransform imageTranslation;
        private ScaleTransform imageScaling;
        private Point mousePos;

        public MainWindow()
        {
            InitializeComponent();
            HandleImageTransformation();
        }

        private void HandleImageTransformation()
        {
            TransformGroup transformGroup = new TransformGroup();
            imageScaling = new ScaleTransform();
            imageTranslation = new TranslateTransform();
            transformGroup.Children.Add(imageScaling);
            transformGroup.Children.Add(imageTranslation);
            loadedImage.RenderTransform = transformGroup;

            loadedImage.PreviewMouseWheel += (sender, e) =>
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    double scaleChange = e.Delta > 0 ? 1.1 : 0.9;
                    imageScaling.ScaleX *= scaleChange;
                    imageScaling.ScaleY *= scaleChange;
                    e.Handled = true;
                }
            };

            loadedImage.PreviewMouseLeftButtonDown += (sender, e) =>
            {
                mousePos = e.GetPosition(loadedImage);
                loadedImage.CaptureMouse();
            };

            loadedImage.PreviewMouseLeftButtonUp += (sender, e) =>
            {
                loadedImage.ReleaseMouseCapture();
            };

            loadedImage.PreviewMouseMove += (sender, e) =>
            {
                if (loadedImage.IsMouseCaptured)
                {
                    Point newPosition = e.GetPosition(loadedImage);
                    if (imageScaling.ScaleX > 1 && imageScaling.ScaleY > 1)
                    {
                        double deltaX = newPosition.X - mousePos.X;
                        double deltaY = newPosition.Y - mousePos.Y;
                        mousePos = newPosition;
                        imageTranslation.X += deltaX;
                        imageTranslation.Y += deltaY;

                    }

                }
            };

            loadedImage.MouseMove += ReadCoordsAndColor;
        }

        private void LoadImage(object sender,  RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Pliki PPM|*.ppm|Pliki JPEG|*.jpg;*.jpeg";

            if (openFileDialog.ShowDialog() == true)
            {
                if (openFileDialog.FileName.EndsWith(".ppm", StringComparison.OrdinalIgnoreCase))
                {
                    var streamReader = new StreamReader(new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read));
                    string fileFormat = streamReader.ReadLine().Trim();

                    if (string.Compare(fileFormat, "P3") == 0)
                    {
                        LoadPP3(openFileDialog.FileName);
                    }
                    else 
                        if (string.Compare(fileFormat, "P6") == 0)
                        {
                            LoadPP6(openFileDialog.FileName);
                        }
                        else
                        {
                            MessageBox.Show("Ten format pliku nie jest obsługiwany");
                        }
                }
                else 
                    if (openFileDialog.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) 
                        || openFileDialog.FileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                    {
                        LoadJpg(openFileDialog.FileName);
                    }
                    else
                    {
                        MessageBox.Show("Nieobsługiwany format pliku.");
                    }
            }
        }

        private void SaveImage(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Pliki JPEG|*.jpg";

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                try
                {
                    BitmapSource bitmapSource = (BitmapSource)loadedImage.Source;
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    int.TryParse(quality.Text, out int Quality);
                    encoder.QualityLevel = Quality != 0 ? Quality : 99;
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        encoder.Save(stream);
                    }
                    MessageBox.Show("Zapisano obraz");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Nie udało się zapisać obrazu");
                }
            }
        }

        private void LoadPP3(string filePath)
        {
            try
            {
                using (StreamReader reader = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read)))
                {
                    string format = reader.ReadLine();

                    int width = 0;
                    int height = 0;
                    int maxValue = 0;
                    string dimensionsLine;
                    string tmp = string.Empty;

                    while ((dimensionsLine = reader.ReadLine()) != null)
                    {
                        int commentIndex = dimensionsLine.IndexOf('#');
                        if (commentIndex >= 0)
                        {
                            dimensionsLine = dimensionsLine.Substring(0, commentIndex);
                        }

                        string[] tokens = dimensionsLine.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string token in tokens)
                        {
                            if (int.TryParse(token, out int value))
                            {
                                if (width == 0)
                                {
                                    width = value;
                                }
                                else if (height == 0)
                                {
                                    height = value;
                                }
                                else if (maxValue == 0)
                                {
                                    maxValue = value;
                                }
                                else
                                {
                                    tmp += token + '\n';
                                }
                            }
                        }

                        if (width > 0 && height > 0 && maxValue > 0)
                        {
                            break;
                        }
                    }
                    WriteableBitmap image = new WriteableBitmap(width, height, 96, 96, PixelFormats.Rgb24, null);
                    int dataSize = width * height * 3;
                    List<byte> allPixels = new List<byte>();

                    while (true)
                    {
                        char[] buffer = new char[8192];
                        int bytesRead = reader.ReadBlock(buffer, 0, buffer.Length);

                        if (bytesRead == 0)
                        {
                            break;
                        }

                        int lastNewlineIndex = -1;
                        string dataBlock = tmp + new string(buffer, 0, buffer.Length);

                        if (Array.LastIndexOf(buffer, '\n') != 8191)
                        {
                            lastNewlineIndex = Array.LastIndexOf(buffer, '\n');
                            dataBlock = tmp + new string(buffer, 0, lastNewlineIndex);
                        }
                        tmp = string.Empty;
                        if (lastNewlineIndex >= 0)
                        {
                            if (bytesRead - lastNewlineIndex - 1 > 0)
                                tmp = new string(buffer, lastNewlineIndex + 1, bytesRead - lastNewlineIndex - 1);
                        }

                        if (dataBlock.Contains('#'))
                        {
                            while (dataBlock.Contains('#'))
                            {
                                dataBlock = removeComments(dataBlock);
                            }
                        }

                        dataBlock = removeComments(dataBlock);

                        string[] lines = dataBlock.Split(new string[] { "\n" }, StringSplitOptions.None);

                        foreach (var l in lines)
                        {
                            string[] tokens = l.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var token in tokens)
                            {
                                string value;
                                if (maxValue > 255)
                                {
                                    double scale = 255.0 / maxValue;
                                    value = ((int)(int.Parse(token) * scale)).ToString();
                                }
                                else
                                {
                                    value = token;
                                }
                                allPixels.Add(byte.Parse(value));
                            }
                        }
                    }
                    var pixels = allPixels.ToArray();
                    image.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 3, 0);
                    loadedImage.Source = image;
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas otwierania pliku P3");
            }
        }

        private void LoadPP6(string filePath)
        {
            try
            {
                using (BinaryReader reader = new BinaryReader(new FileStream(filePath, FileMode.Open, FileAccess.Read)))
                {
                    string format = Encoding.ASCII.GetString(reader.ReadBytes(2));

                    int width = 0;
                    int height = 0;
                    int maxValue = 0;
                    string dimensionsLine;

                    while ((dimensionsLine = ReadLine(reader)) != null)
                    {
                        int commentIndex = dimensionsLine.IndexOf('#');
                        if (commentIndex >= 0)
                        {
                            dimensionsLine = dimensionsLine.Substring(0, commentIndex);
                        }

                        string[] tokens = dimensionsLine.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string token in tokens)
                        {
                            if (int.TryParse(token, out int value))
                            {
                                if (width == 0)
                                {
                                    width = value;
                                }
                                else if (height == 0)
                                {
                                    height = value;
                                }
                                else if (maxValue == 0)
                                {
                                    maxValue = value;
                                    break;
                                }
                            }
                        }

                        if (width > 0 && height > 0 && maxValue > 0)
                        {
                            break;
                        }
                    }

                    WriteableBitmap image = new WriteableBitmap(width, height, 96, 96, PixelFormats.Rgb24, null);
                    int dataSize = width * height * 3;
                    byte[] allPixels = new byte[dataSize];
                    int bytesRead = 0;

                    char[] buffer = new char[8192];

                    while (bytesRead < dataSize)
                    {
                        int bytesToRead = Math.Min(dataSize - bytesRead, 8192);
                        int bytesReadThisBlock = reader.Read(allPixels, bytesRead, bytesToRead);
                        if (bytesReadThisBlock == 0)
                        {
                            break;
                        }
                        bytesRead += bytesReadThisBlock;
                    }
                    image.WritePixels(new Int32Rect(0, 0, width, height), allPixels, width * 3, 0);

                    loadedImage.Source = image;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas otwierania pliku P6");
            }
        }

        private string ReadLine(BinaryReader reader)
        {
            List<byte> buffer = new List<byte>();
            byte currentByte;

            while ((currentByte = reader.ReadByte()) != 10)
            {
                buffer.Add(currentByte);
            }

            return Encoding.ASCII.GetString(buffer.ToArray());
        }

        private void LoadJpg(string filePath)
        {
            try
            {
                BitmapImage image = new BitmapImage(new Uri(filePath));
                loadedImage.Source = image;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas otwierania pliku JPEG");
            }
        }

        private void OnlyNumbers(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (regex.IsMatch(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void ReadCoordsAndColor(object sender, MouseEventArgs e)
        {
            Point mousePosition = e.GetPosition(loadedImage);
            BitmapSource bitmapSource = loadedImage.Source as BitmapSource;

            if (bitmapSource != null)
            {
                int x = (int)(mousePosition.X * (bitmapSource.PixelWidth / loadedImage.ActualWidth));
                int y = (int)(mousePosition.Y * (bitmapSource.PixelHeight / loadedImage.ActualHeight));

                if (x >= 0 && x < bitmapSource.PixelWidth && y >= 0 && y < bitmapSource.PixelHeight)
                {
                    byte[] pixelData = new byte[4];
                    CroppedBitmap crop = new CroppedBitmap(bitmapSource, new Int32Rect(x, y, 1, 1));
                    crop.CopyPixels(pixelData, 4, 0);

                    Color pixelColor = Color.FromArgb(pixelData[3], pixelData[0], pixelData[1], pixelData[2]);
                    coordsAndColor.Text = $"Red: {pixelColor.R}, Green: {pixelColor.G}, Blue: {pixelColor.B}\nX: {x} Y: {y}";
                }
            }
        }

        private string removeComments(string line)
        {
            int commentIndex = line.IndexOf('#');
            if (commentIndex == 0)
            {
                return null;
            }
            if (commentIndex >= 0)
            {
                var endcomment = line.Substring(commentIndex);
                var endcommentIndex = endcomment.IndexOf("\n");
                var tmp = line.Substring(0, commentIndex);
                var tmp2 = endcomment.Substring(endcommentIndex);
                line = tmp + tmp2;
            }
            return line;
        }
    }
}
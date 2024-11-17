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
        private BitmapImage originalImage;
        private WriteableBitmap processedImage;

        int Width, Height;
        byte[] Pixels;
        int[] PixelsAsGray;
        List<Color> PixelsAsColors = new List<Color>();
        int bytesPerPixel, stride;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Pliki JPEG|*.jpg;*.jpeg";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                if (filePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || filePath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    LoadAndDisplayJPEG(filePath);
                }
                else
                {
                    MessageBox.Show("Nieobsługiwany format pliku.");
                }
            }
        }

        private void LoadAndDisplayJPEG(string filePath)
        {
            try
            {
                BitmapImage image = new BitmapImage(new Uri(filePath));
                originalImage = image;

                Width = originalImage.PixelWidth;
                Height = originalImage.PixelHeight;
                bytesPerPixel = (originalImage.Format.BitsPerPixel + 7) / 8;
                stride = Width * bytesPerPixel;

                byte[] pixelData = new byte[Height * stride];
                originalImage.CopyPixels(pixelData, stride, 0);
                Pixels = pixelData;

                processedImage = new WriteableBitmap(originalImage);
                loadedImage.Source = originalImage;

                SetColorsFromPixelsArray();
                SetGrayPixels();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas wczytywania pliku JPEG: " + ex.Message);
            }
        }

        private void RozHisto(object sender, RoutedEventArgs e)
        {
            if (originalImage != null)
            {
                RozszerzHistogram();
            }
        }

        private void RowHisto(object sender, RoutedEventArgs e)
        {
            if (originalImage != null)
            {
                WyrownajHistogram();
            }
        }

        private void RozszerzHistogram()
        {
            int[] grayscalePixels = new int[PixelsAsColors.Count];

            for (int i = 0; i < PixelsAsColors.Count; i++)
            {
                grayscalePixels[i] = ((PixelsAsColors[i].R + PixelsAsColors[i].B + PixelsAsColors[i].G) / 3);
            }

            var min = grayscalePixels.Min();
            var max = grayscalePixels.Max();

            for (int i = 0; i < grayscalePixels.Length; i++)
            {
                grayscalePixels[i] = (int)(255 * (double)(grayscalePixels[i] - min) / (max - min));
            }
            ImageFromBytes(PixelsArrayToDrawableArray(grayscalePixels));
        }

        private void WyrownajHistogram()
        {
            int[] grayscalePixels = CalculateGrayPixels();
            int numberOfPixels = Pixels.Length;

            int[] cdf = new int[256];
            cdf[0] = grayscalePixels[0];
            for (int i = 1; i < 256; i++)
            {
                cdf[i] = cdf[i - 1] + grayscalePixels[i];
            }

            var min = cdf.Min();
            var max = cdf.Max();

            byte[] equalizedPixels = new byte[numberOfPixels];
            for (int i = 0; i < numberOfPixels; i += 4)
            {
                int gray = (Pixels[i] + Pixels[i + 1] + Pixels[i + 2]) / 3;
                int newGray = (int)(255.0 * (cdf[gray] - min) / (max - min));
                equalizedPixels[i] = (byte)newGray;
                equalizedPixels[i + 1] = (byte)newGray;
                equalizedPixels[i + 2] = (byte)newGray;
                equalizedPixels[i + 3] = 255;
            }
            ImageFromBytes(equalizedPixels);
            SetColorsFromPixelsArray(equalizedPixels);
        }

        private void Manual(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(value.Text, out int threshold))
            {
                byte[] binarizedPixels = RecznaBin(threshold);
                ImageFromBytes(binarizedPixels);
            }
            else
            {
                MessageBox.Show("Wprowadz wartosc");
            }
        }

        private void ProcentCzarny(object sender, RoutedEventArgs e)
        {
            int[] histogramArray = CalculateGrayPixels();
            int numberOfPixels = PixelsAsColors.Count;
            int newThreshold = 0;
            int percentage = 50;
            bool isFoundThreshold = false;
            int cumulativeNumber = 0, index = 0;

            while (!isFoundThreshold)
            {
                cumulativeNumber += histogramArray[index];
                double currentPercentage = (double)cumulativeNumber / numberOfPixels * 100;
                if (currentPercentage < percentage)
                {
                    index++;
                }
                else
                {
                    isFoundThreshold = true;
                }
                newThreshold = index;
            }
            byte[] binarized = RecznaBin(newThreshold);

            ImageFromBytes(binarized);
        }

        private void IterSrednia(object sender, RoutedEventArgs e)
        {
            int threshold = 128;
            int[] histogramArray = CalculateGrayPixels();
            while (true)
            {
                int sumBelow = 0;
                int countBelow = 0;
                int sumAbove = 0;
                int countAbove = 0;

                for (int i = 0; i < histogramArray.Length; i++)
                {
                    if (i < threshold)
                    {
                        sumBelow += i * histogramArray[i];
                        countBelow += histogramArray[i];
                    }
                    else
                    {
                        sumAbove += i * histogramArray[i];
                        countAbove += histogramArray[i];
                    }
                }

                int newThreshold = (sumBelow / countBelow + sumAbove / countAbove) / 2;
                if (newThreshold == threshold)
                {
                    break;
                }

                threshold = newThreshold;
            }

            byte[] binarizedPixels = RecznaBin(threshold);

            ImageFromBytes(binarizedPixels);
        }

        private byte[] RecznaBin(int threshold)
        {
            byte[] binarized = new byte[Pixels.Length];
            for (int i = 0; i < Pixels.Length; i += 4)
            {
                binarized[i] = (byte)(PixelsAsGray[i] < threshold ? 0 : 255);
                binarized[i + 1] = (byte)(PixelsAsGray[i + 1] < threshold ? 0 : 255);
                binarized[i + 2] = (byte)(PixelsAsGray[i + 2] < threshold ? 0 : 255);
                binarized[i + 3] = 255;
            }

            return binarized;
        }

        private byte[] PixelsArrayToDrawableArray(int[] array)
        {
            byte[] bytes = new byte[Pixels.Length];
            int index = 0;
            for (int i = 0; i < Pixels.Length; i += 4)
            {
                bytes[i] = (byte)array[index];
                bytes[i + 1] = (byte)array[index];
                bytes[i + 2] = (byte)array[index];
                bytes[i + 3] = 255;
                index++;
            }
            return bytes;
        }

        private int[] CalculateGrayPixels()
        {
            int[] grayscaleBytes = new int[256];
            int average;

            for (int i = 0; i < PixelsAsColors.Count; i++)
            {
                average = ((PixelsAsColors[i].R + PixelsAsColors[i].B + PixelsAsColors[i].G) / 3);
                grayscaleBytes[average]++;
            }
            return grayscaleBytes;
        }

        private void ImageFromBytes(byte[] bytes)
        {
            processedImage.WritePixels(new Int32Rect(0, 0, Width, Height), bytes, stride, 0);
            loadedImage.Source = processedImage;
        }

        private void SetColorsFromPixelsArray()
        {
            PixelsAsColors.Clear();
            for (int i = 0; i + 3 < Pixels.Length; i += 4)
            {
                PixelsAsColors.Add(Color.FromArgb(255, Pixels[i + 2], Pixels[i + 1], Pixels[i]));
            }
        }

        private void SetColorsFromPixelsArray(byte[] array)
        {
            PixelsAsColors.Clear();
            for (int i = 0; i + 3 < array.Length; i += 4)
            {
                PixelsAsColors.Add(Color.FromArgb(255, array[i + 2], array[i + 1], array[i]));
            }
        }

        private void SetGrayPixels()
        {
            int average;
            PixelsAsGray = new int[Pixels.Length];

            for (int i = 0; i < Pixels.Length; i += 4)
            {
                average = (Pixels[i] + Pixels[i + 1] + Pixels[i + 2]) / 3;
                PixelsAsGray[i] = PixelsAsGray[i + 1] = PixelsAsGray[i + 2] = average;
                PixelsAsGray[i + 3] = 255;
            }
        }
    }
}
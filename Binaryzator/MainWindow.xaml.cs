using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;

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
            int minR = 255, maxR = 0;
            int minG = 255, maxG = 0;
            int minB = 255, maxB = 0;

            foreach (var pixel in PixelsAsColors)
            {
                minR = Math.Min(minR, pixel.R);
                maxR = Math.Max(maxR, pixel.R);
                minG = Math.Min(minG, pixel.G);
                maxG = Math.Max(maxG, pixel.G);
                minB = Math.Min(minB, pixel.B);
                maxB = Math.Max(maxB, pixel.B);
            }

            int diffR = maxR - minR > 0 ? maxR - minR : 1;
            int diffG = maxG - minG > 0 ? maxG - minG : 1;
            int diffB = maxB - minB > 0 ? maxB - minB : 1;

            byte[] pixelsArray = new byte[Pixels.Length];
            for (int i = 0; i < Pixels.Length; i += 4)
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1];
                byte r = Pixels[i + 2];

                byte newR = (byte)((r - minR) * 255 / diffR);
                byte newG = (byte)((g - minG) * 255 / diffG);
                byte newB = (byte)((b - minB) * 255 / diffB);


                pixelsArray[i] = newB;
                pixelsArray[i + 1] = newG;
                pixelsArray[i + 2] = newR;
                pixelsArray[i + 3] = 255; 
            }

            ImageFromBytes(pixelsArray);
        }

        private void WyrownajHistogram()
        {
            int[] histR = new int[256];
            int[] histG = new int[256];
            int[] histB = new int[256];

            foreach (var color in PixelsAsColors)
            {
                histR[color.R]++;
                histG[color.G]++;
                histB[color.B]++;
            }

            int[] cdfR = CalculateCDF(histR);
            int[] cdfG = CalculateCDF(histG);
            int[] cdfB = CalculateCDF(histB);

            int totalPixels = PixelsAsColors.Count;

            int[] normalizedR = NormalizeCDF(cdfR, totalPixels);
            int[] normalizedG = NormalizeCDF(cdfG, totalPixels);
            int[] normalizedB = NormalizeCDF(cdfB, totalPixels);

            byte[] pixelsArray = new byte[Pixels.Length];

            for (int i = 0; i < Pixels.Length; i += 4)
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1]; 
                byte r = Pixels[i + 2];

                pixelsArray[i] = (byte)normalizedB[b]; 
                pixelsArray[i + 1] = (byte)normalizedG[g]; 
                pixelsArray[i + 2] = (byte)normalizedR[r]; 
                pixelsArray[i + 3] = 255; 
            }

            ImageFromBytes(pixelsArray);
        }

        private int[] CalculateCDF(int[] histogram)
        {
            int[] cdf = new int[histogram.Length];
            cdf[0] = histogram[0];
            for (int i = 1; i < histogram.Length; i++)
            {
                cdf[i] = cdf[i - 1] + histogram[i];
            }
            return cdf;
        }

        private int[] NormalizeCDF(int[] cdf, int totalPixels)
        {
            int cdfMin = cdf.First(value => value > 0); // Znajdź pierwszą niezerową wartość
            int[] normalized = new int[cdf.Length];

            for (int i = 0; i < cdf.Length; i++)
            {
                normalized[i] = (int)Math.Round((double)(cdf[i] - cdfMin) / (totalPixels - cdfMin) * 255);
                normalized[i] = Math.Clamp(normalized[i], 0, 255); // Upewnij się, że wartości mieszczą się w zakresie
            }

            return normalized;
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
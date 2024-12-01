using HelixToolkit.Wpf;
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
using System.Windows.Shapes;
using System.Xml.Serialization;
using WczytywaczObrazow.Models;
using Polygon = System.Windows.Shapes.Polygon;

namespace WczytywaczObrazow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isDrawing = false;
        private bool isDragging = false;
        private Point startPoint;
        private Polygon currentPolygon;
        private List<Polygon> shapes = new List<Polygon>();
        private List<Point> points = new List<Point>();
        Point centerOfPolygon;


        public MainWindow()
        {
            InitializeComponent();
            Load("D:\\Kurs\\GrafikaKomputerowa\\RysowaczPunktowacz\\rys.xml");
            Closed += Window_Closed;

            pointListBox.ItemsSource = points;
            DataContext = this;
            this.WindowState = WindowState.Maximized;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            loadedImage.Focus();
            startPoint = e.GetPosition(loadedImage);
            currentPolygon = GetShapeUnderMouse();

            if (currentPolygon != null)
            {
                isDragging = true;
                isDrawing = false;
            }
            else
            {
                isDrawing = true;
                isDragging = false;
            }

        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isDrawing)
            {
                Point mousePoint = e.GetPosition(loadedImage);

                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    points.Add(mousePoint);
                    DrawPoints();
                    pointListBox.Items.Refresh();
                }

            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                Draw();
                ClearPoints();
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging & currentPolygon != null)
            {
                Point currentPoint = Mouse.GetPosition(this);
                double deltaX = currentPoint.X - startPoint.X;
                double deltaY = currentPoint.Y - startPoint.Y;

                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    centerOfPolygon = new Point((loadedImage.ActualWidth / 2), loadedImage.ActualHeight / 2);
                    double alfa = Math.Atan2(deltaY, deltaX);
                    Rotation(alfa, centerOfPolygon);
                }
                else if (Keyboard.IsKeyDown(Key.LeftAlt))
                {
                    double scale = 1 + (deltaY / 100.0); 
                    Scale(scale, currentPoint);
                }
                else
                {
                    var shape = currentPolygon;

                    for (int i = 0; i < shape.Points.Count; i++)
                    {
                        shape.Points[i] = new Point(shape.Points[i].X + deltaX, shape.Points[i].Y + deltaY);
                    }
                }
                startPoint = currentPoint;
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;
            isDragging = false;
        }


        private void AddPoint()
        {
            try
            {
                double x = Convert.ToDouble(X.Text);
                double y = Convert.ToDouble(Y.Text);
                points.Add(new Point(x, y));
                pointListBox.Items.Refresh();
                DrawPoints();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nieprawidłowe wartości wprowadzone do dodania punktu");
            }
        }

        private void Draw()
        {
            if (points.Count < 3)
            {
                MessageBox.Show("Za mało punktów do utworzenia kształtu. Dodaj conajmniej 3");
                return;
            }
            Polygon polygon = new Polygon
            {
                Points = new PointCollection(points),
                Stroke = Brushes.Black,
                StrokeThickness = 3,
                Fill = Brushes.DarkCyan,
            };
            shapes.Add(polygon);
            loadedImage.Children.Add(polygon);
            points.Clear();
            pointListBox.Items.Refresh();
        }

        private void Transform(double h, double v)
        {
            try
            {
                var shape = currentPolygon as Polygon;
                List<Point> rotatedPoints = new List<Point>();
                foreach (var point in shape.Points)
                {

                    double x = point.X + h;
                    double y = point.Y + v;

                    rotatedPoints.Add(new Point(x, y));

                }
                shape.Points = new PointCollection(rotatedPoints);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nieprawidłowe wartości wprowadzone do przesunięcia");
            }
        }

        private void Rotation(double alfa, Point givenPoint)
        {
            try
            {
                var shape = currentPolygon;
                if (shape == null)
                {
                    MessageBox.Show("Nie wybrano kształtu");
                    return;
                }
                List<Point> rotatedPoints = new List<Point>();
                var rad = (alfa * (Math.PI)) / 180;
                foreach (var point in shape.Points)
                {
                    double translatedX = point.X - givenPoint.X;
                    double translatedY = point.Y - givenPoint.Y;

                    double x = givenPoint.X + translatedX * Math.Cos(rad) - translatedY * Math.Sin(rad);
                    double y = givenPoint.Y + translatedX * Math.Sin(rad) + translatedY * Math.Cos(rad);

                    rotatedPoints.Add(new Point(x, y));

                }
                shape.Points = new PointCollection(rotatedPoints);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nieprawidłowe wartości wprowadzone do obrócenia");
            }
        }

        private void Scale(double k, Point givenPoint)
        {
            try
            {
                var shape = currentPolygon as Polygon;
                if (shape == null)
                {
                    MessageBox.Show("Nie wybrano kształtu");
                    return;
                }
                List<Point> rotatedPoints = new List<Point>();
                foreach (var point in shape.Points)
                {
                    double x = point.X * k + (1 - k) * givenPoint.X;
                    double y = point.Y * k + (1 - k) * givenPoint.Y;

                    rotatedPoints.Add(new Point(x, y));

                }
                shape.Points = new PointCollection(rotatedPoints);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nieprawidłowe wartości wprowadzone do skalowania");
            }
        }

        private void AddPointButton_Click(object sender, RoutedEventArgs e)
        {
            AddPoint();
        }

        private void AddPolygonButton_Click(object sender, RoutedEventArgs e)
        {
            Draw();
            ClearPoints();
        }


        private void TranslationButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(XTrans.Text, out double h) & double.TryParse(YTrans.Text, out double v))
            {
                Transform(h, v);
            }
        }

        private void RotationButton_Click(object sender, RoutedEventArgs e)
        {
            centerOfPolygon = new Point((loadedImage.ActualWidth / 2), loadedImage.ActualHeight / 2);
            if (double.TryParse(XTrans.Text, out double x) & double.TryParse(YTrans.Text, out double y) & double.TryParse(Angle.Text, out double alfa))
            {
                Point givenPoint = new Point(x, y);
                Rotation(alfa, givenPoint);

            }
            else if (alfa != 0)
            {
                Rotation(alfa, centerOfPolygon);
            }
            else
            {
                MessageBox.Show("Nieprawidłowe wartości wprowadzone do obrócenia");
            }
        }

        private void ScalingButton_Click(object sender, RoutedEventArgs e)
        {
            centerOfPolygon = new Point((loadedImage.ActualWidth / 2), loadedImage.ActualHeight / 2);
            if (double.TryParse(XTrans.Text, out double x) & double.TryParse(YTrans.Text, out double y) & double.TryParse(Scalee.Text, out double k))
            {
                Point givenPoint = new Point(x, y);
                Scale(k, givenPoint);
            }
            else if (k != 0)
            {
                Scale(k, centerOfPolygon);
            }
            else
            {
                MessageBox.Show("Nieprawidłowe wartości wprowadzone do skalowania");
            }
        }

        private void Save(string fileName)
        {
            List<PolygonData> shapeDataList = new List<PolygonData>();

            foreach (var shape in shapes)
            {
                var pointsList = new List<Point>();
                foreach (var point in shape.Points)
                {
                    pointsList.Add(point);
                }

                shapeDataList.Add(new PolygonData
                {
                    points = pointsList,
                    FillColor = shape.Fill.ToString(),
                });

            }

            PolygonList polygonList = new PolygonList { Shapes = shapeDataList };

            using (StreamWriter sw = new StreamWriter(fileName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PolygonList));
                serializer.Serialize(sw, polygonList);
            }
        }

        private void Load(string fileName)
        {
            using (StreamReader sr = new StreamReader(fileName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PolygonList));
                PolygonList polygonList = (PolygonList)serializer.Deserialize(sr);

                foreach (var shapeData in polygonList.Shapes)
                {
                    PolygonData polygonData = (PolygonData)shapeData;
                    var polygon = new Polygon
                    {
                        Points = new PointCollection(polygonData.points),
                        Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom(polygonData.FillColor)),
                        Stroke = Brushes.Black,
                        StrokeThickness = 3,
                    };
                    loadedImage.Children.Add(polygon);
                    shapes.Add(polygon);
                }
            }
        }

        private void SaveToFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "XML Files|*.xml";
            if (dialog.ShowDialog() == true)
            {
                Save(dialog.FileName);
            }
        }

        private void LoadFromFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "XML Files|*.xml";
            if (dialog.ShowDialog() == true)
            {
                Load(dialog.FileName);
            }
        }

        private Polygon GetShapeUnderMouse()
        {
            foreach (var shape in loadedImage.Children)
            {
                if (shape is Polygon polygon)
                {
                    if (IsPointInsidePolygon(startPoint, polygon.Points))
                    {
                        return polygon;
                    }
                }
            }

            return null;
        }

        private bool IsPointInsidePolygon(Point point, PointCollection polygonPoints)
        {
            int count = polygonPoints.Count;
            bool inside = false;

            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                if (((polygonPoints[i].Y > point.Y) != (polygonPoints[j].Y > point.Y)) &&
                    (point.X < (polygonPoints[j].X - polygonPoints[i].X) * (point.Y - polygonPoints[i].Y) / (polygonPoints[j].Y - polygonPoints[i].Y) + polygonPoints[i].X))
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            loadedImage.Children.Clear();
            X.Text = "";
            Y.Text = "";
            XTrans.Text = "";
            YTrans.Text = "";
            Angle.Text = "";
            Scalee.Text = "";
            shapes.Clear();
        }

        private void ClearPointButton_Click(object sender, RoutedEventArgs e)
        {
            points.Clear();
            pointListBox.Items.Refresh();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            GC.SuppressFinalize(this);
            Save("D:\\Kurs\\GrafikaKomputerowa\\RysowaczPunktowacz\\rys.xml");
        }

        private void StartPoints()
        {
            points.Add(new Point(200, 200));
            points.Add(new Point(400, 200));
            points.Add(new Point(450, 400));
            points.Add(new Point(400, 600));
            points.Add(new Point(200, 600));
        }

        public void DrawPoints()
        {
            foreach (var p in points)
            {
                Ellipse ellipse = new Ellipse
                {
                    Fill = Brushes.Red,
                    Width = 8,
                    Height = 8,
                };
                Canvas.SetLeft(ellipse, p.X - ellipse.Width / 2);
                Canvas.SetTop(ellipse, p.Y - ellipse.Height / 2);
                loadedImage.Children.Add(ellipse);
            }
            pointListBox.Items.Refresh();
        }
        public void ClearPoints()
        {
            loadedImage.Children.Clear();
            foreach (var poly in shapes)
            {
                loadedImage.Children.Add(poly);
            }
        }
    }
}
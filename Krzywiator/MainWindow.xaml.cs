﻿using System;
using System.Collections.Generic;
using System.Drawing;
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
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;

namespace WczytywaczObrazow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int degree = 5; 
        List<Point> controlPoints;
        private Point selectedPoint;
        private int selectedPointIndex;
        private bool isDragging = false;

        public MainWindow()
        {
            InitializeComponent();
            controlPoints = new List<Point>();
            kat.Text = degree.ToString();
            pointListBox.ItemsSource = controlPoints;
            DataContext = this;
            DrawPoints();
            Closed += Window_Closed;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                int indexOfPoint = controlPoints.IndexOf(selectedPoint);
                if (indexOfPoint == -1) return;

                Point mousePosition = e.GetPosition(loadedImage);
                selectedPoint.X = mousePosition.X;
                selectedPoint.Y = mousePosition.Y;
                controlPoints[indexOfPoint] = selectedPoint;
                DrawBezierCurve();
                DrawPoints();
            }
        }
        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
        }

        private void SetPointClick(object sender, MouseButtonEventArgs e)
        {

            Point mousePosition = e.GetPosition(loadedImage);

            bool existsPoint = controlPoints.Any(p => CalculateDistance(p, mousePosition) < 10);

            if (int.TryParse(kat.Text, out int newDegree))
            {
                degree = newDegree;
            }

            if (!existsPoint)
            {
                if (degree >= controlPoints.Count)
                {
                    controlPoints.Add(new Point(mousePosition.X, mousePosition.Y));
                }
                else
                {
                    MessageBox.Show($"Za dużo punktów, Stopień równy {degree} pozwala na dodanie {degree + 1} punktów");
                }
            }
            else if (existsPoint)
            {
                isDragging = true;
                selectedPoint = controlPoints.FirstOrDefault(p => CalculateDistance(p, mousePosition) < 10);
            }
            DrawBezierCurve();
            DrawPoints();
        }

        private void pointListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pointListBox.SelectedItem != null)
            {
                Point selectedPoint = (Point)pointListBox.SelectedItem;

                X.Text = selectedPoint.X.ToString("F2");
                Y.Text = selectedPoint.Y.ToString("F2");

                selectedPointIndex = controlPoints.IndexOf(selectedPoint);
            }
        }

        public void DrawPoints()
        {
            foreach (var p in controlPoints)
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

        private void DrawBezierCurve()
        {
            loadedImage.Children.Clear();
            Path bezierPath = new Path();
            bezierPath.Stroke = Brushes.Black;
            bezierPath.StrokeThickness = 5;

            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = controlPoints[0];

            PolyBezierSegment polyBezierSegment = new PolyBezierSegment();
            polyBezierSegment.Points = new PointCollection(CalculateBezierPoints());

            pathFigure.Segments.Add(polyBezierSegment);
            pathGeometry.Figures.Add(pathFigure);
            bezierPath.Data = pathGeometry;

            loadedImage.Children.Add(bezierPath);
        }

        private PointCollection CalculateBezierPoints()
        {
            PointCollection bezierPoints = new PointCollection();

            for (double i = 0; i <= 1; i += 0.005)
            {
                Point bezierPoint = CalculateBezierPoint(i);
                bezierPoints.Add(bezierPoint);
            }

            for (int i = 0; i < bezierPoints.Count - 1; i++)
            {
                Line line = new Line
                {
                    X1 = bezierPoints[i].X,
                    Y1 = bezierPoints[i].Y,
                    X2 = bezierPoints[i + 1].X,
                    Y2 = bezierPoints[i + 1].Y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 5
                };

                loadedImage.Children.Add(line);
            }

            return bezierPoints;
        }

        private Point CalculateBezierPoint(double t)
        {
            int n = controlPoints.Count - 1;

            double x = 0;
            double y = 0;

            for (int i = 0; i <= n; i++)
            {
                double berstein = Bernstein(n, i, t);
                x += berstein * controlPoints[i].X;
                y += berstein * controlPoints[i].Y;
            }

            return new Point(x, y);
        }

        private int Newton(int n, int i)  
        {
            int result = 1;

            for (int j = 1; j <= i; j++)
            {
                result = result * (n - j + 1) / j;
            }

            return result;
        }

        private double Bernstein(int n, int i, double t)
        {
            return Newton(n, i) * Math.Pow(1.0 - t, n - i) * Math.Pow(t, i);
        }

        private double CalculateDistance(Point point1, Point point2)
        {
            double deltaX = point2.X - point1.X;
            double deltaY = point2.Y - point1.Y;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        private void ClearCanvas(object sender, RoutedEventArgs e)
        {
            loadedImage.Children.Clear();
            controlPoints.Clear();
        }

        private void AddPoint_Click(object sender, RoutedEventArgs e)
        {
            if (degree >= controlPoints.Count && double.TryParse(X.Text, out double Xx) & double.TryParse(Y.Text, out double Yy))
            {
                controlPoints.Add(new Point(Xx, Yy));
            }
            else
            {
                MessageBox.Show($"Za dużo punktów, Stopień równy {degree} pozwala na dodanie {degree + 1} punktów");
            }
            RefreshCanvas();
        }

        private void EditPoint_Click(object sender, RoutedEventArgs e)
        {
            if (pointListBox.SelectedItem != null)
            {
                Point selectedPoint = (Point)pointListBox.SelectedItem;
                selectedPoint.X = double.Parse(X.Text);
                selectedPoint.Y = double.Parse(Y.Text);
                controlPoints[selectedPointIndex] = selectedPoint;

                RefreshCanvas();
            }
            else
            {
                MessageBox.Show("Wybierz punkt do edycji");

            }
        }
        private void RefreshCanvas()
        {
            pointListBox.Items.Refresh();
            loadedImage.Children.Clear();
            DrawBezierCurve();
            DrawPoints();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            GC.SuppressFinalize(this);
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
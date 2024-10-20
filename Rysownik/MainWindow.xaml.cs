using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace Rysownik
{
    public partial class MainWindow : Window
    {
        private List<Shape> shapes = new List<Shape>();
        private int checkedShape = 0;
        private UserControl? selectedShapeControl = null;
        private UserControl? draggedShapeControl = null;
        private UserControl? drawingControl = null;
        private UserControl? resizeShapeControl = null;
        private Shape? drawingShape = null;
        private Point startPosition;
        private Point endPosition;
        private bool mouseDown = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void UILine()
        {
            X1Label.Content = "X1";
            X2Label.Content = "X2";
            Y1Label.Content = "Y1";
            Y2Label.Content = "Y2";
            X1Label.Visibility = Visibility.Visible;
            X2Label.Visibility = Visibility.Visible;
            Y1Label.Visibility = Visibility.Visible;
            Y2Label.Visibility = Visibility.Visible;
            X1Input.Visibility = Visibility.Visible;
            X2Input.Visibility = Visibility.Visible;
            Y1Input.Visibility = Visibility.Visible;
            Y2Input.Visibility = Visibility.Visible;
        }

        private void UIRectangle()
        {
            X1Label.Content = "X";
            X2Label.Content = "Szerokość";
            Y1Label.Content = "Y";
            Y2Label.Content = "Wysokość";
            X1Label.Visibility = Visibility.Visible;
            X2Label.Visibility = Visibility.Visible;
            Y1Label.Visibility = Visibility.Visible;
            Y2Label.Visibility = Visibility.Visible;
            X1Input.Visibility = Visibility.Visible;
            X2Input.Visibility = Visibility.Visible;
            Y1Input.Visibility = Visibility.Visible;
            Y2Input.Visibility = Visibility.Visible;
        }

        private void UICircle()
        {
            X1Label.Content = "X";
            X2Label.Content = "Średnica";
            Y1Label.Content = "Y";
            Y2Label.Visibility = Visibility.Collapsed;
            Y2Input.Visibility = Visibility.Collapsed;
        }

        private void ChangeCheckedFigure(object sender, RoutedEventArgs e)
        {
            RadioButton checkedFigureButton = (RadioButton)sender;
            checkedShape = int.Parse(checkedFigureButton.Tag.ToString());
            if (X1Label is null)
            {
                return;
            }
            switch (checkedShape)
            {
                case 0:
                    UILine();
                    break;
                case 1:
                    UIRectangle();
                    break;
                case 2:
                    UICircle();
                    break;
                default:
                    break;
            }
            X1Input.Text = ""; X2Input.Text = ""; Y1Input.Text = ""; Y2Input.Text = "";
        }

        private void Draw(object sender, RoutedEventArgs e)
        {
            if (SubmitButton.Content == "Edytuj")
            {
                switch (checkedShape)
                {
                    case 0:
                        if (X1Input.Text.Length == 0 || X2Input.Text.Length == 0 || Y1Input.Text.Length == 0 || Y2Input.Text.Length == 0)
                        {
                            return;
                        }
                        Canvas.SetLeft(selectedShapeControl, double.Parse(X1Input.Text));
                        Canvas.SetTop(selectedShapeControl, double.Parse(Y1Input.Text));
                        Canvas.SetRight(selectedShapeControl, double.Parse(X2Input.Text));
                        Canvas.SetBottom(selectedShapeControl, double.Parse(Y2Input.Text));

                        ((Line)selectedShapeControl.Content).X1 = double.Parse(X1Input.Text);
                        ((Line)selectedShapeControl.Content).X2 = double.Parse(X2Input.Text);
                        ((Line)selectedShapeControl.Content).Y1 = double.Parse(Y1Input.Text);
                        ((Line)selectedShapeControl.Content).Y2 = double.Parse(Y2Input.Text);
                        break;
                    case 1:
                        if (X1Input.Text.Length == 0 || X2Input.Text.Length == 0 || Y1Input.Text.Length == 0 || Y2Input.Text.Length == 0)
                        {
                            return;
                        }
                        Canvas.SetLeft(selectedShapeControl, double.Parse(X1Input.Text));
                        Canvas.SetTop(selectedShapeControl, double.Parse(Y1Input.Text));
                        Canvas.SetRight(selectedShapeControl, double.Parse(X2Input.Text));
                        Canvas.SetBottom(selectedShapeControl, double.Parse(Y2Input.Text));

                        ((Rectangle)selectedShapeControl.Content).Width = double.Parse(X2Input.Text);
                        ((Rectangle)selectedShapeControl.Content).Height = double.Parse(Y2Input.Text);

                        Canvas.SetLeft(((Rectangle)selectedShapeControl.Content), double.Parse(X1Input.Text));
                        Canvas.SetTop(((Rectangle)selectedShapeControl.Content), double.Parse(Y1Input.Text));
                        break;
                    case 2:
                        if (X1Input.Text.Length == 0 || X2Input.Text.Length == 0 || Y1Input.Text.Length == 0)
                        {
                            return;
                        }
                        Canvas.SetLeft(selectedShapeControl, double.Parse(X1Input.Text));
                        Canvas.SetTop(selectedShapeControl, double.Parse(Y1Input.Text));
                        Canvas.SetRight(selectedShapeControl, double.Parse(X2Input.Text));
                        Canvas.SetBottom(selectedShapeControl, double.Parse(X2Input.Text));

                        ((Ellipse)selectedShapeControl.Content).Width = double.Parse(X2Input.Text);
                        ((Ellipse)selectedShapeControl.Content).Height = double.Parse(X2Input.Text);

                        Canvas.SetLeft(((Ellipse)selectedShapeControl.Content), double.Parse(X1Input.Text));
                        Canvas.SetTop(((Ellipse)selectedShapeControl.Content), double.Parse(Y1Input.Text));

                        break;
                }
                return;
            }
            switch (checkedShape)
            {
                case 0:
                    if (X1Input.Text.Length == 0 || X2Input.Text.Length == 0 || Y1Input.Text.Length == 0 || Y2Input.Text.Length == 0)
                    {
                        return;
                    }
                    var lineControl = new UserControl();
                    Canvas.SetLeft(lineControl, double.Parse(X1Input.Text));
                    Canvas.SetTop(lineControl, double.Parse(Y1Input.Text));
                    Canvas.SetRight(lineControl, double.Parse(X2Input.Text));
                    Canvas.SetBottom(lineControl, double.Parse(Y2Input.Text));

                    var line = new Line();
                    line.X1 = double.Parse(X1Input.Text);
                    line.X2 = double.Parse(X2Input.Text);
                    line.Y1 = double.Parse(Y1Input.Text);
                    line.Y2 = double.Parse(Y2Input.Text);
                    line.Stroke = Brushes.Black;
                    line.StrokeThickness = 10;

                    lineControl.Content = line;
                    lineControl.MouseLeftButtonDown += new MouseButtonEventHandler(SelectShape);
                    lineControl.MouseMove += new MouseEventHandler(MoveShape);
                    lineControl.MouseLeftButtonUp += new MouseButtonEventHandler(DropShape);
                    lineControl.MouseRightButtonDown += new MouseButtonEventHandler(SelectResizeShape);
                    lineControl.MouseMove += new MouseEventHandler(ResieShapeMouse);
                    lineControl.MouseRightButtonUp += new MouseButtonEventHandler(DropResizeShape);
                    shapes.Add(line);
                    canvas.Children.Add(lineControl);
                    break;
                case 1:
                    if (X1Input.Text.Length == 0 || X2Input.Text.Length == 0 || Y1Input.Text.Length == 0 || Y2Input.Text.Length == 0)
                    {
                        return;
                    }
                    var rectangleControl = new UserControl();
                    Canvas.SetLeft(rectangleControl, double.Parse(X1Input.Text));
                    Canvas.SetTop(rectangleControl, double.Parse(Y1Input.Text));
                    Canvas.SetRight(rectangleControl, double.Parse(X2Input.Text));
                    Canvas.SetBottom(rectangleControl, double.Parse(Y2Input.Text));

                    var rectangle = new Rectangle();
                    rectangle.Width = double.Parse(X2Input.Text);
                    rectangle.Height = double.Parse(Y2Input.Text);
                    rectangle.Stroke = Brushes.Black;
                    rectangle.Fill = Brushes.White;
                    rectangle.StrokeThickness = 10;
                    Canvas.SetLeft(rectangle, double.Parse(X1Input.Text));
                    Canvas.SetTop(rectangle, double.Parse(Y1Input.Text));

                    rectangleControl.Content = rectangle;
                    rectangleControl.MouseLeftButtonDown += new MouseButtonEventHandler(SelectShape);
                    rectangleControl.MouseMove += new MouseEventHandler(MoveShape);
                    rectangleControl.MouseLeftButtonUp += new MouseButtonEventHandler(DropShape);
                    rectangleControl.MouseRightButtonDown += new MouseButtonEventHandler(SelectResizeShape);
                    rectangleControl.MouseMove += new MouseEventHandler(ResieShapeMouse);
                    rectangleControl.MouseRightButtonUp += new MouseButtonEventHandler(DropResizeShape);
                    shapes.Add(rectangle);
                    canvas.Children.Add(rectangleControl);
                    break;
                case 2:
                    if (X1Input.Text.Length == 0 || X2Input.Text.Length == 0 || Y1Input.Text.Length == 0)
                    {
                        return;
                    }
                    var circleControl = new UserControl();
                    Canvas.SetLeft(circleControl, double.Parse(X1Input.Text));
                    Canvas.SetTop(circleControl, double.Parse(Y1Input.Text));
                    Canvas.SetRight(circleControl, double.Parse(X2Input.Text));
                    Canvas.SetBottom(circleControl, double.Parse(X2Input.Text));

                    var circle = new Ellipse();
                    circle.Width = double.Parse(X2Input.Text);
                    circle.Height = double.Parse(X2Input.Text);
                    circle.Stroke = Brushes.Black;
                    circle.Fill = Brushes.White;
                    circle.StrokeThickness = 10;
                    Canvas.SetLeft(circle, double.Parse(X1Input.Text));
                    Canvas.SetTop(circle, double.Parse(Y1Input.Text));

                    circleControl.Content = circle;
                    circleControl.MouseLeftButtonDown += new MouseButtonEventHandler(SelectShape);
                    circleControl.MouseMove += new MouseEventHandler(MoveShape);
                    circleControl.MouseLeftButtonUp += new MouseButtonEventHandler(DropShape);
                    circleControl.MouseRightButtonDown += new MouseButtonEventHandler(SelectResizeShape);
                    circleControl.MouseMove += new MouseEventHandler(ResieShapeMouse);
                    circleControl.MouseRightButtonUp += new MouseButtonEventHandler(DropResizeShape);
                    shapes.Add(circle);
                    canvas.Children.Add(circleControl);
                    break;
            }
        }

        private void SelectShape(object sender, MouseButtonEventArgs e)
        {
            SubmitButton.Content = "Edytuj";
            startPosition = e.GetPosition(canvas);
            var control = sender as UserControl;
            selectedShapeControl = control;
            draggedShapeControl = control;
            var selectedShape = selectedShapeControl.Content;
            if (selectedShape is Line line)
            {
                X1Input.Text = Math.Abs(Canvas.GetLeft(selectedShapeControl)).ToString();
                X2Input.Text = Math.Abs(line.X2).ToString();
                Y1Input.Text = Math.Abs(Canvas.GetTop(selectedShapeControl)).ToString();
                Y2Input.Text = Math.Abs(line.Y2).ToString();
                UILine();
            }
            if (selectedShape is Rectangle rectangle)
            {
                X1Input.Text = Math.Abs(Canvas.GetLeft(selectedShapeControl)).ToString();
                X2Input.Text = Math.Abs(rectangle.Width).ToString();
                Y1Input.Text = Math.Abs(Canvas.GetTop(selectedShapeControl)).ToString();
                Y2Input.Text = Math.Abs(rectangle.Height).ToString();
                UIRectangle();
            }
            if (selectedShape is Ellipse circle)
            {
                X1Input.Text = Math.Abs(Canvas.GetLeft(selectedShapeControl)).ToString();
                X2Input.Text = Math.Abs(circle.Width).ToString();
                Y1Input.Text = Math.Abs(Canvas.GetTop(selectedShapeControl)).ToString();
                UICircle();
            }
            e.Handled = true;
        }

        private void UnselectShape(object sender, MouseButtonEventArgs e)
        {
            if(selectedShapeControl == null)
            {
                SubmitButton.Content = "Rysuj";
            }
            startPosition = e.GetPosition(canvas);
            selectedShapeControl = null;
            mouseDown = true;
        }

        private void SelectResizeShape(object sender, MouseButtonEventArgs e)
        {
            SubmitButton.Content = "Edytuj";
            startPosition = e.GetPosition(canvas);
            var control = sender as UserControl;
            selectedShapeControl = control;
            resizeShapeControl = control;
            var selectedShape = selectedShapeControl.Content;
            if (selectedShape is Line line)
            {
                X1Input.Text = Math.Abs(Canvas.GetLeft(selectedShapeControl)).ToString();
                X2Input.Text = Math.Abs(line.X2).ToString();
                Y1Input.Text = Math.Abs(Canvas.GetTop(selectedShapeControl)).ToString();
                Y2Input.Text = Math.Abs(line.Y2).ToString();
                UILine();
            }
            if (selectedShape is Rectangle rectangle)
            {
                X1Input.Text = Math.Abs(Canvas.GetLeft(selectedShapeControl)).ToString();
                X2Input.Text = Math.Abs(rectangle.Width).ToString();
                Y1Input.Text = Math.Abs(Canvas.GetTop(selectedShapeControl)).ToString();
                Y2Input.Text = Math.Abs(rectangle.Height).ToString();
                UIRectangle();
            }
            if (selectedShape is Ellipse circle)
            {
                X1Input.Text = Math.Abs(Canvas.GetLeft(selectedShapeControl)).ToString();
                X2Input.Text = Math.Abs(circle.Width).ToString();
                Y1Input.Text = Math.Abs(Canvas.GetTop(selectedShapeControl)).ToString();
                UICircle();
            }
            e.Handled = true;
        }

        private void DrawMouse(object sender, MouseEventArgs e)
        {
            if (selectedShapeControl is null && mouseDown)
            {
                endPosition = e.GetPosition(canvas);
                double offsetX = Math.Abs(startPosition.X - endPosition.X);
                double offsetY = Math.Abs(startPosition.Y - endPosition.Y);
                switch (checkedShape)
                {
                    case 0:
                        if (drawingControl is null || drawingShape is null)
                        {
                            drawingControl = new UserControl();
                            var line = new Line();
                            drawingShape = line;
                            shapes.Add(drawingShape);
                            drawingControl.Content = drawingShape;
                            drawingControl.MouseLeftButtonDown += new MouseButtonEventHandler(SelectShape);
                            drawingControl.MouseMove += new MouseEventHandler(MoveShape);
                            drawingControl.MouseLeftButtonUp += new MouseButtonEventHandler(DropShape);
                            drawingControl.MouseRightButtonDown += new MouseButtonEventHandler(SelectResizeShape);
                            drawingControl.MouseMove += new MouseEventHandler(ResieShapeMouse);
                            drawingControl.MouseRightButtonUp += new MouseButtonEventHandler(DropResizeShape);
                            canvas.Children.Add(drawingControl);
                        }

                        Canvas.SetLeft(drawingControl, startPosition.X);
                        Canvas.SetTop(drawingControl, startPosition.Y);
                        Canvas.SetRight(drawingControl, endPosition.X);
                        Canvas.SetBottom(drawingControl, endPosition.Y);
                        ((Line)drawingShape).StrokeThickness = 10;
                        ((Line)drawingShape).X1 = 0;
                        ((Line)drawingShape).X2 = endPosition.X - startPosition.X;
                        ((Line)drawingShape).Y1 = 0;
                        ((Line)drawingShape).Y2 = endPosition.Y - startPosition.Y;
                        drawingShape.Stroke = Brushes.Black;
                        break;
                    case 1:
                        if (drawingControl is null || drawingShape is null)
                        {
                            drawingControl = new UserControl();
                            var rectangle = new Rectangle();
                            drawingShape = rectangle;
                            shapes.Add(drawingShape);
                            drawingControl.Content = drawingShape;
                            drawingControl.MouseLeftButtonDown += new MouseButtonEventHandler(SelectShape);
                            drawingControl.MouseMove += new MouseEventHandler(MoveShape);
                            drawingControl.MouseLeftButtonUp += new MouseButtonEventHandler(DropShape);
                            drawingControl.MouseRightButtonDown += new MouseButtonEventHandler(SelectResizeShape);
                            drawingControl.MouseMove += new MouseEventHandler(ResieShapeMouse);
                            drawingControl.MouseRightButtonUp += new MouseButtonEventHandler(DropResizeShape);
                            canvas.Children.Add(drawingControl);
                        }

                        Canvas.SetLeft(drawingControl, startPosition.X);
                        Canvas.SetTop(drawingControl, startPosition.Y);
                        Canvas.SetRight(drawingControl, endPosition.X);
                        Canvas.SetBottom(drawingControl, endPosition.Y);

                        drawingShape.StrokeThickness = 10;
                        drawingShape.Width = offsetX;
                        drawingShape.Height = offsetY;
                        drawingShape.Stroke = Brushes.Black;
                        drawingShape.Fill = Brushes.White;
                        break;
                    case 2:
                        if (drawingControl is null || drawingShape is null)
                        {
                            drawingControl = new UserControl();
                            var circle = new Ellipse();
                            drawingShape = circle;
                            shapes.Add(drawingShape);
                            drawingControl.Content = drawingShape;
                            drawingControl.MouseLeftButtonDown += new MouseButtonEventHandler(SelectShape);
                            drawingControl.MouseMove += new MouseEventHandler(MoveShape);
                            drawingControl.MouseLeftButtonUp += new MouseButtonEventHandler(DropShape);
                            drawingControl.MouseRightButtonDown += new MouseButtonEventHandler(SelectResizeShape);
                            drawingControl.MouseMove += new MouseEventHandler(ResieShapeMouse);
                            drawingControl.MouseRightButtonUp += new MouseButtonEventHandler(DropResizeShape);
                            canvas.Children.Add(drawingControl);
                        }

                        Canvas.SetLeft(drawingControl, startPosition.X);
                        Canvas.SetTop(drawingControl, startPosition.Y);
                        Canvas.SetRight(drawingControl, endPosition.X);
                        Canvas.SetBottom(drawingControl, endPosition.X);

                        drawingShape.StrokeThickness = 10;
                        drawingShape.Width = offsetX;
                        drawingShape.Height = offsetX;
                        drawingShape.Stroke = Brushes.Black;
                        drawingShape.Fill = Brushes.White;
                        break;
                }
            }
        }

        private void StopDrawing(object sender, MouseButtonEventArgs e)
        {
            drawingControl = null;
            drawingShape = null;
            mouseDown = false;
        }

        private void ResieShapeMouse(object sender, MouseEventArgs e)
        {
            if (resizeShapeControl is not null)
            {
                var resizeShape = resizeShapeControl.Content;
                endPosition = e.GetPosition(canvas);
                if (resizeShape is Line line)
                {
                    double offsetX = endPosition.X - startPosition.X;
                    double offsetY = endPosition.Y - startPosition.Y;
                    Canvas.SetLeft(resizeShapeControl, Canvas.GetLeft(resizeShapeControl));
                    Canvas.SetTop(resizeShapeControl, Canvas.GetTop(resizeShapeControl));
                    Canvas.SetRight(resizeShapeControl, Canvas.GetRight(resizeShapeControl) + offsetX);
                    Canvas.SetBottom(resizeShapeControl, Canvas.GetBottom(resizeShapeControl) + offsetY);
                    if (Canvas.GetRight(resizeShapeControl) + offsetX > 0 && Canvas.GetBottom(resizeShapeControl) + offsetY > 0)
                    {
                        ((Shape)resizeShape).Width = Canvas.GetRight(resizeShapeControl) + offsetX;
                        ((Shape)resizeShape).Height = Canvas.GetBottom(resizeShapeControl) + offsetY;
                    }
                }
                if (resizeShape is Rectangle rectangle)
                {
                    double offsetX = endPosition.X - startPosition.X;
                    double offsetY = endPosition.Y - startPosition.Y;
                    Canvas.SetLeft(resizeShapeControl, Canvas.GetLeft(resizeShapeControl));
                    Canvas.SetTop(resizeShapeControl, Canvas.GetTop(resizeShapeControl));
                    Canvas.SetRight(resizeShapeControl, Canvas.GetRight(resizeShapeControl) + offsetX);
                    Canvas.SetBottom(resizeShapeControl, Canvas.GetBottom(resizeShapeControl) + offsetY);
                    if(Canvas.GetRight(resizeShapeControl) + offsetX > 0 && Canvas.GetBottom(resizeShapeControl) + offsetY > 0)
                    {
                        ((Shape)resizeShape).Width = Canvas.GetRight(resizeShapeControl) + offsetX;
                        ((Shape)resizeShape).Height = Canvas.GetBottom(resizeShapeControl) + offsetY;
                    }
                }
                if (resizeShape is Ellipse circle)
                {
                    double offsetX = endPosition.X - startPosition.X;
                    double offsetY = endPosition.Y - startPosition.Y;
                    Canvas.SetLeft(resizeShapeControl, Canvas.GetLeft(resizeShapeControl));
                    Canvas.SetTop(resizeShapeControl, Canvas.GetTop(resizeShapeControl));
                    Canvas.SetRight(resizeShapeControl, Canvas.GetRight(resizeShapeControl) + offsetX);
                    Canvas.SetBottom(resizeShapeControl, Canvas.GetBottom(resizeShapeControl) + offsetX);
                    if (Canvas.GetRight(resizeShapeControl) + offsetX > 0 && Canvas.GetBottom(resizeShapeControl) + offsetY > 0)
                    {
                        ((Shape)resizeShape).Width = Canvas.GetRight(resizeShapeControl) + offsetX;
                        ((Shape)resizeShape).Height = Canvas.GetBottom(resizeShapeControl) + offsetY;
                    }
                }
                startPosition = e.GetPosition(canvas);
            }
        }

        private void MoveShape(object sender, MouseEventArgs e)
        {
            if (draggedShapeControl is not null)
            {
                var draggedShape = draggedShapeControl.Content;
                endPosition = e.GetPosition(canvas);
                if (draggedShape is Line line)
                {
                    double offsetX = endPosition.X - startPosition.X;
                    double offsetY = endPosition.Y - startPosition.Y;
                    Canvas.SetLeft(draggedShapeControl, Canvas.GetLeft(draggedShapeControl) + offsetX);
                    Canvas.SetTop(draggedShapeControl, Canvas.GetTop(draggedShapeControl) + offsetY);
                    Canvas.SetRight(draggedShapeControl, Canvas.GetRight(draggedShapeControl) + offsetX);
                    Canvas.SetBottom(draggedShapeControl, Canvas.GetBottom(draggedShapeControl) + offsetY);
                }
                if (draggedShape is Rectangle rectangle)
                {
                    double offsetX = endPosition.X - startPosition.X;
                    double offsetY = endPosition.Y - startPosition.Y;
                    Canvas.SetLeft(draggedShapeControl, Canvas.GetLeft(draggedShapeControl) + offsetX);
                    Canvas.SetTop(draggedShapeControl, Canvas.GetTop(draggedShapeControl) + offsetY);
                    Canvas.SetRight(draggedShapeControl, Canvas.GetRight(draggedShapeControl) + offsetX);
                    Canvas.SetBottom(draggedShapeControl, Canvas.GetBottom(draggedShapeControl) + offsetY);
                }
                if (draggedShape is Ellipse circle)
                {
                    double offsetX = endPosition.X - startPosition.X;
                    double offsetY = endPosition.Y - startPosition.Y;
                    Canvas.SetLeft(draggedShapeControl, Canvas.GetLeft(draggedShapeControl) + offsetX);
                    Canvas.SetTop(draggedShapeControl, Canvas.GetTop(draggedShapeControl) + offsetY);
                    Canvas.SetRight(draggedShapeControl, Canvas.GetRight(draggedShapeControl) + offsetX);
                    Canvas.SetBottom(draggedShapeControl, Canvas.GetBottom(draggedShapeControl) + offsetX);
                }
                startPosition = e.GetPosition(canvas);
            }

        }

        private void SaveShapesToFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "Xml files|*.xml";
            if (dialog.ShowDialog() == true)
            {
                List<ShapeSerialize> canvasShapes = new List<ShapeSerialize>();

                foreach (var shape in shapes)
                {
                    if (shape is Line line)
                    {
                        canvasShapes.Add(new LineSerialize
                        {
                            X1 = Canvas.GetLeft((UserControl)line.Parent),
                            Y1 = Canvas.GetTop((UserControl)line.Parent),
                            X2 = Math.Abs(line.X2),
                            Y2 = Math.Abs(line.Y2),
                        });
                    }
                    if (shape is Rectangle rectangle)
                    {
                        canvasShapes.Add(new RectangleSerialize
                        {
                            X = Canvas.GetLeft((UserControl)rectangle.Parent),
                            Y = Canvas.GetTop((UserControl)rectangle.Parent),
                            Width = rectangle.Width,
                            Height = rectangle.Height,
                        });
                    }
                    if (shape is Ellipse circle)
                    {
                        canvasShapes.Add(new CircleSerialize
                        {
                            X = Canvas.GetLeft((UserControl)circle.Parent),
                            Y = Canvas.GetTop((UserControl)circle.Parent),
                            Diameter = circle.Width,
                        });
                    }
                }
                CanvasShapes shapesToSerialize = new CanvasShapes { Shapes = canvasShapes };

                using (StreamWriter streamWriter = new StreamWriter(dialog.FileName))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(CanvasShapes));
                    serializer.Serialize(streamWriter, shapesToSerialize);
                }
            }
        }

        private void LoadShapesFromFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "XML Files|*.xml";
            if (dialog.ShowDialog() == true)
            {
                canvas.Children.Clear();
                shapes = new List<Shape>();
                using (StreamReader streamReader = new StreamReader(dialog.FileName))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(CanvasShapes));
                    CanvasShapes deserializedShapes = (CanvasShapes)serializer.Deserialize(streamReader);

                    foreach (var shape in deserializedShapes.Shapes)
                    {
                        if (shape is LineSerialize lineS)
                        {
                            var lineControl = new UserControl();
                            Canvas.SetLeft(lineControl, lineS.X1);
                            Canvas.SetTop(lineControl, lineS.Y1);
                            Canvas.SetRight(lineControl, lineS.X2);
                            Canvas.SetBottom(lineControl, lineS.Y2);

                            var line = new Line();
                            line.X1 = lineS.X1;
                            line.X2 = lineS.X2;
                            line.Y1 = lineS.Y1;
                            line.Y2 = lineS.Y2;
                            line.Stroke = Brushes.Black;
                            line.StrokeThickness = 10;

                            lineControl.Content = line;
                            lineControl.MouseLeftButtonDown += new MouseButtonEventHandler(SelectShape);
                            lineControl.MouseMove += new MouseEventHandler(MoveShape);
                            lineControl.MouseLeftButtonUp += new MouseButtonEventHandler(DropShape);
                            lineControl.MouseRightButtonDown += new MouseButtonEventHandler(SelectResizeShape);
                            lineControl.MouseMove += new MouseEventHandler(ResieShapeMouse);
                            lineControl.MouseRightButtonUp += new MouseButtonEventHandler(DropResizeShape);
                            shapes.Add(line);
                            canvas.Children.Add(lineControl);
                        }
                        if (shape is RectangleSerialize rectangleS)
                        {
                            var rectangleControl = new UserControl();
                            Canvas.SetLeft(rectangleControl, rectangleS.X);
                            Canvas.SetTop(rectangleControl, rectangleS.Y);
                            Canvas.SetRight(rectangleControl, rectangleS.Width);
                            Canvas.SetBottom(rectangleControl, rectangleS.Height);

                            var rectangle = new Rectangle();
                            rectangle.Width = rectangleS.Width;
                            rectangle.Height = rectangleS.Height;
                            rectangle.Stroke = Brushes.Black;
                            rectangle.Fill = Brushes.White;
                            rectangle.StrokeThickness = 10;
                            Canvas.SetLeft(rectangle, rectangleS.X);
                            Canvas.SetTop(rectangle, rectangleS.Y);

                            rectangleControl.Content = rectangle;
                            rectangleControl.MouseLeftButtonDown += new MouseButtonEventHandler(SelectShape);
                            rectangleControl.MouseMove += new MouseEventHandler(MoveShape);
                            rectangleControl.MouseLeftButtonUp += new MouseButtonEventHandler(DropShape);
                            rectangleControl.MouseRightButtonDown += new MouseButtonEventHandler(SelectResizeShape);
                            rectangleControl.MouseMove += new MouseEventHandler(ResieShapeMouse);
                            rectangleControl.MouseRightButtonUp += new MouseButtonEventHandler(DropResizeShape);
                            shapes.Add(rectangle);
                            canvas.Children.Add(rectangleControl);
                        }
                        if (shape is CircleSerialize circleS)
                        {
                            var circleControl = new UserControl();
                            Canvas.SetLeft(circleControl, circleS.X);
                            Canvas.SetTop(circleControl, circleS.Y);
                            Canvas.SetRight(circleControl, circleS.X + circleS.Diameter);
                            Canvas.SetBottom(circleControl, circleS.Y + circleS.Diameter);

                            var circle = new Ellipse();
                            circle.Width = circleS.Diameter;
                            circle.Height = circleS.Diameter;
                            circle.Stroke = Brushes.Black;
                            circle.Fill = Brushes.White;
                            circle.StrokeThickness = 10;
                            Canvas.SetLeft(circle, circleS.X);
                            Canvas.SetTop(circle, circleS.Y);

                            circleControl.Content = circle;
                            circleControl.MouseLeftButtonDown += new MouseButtonEventHandler(SelectShape);
                            circleControl.MouseMove += new MouseEventHandler(MoveShape);
                            circleControl.MouseLeftButtonUp += new MouseButtonEventHandler(DropShape);
                            circleControl.MouseRightButtonDown += new MouseButtonEventHandler(SelectResizeShape);
                            circleControl.MouseMove += new MouseEventHandler(ResieShapeMouse);
                            circleControl.MouseRightButtonUp += new MouseButtonEventHandler(DropResizeShape);
                            shapes.Add(circle);
                            canvas.Children.Add(circleControl);
                        }
                    }
                }
            }
        }

        private void DropResizeShape(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            resizeShapeControl = null;
        }

        private void DropShape(object sender, MouseButtonEventArgs e)
        {
            draggedShapeControl = null;
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
    }
}
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace ParkingLotSlicer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void UpdateMousePosition(System.Windows.Point point)
        {
            MousePositionTextBlock.Text = $"Mouse Position: {Math.Round(point.X, 3)} | {Math.Round(point.Y, 3)}";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                var file = dialog.FileName;
                var bitmap = new Bitmap(file);
                OriginalBitmap = bitmap;
                BitmapDisplayed = bitmap;
            }
        }

        private Bitmap _originalBitmap;

        public Bitmap OriginalBitmap
        {
            get { return _originalBitmap; }
            set { _originalBitmap = value; }
        }


        private Bitmap _bitmapDisplayed;

        public Bitmap BitmapDisplayed
        {
            get { return _bitmapDisplayed; }
            set
            {
                _bitmapDisplayed = value;
                if (value != null)
                    DisplayImage.Source = value.ToBitmapImage();
            }
        }


        private double _currentScale = 1.0;
        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var pos = e.MouseDevice.GetPosition(DisplayImage);
            if (e.Delta > 0)
            {
                _currentScale += .1;
            }
            else
            {
                if (_currentScale > 1)
                    _currentScale += -.1;
            }
            DisplayImage.RenderTransform = new ScaleTransform(_currentScale, _currentScale, pos.X, pos.Y);
            UpdateMousePosition(e.GetPosition(DisplayImage));
        }

        private void DisplayImage_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateMousePosition(e.GetPosition(DisplayImage));
            if (IsSelectingPoint && PointSelected) //first point selected - then draw the lines for the square... 
            {
                DrawTemporaryArea(new ParkingArea(SelectedPointStart,
                    new PointHolder(e.GetPosition(DisplayImage).X, e.GetPosition(DisplayImage).Y)));
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _currentScale = 1;
            DisplayImage.RenderTransform = new ScaleTransform(_currentScale, _currentScale, 0, 0);
        }


        private PointHolder _selectedPointStart;

        public PointHolder SelectedPointStart
        {
            get { return _selectedPointStart; }
            set
            {
                _selectedPointStart = value;
                OnPropertyChanged();
            }
        }





        private bool _isSelectingPoint;

        public bool IsSelectingPoint
        {
            get { return _isSelectingPoint; }
            set
            {
                _isSelectingPoint = value;
                if (value) Mouse.OverrideCursor = Cursors.Hand;
                else Mouse.OverrideCursor = null;
                OnPropertyChanged();
            }
        }

        private bool _pointSelected;

        public bool PointSelected
        {
            get { return _pointSelected; }
            set
            {
                _pointSelected = value;
                OnPropertyChanged();
            }
        }

        private ParkingArea _selectedParkingArea;

        public ParkingArea SelectedParkingArea
        {
            get { return _selectedParkingArea; }
            set
            {
                _selectedParkingArea = value;
                OnPropertyChanged();
                //if (value != null)
                //    HighlightParkingArea(value);
            }
        }


        private ObservableCollection<ParkingArea> _ParkingAreasCollection = new ObservableCollection<ParkingArea>();

        public ObservableCollection<ParkingArea> ParkingAreasCollection
        {
            get { return _ParkingAreasCollection; }
            set
            {
                _ParkingAreasCollection = value;
                OnPropertyChanged();
            }
        }


        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            SelectedPointStart = new PointHolder();
            PointSelected = false;
            IsSelectingPoint = true;


        }

        private void DisplayImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsSelectingPoint)
            {
                foreach (var parkingArea in ParkingAreasCollection)
                {
                    if (GetRectangle(parkingArea.StartingPoint, parkingArea.EndingPoint).Contains((int)e.GetPosition(DisplayImage).X,
                            (int)e.GetPosition(DisplayImage).Y))
                    {
                        HighlightParkingArea(parkingArea);
                        SelectedParkingArea = parkingArea;
                        return;
                    }
                }
                return;
            }

            if (PointSelected)
            {
                ParkingAreasCollection.Add(
                    new ParkingArea(
                        SelectedPointStart,
                        new PointHolder(
                            e.GetPosition(DisplayImage).X,
                            e.GetPosition(DisplayImage).Y)));
                IsSelectingPoint = false;
                DrawAreaOnBitmap(ParkingAreasCollection.Last());
            }
            else
            {
                PointSelected = true;
                SelectedPointStart = new PointHolder();
                SelectedPointStart.X = e.GetPosition(DisplayImage).X;
                SelectedPointStart.Y = e.GetPosition(DisplayImage).Y;
            }
        }

        private void DrawAreaOnBitmap(ParkingArea parkingArea, System.Drawing.Pen color = null)
        {
            if (color == null) color = Pens.Red;
            Bitmap tempBitmap = new Bitmap(BitmapDisplayed.Width, BitmapDisplayed.Height);
            using (Graphics g = Graphics.FromImage(tempBitmap))
            {
                g.DrawImage(BitmapDisplayed, 0, 0);
                g.DrawRectangle(color, GetRectangle(parkingArea.StartingPoint, parkingArea.EndingPoint));
                BitmapDisplayed = tempBitmap;
            }
        }

        private void DrawTemporaryArea(ParkingArea parkingArea)
        {
            Bitmap tempBitmap = new Bitmap(BitmapDisplayed.Width, BitmapDisplayed.Height);
            using (Graphics g = Graphics.FromImage(tempBitmap))
            {
                g.DrawImage(BitmapDisplayed, 0, 0);
                g.DrawRectangle(Pens.Red, GetRectangle(parkingArea.StartingPoint, parkingArea.EndingPoint));
                this.DisplayImage.Source = tempBitmap.ToBitmapImage();
            }
        }

        private void UpdateImage()
        {
            throw new NotImplementedException();
        }

        private System.Drawing.Rectangle GetRectangle(PointHolder start, PointHolder end)
        {
            return new System.Drawing.Rectangle((int)Math.Min(start.X, end.X),
               (int)Math.Min(start.Y, end.Y),
               (int)Math.Abs(start.X - end.X),
               (int)Math.Abs(start.Y - end.Y));
        }


        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            ParkingArea toRemove = (ParkingArea)((Button)sender).DataContext;
            RemoveParkingArea(toRemove);

        }

        //select area button.. press once to toggle on and again to toggle off.. 
        //when on, points pressed will be added to the list of points.
        //
        //when toggled on create two points on first click.. have at same point..and draw line between them.. 
        //then when new point added, add to list inbetween and draw lines to those..

        private void HighlightParkingArea(ParkingArea toHighlight, System.Drawing.Pen penColor = null)
        {
            if (penColor == null) penColor = Pens.GreenYellow;
            ResetBitmapToOriginal();
            foreach (var area in ParkingAreasCollection)
            {
                if (area == toHighlight)
                    DrawAreaOnBitmap(area, penColor);
                else
                    DrawAreaOnBitmap(area);
            }
        }
        private void ResetBitmapToOriginal()
        {
            BitmapDisplayed = OriginalBitmap;
        }

        private void RemoveParkingArea(ParkingArea toRemove)
        {
            ParkingAreasCollection.Remove(toRemove);
            ResetBitmapToOriginal();
            foreach (var area in ParkingAreasCollection)
            {
                DrawAreaOnBitmap(area);
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            CSVExporter.ExportToCSV(ParkingAreasCollection.ToList());
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            ParkingArea areaToHighlight = (ParkingArea)((Button)sender).DataContext;
            HighlightParkingArea(areaToHighlight);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ParkingArea areaToHighlight = (ParkingArea)((CheckBox)sender).DataContext;

            if (areaToHighlight.Unavailable)
                DrawAreaOnBitmap(areaToHighlight, Pens.Yellow);
            else DrawAreaOnBitmap(areaToHighlight);
        }
    }
    public class ParkingArea : INotifyPropertyChanged
    {
        public ParkingArea(PointHolder p1, PointHolder p2)
        {
            StartingPoint = p1;
            EndingPoint = p2;
        }

        public string Name
        {
            get { return this.ToString(); }
        }


        public override string ToString()
        {
            return $"Start: {StartingPoint.ToString()}, End: {EndingPoint.ToString()}";
        }

        private PointHolder _endingPoint;

        public PointHolder EndingPoint
        {
            get { return _endingPoint; }
            set
            {
                _endingPoint = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Name));
            }
        }

        private PointHolder _startingPoint;

        public PointHolder StartingPoint
        {
            get { return _startingPoint; }
            set
            {
                _startingPoint = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Name));
            }
        }

        private bool _unavailable;

        public bool Unavailable
        {
            get { return _unavailable; }
            set
            {
                _unavailable = value;
                OnPropertyChanged();
            }
        }

        private int _spotNumber;

        public int SpotNumber
        {
            get { return _spotNumber; }
            set
            {
                _spotNumber = value;
                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class PointHolder : INotifyPropertyChanged
    {
        public PointHolder()
        {

        }

        public PointHolder(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"{Math.Round(X)}, {Math.Round(Y)}";
        }

        private double _x;

        public double X
        {
            get { return _x; }
            set
            {
                _x = value;
                OnPropertyChanged();
            }
        }

        private double _y;

        public double Y
        {
            get { return _y; }
            set
            {
                _y = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

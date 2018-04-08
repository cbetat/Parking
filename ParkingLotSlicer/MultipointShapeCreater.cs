using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
//TODO Percentage points as in pointX/ResX, pointY/ResY
namespace ParkingLotSlicer
{
    public class MultipointShapeCreater : INotifyPropertyChanged
    {
        private PointHolder _startingPoint;

        public PointHolder StartingPoint
        {
            get { return _startingPoint; }
            set
            {
                _startingPoint = value;
                OnPropertyChanged();
            }
        }

        private PointHolder _endingPoint;

        public PointHolder EndingPoint
        {
            get { return _endingPoint; }
            set
            {
                _endingPoint = value;
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

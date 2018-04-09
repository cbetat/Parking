using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkingLotSlicer
{
    public static class CSVExporter
    {
        public static void ExportToCSVPerc(List<ParkingArea> areas, int height, int width)
        {
            //repeat as below but with % of length/width of the total res.. 
            var fd = new SaveFileDialog();
            if (fd.ShowDialog() != true)
                return;

            string csv = "";
            int count = 0;
            foreach (var area in areas)
            {
                //csv += count++ + ", " + area.StartingPoint.X + ", " + width/something... + "\n";
            }
        }
        public static void ExportToCSV(List<ParkingArea> areas)
        {
            var fd = new SaveFileDialog();
            if (fd.ShowDialog() != true)
                return;

            string csv = "";
            foreach (var area in areas)
            {
                csv += area.SpotNumber + ", " + area.StartingPoint.ToString() + ", " + area.EndingPoint.ToString() + "," + (area.Unavailable ? "Unavailable" : "") + "\n";
            }
            SaveStringToDisk(csv, fd.FileName + ".csv");
        }

        private static void SaveStringToDisk(string csv, string filepath)
        {
            File.WriteAllText(filepath, csv);
        }


    }
}

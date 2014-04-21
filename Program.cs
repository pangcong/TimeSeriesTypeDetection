using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TimeSeriesAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {

            CsvStreamReader reader = null;

            List<List<DataPoint>> timeseriesList = new List<List<DataPoint>>();

            // read all the datas from the file
            DirectoryInfo diParent = new DirectoryInfo(@"C:\Users\v-cheluo\Desktop\correlation detection\Timeserie Data\TEST\");
            foreach (FileInfo diChild in diParent.GetFiles())
            {
                reader = new CsvStreamReader(diChild.FullName);
                List<DataPoint> timeseries = new List<DataPoint>();
                Console.WriteLine(diChild.Name+" has loeaded!") ;
                for (int i = 2; i <= reader.RowCount; i++)
                {
                    DataPoint data = new DataPoint() ;
                    data.Date = DateTime.Parse(reader[i,1]) ;
                    data.Value = double.Parse(reader[i, 2]);
                    timeseries.Add(data);
                }
                timeseriesList.Add(timeseries);
            }

            for (int j = 0; j < timeseriesList.Count; j++)
            {
                TSTypeDetector series = new TSTypeDetector(timeseriesList[j]);
                List<int> periods = series.getPeriods();
                Console.WriteLine("the time series:" + diParent.GetFiles()[j].Name );
                switch (series.GetTimeSeriesType())
                {
                    case TimeSeriesType.PERIODICAL:
                        for (int i = 0; i < periods.Count; i++)
                        {
                            Console.WriteLine("the " + i + "th period is" + periods[i]);
                        }
                        break;
                    case TimeSeriesType.LINEAR:
                        Console.WriteLine("The time series is linear!");
                        break ;
                    case TimeSeriesType.UNKNOWN:
                        Console.WriteLine("The time series dose not have any thing!");
                        break;
                }
            }

            Console.ReadLine();
        }
    }
}

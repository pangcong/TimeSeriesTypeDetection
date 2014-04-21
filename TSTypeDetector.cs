using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.SkyTerrace.Algorithm.Basic;

namespace TimeSeriesAnalysis
{
    public enum TimeSeriesType
    {
        PERIODICAL,
        LINEAR,
        UNKNOWN
    }

    /// <summary>
    /// This class detects the type of time series
    /// 1. Periodical
    /// 2. Linear
    /// 3. Others
    /// </summary>
    public class TSTypeDetector
    {

        // the thershold for the linear regression
        private double linearlyDependentCoefficientThershold = 0.05;

        // the list of all the period calculated by the method
        private List<int> periodList = new List<int>();

        // the level of a time series
        private double level = 0;

        //the trend of a time series
        private double trend = 0;

        // A list use to accuracy the period result
        private List<int> accPeriodList = new List<int>();

        // interval of the time series
        private TimeSpan interval = new TimeSpan();

        // the timeserieType input by the caller
        private TimeSeriesType timeSerieType = TimeSeriesType.UNKNOWN;

        // return whether the time series is periodicity 
        public bool isPeriod()
        {
            if (this.timeSerieType == TimeSeriesType.PERIODICAL)
                return true;
            else
                return false;
        }

        //return the level of the time series
        public double getLevel()
        {
            return level;
        }

        //return the trend of the time series
        public double getTrend()
        {
            return trend;
        }

        //return the periods of the time series
        public List<int> getPeriods()
        {
            for (int i = 0; i < this.periodList.Count; i++)
            {
                if (i>0)
                    this.periodList[i] = this.periodList[i] * this.periodList[i - 1];
                for (int j=0;j<this.accPeriodList.Count;j++)
                {
                    if (Math.Abs(this.accPeriodList[j] - this.periodList[i]) < (this.accPeriodList[j] / 10))
                    {
                        this.periodList[i] = this.accPeriodList[j]; 
                    }
                }

            }
            return this.periodList;
        }

        //return the interval of the data
        public TimeSpan getInterval()
        {
            return this.interval ;
        }

        /// <summary>
        /// return the type of the time series
        /// This class detects the type of time series
        /// 1. Periodical
        /// 2. Linear
        /// 3. Others
        /// </summary>
        /// <returns>the type: 1 for Periodical,2 for Linear,3 for Others</returns>
        public TimeSeriesType GetTimeSeriesType ()
        {
            return this.timeSerieType;
        }

        // using a time series to construct a Timeseries class
        public TSTypeDetector(List<DataPoint> sourceList, TimeSeriesType timeSerieType = TimeSeriesType.UNKNOWN)
        {
            this.timeSerieType = timeSerieType;

            // pre-assign the period list
            initialAccList();

            if (sourceList == null) return;

            // transfer it to the list
            List<double> timeseries = this.dataPointToList(sourceList);

            // calculate the time interval
            this.interval = this.calculateInterval(this.dataPointToData(sourceList));

            switch (timeSerieType)
            {
                case TimeSeriesType.LINEAR:
                    this.LevelTrendEvaluation(timeseries);
                    this.timeSerieType = TimeSeriesType.LINEAR;
                    break;
                case TimeSeriesType.PERIODICAL:
                    this.periodList.Add(this.PeriodEvaluation(timeseries));

                    int period = this.periodList[0] ;
                    int index = 0 ;
                    while (this.TypeDetection(this.smoothing(timeseries, period)) == TimeSeriesType.PERIODICAL)
                    {
                        index++;
                        period *= this.periodList[index];
                    }

                    this.timeSerieType = TimeSeriesType.PERIODICAL;
                    break;
                default:
                    TimeSeriesType type = this.TypeDetection(timeseries);
                    this.timeSerieType = type ;

                    if (type == TimeSeriesType.PERIODICAL)
                    {
                        period = this.periodList[0] ;
                        index = 0 ;
                        while (this.TypeDetection(this.smoothing(timeseries,period)) == TimeSeriesType.PERIODICAL)
                        {
                            index ++ ;
                            period *= this.periodList[index] ;
                        }
                    }
                    

                    break;
            }
        }

        /// <summary>
        /// Evuation period when we know it is periodical
        /// </summary>
        /// <param name="timeseries"></param>
        private int PeriodEvaluation(List<double> timeseries)
        {
            List<double> ffSeries;
            FastFourierTransform.FFT(timeseries, out ffSeries);

            ffSeries[0] = 0;
            return timeseries.Count / (1+ffSeries.IndexOf(ffSeries.Max()));
        }

        /// <summary>
        /// Detect Type if we don not know the type of the time series
        /// </summary>
        /// <param name="timeseries"></param>
        /// <returns></returns>
        private TimeSeriesType TypeDetection(List<double> timeseries)
        {
            TimeSeriesType seriesType = TimeSeriesType.UNKNOWN;

            List<double> ffSeries;

            int n = FastFourierTransform.FFT(timeseries, out ffSeries);

            double temp = ffSeries[0];
            ffSeries[0] = 0;
            int frequency = ffSeries.IndexOf(ffSeries.Max());
            ffSeries[0] = temp ;

            if (ffSeries[frequency] / ffSeries[0] > 0.1 && ffSeries[frequency] > ffSeries[frequency - 1] && ffSeries[frequency] > ffSeries[frequency + 1])
            {
                int period = (int)Math.Ceiling((double)n / (double)frequency - 0.5) ;
                this.periodList.Add(period);
                seriesType = TimeSeriesType.PERIODICAL; 
            }
            else if (this.isLinerly(timeseries))
            {
                seriesType = TimeSeriesType.LINEAR;
            }
            
            return seriesType;
        }

        // initial accuracy list
        private void initialAccList()
        {
            this.accPeriodList.Add(12);
            this.accPeriodList.Add(60);
            this.accPeriodList.Add(288);
            this.accPeriodList.Add(1440);
            this.accPeriodList.Add(2016);
            this.accPeriodList.Add(10080);
        }

        // Data Point to List
        private List<double> dataPointToList(IList<DataPoint> sourceData)
        {
            List<double> series = new List<double>();
            foreach (var aPoint in sourceData)
            {
                series.Add(aPoint.Value);
            }
            return series;
        }

        // Data Point to times
        private List<DateTime> dataPointToData(IList<DataPoint> sourceData)
        {
            List<DateTime> series = new List<DateTime>();
            foreach (var aPoint in sourceData)
            {
                series.Add(aPoint.Date);
            }
            return series;
        }

        // calculate the interval
        private TimeSpan calculateInterval(List<DateTime> times)
        {
            List<TimeSpan> spans = new List<TimeSpan>();
            for (int i = 0; i < times.Count - 1; i++)
            {
                spans.Add(times[i+1]-times[i]);
            }
            return this.findMajoritySpan(spans);
        }


        // smoothing the time series using a period value 
        private List<double> smoothing(List<double> timeseries, int noise)
        {
            int steps = timeseries.Count/noise;

            List<double> timeSeriesSmoothing = new List<double>() ;

            List<double> timeseriesTemp = new List<double> ();

            for (int j=0;j<noise;j++)
            {
                timeseriesTemp.Add (timeseries[j]) ;
            }
            timeSeriesSmoothing.Add(timeseriesTemp.Sum()/noise) ;
            timeseriesTemp.Clear() ;

            for (int i=1; i<steps ;i++)
            {
                for (int j = i * noise; j < (i + 1) * noise; j++)
                {
                    timeseriesTemp.Add(timeseries[j]) ;
                }
                timeSeriesSmoothing.Add(timeseriesTemp.Sum()/noise) ;
                timeseriesTemp.Clear();
            }
            
            return timeSeriesSmoothing ;
        }

        // calculate the trend of a time series
        private void LevelTrendEvaluation(List<double> timeseries)
        {

            double[,] a = new double[timeseries.Count, 2];
            double[,] b = new double[timeseries.Count, 1];

            for (int i = 0; i < timeseries.Count; i++)
            {
                a[i, 0] = i;
                a[i, 1] = 1;
                b[i, 0] = timeseries[i];
            }


            double[,] c = MatrixEx.Solve(a, b);
            this.trend = c[0, 0];
            this.level = c[1, 0];

        }

        // calculate a majority span 
        private TimeSpan findMajoritySpan(List<TimeSpan> spans)
        {
            if (spans.Count == 0)
                return TimeSpan.MaxValue;

            List<int> count = new List<int>();
            List<TimeSpan> value = new List<TimeSpan>();

            for (int i = 0; i < spans.Count; i++)
            {
                if (value.Contains(spans[i]))
                {
                    count[value.IndexOf(spans[i])]++;
                }
                else
                {
                    value.Add(spans[i]);
                    count.Add(1);
                }
            }
            return value[count.IndexOf(count.Max())];
        }

        // Judge wether the the list is linearizable
        private bool isLinerly (List<double> Y)
        {
            this.LevelTrendEvaluation(Y);
            
            List<double> X = new List<double>();

            for (int i = 0; i < Y.Count; i++)
            {
                X.Add(i*this.trend + level);
            }
            double LDCoeficient = this.linearlyCoefficient(X, Y);

            if (Math.Abs(LDCoeficient) > this.linearlyDependentCoefficientThershold) 
                return true;
            else
                return false;
        }

        // Calculate the linearly Dependent Coefficient
        private double linearlyCoefficient(List<double> X, List<double> Y)
        {
            if (X.Count == 0 || Y.Count == 0 || X.Count != Y.Count)
                return 0.0;
            double xVariance = 0.0;
            double yVariance = 0.0;
            double xyVariance = 0.0;
            double xMean = X.Average() ;
            double yMean = Y.Average() ;

            for (int i = 0; i < X.Count; i++)
            {
                xVariance += (X[i] - xMean)*(X[i] - xMean);
                yVariance += (Y[i] - yMean)*(Y[i] - yMean);
                xyVariance += (X[i] - xMean) * (Y[i] - yMean);
            }

            if (Math.Sqrt(yVariance / X.Count) < yMean*0.02)
                return 1;
            else
                return  xyVariance / Math.Sqrt(xVariance * yVariance);
        }
    }

    public class DataPoint
    {
        public DateTime Date { get; set; }
        public double Value { get; set; }
    }
}

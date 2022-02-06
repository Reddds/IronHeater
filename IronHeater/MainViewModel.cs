using OxyPlot;
using OxyPlot.Series;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;
using OxyPlot.Axes;
using OxyPlot.Annotations;

namespace IronHeater
{

    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        // try to change might be lower or higher than the rendering interval
        private const int UpdateInterval = 300;

        private bool disposed;
        private readonly Timer timer;
        private readonly Stopwatch watch = new Stopwatch();
        private int numberOfSeries;

        public MainViewModel()
        {
            this.timer = new Timer(OnTimerElapsed);
            this.Function = (t, x, a) => Math.Cos(t * a) * (x == 0 ? 1 : Math.Sin(x * a) / x);

            SetupModel();
        }



        private void SetupModel()
        {
            this.timer.Change(Timeout.Infinite, Timeout.Infinite);

            PlotModel = new PlotModel();
            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = 0,
                Maximum = 400,
                MajorGridlineStyle = LineStyle.Dot,
                //MajorGridlineColor = OxyColor.Gra
            });

            PlotModel.Axes.Add(new TimeSpanAxis 
            { 
                Position = AxisPosition.Bottom, 
                Minimum = 0,
                Maximum = TimeSpanAxis.ToDouble(TimeSpan.FromMinutes(10)), 
                StringFormat = "mm:ss" 
            });

            PlotModel.IsLegendVisible = true;

            LineAnnotation Line = new LineAnnotation()
            {
                StrokeThickness = 1,
                Color = OxyColors.Green,
                Type = LineAnnotationType.Horizontal,
                Text = (1).ToString(),
                TextColor = OxyColors.White,
                X = 0,
                Y = 1
            };

            PlotModel.Annotations.Add(Line);

            this.numberOfSeries = 2;

            for (int i = 0; i < this.numberOfSeries; i++)
            {
                PlotModel.Series.Add(new LineSeries { LineStyle = LineStyle.Solid });
            }

            this.watch.Start();

            //this.RaisePropertyChanged("PlotModel");

            //this.timer.Change(1000, UpdateInterval);
        }

        public int TotalNumberOfPoints { get; private set; }

        private Func<double, double, double, double> Function { get; set; }

        public PlotModel PlotModel { get; private set; }

        private void OnTimerElapsed(object state)
        {
            lock (this.PlotModel.SyncRoot)
            {
                this.Update();
            }

            this.PlotModel.InvalidatePlot(true);
        }

        public void AddData(double t1, double t2, TimeSpan time)
        {
            lock (this.PlotModel.SyncRoot)
            {
                int n = 0;
                var x = TimeSpanAxis.ToDouble(time);

                var s1 = (LineSeries)PlotModel.Series[0];
                s1.Points.Add(new DataPoint(x, t1));
                n += s1.Points.Count;

                var s2 = (LineSeries)PlotModel.Series[1];
                s2.Points.Add(new DataPoint(x, t2));
                n += s2.Points.Count;

                if (TotalNumberOfPoints != n)
                {
                    TotalNumberOfPoints = n;
                    RaisePropertyChanged("TotalNumberOfPoints");
                }
            }

            this.PlotModel.InvalidatePlot(true);
        }

        private void Update()
        {
            double t = this.watch.ElapsedMilliseconds * 0.001;
            int n = 0;

            for (int i = 0; i < PlotModel.Series.Count; i++)
            {
                var s = (LineSeries)PlotModel.Series[i];


                double x = s.Points.Count > 0 ? s.Points[s.Points.Count - 1].X + 1 : 0;
                //if (s.Points.Count >= 200)
                //    s.Points.RemoveAt(0);
                double y = 0;
                int m = 80;
                for (int j = 0; j < m; j++)
                    y += Math.Cos(0.001 * x * j * j);
                y /= m;
                y += i;
                s.Points.Add(new DataPoint(x, y));

                n += s.Points.Count;
            }

            if (this.TotalNumberOfPoints != n)
            {
                this.TotalNumberOfPoints = n;
                this.RaisePropertyChanged("TotalNumberOfPoints");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string property)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.timer.Dispose();
                }
            }

            this.disposed = true;
        }
    }
}

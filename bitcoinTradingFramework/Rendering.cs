using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
#if WINDOWS
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

#endif

namespace bitcoinTradingFramework
{
    internal class MarkerContainer
    {
        public bool m_actioned;
        public decimal m_price;
        public bool m_up;
    }

    internal struct Vector2
    {
        public double m_x;
        public double m_y;

        public Vector2(double x, double y)
        {
            m_x = x;
            m_y = y;
        }

        public Vector2(int x, int y) : this(x, (double) y)
        {
        }
    }

    public class Rendering
    {
#if WINDOWS
        private const double kZoomScale = 0.05;
        private const int kBorder = 5;

        private Form m_debugRenderForm;
        private readonly int m_width;
        private readonly int m_height;

        private const string kAskSeriesName = "Ask";
        private const string kBidSeriesName = "Bid";
        private const string kPriceSeriesName = "price";
        private const string kChartAreaName = "area";

        private const string kAskSeries = "ask";
        private const string kBidSeries = "bid";
        //const string kDepthChartAreaName = "depth";

        private const string kProfitChartAreaName = "profitarea";
        private const string kProfitSeriesName = "ROI %";

        private const string kDateFormat = "dd/MMM\nhh:mm";

        private const int kNumCharts = 2;

        private Chart m_chart;
        private ChartArea m_area;
        private double m_deltaScrollTotal;
        private Vector2? m_mouseDown;
        private HitTestResult m_mouseOverResult;

        //Chart m_depthChart;
        //ChartArea m_depthArea;

        private Chart m_profitChart;
        private ChartArea m_profitArea;

        private readonly List<decimal> m_bidData;
        private readonly List<decimal> m_askData;
        private readonly List<decimal> m_lastPrice;
        private readonly List<DateTime> m_times;

        private List<decimal> m_tradeDistanceFromTrue;
        private List<decimal> m_tradeVolume;

        private readonly List<decimal> m_profitPercentage;
        private readonly List<DateTime> m_profitTimeSeconds;

        private readonly Dictionary<DateTime, List<MarkerContainer>> m_markers;

        public Rendering(int width, int height)
        {
            m_width = width;
            m_height = height;

            m_bidData = new List<decimal>();
            m_askData = new List<decimal>();
            m_lastPrice = new List<decimal>();
            m_times = new List<DateTime>();

            m_tradeDistanceFromTrue = new List<decimal>();
            m_tradeVolume = new List<decimal>();

            m_profitPercentage = new List<decimal>();
            m_profitTimeSeconds = new List<DateTime>();

            m_markers = new Dictionary<DateTime, List<MarkerContainer>>();

            // create a windows thread for the display
            var windowsThread = new Thread(WindowsThread);
            windowsThread.Start();
        }

        /// <summary>
        /// </summary>
        /// <param name="smaF"></param>
        /// <param name="smaS"></param>
        /// <param name="price"></param>
        /// <param name="now"></param>
        public void AddDataPoint(decimal smaF, decimal smaS, decimal price, DateTime now, bool reformatGraph = true)
        {
            if (_formIsClosing)
                return;

            m_debugRenderForm.BeginInvoke((Action) (() =>
            {
                int i = m_bidData.Count - 1;
                var halfHour = new TimeSpan(0, 30, 0);
                while (i >= 0 && now - m_times[i] < halfHour)
                {
                    i--;
                }

                if (i > 0)
                {
                    m_askData.RemoveRange(0, i);
                    m_bidData.RemoveRange(0, i);
                    m_lastPrice.RemoveRange(0, i);

                    lock (m_times)
                    {
                        m_times.RemoveRange(0, i);
                    }

                    foreach (var kvp in m_markers.ToList())
                    {
                        if (now - kvp.Key > halfHour)
                        {
                            m_markers.Remove(kvp.Key);
                        }
                    }
                }

                m_bidData.Add(smaF);
                m_askData.Add(smaS);
                m_lastPrice.Add(price);
                lock (m_times)
                {
                    m_times.Add(now);
                }

                if (reformatGraph)
                {
                    ReformatGraphInternal();
                }
            }));
        }


        /// <summary>
        /// </summary>
        /// <param name="profitPercentage"></param>
        /// <param name="timeSeconds"></param>
        public void AddProfitDataPoints(decimal profitPercentage, DateTime timeSeconds)
        {
            if (_formIsClosing)
                return;

            m_debugRenderForm.BeginInvoke((Action) (() =>
            {
                /*if (m_profitPercentage.Count > kBufferedPoints)
				{
					m_profitPercentage.RemoveRange(0, 1);
					m_profitTimeSeconds.RemoveRange(0, 1);
				}*/

                m_profitPercentage.Add(profitPercentage);
                m_profitTimeSeconds.Add(timeSeconds);

                m_profitChart.Series[kProfitSeriesName].Points.DataBindXY(m_profitTimeSeconds, m_profitPercentage);

                decimal ymin = m_profitPercentage.Min();
                decimal ymax = m_profitPercentage.Max();
                decimal range = ymax - ymin;

                if (range > 0)
                {
                    m_profitArea.AxisY.Minimum = (double) (ymin - range/2);
                    m_profitArea.AxisY.Maximum = (double) (ymax + range/2);
                }

                m_profitChart.Invalidate();
            }));
        }

        /// <summary>
        /// </summary>
        /// <param name="up"></param>
        public void AddMarker(bool up, bool actioned, decimal price, DateTime? time = null)
        {
            lock (m_markers)
            {
                DateTime last;

                if (time != null)
                {
                    last = (DateTime) time;
                }
                else
                {
                    lock (m_times)
                    {
                        last = m_times.Last();
                    }
                }

                if (!m_markers.ContainsKey(last))
                {
                    m_markers[last] = new List<MarkerContainer>();
                }

                m_markers[last].Add(new MarkerContainer {m_actioned = actioned, m_up = up, m_price = price});
            }
        }


        /// <summary>
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private int FindIndexOfDate(DateTime d)
        {
            for (int i = 0; i < m_times.Count; i++)
            {
                if (m_times[i] == d)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private int FindClosestDate(DateTime d)
        {
            int closestI = -1;
            double closestSeconds = double.MaxValue;

            for (int i = 0; i < m_times.Count; i++)
            {
                double dd = Math.Abs((m_times[i] - d).TotalSeconds);
                if (dd < closestSeconds)
                {
                    closestSeconds = dd;
                    closestI = i;
                }
            }

            return closestI;
        }

        /// <summary>
        /// </summary>
        private void ReformatGraphInternal()
        {
            if (m_askData.Count > 0)
            {
                m_chart.Series[kAskSeriesName].Points.DataBindXY(m_times, m_askData);
                m_chart.Series[kBidSeriesName].Points.DataBindXY(m_times, m_bidData);
                m_chart.Series[kPriceSeriesName].Points.DataBindXY(m_times, m_lastPrice);

                m_chart.Annotations.Clear();

                decimal markerMin = decimal.MaxValue;
                decimal markerMax = 0;
                lock (m_markers)
                {
                    foreach (var kvp in m_markers)
                    {
                        int index = FindClosestDate(kvp.Key);

                        foreach (MarkerContainer c in kvp.Value)
                        {
                            if (index != -1)
                            {
                                var annotation = new ArrowAnnotation {ArrowSize = 3, ArrowStyle = ArrowStyle.Simple};

                                annotation.AnchorDataPoint = m_chart.Series[kAskSeriesName].Points[index];
                                if (c.m_actioned)
                                {
                                    annotation.BackColor = c.m_up ? Color.Green : Color.Red;
                                }
                                annotation.ForeColor = c.m_actioned ? Color.Green : Color.Red;
                                annotation.Width = 0;
                                annotation.Height = c.m_up ? 5 : -5;
                                annotation.SmartLabelStyle.IsOverlappedHidden = false;
                                annotation.AnchorY = (double) c.m_price;

                                markerMin = Math.Min(c.m_price, markerMin);
                                markerMax = Math.Max(c.m_price, markerMax);

                                // only keep unaction markers which are less than 10 mins old
                                bool allow = (!c.m_actioned && (DateTime.UtcNow - kvp.Key) < new TimeSpan(0, 10, 0)) ||
                                             c.m_actioned;
                                if (allow)
                                {
                                    m_chart.Annotations.Add(annotation);
                                }
                            }
                        }
                    }
                }


                decimal ymin = Math.Min(Math.Min(Math.Min(m_askData.Min(), m_bidData.Min()), m_lastPrice.Min()),
                    markerMin);
                decimal ymax = Math.Max(Math.Max(Math.Max(m_askData.Max(), m_bidData.Max()), m_lastPrice.Max()),
                    markerMax);
                decimal range = ymax - ymin;

                m_area.AxisY.Minimum = (double) (ymin - range/2);
                m_area.AxisY.Maximum = (double) (ymax + range/2);

                m_chart.Invalidate();
            }
        }

        /// <summary>
        /// </summary>
        public void ReformatGraph()
        {
            if (_formIsClosing)
                return;

            m_debugRenderForm.BeginInvoke((Action) (() => { ReformatGraphInternal(); }));
        }

        [STAThread]
        private void WindowsThread()
        {
            Application.EnableVisualStyles();

            m_debugRenderForm = new Form {Width = m_width, Height = m_height, Text = "Debug render"};
            m_debugRenderForm.Paint += RenderDebug;
            m_debugRenderForm.SizeChanged += OnWindowResized;
            m_debugRenderForm.Show();

            m_chart = new Chart {Size = new Size(m_width - kBorder, m_height/kNumCharts - kBorder*4)};
            //m_depthChart = new Chart { Size = new Size(m_width - kBorder, m_height / kNumCharts - kBorder * 4), Top = m_chart.Bottom };
            //m_profitChart = new Chart { Size = new Size(m_width - kBorder, m_height / kNumCharts - kBorder * 4), Top = m_depthChart.Bottom };
            m_profitChart = new Chart
            {
                Size = new Size(m_width - kBorder, m_height/kNumCharts - kBorder*4),
                Top = m_chart.Bottom
            };

            var smaSLegend = new Legend(kAskSeriesName);
            var smaFLegend = new Legend(kBidSeriesName);
            var priceLegend = new Legend(kPriceSeriesName);
            m_chart.Legends.Add(smaSLegend);
            m_chart.Legends.Add(smaFLegend);
            m_chart.Legends.Add(priceLegend);

            //m_depthChart.Legends.Add( new Legend(kAskSeries) );
            //m_depthChart.Legends.Add( new Legend(kBidSeries) );

            m_profitChart.Legends.Add(new Legend(kProfitSeriesName));

            var f = new Font("Consolas", 8);

            // market trades ticker and moving average
            m_area = new ChartArea();
            m_area.AxisX.LabelStyle.Format = kDateFormat;
            m_area.AxisX.MajorGrid.LineColor = Color.LightGray;
            m_area.AxisY.MajorGrid.LineColor = Color.LightGray;
            m_area.AxisX.LabelStyle.Font = m_area.AxisY.LabelStyle.Font = f;
            m_area.Name = kChartAreaName;

            // trade analysis
            /*m_depthArea = new ChartArea();
			m_depthArea.AxisX.MajorGrid.LineColor = Color.LightGray;
			m_depthArea.AxisY.MajorGrid.LineColor = Color.LightGray;
			m_depthArea.AxisX.LabelStyle.Font = m_depthArea.AxisY.LabelStyle.Font = f;
			m_depthArea.Name = kDepthChartAreaName;*/

            // profit analysis
            m_profitArea = new ChartArea();
            m_profitArea.AxisX.MajorGrid.LineColor = Color.LightGray;
            m_profitArea.AxisY.MajorGrid.LineColor = Color.LightGray;
            m_profitArea.AxisY.LabelStyle.Format = "{0.00}";
            m_profitArea.AxisX.LabelStyle.Format = kDateFormat;
            m_profitArea.AxisX.LabelStyle.Font = m_profitArea.AxisY.LabelStyle.Font = f;
            m_profitArea.Name = kProfitChartAreaName;

            //m_depthArea.AxisY.ScaleView.Zoomable = m_area.AxisX.ScaleView.Zoomable = true;


            m_area.AxisX.ScrollBar.Enabled = true;
            m_area.AxisY.ScrollBar.Enabled = true;

            //m_chart.MouseDown += new MouseEventHandler(OnMouseDown);
            //m_chart.MouseMove += new MouseEventHandler(OnMouseMove);
            //m_chart.MouseUp += new MouseEventHandler(OnMouseUp);
            //m_chart.MouseWheel += new MouseEventHandler(OnMouseWheel);
            //m_chart.PostPaint += new EventHandler<ChartPaintEventArgs>(OnPostPaint);


            m_chart.ChartAreas.Add(m_area);
            m_profitChart.ChartAreas.Add(m_profitArea);

            //
            // series for prices
            //

            var series = new Series();
            series.Name = series.Legend = kAskSeriesName;
            series.ChartType = SeriesChartType.StepLine;
            series.XValueType = ChartValueType.DateTime;
            series.BorderWidth = 2;
            m_chart.Series.Add(series);

            series = new Series();
            series.Name = series.Legend = kBidSeriesName;
            series.ChartType = SeriesChartType.StepLine;
            series.XValueType = ChartValueType.DateTime;
            series.BorderWidth = 2;
            m_chart.Series.Add(series);

            series = new Series();
            series.Name = series.Legend = kPriceSeriesName;
            series.ChartType = SeriesChartType.StepLine;
            series.XValueType = ChartValueType.DateTime;
            series.BorderWidth = 2;
            m_chart.Series.Add(series);


            //
            // series for profit
            //

            series = new Series();
            series.Name = series.Legend = kProfitSeriesName;
            series.ChartType = SeriesChartType.StepLine;
            series.XValueType = ChartValueType.DateTime;
            series.YValueType = ChartValueType.Double;
            m_profitChart.Series.Add(series);

            // bind the datapoints
            //chart.Series["Series1"].Points.DataBindXY(xvals, yvals);


            m_debugRenderForm.Controls.AddRange(new Control[] {m_chart, m_profitChart});
            m_debugRenderForm.FormClosing += m_debugRenderForm_FormClosing;

            Application.Run(m_debugRenderForm);
        }

        private bool _formIsClosing;

        private void m_debugRenderForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _formIsClosing = true;
        }

        /// <summary>
        /// </summary>
        /// <param name="cg"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private PointF ToPixelsPos(ChartGraphics cg, double x, double y)
        {
            var p = new PointF((float) cg.GetPositionFromAxis(kChartAreaName, AxisName.X, x),
                (float) cg.GetPositionFromAxis(kChartAreaName, AxisName.Y, y));


            return cg.GetAbsolutePoint(p);
        }


        private void OnPostPaintDepth(object sender, ChartPaintEventArgs e)
        {
            /*ChartGraphics cg = e.ChartGraphics;

			PointF p = new PointF { X = (float)cg.GetPositionFromAxis(kDepthChartAreaName, AxisName.X, m_vwap) };
			p = cg.GetAbsolutePoint(p);

			Pen bp = new Pen(Color.Black);

			cg.Graphics.DrawLine(bp, p.X, 0, p.X, m_depthChart.Height);*/
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowResized(object sender, EventArgs e)
        {
            m_chart.Width = m_profitChart.Width = m_debugRenderForm.Width - kBorder;
            m_chart.Height = m_profitChart.Height = m_debugRenderForm.Height/kNumCharts - kBorder*4;

            m_profitChart.Top = m_chart.Bottom;

            m_chart.Invalidate();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            double deltaScroll = kZoomScale*e.Delta/Math.Abs(e.Delta);

            m_deltaScrollTotal += deltaScroll;

            Vector2 x, y;
            GetZoomRange(e, out x, out y);

            m_area.AxisX.ScaleView.Zoom(x.m_x, x.m_y);
            m_area.AxisY.ScaleView.Zoom(y.m_x, y.m_y);
        }

        /// <summary>
        /// </summary>
        /// <param name="e"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void GetZoomRange(MouseEventArgs e, out Vector2 x, out Vector2 y, Vector2? startPos = null)
        {
            // Additional calculation in order to obtain pseudo
            // "positional zoom" feature

            var now = new Vector2(e.X, e.Y);

            double minXScale = e.X/(double) m_chart.Width;
            double maxXScale = 1 - minXScale;
            double minYScale = e.Y/(double) m_chart.Height;
            double maxYScale = 1 - minYScale;

            // Max and min values into which axis need to be scaled/zoomed
            double minX = m_area.AxisX.Minimum + m_deltaScrollTotal*minXScale;
            double maxX = m_area.AxisX.Maximum - m_deltaScrollTotal*maxXScale;

            double minY = m_area.AxisY.Minimum + m_deltaScrollTotal*maxYScale;
            double maxY = m_area.AxisY.Maximum - m_deltaScrollTotal*minYScale;


            x = new Vector2(minX, maxX);
            y = new Vector2(minY, maxY);
        }


        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            m_mouseDown = null;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            /*if (m_mouseDown != null)
			{
				Vector2 x, y;
				GetZoomRange(e, out x, out y);

				m_area.AxisX.ScaleView.Zoom(x.m_x, x.m_y);
				m_area.AxisY.ScaleView.Zoom(y.m_x, y.m_y);
			}*/

            // Call HitTest
            HitTestResult result = m_chart.HitTest(e.X, e.Y);

            // If the mouse if over a data point
            if (result.ChartElementType == ChartElementType.DataPoint)
            {
                // Find selected data point
                //DataPoint point = m_chart.Series[kSmaSSeriesName].Points[result.PointIndex];

                m_mouseOverResult = result;
                m_chart.Invalidate();
            }
            else
            {
                m_mouseOverResult = null;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            m_mouseDown = new Vector2(e.X, e.Y);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RenderDebug(object sender, PaintEventArgs e)
        {
        }

#else

		public Rendering(int width, int height, int numFast, int numSlow) { }
		public void ReformatGraph() 	{}
		public void AddDataPoint(decimal smaF, decimal smaS, decimal price, DateTime now, decimal volatility, bool reformatGraph = true) { }
		public void AddProfitDataPoints(decimal profitPercentage, double timeSeconds) { }
		#endif
    }
}
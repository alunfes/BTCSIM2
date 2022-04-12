using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace BTCSIM2
{
    static public class MarketData
    {
        static private List<double> unix_time;
        static private List<DateTimeOffset> dt;
        static private List<double> open;
        static private List<double> high;
        static private List<double> low;
        static private List<double> close;
        static private List<double> volume;
        static public List<int> terms;
        static private Dictionary<int, List<double>> sma;
        static private Dictionary<int, List<double>> divergence;

        static public ref List<double> UnixTime
        {
            get { return ref unix_time; }
        }
        static public ref List<DateTimeOffset> Dt
        {
            get { return ref dt; }
        }
        static public ref List<double> Open
        {
            get { return ref open; }
        }
        static public ref List<double> High
        {
            get { return ref high; }
        }
        static public ref List<double> Low
        {
            get { return ref low; }
        }
        static public ref List<double> Close
        {
            get { return ref close; }
        }
        static public ref List<double> Volume
        {
            get { return ref volume; }
        }
        static public ref Dictionary<int, List<double>> Sma
        {
            get { return ref sma; }
        }
        static public ref Dictionary<int, List<double>> Divergence
        {
            get { return ref divergence; }
        }

        static public void initializer(List<int> terms_list)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            unix_time = new List<double>();
            dt = new List<DateTimeOffset>();
            open = new List<double>();
            high = new List<double>();
            low = new List<double>();
            close = new List<double>();
            volume = new List<double>();
            sma = new Dictionary<int, List<double>>();
            divergence = new Dictionary<int, List<double>>();
            read_data();
            calc_index(terms_list);
        }

        static private void read_data()
        {
            var d = Directory.GetFiles(@"./Data");
            StreamReader sr = new StreamReader(@"./Data/BTC-PERP-1m(mock).csv");
            sr.ReadLine();
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                var data = line.Split(',');
                //unix_time.Add(Convert.ToDouble(data[6]));
                dt.Add(DateTimeOffset.FromUnixTimeSeconds(Convert.ToUInt32(Convert.ToDouble(data[1]) / 1000)));
                //dt.Add(Convert.ToDateTime(Convert.ToInt32(data[0])/1000));
                open.Add(Convert.ToDouble(data[2]));
                high.Add(Convert.ToDouble(data[3]));
                low.Add(Convert.ToDouble(data[4]));
                close.Add(Convert.ToDouble(data[5]));
                volume.Add(Convert.ToDouble(data[6]));
            }
            /*
            Console.WriteLine("datetime, open, high, low, close, volume");
            for (int i = 0; i < 30; i++)
            {
                Console.WriteLine(dt[i].ToString() + ", " + open[i].ToString() + ", " + high[i].ToString() + ", " + low[i].ToString() + ", " + close[i].ToString() + ", " + volume[i].ToString());
            }
            */
            Console.WriteLine("Completed read data.");
        }


        //
        static public void write_data(int ma_term)
        {
            using (StreamWriter sw = new StreamWriter(@"./Data/MarketData.csv"))
            {
                sw.WriteLine("dt,open,high,low,close,volume,sma-" + ma_term.ToString()+",sma divergence-"+ma_term.ToString());
                for(int i= close.Count-1000; i<close.Count; i++)
                {
                    sw.WriteLine(dt[i].ToString() + "," + open[i].ToString() + "," + high[i].ToString() + "," + low[i].ToString() + "," +
                        close[i].ToString() + "," + volume[i].ToString() + "," + Sma[ma_term][i].ToString() + "," + Divergence[ma_term][i].ToString());
                }
            }
        }

        static private void calc_index(List<int> terms_list)
        {
            terms = terms_list;
            foreach (int t in terms)
            {
                sma[t] = new List<double>();
                divergence[t] = new List<double>();
                sma[t] = calc_sma(t, close);
                divergence[t] = calc_divergence(close, sma[t]);
            }

        }


        static private List<double> calc_sma(int term, List<double> data)
        {
            var res = new List<double>();
            //detect nan_ind
            var nan_ind = 0;
            for (int i = 0; i < data.Count; i++)
            {
                if (double.IsNaN(data[i]) == false)
                {
                    nan_ind = i;
                    break;
                }
            }
            for (int i = 0; i < term + nan_ind - 1; i++) { res.Add(double.NaN); }
            var sumv = 0.0;
            for (int i = nan_ind; i < term + nan_ind; i++) { sumv += data[i]; }
            res.Add(sumv / term);
            for (int i = term + nan_ind; i < data.Count; i++)
            {
                sumv = sumv - data[i - term] + data[i];
                res.Add(sumv / term);
            }
            return res;
        }

        static private List<double> calc_divergence(List<double> price, List<double> ma)
        {
            List<double> res = new List<double>();
            if (price.Count == ma.Count)
            {
                for (int i = 0; i < price.Count; i++)
                {
                    if (price[i] == double.NaN || ma[i] == double.NaN) { res.Add(double.NaN); }
                    else if (price[i] == 0 && ma[i] == 0) { res.Add(0); }
                    else { res.Add(100.0 * (price[i] - ma[i]) / ma[i]); }
                }

            }
            else
                Console.WriteLine("marketData: Length is not matched in calc_divergence !");
            return res;
        }
    }
}
    
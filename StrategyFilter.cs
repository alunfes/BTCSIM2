using System;
using System.Linq;



namespace BTCSIM2
{
    public class StrategyFilter
    {
        private static object lock_master = new object();

        public static bool applyFilter(int i, ref Account ac, int filter_id, int kijun_time_window, double kijun_change, int kijun_time_suspension)
        {
            if (filter_id == 0)
                return priceChangeFilter(i, kijun_time_window, kijun_change);
            else if (filter_id == 1)
                return lcFilter(i, ref ac, kijun_time_suspension);
            else if (filter_id == 2)
                return lcPriceChangeFilter(i, ref ac, kijun_time_window, kijun_time_suspension, kijun_change);
            if (filter_id == 3)
                return false;//priceChangeRangeFilter(i, kijun_time_window, kijun_change);
            else
            {
                Console.WriteLine("StrategyFilter: Invalid filter id !");
                return false;
            }
        }

        private static bool priceChangeFilter(int i, int kijun_time_window, double kijun_change)
        {
            if (Math.Abs((MarketData.Close[i] - MarketData.Close[i - kijun_time_window]) / MarketData.Close[i - kijun_time_window]) >= kijun_change)
                return true;
            else
                return false;
        }

        //lcした場合にその後x時間のエントリーを中止
        private static bool lcFilter(int i, ref Account ac, int suspension_time)
        {
            if (ac.log_data.lc_points.Count > 0)
            {
                if (i - ac.log_data.lc_points[ac.log_data.lc_points.Count - 1] <= suspension_time)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }


        //一定時間にx%以上変動してlcとなった場合に、その後x時間のエントリーを中止
        private static bool lcPriceChangeFilter(int i, ref Account ac, int price_change_kijun_time_window, int suspension_time, double kijun_change)
        {
            if (priceChangeFilter(i, price_change_kijun_time_window, kijun_change))
            {
                return lcFilter(i, ref ac, suspension_time);
            }
            else
                return false;
        }


        //一定時間内の最大・最小の変化率が一定値以上の場合に中止
        private static bool priceChangeRangeFilter(int i, int kijun_time_window, double kijun_change)
        {
            var max_change = (MarketData.Close.GetRange(i - kijun_time_window, i + 1).Max() - MarketData.Close.GetRange(i - kijun_time_window, i + 1).Min()) / MarketData.Close.GetRange(i - kijun_time_window, i + 1).Min();
            if (max_change >= kijun_change)
                return true;
            else
                return false;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Linq;
using System.IO;
using System.Text;


namespace BTCSIM2
{
    public class PerformanceData
    {
        public double total_pl { get; set; }
        public double realized_pl { get; set; }
        public double total_capital { get; set; }
        public double initial_capital { get; set; }
        public List<double> total_pl_list { get; set; }
        public List<double> realized_pl_list { get; set; }
        public List<double> total_capital_list { get; set; }
        public double total_capital_variance { get; set; }
        public List<double> buy_pl_list { get; set; }
        public List<double> sell_pl_list { get; set; }
        public double unrealized_pl { get; set; }
        public List<double> unrealized_pl_list { get; set; } //record unrealided pl during holding period for NN input data
        public double unrealized_pl_ratio { get; set; }
        public List<double> unrealized_pl_ratio_list { get; set; }
        public double total_pl_ratio { get; set; }
        public double realized_pl_ratio_variance { get; set; }
        public List<double> buy_pl_ratio_list { get; set; }
        public List<double> sell_pl_ratio_list { get; set; }
        public double max_dd { get; set; }
        public double max_pl { get; set; }
        public int num_force_exit { get; set; }
        public double total_capital_gradient { get; set; }
        public List<double> log_close { get; set; }

        public int num_trade { get; set; }
        public List<int> num_trade_list { get; set; }
        public int num_buy { get; set; }
        public int num_sell { get; set; }
        public int num_maker_order { get; set; }
        public int num_win { get; set; }
        public double win_rate { get; set; }
        public double total_fee { get; set; }
        public double sharp_ratio { get; set; }

        public PerformanceData(double initial_cap)
        {
            total_pl = 0.0;
            total_pl_ratio = 0.0;
            realized_pl_ratio_variance = 0.0;
            total_capital = initial_cap;
            initial_capital = initial_cap;
            realized_pl = 0.0;
            total_pl_list = new List<double>();
            realized_pl_list = new List<double>();
            total_capital_list = new List<double>();
            total_capital_variance = 0.0;
            buy_pl_list = new List<double>();
            sell_pl_list = new List<double>();
            buy_pl_ratio_list = new List<double>();
            sell_pl_ratio_list = new List<double>();
            unrealized_pl = 0.0;
            unrealized_pl_list = new List<double>();
            unrealized_pl_ratio = 0.0; //
            unrealized_pl_ratio_list = new List<double>();
            log_close = new List<double>();
            num_trade = 0;
            num_trade_list = new List<int>();
            num_buy = 0;
            num_sell = 0;
            win_rate = 0;
            num_win = 0;
            num_maker_order = 0;
            total_fee = 0.0;
            sharp_ratio = 0.0;
            max_dd = 0.0;
            max_pl = 0.0;
            num_force_exit = 0;
            total_capital_gradient = 0.0;
        }
    }



    public class OrderData
    {
        public int order_serial_num { get; set; } //active order serial num list
        public List<int> order_serial_list { get; set; } //latest order serial num
        public Dictionary<int, string> order_side { get; set; }
        public Dictionary<int, double> order_size { get; set; }
        public Dictionary<int, double> order_price { get; set; }
        public Dictionary<int, string> order_type { get; set; } //market, limit, stop market
        public Dictionary<int, int> order_i { get; set; }
        public Dictionary<int, string> order_dt { get; set; }
        public Dictionary<int, Boolean> order_cancel { get; set; }
        public Dictionary<int, string> order_message { get; set; } //entry, pt, exit&entry
        public double pt_order { get; set; } //pt price should be checked when only holding exist and exact same volume shall be processed.
        public double lc_order { get; set; } //pt or lc order shall be immediaterly removed when another one is processed.


        public OrderData()
        {
            order_serial_num = -1;
            order_serial_list = new List<int>();
            order_side = new Dictionary<int, string>();
            order_size = new Dictionary<int, double>();
            order_price = new Dictionary<int, double>();
            order_type = new Dictionary<int, string>();
            order_i = new Dictionary<int, int>();
            order_dt = new Dictionary<int, string>();
            order_cancel = new Dictionary<int, bool>();
            order_message = new Dictionary<int, string>();
            pt_order = 0; //ptの値幅を入力、例えば５００ドルとか
            lc_order = 0;
        }
        public string getLastOrderSide()
        {
            if (order_serial_list.Count > 0)
                return order_side[order_serial_list.Last()];
            else
                return "";
        }
        public double getLastOrderSize()
        {
            if (order_serial_list.Count > 0)
                return order_size[order_serial_list.Last()];
            else
                return 0;
        }
        public double getLastOrderPrice()
        {
            if (order_serial_list.Count > 0)
                return order_price[order_serial_list.Last()];
            else
                return 0;
        }
        public int getNumOrders()
        {
            if (order_serial_list.Count > 0)
                return order_serial_list.Count;
            else
                return 0;
        }
        public int getLastSerialNum()
        {
            if (order_serial_list.Count() > 0)
                return order_serial_list.Last();
            else
                return -1;
        }
    }


    public class HoldingData
    {
        public string holding_side { get; set; }
        public double holding_price { get; set; }
        public double holding_size { get; set; }
        public double holding_volume { get; set; }
        public int holding_entry_num { get; set; }
        public int holding_i { get; set; }
        public int holding_period { get; set; }
        public int holding_initial_i { get; set; }
        public List<int> holding_period_list { get; set; }

        public HoldingData()
        {
            holding_i = -1;
            holding_period = 0;
            holding_price = 0;
            holding_size = 0;
            holding_volume = 0;
            holding_entry_num = 0;
            holding_side = "";
            holding_period_list = new List<int>();
        }

        public void initialize_holding()
        {
            holding_i = -1;
            holding_period = 0;
            holding_price = 0;
            holding_size = 0;
            holding_volume = 0;
            holding_entry_num = 0;
            holding_side = "";
        }


        public void update_holding(string side, double price, double size, int i)
        {
            if (holding_side == "") //New Entry
            {
                holding_side = side;
                holding_price = price;
                holding_size = size;
                holding_volume = size * price;
                holding_entry_num++;
                holding_i = i;
                holding_period = 0;
                holding_initial_i = i;
            }
            else if (holding_side != side) //Opposit Entry
            {
                holding_period_list.Add(holding_period);
                holding_side = side;
                holding_price = price;
                holding_size = size;
                holding_volume = size * price;
                holding_i = i;
                holding_period = 0;
                holding_initial_i = i;
            }
            else if (holding_side == side) //Additional Entry
            {
                holding_side = side;
                holding_price = price;
                holding_size = size;
                holding_volume = size * price;
                holding_entry_num++;
                holding_i = i;
            }
        }
    }


    public class LogData
    {
        public DataSet log_data_set { get; set; }
        public DataTable log_data_table { get; set; }
        public List<double> total_pl_log { get; set; }
        public List<double> total_pl_ratio { get; set; }
        public List<double> close_log { get; set; }
        public Dictionary<int, double> buy_points { get; set; }
        public Dictionary<int, double> sell_points { get; set; }
        public bool silent_mode { get; set; }

        public LogData(bool silent)
        {
            log_data_set = new DataSet();
            log_data_table = new DataTable("LodData");
            log_data_table.Columns.Add("total_pl", typeof(double));
            log_data_table.Columns.Add("total_fee", typeof(double));
            log_data_table.Columns.Add("dt", typeof(string));
            log_data_table.Columns.Add("i", typeof(Int64));
            log_data_table.Columns.Add("open", typeof(double));
            log_data_table.Columns.Add("high", typeof(double));
            log_data_table.Columns.Add("low", typeof(double));
            log_data_table.Columns.Add("close", typeof(double));
            log_data_table.Columns.Add("order_side", typeof(string));
            log_data_table.Columns.Add("order_type", typeof(string));
            log_data_table.Columns.Add("order_size", typeof(string));
            log_data_table.Columns.Add("order_price", typeof(string));
            log_data_table.Columns.Add("order_message", typeof(string));
            log_data_table.Columns.Add("pt price", typeof(double));
            log_data_table.Columns.Add("lc price", typeof(double));
            log_data_table.Columns.Add("holding_side", typeof(string));
            log_data_table.Columns.Add("holding_price", typeof(double));
            log_data_table.Columns.Add("holding_size", typeof(double));
            log_data_table.Columns.Add("holding_volume", typeof(double));
            log_data_table.Columns.Add("action", typeof(string));
            log_data_set.Tables.Add(log_data_table);
            total_pl_log = new List<double>();
            total_pl_ratio = new List<double>();
            close_log = new List<double>();
            buy_points = new Dictionary<int, double>();
            sell_points = new Dictionary<int, double>();
            silent_mode = silent;
        }

        public void add_log_data(int i, string dt, string action, HoldingData hd, OrderData od, PerformanceData pd)
        {
            if (silent_mode == false)
            {
                total_pl_log.Add(pd.total_pl);
                var pt = hd.holding_side == "buy" ? od.pt_order + hd.holding_price : hd.holding_price - od.pt_order;
                var lc = hd.holding_side == "buy" ? hd.holding_price - od.lc_order : hd.holding_price + od.lc_order;
                if (hd.holding_side == "")
                {
                    pt = 0;
                    lc = 0;
                }
                if (od.order_serial_list.Count > 0)
                {
                    log_data_table.Rows.Add(pd.total_pl, pd.total_fee, dt, i, MarketData.Open[i], MarketData.High[i], MarketData.Low[i], MarketData.Close[i],
                        String.Join(":",od.order_side.Values.ToList()), String.Join(":", od.order_type.Values.ToList()), String.Join(":", od.order_size.Values.ToList()),
                        String.Join(":", od.order_price.Values.ToList()), String.Join(":", od.order_message.Values.ToList()), pt, lc, hd.holding_side, hd.holding_price,
                        hd.holding_size, hd.holding_volume, action);
                }
                else
                {
                    log_data_table.Rows.Add(pd.total_pl, pd.total_fee, dt, i, MarketData.Open[i], MarketData.High[i], MarketData.Low[i], MarketData.Close[i], "", "", 0, 0, "",
                        pt, lc, hd.holding_side, hd.holding_price, hd.holding_size, hd.holding_volume, action);
                }
            }
        }
    }


    /*
     * 
     * leveraged trading: always place orders of same leverage using the total capital. (stop simulation when capital become less than min required capital)
     * fixed amount trading: always place a same amount order. (it may take minus capital)
     */
    public class Account
    {
        public PerformanceData performance_data;
        public OrderData order_data;
        public HoldingData holding_data;
        public LogData log_data;

        public const double initial_capital = 10000.0; //in usd
        public const double minimal_required_capital = 100; //finish trading when initial capital become less than the minimal required capital
        public const double minimal_order_amount = 50;
        public const double taker_fee = 0.00075;
        public const double maker_fee = -0.00025;
        public const double slip_page = 5.0; // applied only for market / stop market order
        public const double leverage_limit = 1.5;
        public const double required_margin_maintenace_rate = 0.8;
        public double margin_required = 0.0;
        public double margin_maintenace_rate = 0.0;
        public double leverage = 0.0;
        public string leveraged_or_fixed_amount_trading = "fixed"; //leveraged trade or fixed amount trade
        public double fixed_amount = 0.0;
        public double max_lev_total_amount = 0.0; //maximum position size in leveraged trading
        public bool silent_mode = false; //true: do not display message, use for opt nanpin and etc.
        public bool stop_sim = false; //true when stop sim trigger was hit (i.e. minimal capital requirement)


        public int start_ind = 0;
        public int end_ind = 0;



        public List<double> total_pl_list = new List<double>();
        public List<double> total_pl_ratio_list = new List<double>();
        public List<double> total_fund_list = new List<double>();

        public Account(string leveraged_or_fixed_amount, bool silent)
        {
            log_data = new LogData(silent);
            performance_data = new PerformanceData(initial_capital);
            order_data = new OrderData();
            holding_data = new HoldingData();
            total_pl_list = new List<double>();
            total_pl_ratio_list = new List<double>();
            margin_required = 0.0;
            leverage = 0.0;
            start_ind = 0;
            end_ind = 0;
            silent_mode = silent;
            if (leveraged_or_fixed_amount == "leveraged")
                leveraged_or_fixed_amount_trading = leveraged_or_fixed_amount;
            else if (leveraged_or_fixed_amount == "fixed")
                leveraged_or_fixed_amount_trading = leveraged_or_fixed_amount;
            else
                Console.WriteLine("Account:Invalid  leveraged or fixed amount trading flg !" + ", " + leveraged_or_fixed_amount);
            fixed_amount = Math.Round(initial_capital * 0.5, 2);
            max_lev_total_amount = initial_capital * leverage_limit;
        }


        /*should be called after all sim calc*/
        public void calc_sharp_ratio()
        {
            List<double> change = new List<double>();
            for (int i = 1; i < performance_data.total_capital_list.Count; i++)
                if (performance_data.total_capital_list[i-1] > 0)
                    change.Add( (performance_data.total_capital_list[i] - performance_data.total_capital_list[i-1]) / performance_data.total_capital_list[i - 1]);
                else
                    change.Add(-1.0 * (performance_data.total_capital_list[i] - performance_data.total_capital_list[i - 1]) / performance_data.total_capital_list[i - 1]);
            var mean = change.Average();
            var stdv = 0.0;
            for (int i = 0; i < change.Count; i++)
                stdv += Math.Pow(change[i] - mean, 2.0);
            stdv = Math.Sqrt(stdv / Convert.ToDouble(change.Count - 1));
            /*
            var doubleList = change.Select(a => Convert.ToDouble(a)).ToArray();
            //平均値算出
            double mean = doubleList.Average();
            //自乗和算出
            double sum2 = doubleList.Select(a => a * a).Sum();
            //分散 = 自乗和 / 要素数 - 平均値^2
            double variance = sum2 / Convert.ToDouble(doubleList.Length) - mean * mean;
            //標準偏差 = 分散の平方根
            var stdv = Math.Sqrt(variance);
            */
            if (stdv != 0)
                performance_data.sharp_ratio = Math.Round(((performance_data.total_capital - performance_data.initial_capital) / performance_data.initial_capital) / stdv, 6);
            else
                performance_data.sharp_ratio = 0;
        }


        //total capの高値を越えられなかった日の合計　÷　end_ind - start_ind
        private void calcDDPeriodRatio()
        {
            var grad = 0.0;
            for (int i = 0; i < performance_data.total_capital_list.Count - 1; i++)
                grad += performance_data.total_capital_list[i + 1] - performance_data.total_capital_list[i];
            performance_data.total_capital_gradient = Math.Round(grad / Convert.ToDouble(performance_data.total_capital_list.Count-1), 4);
        }



        /*
         * * total_capital = initial_capital + total_pl
         * total_pl = realized_pl + unrealized_pl + total_fee
         * realized_pl = (exec_price - entry_price) * size
         * unrealized_pl = (close - entry_price) * size
         * fee = fee_rate * exec_price * size
         */
        private void calc_performance_data(double close)
        {
            if (holding_data.holding_side != "")
            {
                var price_change_ratio = holding_data.holding_side == "buy" ? (close - holding_data.holding_price) / holding_data.holding_price : (holding_data.holding_price - close) / holding_data.holding_price;
                performance_data.unrealized_pl = holding_data.holding_side == "buy" ? (close - holding_data.holding_price) * holding_data.holding_size : (holding_data.holding_price - close) * holding_data.holding_size;
                performance_data.total_pl = Math.Round(performance_data.realized_pl + performance_data.unrealized_pl - performance_data.total_fee, 2);
                performance_data.total_capital = Math.Round(performance_data.total_pl + performance_data.initial_capital,2);
            }
            else
            {
                performance_data.unrealized_pl = 0.0;
                performance_data.unrealized_pl_ratio = 0.0;
                performance_data.total_pl = Math.Round( performance_data.realized_pl + performance_data.unrealized_pl - performance_data.total_fee,2);
                performance_data.total_capital = Math.Round(performance_data.total_pl + performance_data.initial_capital,2);
                //performance_data.unrealized_pl_list = new List<double>();
            }
            performance_data.unrealized_pl_ratio = performance_data.unrealized_pl != 0 ? Math.Round(performance_data.unrealized_pl / (performance_data.total_capital - performance_data.unrealized_pl),4) : 0;
            performance_data.total_pl_ratio = Math.Round((performance_data.total_capital - performance_data.initial_capital) / performance_data.initial_capital,4);
            performance_data.unrealized_pl_list.Add(performance_data.unrealized_pl);
            performance_data.total_capital_list.Add(performance_data.total_capital);
            performance_data.unrealized_pl_ratio_list.Add(performance_data.unrealized_pl_ratio);
            total_pl_list.Add(performance_data.total_pl);
            total_pl_ratio_list.Add(performance_data.total_pl_ratio);
            performance_data.log_close.Add(close);


            if (performance_data.unrealized_pl_ratio < -0.3 && silent_mode == false && leveraged_or_fixed_amount_trading == "leveraged")
                Console.WriteLine("high unrealized pl ratio!");
            performance_data.num_trade_list.Add(performance_data.num_trade);
        }


        private void calc_margin_data(int i, double close)
        {
            if (holding_data.holding_side != "" && leveraged_or_fixed_amount_trading == "leveraged")
            {
                margin_required = Math.Round(holding_data.holding_volume / leverage_limit, 2);
                margin_maintenace_rate = Math.Round(performance_data.total_capital / margin_required, 4);
                leverage = Math.Round(holding_data.holding_volume / performance_data.total_capital, 4);
                max_lev_total_amount = Math.Round(performance_data.total_capital * leverage_limit, 4);
                if (margin_maintenace_rate <= required_margin_maintenace_rate)
                {
                    if (silent_mode == false)
                    {
                        Console.WriteLine("Maintenace Margin is too small, force close all positions!");
                        Console.WriteLine("Margin Rate=" + margin_maintenace_rate + ", leverage=" + leverage.ToString());
                    }
                    performance_data.num_force_exit++;
                    exit_all(i, MarketData.Dt[i].ToString());
                }
                if (leverage >= leverage_limit * 1.5)
                {
                    if (silent_mode == false)
                        Console.WriteLine("Leverage is higer than the limit !" + ", leverage="+leverage.ToString());
                    performance_data.num_force_exit++;
                    exit_all(i, MarketData.Dt[i].ToString());
                }
            }
            else
            {
                leverage = 0.0;
                margin_required = 0.0;
                margin_maintenace_rate = 0.0;
            }
        }


        /*
         * i番目のデータで判断・発注して、i+1番目のデータで約定判定
         * Sim側でmove to nextの時にはi+1を入力するようにする。
         */
        public void move_to_next(int i)
        {
            if (start_ind <= 0)
                start_ind = i;
            check_cancel(i);
            check_execution(i);
            holding_data.holding_period = holding_data.holding_initial_i > 0 ? i - holding_data.holding_initial_i : 0;
            holding_data.holding_volume = holding_data.holding_side != "" ? holding_data.holding_size * MarketData.Close[i] : 0;
            calc_performance_data(MarketData.Close[i]);
            calc_margin_data(i, MarketData.Close[i]);
            if (performance_data.num_trade > 0)
                performance_data.win_rate = Math.Round(Convert.ToDouble(performance_data.num_win) / Convert.ToDouble(performance_data.num_trade), 4);
            log_data.add_log_data(i, MarketData.Dt[i].ToString(), "move to next", holding_data, order_data, performance_data);

            log_data.close_log.Add(MarketData.Close[i]);
            if (log_data.buy_points.Keys.Contains(i) == false)
                log_data.buy_points[i] = 0;
            if (log_data.sell_points.Keys.Contains(i) == false)
                log_data.sell_points[i] = 0;
            var a = order_data.order_side;
            //Console.WriteLine("unrealized pl=" + performance_data.unrealized_pl + ", maring rate=" + margin_maintenace_rate + ", lev=" + leverage + ", size=" + holding_data.holding_size + ", price=" + holding_data.holding_price + ", capital=" + performance_data.total_capital + ", close=" + close);
        }

        //close all holding positions and calc pl
        public void last_day(int i, double close)
        {
            end_ind = i;
            order_data = new OrderData();
            if (holding_data.holding_side != "")
            {
                calc_executed_pl(close, holding_data.holding_size, i, true);
                holding_data.holding_period_list.Add(holding_data.holding_period);
                holding_data.initialize_holding();
                performance_data.unrealized_pl = 0;
            }
            calc_performance_data(close);
            calc_margin_data(i, close);
            performance_data.max_dd = Math.Round(performance_data.unrealized_pl_ratio_list.Min(), 6);
            performance_data.max_pl = Math.Round(performance_data.unrealized_pl_ratio_list.Max(), 6);
            performance_data.realized_pl_ratio_variance = calcStdiv(performance_data.realized_pl_list);
            performance_data.total_capital_variance = calcStdiv(performance_data.total_capital_list);
            double calcStdiv(List<double> data)
            {
                var mean = data.Average();
                double sum2 = data.Select(a => a * a).Sum();
                double variance = sum2 / data.Count - mean * mean;
                return Math.Sqrt(variance);
            }
            if (performance_data.num_trade > 0)
                performance_data.win_rate = Math.Round(Convert.ToDouble(performance_data.num_win) / Convert.ToDouble(performance_data.num_trade), 4);
            calc_sharp_ratio();
            calcDDPeriodRatio();
            if (log_data.silent_mode == false)
            {
                log_data.close_log.Add(MarketData.Close[i]);
                if (log_data.buy_points.Keys.Contains(i) == false)
                    log_data.buy_points[i] = 0;
                if (log_data.sell_points.Keys.Contains(i) == false)
                    log_data.sell_points[i] = 0;
            }
            //log_data.log_data_table.WriteXml("log.html", XmlWriteMode.DiffGram);
            if (log_data.silent_mode==false)
                writeCsv("log.csv", log_data.log_data_table);
        }


        //leveraged trading: order size should be percentage for the total available size (max_lev_total_amount)
        //fixed amount trading: order size should be percentage for the total fixe amount. no requirement for minimal capital, can be taken minus capital val
        public void entry_order(string type, string side, double size, double price, int i, string dt, string message)
        {
            if (stop_sim == false || (message.Contains("pt") || message.Contains("lc") || message.Contains("exit all"))) //can place order when stop_sim == false, pt, lc, exit all order can be placed as exception 
            {
                var flg_check_order_side = true;
                var flg_check_leverage = true;
                var flg_check_min_capital = true;
                var flg_check_order_type = true;
                var flg_check_order_size = true;
                if (side != "buy" && side != "sell")
                {
                    flg_check_order_side = false;
                    Console.WriteLine("Entry Order failed due to order side check !");
                }
                if (leveraged_or_fixed_amount_trading == "leveraged")
                {
                    if (side == holding_data.holding_side)
                    {
                        if (((MarketData.Close[i] * size) + holding_data.holding_volume) / performance_data.total_capital > leverage_limit)
                        {
                            flg_check_leverage = false;
                            stop_sim = true;
                            Console.WriteLine("Entry order failed due to over leverage !");
                        }
                    }
                }
                if (leveraged_or_fixed_amount_trading == "leveraged" && performance_data.total_capital < minimal_required_capital)
                {
                    flg_check_min_capital = false;
                    stop_sim = true;
                    Console.WriteLine("Entry order failed due to too small capital !");
                }
                if (type != "market" && type != "limit" && type != "stop market")
                {
                    flg_check_order_type = false;
                    Console.WriteLine("Entry order type should be market or limit !");
                }
                var order_size = 0.0;
                if (leveraged_or_fixed_amount_trading == "leveraged")
                    order_size = Math.Round(max_lev_total_amount * size / MarketData.Open[i], 4);
                else if (leveraged_or_fixed_amount_trading == "fixed")
                    order_size = Math.Round(fixed_amount * size / MarketData.Open[i], 4);
                if (order_size * MarketData.Open[i] <= minimal_order_amount)
                {
                    flg_check_order_size = false;
                    stop_sim = true;
                    Console.WriteLine("Entry order amount is less than the minimal amount !");
                }

                //allow order entry only when all flg cleared or opposite side entry (losscut and etc)
                if ((flg_check_order_side && flg_check_leverage && flg_check_order_type && flg_check_min_capital && flg_check_order_size) || (side != holding_data.holding_side && holding_data.holding_side != ""))
                {
                    order_data.order_serial_num++;
                    order_data.order_serial_list.Add(order_data.order_serial_num);
                    order_data.order_type[order_data.order_serial_num] = type;
                    order_data.order_side[order_data.order_serial_num] = side;
                    if (message.Contains("pt") || message.Contains("lc") || message.Contains("exit all"))
                    {
                        order_data.order_size[order_data.order_serial_num] = size; //exit exact the same size of current holding when pt, lc or force exit
                    }
                    else
                    {
                        order_data.order_size[order_data.order_serial_num] = order_size;
                    }
                    order_data.order_price[order_data.order_serial_num] = price;
                    order_data.order_i[order_data.order_serial_num] = i;
                    order_data.order_dt[order_data.order_serial_num] = dt;
                    order_data.order_cancel[order_data.order_serial_num] = false;
                    order_data.order_message[order_data.order_serial_num] = message;
                    log_data.add_log_data(i, dt, "entry order " + side + "-" + type, holding_data, order_data, performance_data);
                }
            }
        }

        /*
         * pt: limit order with price of pt price +- holding price
         * lc: stop market order with price of price +- holding price
         */
        public void entry_ptlc(double pt_price, double lc_price)
        {
            if (pt_price > 0 && lc_price > 0)
            {
                order_data.pt_order = pt_price;
                order_data.lc_order = lc_price;
            }
        }

        public void update_order_price(double update_price, int order_serial_num, int i, string dt)
        {
            if (order_data.getLastOrderSide() == "buy" && order_data.getLastOrderPrice() > update_price) { }
            //Console.WriteLine(i.ToString()+": buy update issue:"+order_data.getLastOrderPrice().ToString() + " -> "+update_price.ToString());
            else if (order_data.getLastOrderSide() == "sell" && order_data.getLastOrderPrice() < update_price) { }
            //Console.WriteLine(i.ToString() + ": sell update issue:" + order_data.getLastOrderPrice().ToString() + " -> " + update_price.ToString());

            if (update_price > 0 && order_data.order_serial_list.Contains(order_serial_num))
            {
                order_data.order_price[order_serial_num] = update_price;
                //order_data.order_i[order_serial_num] = i;
                //order_data.order_message[order_serial_num] = "updated-" + order_data.order_message[order_serial_num];
                log_data.add_log_data(i, dt, "update order price", holding_data, order_data, performance_data);
            }
            else
            {
                Console.WriteLine("invalid update price or order_serial_num in update_order_price !");
            }
        }

        public void update_order_amount(double update_amount, int order_serial_num, int i, string dt)
        {
            if (update_amount > 0 && order_data.order_serial_list.Contains(order_serial_num))
            {
                order_data.order_size[order_serial_num] = update_amount;
                log_data.add_log_data(i, dt, "update order amount", holding_data, order_data, performance_data);
            }
            else
            {
                Console.WriteLine("invalid update amount or order_serial_num in update_order_amount !");
            }

        }

        private void del_order(int order_serial_num, int i)
        {
            if (order_data.order_serial_list.Contains(order_serial_num))
            {
                order_data.order_serial_list.Remove(order_serial_num);
                order_data.order_side.Remove(order_serial_num);
                order_data.order_type.Remove(order_serial_num);
                order_data.order_size.Remove(order_serial_num);
                order_data.order_price.Remove(order_serial_num);
                order_data.order_i.Remove(order_serial_num);
                order_data.order_dt.Remove(order_serial_num);
                order_data.order_cancel.Remove(order_serial_num);
                order_data.order_message.Remove(order_serial_num);
            }
        }

        public void cancel_order(int order_serial_num, int i, string dt)
        {
            if (order_data.order_serial_list.Contains(order_serial_num))
            {
                if (order_data.order_cancel[order_serial_num] != true)
                {
                    order_data.order_cancel[order_serial_num] = true;
                    //order_data.order_i[order_serial_num] = i;
                }
                else
                {
                    Console.WriteLine("Cancel Failed!");
                }
            }
        }

        public void cancel_all_order(int i, string dt)
        {
            for (int s = 0; s < order_data.order_serial_list.Count; s++) { cancel_order(order_data.order_serial_list[s], i, dt); }
        }


        public void exit_all(int i, string dt)
        {
            if (holding_data.holding_size > 0)
                entry_order("market", holding_data.holding_side == "sell" ? "buy" : "sell", holding_data.holding_size, 0, i, dt, "exit all");
        }

        private void calc_fee(double size, double price, string maker_taker)
        {
            if (maker_taker == "maker")
            {
                performance_data.total_fee += price * size * maker_fee;
            }
            else if (maker_taker == "taker")
            {
                performance_data.total_fee += price * size * taker_fee;
            }
            else
            {
                Console.WriteLine("Invalid maker_taker type in calc_fee ! " + maker_taker);
            }
        }


        private void check_cancel(int i)
        {
            var serial_list = order_data.order_serial_list.ToArray();
            foreach (int s in serial_list)
            {
                //if (order_data.order_cancel[s] == true && order_data.order_i[s] < i)
                if (order_data.order_cancel[s] == true)
                {
                    del_order(s, i);
                    log_data.add_log_data(i, MarketData.Dt[i].ToString(), "cancelled", holding_data, order_data, performance_data);
                }
            }
        }

        //move_to_nextで呼び出す時には、i+1を使うべき
        //order priceよりも低い・高い価格をつけたら約定と判定する。
        //buy: order price=10000のときにlowが9999.5以下になったら約定。
        //sell: order price=10000のときにhighが10000.5以上になったら約定。
        //pt, lcはどちらかが約定した時点で他方をキャンセルする。
        //約定確認は陽線・陰線によってOHLCの時系列を考慮し約定を確認する。
        private void check_execution(int i)
        {
            var serial_list = order_data.order_serial_list.ToArray();
            //process market order in the first priotity
            foreach (int s in serial_list)
            {
                if (order_data.order_type[s] == "market")
                {
                    performance_data.num_maker_order++;
                    process_execution(order_data.order_side[s] == "buy" ? MarketData.Open[i] + slip_page : MarketData.Open[i] - slip_page, s, i, MarketData.Dt[i].ToString());
                    del_order(s, i);
                }
            }

            var ohlc_order = new List<double>();
            if (MarketData.Open[i] <= MarketData.Close[i])//yosen
                ohlc_order = new List<double>() { MarketData.Open[i], MarketData.Low[i], MarketData.High[i], MarketData.Close[i] };
            else //insen
                ohlc_order = new List<double>() { MarketData.Open[i], MarketData.High[i], MarketData.Low[i], MarketData.Close[i] };

            foreach (double p in ohlc_order)
            {
                serial_list = order_data.order_serial_list.ToArray();
                foreach (int s in serial_list)
                {
                    if (order_data.order_side[s] == "buy")
                    {
                        if (order_data.order_type[s] == "limit")
                        {
                            if (order_data.order_price[s] > p)
                            {
                                process_execution(order_data.order_price[s], s, i, MarketData.Dt[i].ToString());
                                del_order(s, i);
                            }
                        }
                        else if (order_data.order_type[s] == "stop market")
                        {
                            if (order_data.order_price[s] > p)
                            {
                                process_execution(order_data.order_price[s] + slip_page, s, i, MarketData.Dt[i].ToString());
                                del_order(s, i);
                            }
                        }
                        else
                        {
                            Console.WriteLine(order_data.order_type[s] + " - Unknown order type!");
                        }
                    }
                    else if (order_data.order_side[s] == "sell")
                    {
                        if (order_data.order_type[s] == "limit")
                        {
                            if (order_data.order_price[s] < p)
                            {
                                process_execution(order_data.order_price[s], s, i, MarketData.Dt[i].ToString());
                                del_order(s, i);
                            }
                        }
                        else if (order_data.order_type[s] == "stop market")
                        {
                            if (order_data.order_price[s] < p)
                            {
                                process_execution(order_data.order_price[s] - slip_page, s, i, MarketData.Dt[i].ToString());
                                del_order(s, i);
                            }
                        }
                        else
                            Console.WriteLine(order_data.order_side[s] + " - Unknown order side!");
                    }
                }

                //check for pt lc
                if (holding_data.holding_side == "buy")
                {
                    if (order_data.pt_order > 0) //pt order is limit order
                    {
                        if (order_data.pt_order + holding_data.holding_price < p)
                        {
                            entry_order("limit", "sell", holding_data.holding_size, order_data.pt_order + holding_data.holding_price, i, MarketData.Dt[i].ToString(), "pt limit order");
                            process_execution(order_data.order_price[order_data.getLastSerialNum()], order_data.getLastSerialNum(), i, MarketData.Dt[i].ToString());
                            del_order(order_data.getLastSerialNum(), i);
                            order_data.pt_order = 0;
                            order_data.lc_order = 0;
                        }
                    }
                    if (order_data.lc_order > 0) //lc order is stop market order
                    {
                        if (holding_data.holding_price - order_data.lc_order >= p)
                        {
                            performance_data.num_maker_order++;
                            entry_order("market", "sell", holding_data.holding_size, holding_data.holding_price - order_data.lc_order - slip_page, i, MarketData.Dt[i].ToString(), "lc market order");
                            process_execution(order_data.order_price[order_data.getLastSerialNum()], order_data.getLastSerialNum(), i, MarketData.Dt[i].ToString());
                            del_order(order_data.getLastSerialNum(), i);
                            order_data.pt_order = 0;
                            order_data.lc_order = 0;
                        }
                    }
                }
                else if (holding_data.holding_side == "sell")
                {
                    if (order_data.pt_order > 0)
                    {
                        if (holding_data.holding_price - order_data.pt_order > p)
                        {
                            entry_order("limit", "buy", holding_data.holding_size, holding_data.holding_price - order_data.pt_order, i, MarketData.Dt[i].ToString(), "pt limit order");
                            process_execution(order_data.order_price[order_data.getLastSerialNum()], order_data.getLastSerialNum(), i, MarketData.Dt[i].ToString());
                            del_order(order_data.getLastSerialNum(), i);
                            order_data.pt_order = 0;
                            order_data.lc_order = 0;
                        }
                    }
                    if (order_data.lc_order > 0)
                    {
                        if (order_data.lc_order + holding_data.holding_price <= p)
                        {
                            entry_order("market", "buy", holding_data.holding_size, holding_data.holding_price + order_data.lc_order + slip_page, i, MarketData.Dt[i].ToString(), "lc market order");
                            process_execution(order_data.order_price[order_data.getLastSerialNum()], order_data.getLastSerialNum(), i, MarketData.Dt[i].ToString());
                            del_order(order_data.getLastSerialNum(), i);
                            order_data.pt_order = 0;
                            order_data.lc_order = 0;
                        }
                    }
                }
            }
        }



        private void process_execution(double exec_price, int order_serial_num, int i, string dt)
        {
            if (order_data.order_side[order_serial_num] == "buy")
                log_data.buy_points[i] = exec_price;
            else
                log_data.sell_points[i] = exec_price;

            calc_fee(order_data.order_size[order_serial_num], exec_price, order_data.order_type[order_serial_num] == "limit" ? "maker" : "taker");
            if (holding_data.holding_side == "")
            {
                if (order_data.order_side[order_serial_num] == "buy")
                    performance_data.num_buy++;
                else
                    performance_data.num_sell++;
                holding_data.update_holding(order_data.order_side[order_serial_num], exec_price, order_data.order_size[order_serial_num], i);
                log_data.add_log_data(i, dt, "New Entry:" + order_data.order_type[order_serial_num], holding_data, order_data, performance_data);
            }
            else if (holding_data.holding_side == order_data.order_side[order_serial_num])
            {
                var ave_price = Math.Round(((holding_data.holding_price * holding_data.holding_size) + (exec_price * order_data.order_size[order_serial_num])) / (order_data.order_size[order_serial_num] + holding_data.holding_size), 1);
                holding_data.update_holding(holding_data.holding_side, ave_price, order_data.order_size[order_serial_num] + holding_data.holding_size, i);
                log_data.add_log_data(i, dt, "Additional Entry.", holding_data, order_data, performance_data);
            }
            else if (holding_data.holding_size > order_data.order_size[order_serial_num])
            {
                calc_executed_pl(exec_price, order_data.order_size[order_serial_num], i, false);
                holding_data.update_holding(holding_data.holding_side, holding_data.holding_price, holding_data.holding_size - order_data.order_size[order_serial_num], i);
                log_data.add_log_data(i, dt, "Exit Order (h>o)", holding_data, order_data, performance_data);
            }
            else if (holding_data.holding_size == order_data.order_size[order_serial_num])
            {
                calc_executed_pl(exec_price, order_data.order_size[order_serial_num], i, true);
                holding_data.holding_period_list.Add(holding_data.holding_period);
                holding_data.initialize_holding();
                log_data.add_log_data(i, dt, "Exit Order (h=o)", holding_data, order_data, performance_data);
            }
            else if (holding_data.holding_size < order_data.order_size[order_serial_num])
            {
                if (order_data.order_side[order_serial_num] == "buy")
                    performance_data.num_buy++;
                else
                    performance_data.num_sell++;
                calc_executed_pl(exec_price, holding_data.holding_size, i, true);

                holding_data.update_holding(order_data.order_side[order_serial_num], exec_price, order_data.order_size[order_serial_num] - holding_data.holding_size, i);
                log_data.add_log_data(i, dt, "'Exit & Entry Order (h<o)", holding_data, order_data, performance_data);
            }
            else
            {
                Console.WriteLine("Unknown situation in process execution !");
            }
        }

        private void calc_executed_pl(double exec_price, double size, int i, bool count_num_trade)
        {
            //var pl = holding_data.holding_side == "buy" ? ((exec_price - holding_data.holding_price) / holding_data.holding_price) * size : ((holding_data.holding_price - exec_price) / holding_data.holding_price) * size;
            var pl = holding_data.holding_side == "buy" ? (exec_price - holding_data.holding_price) * size : (holding_data.holding_price - exec_price) * size;
            //Console.WriteLine("pl="+pl.ToString() + ", i="+i.ToString());
            performance_data.realized_pl += Math.Round(pl, 6);
            performance_data.realized_pl_list.Add(Math.Round(pl, 6));

            if (count_num_trade)
            {
                performance_data.num_trade++;
                if (pl > 0) { performance_data.num_win++; }
            }
            if (holding_data.holding_side == "buy")
            {
                performance_data.buy_pl_list.Add(Math.Round(pl, 6));
                performance_data.buy_pl_ratio_list.Add(Math.Round(pl, 6) / MarketData.Close[i]);
            }
            else
            {
                performance_data.sell_pl_list.Add(Math.Round(pl, 6));
                performance_data.sell_pl_ratio_list.Add(Math.Round(pl, 6) / MarketData.Close[i]);
            }
        }

        private void writeCsv(string filename, DataTable dt)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                int[] qcols = { };
                string dquote = (qcols == null) ? "" : "\"";
                sw.WriteLine(string.Join(",", dt.Columns.Cast<DataColumn>()
                             .Select(i => dquote + i.ColumnName + dquote).ToArray()));
                foreach (DataRow dr in dt.Rows)
                {
                    sw.WriteLine(string.Join(",", DoubleQuote(dr.ItemArray, qcols)));
                }
            }
            string[] DoubleQuote(IEnumerable<object> p_item, int[] p_qcols)
            {
                int cnt = 0;
                return p_item.Select(i => (p_qcols != null && p_qcols.Contains(cnt++))
                                     ? "\"" + i.ToString()
                                     .Replace("\"", "\"\"") + "\"" : i.ToString()).ToArray();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace BTCSIM2
{
    public class StrategyActionData
    {
        public List<string> action; //entry, cancel, ptlc
        public List<string> order_side;
        public List<string> order_type;
        public List<double> order_price;
        public List<double> order_size;
        public List<int> order_serial_num;
        public List<string> order_message;
        public double pt_price;
        public double lc_price;

        public StrategyActionData()
        {
            action = new List<string>();
            order_price = new List<double>();
            order_side = new List<string>();
            order_size = new List<double>();
            order_type = new List<string>();
            order_serial_num = new List<int>();
            order_message = new List<string>();
            pt_price = 0;
            lc_price = 0;
        }
        public void add_action(string action, string side, string type, double price, double size, double pt, double lc, int serial_num, string message)
        {
            this.action.Add(action);
            order_side.Add(side);
            order_type.Add(type);
            order_price.Add(price);
            order_size.Add(size);
            order_serial_num.Add(serial_num);
            order_message.Add(message);
            pt_price = pt;
            lc_price = lc;
        }
    }

    public class Strategy
    {
        public Strategy()
        {
        }

        /*
         * market entry and place a pt / lc order.
         * entry side will be decided randomly 50% long and 50% short
         * pt, lc order should be checked and processed, and pt or lc order should be cancelled immediaterly.
         * pt / lc price should be updated when holding price is changed.
         */
        public StrategyActionData PtlcStrategy(int i, double pt_ratio, double lc_ratio, Account ac)
        {
            var ad = new StrategyActionData();
            var r = RandomSeed.rnd.NextDouble();
            var side = r > 0.5 ? "buy" : "sell";
            var opposite_side = side == "buy" ? "sell" : "buy";
            if (ac.holding_data.holding_side == "" && ac.order_data.getNumOrders() == 0) //no holding no order
            {
                var pt = (side == "buy" ? Math.Round(MarketData.Close[i] * pt_ratio) : Math.Round(MarketData.Close[i] * pt_ratio));
                var lc = (side == "buy" ? Math.Round(MarketData.Close[i] * lc_ratio) : Math.Round(MarketData.Close[i] * lc_ratio));
                ad.add_action("entry", side, "market", 0, 1, 0, 0, -1, "entry order"); //entry
                ad.add_action("ptlc", opposite_side, "", 0, 1, pt, lc, -1, "pt lc order"); //pt lc order
            }
            else if (ac.holding_data.holding_side == "" && ac.order_data.getNumOrders() > 0) //no holding, limit order
            {
                ad.add_action("cancel", "", "", 0, 0, -1, 0, 0, "cancel all orders");
                Console.WriteLine("PtlcStrategy - no holding but limit order exist, unexpected situation !");
            }
            else if (ac.holding_data.holding_side == "" && (ac.order_data.pt_order == 0 && ac.order_data.lc_order == 0)) //holding position, no pt lc order
            {
                var pt = (side == "buy" ? Math.Round(ac.holding_data.holding_price * pt_ratio) : Math.Round(ac.holding_data.holding_price * pt_ratio));
                var lc = (side == "buy" ? Math.Round(ac.holding_data.holding_price * lc_ratio) : Math.Round(ac.holding_data.holding_price * lc_ratio));
                ad.add_action("ptlc", opposite_side, "", 0, 1, pt, lc, -1, "pt lc order"); //pt lc order
                Console.WriteLine("PtlcStrategy - holding position but no pt lc order exist, unexpected situation !");
            }
            else if (ac.holding_data.holding_side == "" && (ac.order_data.pt_order > 0 && ac.order_data.lc_order > 0)) //holding position, pt lc order
            {
                //update pt lc price if holding price is updated
            }
            return ad;
        }

        /*
         * 
         */
        public StrategyActionData NanpinPtlcStrategy(int i, double pt_ratio, double lc_ratio, List<double> nanpin_timing, List<double> lot_splits, Account ac)
        {
            var ad = new StrategyActionData();
            if (ac.holding_data.holding_side == "") //no holding place a first entry order
            {
                if (ac.order_data.getNumOrders() > 0) //pt or lc was executed and nanpin limit order is remained
                {
                    ad.add_action("cancel", "", "", 0, 0, 0, 0, -1, "cancel all orders");
                    ad.add_action("ptlc", "", "", 0, 0, 0, 0, -1, "cancel pt lc order");
                }
                else //proceed for nanpin entry
                {
                    var r = RandomSeed.rnd.NextDouble();
                    var side = r > 0.5 ? "buy" : "sell";
                    var opposite_side = side == "buy" ? "sell" : "buy";
                    var pt = (side == "buy" ? Math.Round(MarketData.Close[i] * pt_ratio) : Math.Round(MarketData.Close[i] * pt_ratio));
                    var lc = (side == "buy" ? Math.Round(MarketData.Close[i] * lc_ratio) : Math.Round(MarketData.Close[i] * lc_ratio));

                    ad.add_action("entry", side, "market", 0, lot_splits[0], 0, 0, -1, "entry order"); //first entry
                    for (int j = 0; j < nanpin_timing.Count(); j++)
                        ad.add_action("entry", side, "limit", side == "buy" ? Math.Round(MarketData.Close[i] - MarketData.Close[i] * nanpin_timing[j]) : Math.Round(MarketData.Close[i] + MarketData.Close[i] * nanpin_timing[j]), lot_splits[j + 1], 0, 0, 0, "nanpin entry");
                    ad.add_action("ptlc", opposite_side, "", 0, 0, pt, lc, -1, "pt lc order"); //pt lc order
                }
            }
            else //update pt lc price
            {
                var pt = (ac.holding_data.holding_side == "buy" ? Math.Round(ac.holding_data.holding_price * pt_ratio) : Math.Round(ac.holding_data.holding_price * pt_ratio));
                var lc = (ac.holding_data.holding_side == "buy" ? Math.Round(ac.holding_data.holding_price * lc_ratio) : Math.Round(ac.holding_data.holding_price * lc_ratio));
                if (ac.order_data.pt_order != pt || ac.order_data.lc_order != lc)
                    ad.add_action("ptlc", ac.holding_data.holding_side == "buy" ? "sell" : "buy", "", 0, 0, pt, lc, -1, "pt lc order"); //pt lc order
            }

            if (ac.holding_data.holding_size > 1.01)
                Console.WriteLine("Holding size is larger than 1.0!");

            return ad;
        }

        /*
         * 
         */
        public StrategyActionData NanpinPtlcMADivStrategy(int i, double pt_ratio, double lc_ratio, List<double> nanpin_timing, List<double> lot_splits, int ma_term, bool flg_contrarian,  Account ac)
        {
            var ad = new StrategyActionData();
            if (ac.holding_data.holding_side == "") //no holding place a first entry order
            {
                if (ac.order_data.getNumOrders() > 0) //pt or lc was executed and nanpin limit order is remained
                {
                    ad.add_action("cancel", "", "", 0, 0, 0, 0, -1, "cancel all orders");
                    ad.add_action("ptlc", "", "", 0, 0, 0, 0, -1, "cancel pt lc order");
                }
                else //proceed for nanpin entry
                {
                    var side = "";
                    if (flg_contrarian)
                    {
                        if (MarketData.Divergence[ma_term][i] > 0)
                            side = "sell";
                        else
                            side = "buy";
                    }
                    else
                    {
                        if (MarketData.Divergence[ma_term][i] > 0)
                            side = "buy";
                        else
                            side = "sell";
                    }
                    var opposite_side = side == "buy" ? "sell" : "buy";
                    var pt = (side == "buy" ? Math.Round(MarketData.Close[i] * pt_ratio) : Math.Round(MarketData.Close[i] * pt_ratio));
                    var lc = (side == "buy" ? Math.Round(MarketData.Close[i] * lc_ratio) : Math.Round(MarketData.Close[i] * lc_ratio));
                    ad.add_action("entry", side, "market", 0, lot_splits[0], 0, 0, -1, "entry order"); //first entry
                    for (int j = 0; j < nanpin_timing.Count(); j++)
                        ad.add_action("entry", side, "limit", side == "buy" ? Math.Round(MarketData.Close[i] - MarketData.Close[i] * nanpin_timing[j]) : Math.Round(MarketData.Close[i] + MarketData.Close[i] * nanpin_timing[j]), lot_splits[j + 1], 0, 0, 0, "nanpin entry");
                    ad.add_action("ptlc", opposite_side, "", 0, 0, pt, lc, -1, "pt lc order"); //pt lc order
                }
            }
            else //update pt lc price
            {
                var pt = (ac.holding_data.holding_side == "buy" ? Math.Round(ac.holding_data.holding_price * pt_ratio) : Math.Round(ac.holding_data.holding_price * pt_ratio));
                var lc = (ac.holding_data.holding_side == "buy" ? Math.Round(ac.holding_data.holding_price * lc_ratio) : Math.Round(ac.holding_data.holding_price * lc_ratio));
                if (ac.order_data.pt_order != pt || ac.order_data.lc_order != lc)
                    ad.add_action("ptlc", ac.holding_data.holding_side == "buy" ? "sell" : "buy", "", 0, 0, pt, lc, -1, "pt lc order"); //pt lc order
            }

            //if (ac.holding_data.holding_size > 1.01)
            //    Console.WriteLine("Holding size is larger than 1.0!");

            return ad;
        }



        public StrategyActionData NanpinPtlcMADivRapidSideChangeStrategy(int i, double pt_ratio, double lc_ratio, List<double> nanpin_timing, List<double> lot_splits, int ma_term, Account ac)
        {
            var ad = new StrategyActionData();
            var ma_side = MarketData.Divergence[ma_term][i] > 0 ? "sell":"buy";
            var opposite_side = ma_side == "buy" ? "sell" : "buy";

            if (ac.holding_data.holding_side == "") //no holding place a first entry order
            {
                if (ac.order_data.getNumOrders() > 0) //pt or lc was executed and nanpin limit order is remained
                {
                    ad.add_action("cancel", "", "", 0, 0, 0, 0, -1, "cancel all orders");
                    ad.add_action("ptlc", "", "", 0, 0, 0, 0, -1, "cancel pt lc order");
                }
                else //proceed for nanpin entry
                {
                    var pt = (ma_side == "buy" ? Math.Round(MarketData.Close[i] * pt_ratio) : Math.Round(MarketData.Close[i] * pt_ratio));
                    var lc = (ma_side == "buy" ? Math.Round(MarketData.Close[i] * lc_ratio) : Math.Round(MarketData.Close[i] * lc_ratio));
                    ad.add_action("entry", ma_side, "market", 0, lot_splits[0], 0, 0, -1, "entry order"); //first entry
                    for (int j = 0; j < nanpin_timing.Count(); j++)
                        ad.add_action("entry", ma_side, "limit", ma_side == "buy" ? Math.Round(MarketData.Close[i] - MarketData.Close[i] * nanpin_timing[j]) : Math.Round(MarketData.Close[i] + MarketData.Close[i] * nanpin_timing[j]), lot_splits[j + 1], 0, 0, 0, "nanpin entry");
                    ad.add_action("ptlc", opposite_side, "", 0, 0, pt, lc, -1, "pt lc order"); //pt lc order
                }
            }
            else //update pt lc price
            {
                //最初のエントリーサイズに必要なコストの２倍以上の利益が出ていたら売って逆のsideで再エントリーする
                var min_amount_for_side_change = 100000000.0;
                if (ac.leveraged_or_fixed_amount_trading == "fixed")
                    min_amount_for_side_change = ac.fixed_amount * lot_splits[0] *0.014;
                else if (ac.leveraged_or_fixed_amount_trading == "leveraged")
                    min_amount_for_side_change = ac.max_lev_total_amount * lot_splits[0] * 0.014;
                if (ac.holding_data.holding_side != ma_side && ac.performance_data.unrealized_pl > min_amount_for_side_change)
                {
                    var pt = MarketData.Close[i];
                    var lc = (ac.holding_data.holding_side == "buy" ? Math.Round(ac.holding_data.holding_price * lc_ratio) : Math.Round(ac.holding_data.holding_price * lc_ratio));
                    ad.add_action("cancel", "", "", 0, 0, 0, 0, -1, "cancel all orders");
                    ad.add_action("ptlc", ac.holding_data.holding_side == "buy" ? "sell" : "buy", "", 0, 0, pt, lc, -1, "pt lc order"); //revise pt lc order
                }
                else
                {
                    var pt = (ac.holding_data.holding_side == "buy" ? Math.Round(ac.holding_data.holding_price * pt_ratio) : Math.Round(ac.holding_data.holding_price * pt_ratio));
                    var lc = (ac.holding_data.holding_side == "buy" ? Math.Round(ac.holding_data.holding_price * lc_ratio) : Math.Round(ac.holding_data.holding_price * lc_ratio));
                    if (ac.order_data.pt_order != pt || ac.order_data.lc_order != lc)
                        ad.add_action("ptlc", ac.holding_data.holding_side == "buy" ? "sell" : "buy", "", 0, 0, pt, lc, -1, "pt lc order"); //pt lc order
                }
            }

            //if (ac.holding_data.holding_size > 1.01)
            //    Console.WriteLine("Holding size is larger than 1.0!");

            return ad;
        }
    }
}

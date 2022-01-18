using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;


namespace BTCSIM2
{
    static public class LineChart
    {
        static public void DisplayLineChart3(List<double> capital, List<double> close, List<int> num_trade, List<string> table_labels, List<string> table_data, string title)
        {
            List<double> num_data_double = num_trade.Select(i => (double)i).ToList();
            Console.WriteLine("displaying line chart...");
            Encoding enc = Encoding.GetEncoding("UTF-8");
            StreamWriter writer = new StreamWriter(@"./line_chart.html", false, enc);
            writer.WriteLine(@"<!DOCTYPE html>" + "\r\n" +
                @"<html lang=""ja"">" + "\r\n" +
                @"<head>" + "\r\n" +
                @"<meta charset = ""utf-8"">" + "\r\n" +
                @"<meta name=""viewport"" content=""width = device - width, initial - scale = 1"">" + "\r\n" +
                @"<link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.0.0-beta1/dist/css/bootstrap.min.css"" rel=""stylesheet"" integrity=""sha384-giJF6kkoqNQ00vy+HMDP7azOuL0xtbfIcaT9wjKHr8RbDVddVHyTfAAsrekwKmP1"" crossorigin=""anonymous"">" + "\r\n" +
                @"<script src=""https://cdnjs.cloudflare.com/ajax/libs/Chart.js/2.7.2/Chart.bundle.js""></script>" + "\r\n" +
                @"<title> グラフ </title>" + "\r\n" +
                @"</head>" + "\r\n" +
                @"<body>" + "\r\n" +
                @"<h1>" + title + "</h1>" + "\r\n" +
                @"<script src=""https://cdn.jsdelivr.net/npm/bootstrap@5.0.0-beta1/dist/js/bootstrap.bundle.min.js"" integrity=""sha384-ygbV9kiqUc6oa4msXn9868pTtWMgiQaeYH7/t7LECLbyPA2x65Kgf80OJFdroafW"" crossorigin=""anonymous""></script>" + "\r\n" +
                @"<div class=""container"">" + "\r\n" +
                @"<div class=""row"">" + "\r\n" +
                @"<div class=""col-3"">" + "\r\n" +
                generateTable(table_labels, table_data) +
                @"</div>" + "\r\n" +
                @"<div class=""col-9"">" + "\r\n" +
                @"<div class=""row"">" + "\r\n" +
                generateCombinedLineChart(1, capital, close) +
                @"</div>" + "\r\n" +
                @"<div class=""row"">" + "\r\n" +
                generateLineChart(2, "num_trade", num_data_double) + "\r\n" +
                @"</div>" + "\r\n" +
                @"</div>" + "\r\n" +
                @"</div>" + "\r\n" +
                @"</body>" + "\r\n" +
                @"</html>" + "\r\n"
                );
            writer.Close();
        }

        static private string GenerateNumericalLabel(List<double> data)
        {
            string label = "labels: [";
            var num_array = Enumerable.Range(0, data.Count).ToArray();
            label += string.Join(", ", num_array) + "],\r\n";
            return label;
        }

        static private string GenerateData(List<double> data)
        {
            string d = "data: [";
            d += string.Join(", ", data) + "],\r\n";
            return d;
        }


        static private string generateChart(string chart_type, int axis_id, string label, List<double> data, int[] color)
        {
            string res = @"{" + "\r\n" +
                @"type: '" + chart_type + "'," + "\r\n" +
                @"label: '" + label + "'," + "\r\n" +
                GenerateData(data) +
                @"borderColor : 'rgba(" + color[0] + "," + color[1] + "," + color[2] + ",0.8)'," + "\r\n" +
                //@"backgroundColor: 'rgba(" + color[0] + "," + color[1] + "," + color[2] + ",0.8)'," + "\r\n" +
                @"pointStyle: 'circle'," + "\r\n" +
                @"pointRadius: 0, " + "\r\n" +
                @"borderWidth: 1," + "\r\n" +
                @"yAxisID: 'y-axis-" + axis_id + "'," + "\r\n" +
                @"}," + "\r\n";
            return res;
        }

        static private string generateComplexChartOption(List<double> data, List<double> close)
        {
            var data_max = Math.Round(data.Max() + (data.Max() - data.Min()) / 100.0);
            var close_max = Math.Round(close.Max() + (close.Max() - close.Min()) / 100.0);
            var data_min = data.Min();
            var close_min = close.Min();
            var data_stepsize = Math.Round((data.Max() - data.Min()) / 10.0);
            var close_stepsize = Math.Round((close.Max() - close.Min()) / 10.0);
            var res = @"<script>" + "\r\n" +
                @"var complexChartOption = {" + "\r\n" +
                @"responsive: true," + "\r\n" +
                @"scales:" + "\r\n" +
                @"{" + "\r\n" +
                @"yAxes:" + "\r\n" +
                @"[{" + "\r\n" +
                @"id: ""y-axis-1""," + "\r\n" +
                @"type: ""linear"", " + "\r\n" +
                @"position: ""left""," + "\r\n" +
                @"ticks:" + "\r\n" +
                @"{" + "\r\n" +
                @"max: " + data_max.ToString() + "," + "\r\n" +
                @"min: " + data_min.ToString() + "," + "\r\n" +
                @"stepSize: " + data_stepsize.ToString() + "\r\n" +
                @"}," + "\r\n" +
                @"}, {" + "\r\n" +
                @"id: ""y-axis-2""," + "\r\n" +
                @"type: ""linear"", " + "\r\n" +
                @"position: ""right""," + "\r\n" +
                @"ticks:" + "\r\n" +
                @"{" + "\r\n" +
                @"max: " + close_max.ToString() + "," + "\r\n" +
                @"min: " + close_min.ToString() + "," + "\r\n" +
                @"stepSize: " + close_stepsize.ToString() + "\r\n" +
                @"}," + "\r\n" +
                @"gridLines:" + "\r\n" +
                @"{" + "\r\n" +
                @"drawOnChartArea: true, " + "\r\n" +
                @"}," + "\r\n" +
                @"}]," + "\r\n" +
                @"}" + "\r\n" +
                @"};" + "\r\n" +
                @"</script>" + "\r\n";
            return res;
        }


        static private string generatePlotChart(int axis_id, string label, List<double> data, int[] color)
        {
            string res = @"{" + "\r\n" +
                @"type: '" + "line" + "'," + "\r\n" +
                @"label: '" + label + "'," + "\r\n" +
                GenerateData(data) +
                @"borderColor : 'rgba(" + color[0] + "," + color[1] + "," + color[2] + ",0.8)'," + "\r\n" +
                //@"backgroundColor: 'rgba(" + color[0] + "," + color[1] + "," + color[2] + ",0.8)'," + "\r\n" +
                @"borderWidth: 1," + "\r\n" +
                @"showLine: true," + "\r\n" +
                @"pointRadius: 0, " + "\r\n" +
                @"yAxisID: 'y-axis-" + axis_id + "'," + "\r\n" +
                @"}," + "\r\n";
            return res;
        }


        static private string generateTable(List<string> table_labels, List<string> table_data)
        {
            var labels = "";
            for (int i = 0; i < table_labels.Count; i++)
                labels += @"<tr>" + "\r\n" + @"<th scope = ""row"">" + table_labels[i].ToString() + @"</th>" + "\r\n" + @"<td>" + table_data[i].ToString() + @"</td>" + "\r\n";
            string res = @"<table class=""table"">" + "\r\n" +
                @"  <thead>" + "\r\n" +
                @"    <tr>" + "\r\n" +
                @"<th scope = ""col"" >Peformance Index</th>" + "\r\n" +
                @"<th scope = ""col"" >Val</th>" + "\r\n" +
                @"    <tr>" + "\r\n" +
                @"  </thead>" + "\r\n" +
                @"<tbody>" + "\r\n" +
                labels +
                @"</tbody>" + "\r\n" +
                @"</table>" + "\r\n";
            return res;
        }

        static private string generateCombinedLineChart(int chart_id, List<double> data, List<double> close)
        {
            var res = @"<canvas id = 'myLineChart" + chart_id.ToString() + "'></canvas>" + "\r\n" +
                @"<script>" + "\r\n" +
                @"window.addEventListener('load', function(){" + "\r\n" +
                @"ctx = document.getElementById('myLineChart" + chart_id.ToString() + "').getContext('2d');" + "\r\n" +
                @"window.myBar = new Chart(ctx, {" + "\r\n" +
                @"type: 'bar'," + "\r\n" +
                @"data: barChartData," + "\r\n" +
                @"options: complexChartOption" + "\r\n" +
                @"});" + "\r\n" +
                @"});" + "\r\n" +
                @"</script>" + "\r\n" +
                @"<script>" + "\r\n" +
                @"var barChartData = {" + "\r\n" +
                GenerateNumericalLabel(data) + "\r\n" +
                @"datasets:[" + "\r\n" +
                generatePlotChart(1, "Total Capital", data, new int[] { 50, 255, 128 }) + "\r\n" +
                generatePlotChart(2, "Close", close, new int[] { 255, 128, 50 }) + "\r\n" +
                @"]};" + "\r\n" + //need to check
                @"</script>" + "\r\n" +
                generateComplexChartOption(data, close);
            return res;
        }

        static private string generateLineChart(int chart_id, string label, List<double> data)
        {
            var res = @"<canvas id = 'myLineChart" + chart_id.ToString() + "'></canvas>" + "\r\n" +
                @"<script>" + "\r\n" +
                @"window.addEventListener('load', function(){" + "\r\n" +
                @"ctx = document.getElementById('myLineChart" + chart_id.ToString() + "').getContext('2d');" + "\r\n" +
                @"window.myBar = new Chart(ctx, {" + "\r\n" +
                @"type: 'line'," + "\r\n" +
                @"data:{" + "\r\n" +
                GenerateNumericalLabel(data) +
                @"datasets:[{" + "\r\n" +
                @"label:""" + label + @"""," + "\r\n" +
                GenerateData(data) + "\r\n" +
                @"}]}})});" + "\r\n" +
                @"</script>" + "\r\n";
            return res;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ChartJSCore.Models;
using FuncSharp;
using NodaTime;

namespace CodeInsight.Web.Common.Charts
{
    public enum ChartType
    {
        Line
    }
    
    public class Chart
    {
        public Chart(string title, ChartType type, Data data)
        {
            Id = "Chart" + Guid.NewGuid().ToString().Replace("-", "");
            Title = title;
            JsChart = new ChartJSCore.Models.Chart
            {
                Type = type.Match(
                    ChartType.Line, _ => "line"
                ),
                Data = data
            };
        }
        
        public string Id { get; }
        
        public string Title { get; }
        
        public ChartJSCore.Models.Chart JsChart { get; }
        
        public static Chart FromInterval(string title, DateInterval interval, IList<Dataset> dataSets)
        {
            return new Chart(title, ChartType.Line, new Data
            {
                Labels = interval.Select(d => $"{d.Day}.{d.Month}").ToImmutableList(),
                Datasets = dataSets
            });
        }
    }
}
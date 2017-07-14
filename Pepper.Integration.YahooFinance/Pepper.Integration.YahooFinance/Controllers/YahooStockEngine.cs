using Pepper.Framework.Authentication;
using Pepper.Framework.Extensions;
using Pepper.Framework.Validation;
using Pepper.Models.CodeFirst;
using Pepper.Models.CodeFirst.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Pepper.Integration.YahooFinance.Controllers
{
    internal class YahooStockEngine
    {
        private const string BASE_URL = "http://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20yahoo.finance.quotes%20where%20symbol%20in%20({0})&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys";

        public static void Fetch()
        {
            List<EquityRes> equity = GetSymbol();
            int i = 1;
            List<EquityRes> CurrentSymbols = new List<EquityRes>();
            foreach (EquityRes item in equity)
            {
                CurrentSymbols.Add(item);
                if (i % 50 == 0)
                {
                    string symbolList = String.Join("%2C", CurrentSymbols.Select(data => "%22" + data.Symbol + "%22").ToArray());
                    string url = string.Format(BASE_URL, symbolList);
                    XDocument doc = XDocument.Load(url);
                    Parse(CurrentSymbols, doc);
                    CurrentSymbols.Clear();
                    i = 0;
                }
                i++;
            }
            if (CurrentSymbols.Count > 0)
            {
                string symbolList = String.Join("%2C", CurrentSymbols.Select(data => "%22" + data.Symbol + "%22").ToArray());
                string url = string.Format(BASE_URL, symbolList);
                XDocument doc = XDocument.Load(url);
                Parse(CurrentSymbols, doc);
                CurrentSymbols.Clear();
                Console.WriteLine("Enter any key to continue...");
                Console.ReadKey();
            }
        }

        private static void Parse(List<EquityRes> equity, XDocument doc)
        {
            XElement results = doc.Root.Element("results");
            string exception = "";
            foreach (EquityRes item in equity)
            {
                try
                {
                    XElement q = results.Elements("quote").First(data => data.Attribute("symbol").Value == item.Symbol);
                    BackgroundJob _BackgroundJob = GetBackgroundJob(item.EntityId, item.Symbol);
                    _BackgroundJob.EntityID = item.EntityId;
                    _BackgroundJob.Symbol = item.Symbol;
                    _BackgroundJob.Ask = GetDecimal(q.Element("Ask").Value);
                    _BackgroundJob.Bid = GetDecimal(q.Element("Bid").Value);
                    _BackgroundJob.AverageDailyVolume = GetDecimal(q.Element("AverageDailyVolume").Value);
                    _BackgroundJob.BookValue = GetDecimal(q.Element("BookValue").Value);
                    _BackgroundJob.Change = GetDecimal(q.Element("Change").Value);
                    _BackgroundJob.DividendShare = GetDecimal(q.Element("DividendShare").Value);
                    _BackgroundJob.LastTradeDate = GetDateTime(q.Element("LastTradeDate") + " " + q.Element("LastTradeTime").Value);
                    _BackgroundJob.EarningsShare = GetDecimal(q.Element("EarningsShare").Value);
                    _BackgroundJob.EpsEstimateCurrentYear = GetDecimal(q.Element("EPSEstimateCurrentYear").Value);
                    _BackgroundJob.EpsEstimateNextYear = GetDecimal(q.Element("EPSEstimateNextYear").Value);
                    _BackgroundJob.EpsEstimateNextQuarter = GetDecimal(q.Element("EPSEstimateNextQuarter").Value);
                    _BackgroundJob.DailyLow = GetDecimal(q.Element("DaysLow").Value);
                    _BackgroundJob.DailyHigh = GetDecimal(q.Element("DaysHigh").Value);
                    _BackgroundJob.YearlyLow = GetDecimal(q.Element("YearLow").Value);
                    _BackgroundJob.YearlyHigh = GetDecimal(q.Element("YearHigh").Value);
                    _BackgroundJob.MarketCapitalization = GetDecimal(q.Element("MarketCapitalization").Value);
                    _BackgroundJob.Ebitda = GetDecimal(q.Element("EBITDA").Value);
                    _BackgroundJob.ChangeFromYearLow = GetDecimal(q.Element("ChangeFromYearLow").Value);
                    _BackgroundJob.PercentChangeFromYearLow = GetDecimal(q.Element("PercentChangeFromYearLow").Value);
                    _BackgroundJob.ChangeFromYearHigh = GetDecimal(q.Element("ChangeFromYearHigh").Value);
                    _BackgroundJob.LastTradePrice = GetDecimal(q.Element("LastTradePriceOnly").Value);
                    _BackgroundJob.PercentChangeFromYearHigh = GetDecimal(q.Element("PercebtChangeFromYearHigh").Value); //missspelling in yahoo for field name
                    _BackgroundJob.FiftyDayMovingAverage = GetDecimal(q.Element("FiftydayMovingAverage").Value);
                    _BackgroundJob.TwoHunderedDayMovingAverage = GetDecimal(q.Element("TwoHundreddayMovingAverage").Value);
                    _BackgroundJob.ChangeFrom200DayMovingAverage = GetDecimal(q.Element("ChangeFromTwoHundreddayMovingAverage").Value);
                    _BackgroundJob.PercentChangeFrom200DayMovingAverage = GetDecimal(q.Element("PercentChangeFromTwoHundreddayMovingAverage").Value);
                    _BackgroundJob.PercentChangeFrom50DayMovingAverage = GetDecimal(q.Element("PercentChangeFromFiftydayMovingAverage").Value);
                    _BackgroundJob.Name = q.Element("Name").Value;
                    _BackgroundJob.Open = GetDecimal(q.Element("Open").Value);
                    _BackgroundJob.PreviousClose = GetDecimal(q.Element("PreviousClose").Value);
                    _BackgroundJob.ChangeInPercent = GetDecimal(q.Element("ChangeinPercent").Value);
                    _BackgroundJob.PriceSales = GetDecimal(q.Element("PriceSales").Value);
                    _BackgroundJob.PriceBook = GetDecimal(q.Element("PriceBook").Value);
                    _BackgroundJob.ExDividendDate = GetDateTime(q.Element("ExDividendDate").Value);
                    _BackgroundJob.PeRatio = GetDecimal(q.Element("PERatio").Value);
                    _BackgroundJob.DividendPayDate = GetDateTime(q.Element("DividendPayDate").Value);
                    _BackgroundJob.PegRatio = GetDecimal(q.Element("PEGRatio").Value);
                    _BackgroundJob.PriceEpsEstimateCurrentYear = GetDecimal(q.Element("PriceEPSEstimateCurrentYear").Value);
                    _BackgroundJob.PriceEpsEstimateNextYear = GetDecimal(q.Element("PriceEPSEstimateNextYear").Value);
                    _BackgroundJob.ShortRatio = GetDecimal(q.Element("ShortRatio").Value);
                    _BackgroundJob.OneYearPriceTarget = GetDecimal(q.Element("OneyrTargetPrice").Value);
                    _BackgroundJob.Volume = GetDecimal(q.Element("Volume").Value);
                    _BackgroundJob.StockExchange = q.Element("StockExchange").Value;
                    _BackgroundJob.LastUpdatedDate = DateTime.Now;
                    _BackgroundJob.LastUpdatedBy = Authentication.CurrentUser.UserID;
                    IEnumerable<ErrorInfo> errorInfo = _BackgroundJob.Save();
                }
                catch (Exception e)
                {
                    exception += e.Message;
                }
            }

            #region comments

            //foreach (Quote quote in quotes)
            //{
            //    XElement q = results.Elements("quote").First(w => w.Attribute("symbol").Value == quote.Symbol);

            //    quote.Ask = GetDecimal(q.Element("Ask").Value);
            //    quote.Bid = GetDecimal(q.Element("Bid").Value);
            //    quote.AverageDailyVolume = GetDecimal(q.Element("AverageDailyVolume").Value);
            //    quote.BookValue = GetDecimal(q.Element("BookValue").Value);
            //    quote.Change = GetDecimal(q.Element("Change").Value);
            //    quote.DividendShare = GetDecimal(q.Element("DividendShare").Value);
            //    quote.LastTradeDate = GetDateTime(q.Element("LastTradeDate") + " " + q.Element("LastTradeTime").Value);
            //    quote.EarningsShare = GetDecimal(q.Element("EarningsShare").Value);
            //    quote.EpsEstimateCurrentYear = GetDecimal(q.Element("EPSEstimateCurrentYear").Value);
            //    quote.EpsEstimateNextYear = GetDecimal(q.Element("EPSEstimateNextYear").Value);
            //    quote.EpsEstimateNextQuarter = GetDecimal(q.Element("EPSEstimateNextQuarter").Value);
            //    quote.DailyLow = GetDecimal(q.Element("DaysLow").Value);
            //    quote.DailyHigh = GetDecimal(q.Element("DaysHigh").Value);
            //    quote.YearlyLow = GetDecimal(q.Element("YearLow").Value);
            //    quote.YearlyHigh = GetDecimal(q.Element("YearHigh").Value);
            //    quote.MarketCapitalization = GetDecimal(q.Element("MarketCapitalization").Value);
            //    quote.Ebitda = GetDecimal(q.Element("EBITDA").Value);
            //    quote.ChangeFromYearLow = GetDecimal(q.Element("ChangeFromYearLow").Value);
            //    quote.PercentChangeFromYearLow = GetDecimal(q.Element("PercentChangeFromYearLow").Value);
            //    quote.ChangeFromYearHigh = GetDecimal(q.Element("ChangeFromYearHigh").Value);
            //    quote.LastTradePrice = GetDecimal(q.Element("LastTradePriceOnly").Value);
            //    quote.PercentChangeFromYearHigh = GetDecimal(q.Element("PercebtChangeFromYearHigh").Value); //missspelling in yahoo for field name
            //    quote.FiftyDayMovingAverage = GetDecimal(q.Element("FiftydayMovingAverage").Value);
            //    quote.TwoHunderedDayMovingAverage = GetDecimal(q.Element("TwoHundreddayMovingAverage").Value);
            //    quote.ChangeFromTwoHundredDayMovingAverage = GetDecimal(q.Element("ChangeFromTwoHundreddayMovingAverage").Value);
            //    quote.PercentChangeFromTwoHundredDayMovingAverage = GetDecimal(q.Element("PercentChangeFromTwoHundreddayMovingAverage").Value);
            //    quote.PercentChangeFromFiftyDayMovingAverage = GetDecimal(q.Element("PercentChangeFromFiftydayMovingAverage").Value);
            //    quote.Name = q.Element("Name").Value;
            //    quote.Open = GetDecimal(q.Element("Open").Value);
            //    quote.PreviousClose = GetDecimal(q.Element("PreviousClose").Value);
            //    quote.ChangeInPercent = GetDecimal(q.Element("ChangeinPercent").Value);
            //    quote.PriceSales = GetDecimal(q.Element("PriceSales").Value);
            //    quote.PriceBook = GetDecimal(q.Element("PriceBook").Value);
            //    quote.ExDividendDate = GetDateTime(q.Element("ExDividendDate").Value);
            //    quote.PeRatio = GetDecimal(q.Element("PERatio").Value);
            //    quote.DividendPayDate = GetDateTime(q.Element("DividendPayDate").Value);
            //    quote.PegRatio = GetDecimal(q.Element("PEGRatio").Value);
            //    quote.PriceEpsEstimateCurrentYear = GetDecimal(q.Element("PriceEPSEstimateCurrentYear").Value);
            //    quote.PriceEpsEstimateNextYear = GetDecimal(q.Element("PriceEPSEstimateNextYear").Value);
            //    quote.ShortRatio = GetDecimal(q.Element("ShortRatio").Value);
            //    quote.OneYearPriceTarget = GetDecimal(q.Element("OneyrTargetPrice").Value);
            //    quote.Volume = GetDecimal(q.Element("Volume").Value);
            //    quote.StockExchange = q.Element("StockExchange").Value;
            //    quote.LastUpdate = DateTime.Now;
            //}

            #endregion comments
        }

        private static decimal? GetDecimal(string input)
        {
            if (input == null) return null;

            input = input.Replace("%", "");

            decimal value;

            if (Decimal.TryParse(input, out value)) return value;
            return null;
        }

        private static DateTime? GetDateTime(string input)
        {
            if (input == null) return null;

            DateTime value;

            if (DateTime.TryParse(input, out value)) return value;
            return null;
        }

        public static List<EquityRes> GetSymbol()
        {
            List<EquityRes> Symbol;
            using (PepperContext context = new PepperContext())
            {
                Symbol = (from q in context.Equities
                          join t in context.AppStoreEntityApps
                          on q.EntityID equals t.EntityID
                          where
                             t.IsEnabled == true &&
                             t.AppStoreAppID == (int)AppStoreApplicationType.YahooFinance && // yahoo finance app id
                             q.Symbol != null
                          group q by new
                          {
                              q.Symbol,
                              q.EntityID
                          } into g
                          select new EquityRes
                          {
                              Symbol = g.Key.Symbol,
                              EntityId = g.Key.EntityID
                          }).Distinct().ToList();
            }
            return Symbol;
        }

        public static BackgroundJob FindBackgroundJob(int Id, string Symbol)
        {
            using (PepperContext context = new PepperContext())
            {
                return context.BackgroundJobs.EntityFilter().FirstOrDefault(type => type.EntityID == Id && type.Symbol == Symbol);
            }
        }

        public static BackgroundJob GetBackgroundJob(int Id, String Symbol)
        {
            BackgroundJob _BackgroundJob = null;
            if(Id > 0 && Symbol != "")
               _BackgroundJob = FindBackgroundJob(Id, Symbol);
            if (_BackgroundJob == null)
            {
                _BackgroundJob = new BackgroundJob();
                _BackgroundJob.CreatedBy = Authentication.CurrentUser.UserID;
                _BackgroundJob.CreatedDate = DateTime.Now;
            }
            return _BackgroundJob;
        }
    }

    public class EquityRes
    {
        public string Symbol { get; set; }
        public int EntityId { get; set; }
    }
}
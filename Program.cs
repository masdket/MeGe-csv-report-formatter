using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
using CsvHelper.Configuration.Attributes;
using System.Dynamic;
using CsvHelper;
namespace Orders
{
    class Program
    {         
        static void Main(string[] args) {
            var suburbDic = new Dictionary<string, string>();

            using (var reader = new StreamReader("suburb.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture)) {
                csv.Configuration.HasHeaderRecord = false;
                List<Subrb> records = csv.GetRecords<Subrb>().ToList();
                records.ForEach(x =>  suburbDic.TryAdd(x.name, x.day));
            }

            using (var reader = new StreamReader("orders.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture)) {
                csv.Configuration.HasHeaderRecord = true;
                List<Order> records = csv.GetRecords<Order>().OrderBy(x => x.orderId).ToList();
                List<string> dynColumns = records.Select(x => x.lineItemName).Distinct().ToList();

                var orderList =  records.Aggregate(new List<dynamic>(), (outputOrderList, order) => {
                    if (!outputOrderList.Any() || ((IDictionary<string, object>)outputOrderList.Last())["OrderId"].ToString() != order.orderId) {
                        outputOrderList.Add(GetNewDynOrder(order, suburbDic, dynColumns));
                    } else {
                        SetDynItemValues(order, outputOrderList[outputOrderList.Count-1]);
                    }
                    return outputOrderList;
                });

                var finalOrderOutput = orderList.OrderBy(x=> x.ShippingMethod).ThenBy(x => x.DeliveryDay).ToList();
                using (var writer = new StreamWriter("output.csv"))
                using (var csvOut = new CsvWriter(writer, CultureInfo.InvariantCulture)) {    
                    csvOut.WriteRecords(finalOrderOutput);
                }
            }
        }

        private static ExpandoObject GetNewDynOrder(Order order, Dictionary<string, string> suburbDictionary, List<string> dynColumns) {
            string weekOfDayString = "";

            suburbDictionary.TryGetValue(order.suburb.ToString(), out weekOfDayString);
            weekOfDayString = string.IsNullOrEmpty(weekOfDayString) ? "Sunday" : weekOfDayString;

            var dayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), weekOfDayString);
            dynamic ordOuptput = new ExpandoObject();

            ordOuptput.OrderId = order.orderId;
            ordOuptput.ShippingName = order.shippingName;
            ordOuptput.ShippingMethod = order.shippingMethod;
            ordOuptput.Email = order.emailAddress;
            ordOuptput.ShippingPhone = order.shippingPhone;
            var expandoDict = ordOuptput as IDictionary<string, object>;
            dynColumns.ForEach(col => expandoDict.Add(col, ""));
            ordOuptput.ShippingAddress = $"{order.shippingAddress1}.{order.shippingAddress2}";
            ordOuptput.ShippingZip = order.shippingZip;
            ordOuptput.Suburb = order.suburb;
            ordOuptput.DeliveryDay = dayOfWeek;

            SetDynItemValues(order, ordOuptput);
            return ordOuptput;
        }
        private static void SetDynItemValues(Order order, ExpandoObject outputOrder) {
           
            var expandoDict = outputOrder as IDictionary<string, object>;
            string value = string.IsNullOrEmpty(order.lineItemVariant) ? order.lineItemQuantity : $"({order.lineItemVariant}:{order.lineItemQuantity})";


            if(expandoDict.ContainsKey(order.lineItemName)) {
               expandoDict[order.lineItemName] += $"{value}";
            } else {
               expandoDict.Add(order.lineItemName, value);
            }
        }
    }
    public class Order {
        
        [Name("Order ID")]
        public string orderId { get; set; }
        
        [Name("Shipping Name")]
        public string shippingName {get;set;}
        
        [Name("Email")]
        public string emailAddress { get; set; }
        
        [Name("Lineitem quantity")]
        public string lineItemQuantity { get;set; }
        [Name("Lineitem variant")]
        public string lineItemVariant{ get;set; }

        [Name("Lineitem name")]
        public string lineItemName { get;set; }
        
        [Name("Shipping Address1")]
        public string shippingAddress1{ get;set; }

        [Name("Shipping Address2")]
        public string shippingAddress2{ get;set; }

        [Name("Shipping Zip")]
        public string shippingZip { get;set; }
        
        [Name("Shipping Phone")]
        public string shippingPhone { get;set; }
        
        [Name("Checkout Form: Select Your Delivery Suburb or Pickup Day")]
        public string suburb { get;set; }

        [Name("Shipping Method")]
        public string shippingMethod { get;set;} 
    }
     public class OrderOutput {
        [Name("Order ID")]
        public string orderId { get; set; }
        [Name("Shipping Name")]
        public string shippingName {get;set;}
        [Name("Email")]
        public string emailAddress { get; set; }
        [Name("Shipping Phone")]
        public string shippingPhone {get;set;}
        [Name("MeGe Vege Box")]
        public string meGeVegeBox {get;set;}
        [Name("MeGe Fruit Box")]
        public string meGeFruitBox {get;set;}
        [Name("MeGe Meat Box")]
        public string meGeMeatBox {get;set;}
        [Name("Anchor Milk 2L")]
        public string milk {get;set;}
        [Name("Aoraki Hot Smoked Salmon 180g")]
        public string salmon {get;set;}
        [Name("Shipping Address")]
        public string shippingAddress{get;set;}
        [Name("Shipping Zip")]
        public string shippingZip {get;set;}
        [Name("Suburb")]
        public string suburb {get;set;}
        [Name("Delivery Day")]
        public DayOfWeek deliveryDay {get;set;} 
    }
    public class Subrb {
        public string name {get;set;} 
        public string day {get;set;} 
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Xml.Serialization;

namespace DevExpress.RealtorWorld.Xpf.Model {
    [XmlType("Homes")]
    public class Home {
        public int ID { get; set; }
        public string Address { get; set; }
        public int Beds { get; set; }
        public int Baths { get; set; }
        public double HouseSize { get; set; }
        public double LotSize { get; set; }
        public decimal Price { get; set; }
        public string Features { get; set; }
        public string YearBuilt { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public byte[] Photo { get; set; }
    }
    [XmlType("Homes")]
    public class HomePhoto {
        public int ID { get; set; }
        public int ParentID { get; set; }
        public byte[] Photo { get; set; }
    }
    [XmlType("Agents")]
    public class Agent {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public byte[] Photo { get; set; }
        public string Email { get; set; }
    }
    public class AgentStatisticData {
        public string Region { get; set; }
        public int Year { get; set; }
        public int Value { get; set; }
    }
    public class StaticticPoint {
        public object Argument { get; set; }
        public object Value { get; set; }
    }
    public class MortgageRate {
        public DateTime Date { get; set; }
        public double FRM30 { get; set; }
        public double FRM15 { get; set; }
        public double ARM1 { get; set; }
    }
    public class SimilarHousesStatisticData {
        public int Year { get; set; }
        public int ProposalCount { get; set; }
        public int SoldCount { get; set; }
    }
    public class Period {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public string Region { get; set; }
        public string SeasonallyAdjusted { get; set; }
        public string Type { get; set; }
    }
}

using System.Collections.Generic;

namespace Domain
{
    public class RouteNumber
    {
        public List<Offer> offers;

        public int RouteID { get; set; }
        public int RequiredVehicleType { get; set; }
        public double Weekdays { get; set; }
        public double Holidays { get; set; }
        public int Weekends { get; set; }
        public int ClosedDays { get; set; }
        public int Vacation { get; set; }
        
        public RouteNumber()
        {
            offers = new List<Offer>(); 
        }
        public RouteNumber(int routeID, int requiredVehicleType) : this()
        {          
            this.RouteID = routeID;
            this.RequiredVehicleType = requiredVehicleType;
            
        }
    }
}

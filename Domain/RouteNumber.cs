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
        public double Weekends { get; set; }
        public int ClosedDays { get; set; }
        public int Vacation { get; set; }

        public readonly int PeriodInt = 2;
        public int Period
        {
            get
            {
                return PeriodInt;
            }
        }
        public double CalculatedHours
        {
            get;
            set;
        }

        public RouteNumber()
        {
            offers = new List<Offer>();
        }
        public RouteNumber(int routeID, int requiredVehicleType, double weekdays, double weekends, int closedDays, double holidays, int vacation) : this()
        {          
            this.RouteID = routeID;
            this.RequiredVehicleType = requiredVehicleType;
            this.Weekdays = weekdays;
            this.Holidays = holidays;
            this.Weekends = weekends;
            this.ClosedDays = closedDays;
            this.Vacation = vacation;
            double hoursWithHolidays = CalculateWeeksAndDays(Period, Vacation, Weekdays, Holidays, Weekends, ClosedDays);
            this.CalculatedHours = CalculateHoursPrVehicle(Period, hoursWithHolidays);
            foreach (Offer offer in offers)
            {
                offer.OperationPrice = CalculateSum(offer.OperationPrice);
            }
        }
        

        
        public double CalculateWeeksAndDays(int Period, int Vacation, double Weekdays, double holidays, double weekends, int ClosedDays)
        {
            //period er år i kontraktperioden
            //vacation er uger med ferie

            int weeks = 52; //tager ikke højde for skudår
            int workingWeeks = Period * (weeks - Vacation); //antal uger hvor der arbejdes

            int weekHolidays = 6; //antal helligdage
            double hoursWithHolidays = (workingWeeks * (Weekdays * 5 + Weekends * 2 - ClosedDays)) + (Weekdays * weekHolidays) - (Holidays * weekHolidays);

            return hoursWithHolidays;
        }

        public double CalculateHoursPrVehicle(int period, double hoursWithHolidays) //period og hoursWithHolidays skan hentes fra CalculateWeeksAndDays
        {
            double hoursPrVehicle = hoursWithHolidays / period;
            return hoursPrVehicle;
        }
        public float CalculateSum(float operationPrice) //hoursPrVehicle skal hentes fra CalculateHoursPrVehicle
        {
            float calculatedSum = operationPrice * (float) this.CalculatedHours;
            return calculatedSum;
        }
    }
}

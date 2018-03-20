using System;
using System.Collections.Generic;
using System.Linq;
using Domain;

namespace Logic
{
    public class Selection
    {
        ListContainer listContainer = ListContainer.Instance;

        public double CalculateWeeksAndDays(int period, int vacation, double weekdays, double holidays, double weekends, int closedDays)
        {
            //period er år i kontraktperioden
            //vacation er uger med ferie

            int weeks = 52; //tager ikke højde for skudår
            int workingWeeks = period * (weeks - vacation); //antal uger hvor der arbejdes

            int weekHolidays = 6; //antal helligdage
            double hoursWithHolidays = (workingWeeks * (weekdays * 5 + weekends * 2 - closedDays)) + (weekdays * weekHolidays) - (holidays * weekHolidays);

            return hoursWithHolidays;
        }

        public double CalculateHoursPrVehicle(int period, double hoursWithHolidays) //period og hoursWithHolidays skan hentes fra CalculateWeeksAndDays
        {
            double hoursPrVehicle = hoursWithHolidays / period;
            return hoursPrVehicle;
        }
        public double CalculateSum(int hourlyPrice, double hoursPrVehicle) //hoursPrVehicle skal hentes fra CalculateHoursPrVehicle
        {
            double calculatedSum = hourlyPrice * hoursPrVehicle;
            return calculatedSum;
        }


        public void CalculateOperationPriceDifferenceForOffers(List<RouteNumber> sortedRouteNumberList)
        {
            const int LAST_OPTION_VALUE = int.MaxValue;
            foreach (RouteNumber routeNumber in sortedRouteNumberList)
            {
                int numbersToCalc = (routeNumber.offers.Count()) - 1;
                if (routeNumber.offers.Count == 0)
                {
                    throw new Exception("Der er ingen bud på garantivognsnummer " + routeNumber.RouteID);
                }
                else if (routeNumber.offers.Count == 1)
                {
                    routeNumber.offers[0].DifferenceToNextOffer = LAST_OPTION_VALUE;
                }
                else if (routeNumber.offers.Count == 2)
                {
                    if (routeNumber.offers[0].OperationPrice == routeNumber.offers[1].OperationPrice)
                    {
                        routeNumber.offers[0].DifferenceToNextOffer = LAST_OPTION_VALUE;
                        routeNumber.offers[1].DifferenceToNextOffer = LAST_OPTION_VALUE;
                    }
                    else
                    {
                        routeNumber.offers[0].DifferenceToNextOffer = routeNumber.offers[1].OperationPrice - routeNumber.offers[0].OperationPrice;
                        routeNumber.offers[1].DifferenceToNextOffer = LAST_OPTION_VALUE;
                    }
                }
                else
                {
                    for (int i = 0; i < numbersToCalc; i++)
                    {
                        float difference = 0;
                        int j = i + 1;
                        if (routeNumber.offers[i].OperationPrice != routeNumber.offers[numbersToCalc].OperationPrice)
                        {
                            while (difference == 0 && j <= numbersToCalc)
                            {
                                difference = routeNumber.offers[j].OperationPrice - routeNumber.offers[i].OperationPrice;
                                j++;
                            }
                        }
                        else
                        {
                            while (i < numbersToCalc)
                            {
                                routeNumber.offers[i].DifferenceToNextOffer = LAST_OPTION_VALUE;
                                i++;
                            }
                        }
                        routeNumber.offers[i].DifferenceToNextOffer = difference;
                    }
                    routeNumber.offers[numbersToCalc].DifferenceToNextOffer = LAST_OPTION_VALUE;

                }
            }
        }
        public void CheckIfContractorHasWonTooManyRouteNumbers(List<Offer> offersToCheck, List<RouteNumber> sortedRouteNumberList)
        {
            List<Contractor> contractorsToCheck = new List<Contractor>();
            foreach (Offer offer in offersToCheck)
            {
                foreach (Contractor contractor in listContainer.contractorList)
                {
                    if (contractor.UserID.Equals(offer.UserID))
                    {
                        bool alreadyOnList = contractorsToCheck.Any(obj => obj.UserID.Equals(contractor.UserID));
                        if (!alreadyOnList)
                        {
                            contractorsToCheck.Add(contractor);
                        }
                    }
                }
            }
            foreach (Contractor contractor in contractorsToCheck)
            {
                List<Offer> offers = contractor.CompareNumberOfWonOffersAgainstVehicles();
                if (offers.Count > 0 && offers != null)
                {
                    foreach (Offer offer in contractor.CompareNumberOfWonOffersAgainstVehicles())
                    {
                        bool alreadyOnList = listContainer.conflictList.Any(item => item.OfferReferenceNumber == offer.OfferReferenceNumber);

                        listContainer.conflictList.Add(offer);


                    }
                    throw new Exception("Denne entreprenør har vundet flere garantivognsnumre, end de har biler til.  Der kan ikke vælges imellem dem, da de har samme prisforskel ned til næste bud. Prioriter venligst buddene i den relevante fil i kolonnen Entreprenør Prioritet");
                }
            }
        }
        public List<Offer> FindWinner(RouteNumber routeNumber)
        {
            List<Offer> winningOffers = new List<Offer>();
            List<Offer> listOfOffersWithLowestPrice = new List<Offer>();
            int lengthOfOffers = routeNumber.offers.Count();
            float lowestEligibleOperationPrice = 0;
            bool cheapestNotFound = true;

            for (int i = 0; i < lengthOfOffers; i++)
            {
                if (routeNumber.offers[i].IsEligible && cheapestNotFound)
                {
                    lowestEligibleOperationPrice = routeNumber.offers[i].OperationPrice;
                    cheapestNotFound = false;
                }
            }
            foreach (Offer offer in routeNumber.offers)
            {
                if (offer.IsEligible && offer.OperationPrice == lowestEligibleOperationPrice)
                {
                    listOfOffersWithLowestPrice.Add(offer);
                }
            }

            int count = 0;
            foreach (Offer offer in listOfOffersWithLowestPrice) // Checking if offers with same price are prioritized
            {
                if (offer.RouteNumberPriority != 0)
                {
                    count++;
                }
            }
            if (count != 0) //if routenumberpriority found 

            {
                List<Offer> listOfPriotizedOffers = new List<Offer>();
                foreach (Offer offer in listOfOffersWithLowestPrice)
                {
                    if (offer.RouteNumberPriority > 0)
                    {
                        listOfPriotizedOffers.Add(offer);
                    }
                }

                listOfPriotizedOffers = listOfPriotizedOffers.OrderBy(x => x.RouteNumberPriority).ToList();
                winningOffers.Add(listOfPriotizedOffers[0]);
            }
            else
            {
                foreach (Offer offer in listOfOffersWithLowestPrice)
                {
                    winningOffers.Add(offer);
                }
            }
            return winningOffers;
        }
        public List<Offer> AssignWinners(List<Offer> offersToAssign)
        {
            List<Contractor> contractorsToCheck = new List<Contractor>();

            foreach (Offer offer in offersToAssign)
            {
                if (offer.IsEligible)
                {
                    listContainer.contractorList.Find(x => x.UserID == offer.UserID).AddWonOffer(offer);
                    contractorsToCheck.Add(offer.Contractor);
                }
            }
            return ReturnIneligibleOffers();
        }
        public List<Offer> ReturnIneligibleOffers()
        {
            List<Contractor> contractorsToCheck = new List<Contractor>();
            List<Offer> ineligibleOffersAllContractors = new List<Offer>();
            int lengthOfContractorList = contractorsToCheck.Count();
            for (int i = 0; i < lengthOfContractorList; i++)
            {
                contractorsToCheck[i].CompareNumberOfWonOffersAgainstVehicles();
                List<Offer> ineligibleOffersOneContractor = contractorsToCheck[i].ReturnIneligibleOffers();
                ineligibleOffersAllContractors.AddRange(ineligibleOffersOneContractor);
                contractorsToCheck[i].RemoveIneligibleOffersFromWinningOffers();
            }
            return ineligibleOffersAllContractors;
        }
            
        public void CheckForMultipleWinnersForEachRouteNumber(List<Offer> winnerList)
        {
            int length = winnerList.Count;
            for (int i = 0; i < length; i++)
            {
                for (int j = i + 1; j < length; j++)
                {
                    if (winnerList[i].RouteID == winnerList[j].RouteID)
                    {
                        foreach (Offer offer in winnerList)
                        {
                            if (offer.RouteID == winnerList[i].RouteID)
                            {
                                listContainer.conflictList.Add(offer);
                            }
                        }
                        throw new Exception("Dette garantivognsnummer har flere mulige vindere. Der kan ikke vælges mellem dem, da de har samme prisforskel ned til næste bud. Prioriter venligst buddene i den relevante fil i kolonnen Garantivognsnummer Prioritet.");
                    }
                }
            }
        }
    }
}

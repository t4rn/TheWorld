using System.Collections.Generic;
using System.Threading.Tasks;

namespace TheWorld.Models
{
    public interface IWorldRepository
    {
        IEnumerable<Trip> GetAllTrips();

        IEnumerable<Trip> GetTripsByUsername(string username);

        Trip GetTripByName(string tripName);

        Trip GetTripByNameAndUser(string tripName, string username);

        void AddTrip(Trip trip);

        void AddStop(string tripName, Stop newStop, string username);

        Task<bool> SaveChangesAsync();

        void DeleteTrip(string tripName, string username);

        void DeleteStop(string tripName, string stopName, string username);
    }
}
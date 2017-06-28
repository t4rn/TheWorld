using System.Threading.Tasks;
using TheWorld.Models;

namespace TheWorld.Services
{
    public interface IGeoCoordsService
    {
        Task<GeoCoordsResult> GetCoordsAsync(string name);
    }
}
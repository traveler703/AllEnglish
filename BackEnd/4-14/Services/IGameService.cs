using AllEnBackend.Models;
using AllEnBackend.Dtos;

namespace AllEnBackend.Services
{
    public interface IGameService
    {
        public Task<List<Puzzle>> GetPuzzle();
        public Task<List<Clue>> GetClue(string Id);

    }
}


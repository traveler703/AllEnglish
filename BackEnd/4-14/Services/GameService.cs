using AllEnBackend.Models;
using Microsoft.AspNetCore.Identity;
using Oracle.ManagedDataAccess.Client;
using System.Net.Mail;
using System.Net;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.PortableExecutable;
using Microsoft.EntityFrameworkCore;
using AllEnBackend.Data;

namespace AllEnBackend.Services
{
    public class GameService : IGameService
    {

        //链接数据库
        private readonly string _connectionString;
        private readonly AppDbContext _context;

        // 构造函数
        public GameService(AppDbContext context, IConfiguration configuration)
        {
            // 连接数据库
            _connectionString = configuration.GetConnectionString("OracleDb") ?? throw new ArgumentNullException("OracleDb 连接字符串未配置");
            _context = context;
        }

        public async Task<List<Puzzle>> GetPuzzle()
        {
            return await _context.Puzzle.ToListAsync();
        }

        public async Task<List<Clue>> GetClue(string Id)
        {
            return await _context.Clue
                .Where(e => e.PuzzleId == Id)
                .ToListAsync();
        }

    }
}

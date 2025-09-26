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
using AllEnBackend.Dtos;
using AllEnBackend.Data;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace AllEnBackend.Services
{
    public class PayingService : IPayingService
    {
        private readonly AppDbContext _context;
        private readonly string _connectionString;

        public PayingService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("OracleDb") ?? throw new ArgumentNullException("OracleDb 连接字符串未配置");
        }

        public async Task<int> GetUserCoinAsync(string userId)
        {
            var userCoin = await _context.UserCoins.FirstOrDefaultAsync(u => u.UserId == userId);
            return userCoin?.Coin ?? 0;
        }

        public async Task<SignInResultDto> DailySignInAsync(string userId)
        {
            var userCoin = await _context.UserCoins.FirstOrDefaultAsync(u => u.UserId == userId);
            if (userCoin == null)
            {
                return new SignInResultDto
                {
                    Success = false,
                    Message = "用户不存在虚拟币账户",
                    Coin = 0
                };
            }

            var today = DateTime.Today;
            if (userCoin.LastSignDate != null && userCoin.LastSignDate.Value.Date == today)
            {
                return new SignInResultDto
                {
                    Success = false,
                    Message = "今日已签到",
                    Coin = userCoin.Coin
                };
            }

            userCoin.Coin += 100;
            userCoin.LastSignDate = today;

            if (userCoin.FirstSignDate == null)
            {
                userCoin.FirstSignDate = today;
            }
            else
            {
                Console.WriteLine($"LastSignDate: {userCoin.LastSignDate}, FirstSignDate: {userCoin.FirstSignDate}");
                Console.WriteLine($"Days diff: {(userCoin.LastSignDate.Value - userCoin.FirstSignDate.Value).Days}");

                if ((userCoin.LastSignDate.Value - userCoin.FirstSignDate.Value).Days >= 7)
                {
                    var temp = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                    await _context.SaveChangesAsync();
                    temp.Category = "VIP";
                }
            }

            await _context.SaveChangesAsync();

            return new SignInResultDto
            {
                Success = true,
                Message = "签到成功，获得100虚拟币",
                Coin = userCoin.Coin
            };
        }

        public async Task<PurchaseResultDto> PurchaseMaterialAsync(string userId, string materialId)
        {
            Console.WriteLine($"开始购买流程 - 用户ID: {userId}, 资源ID: {materialId}");
            
            // 检查用户是否存在
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                Console.WriteLine($"用户不存在: {userId}");
                return new PurchaseResultDto
                {
                    Success = false,
                    Message = "用户不存在",
                    RemainingCoins = 0
                };
            }
            Console.WriteLine($"用户存在: {user.UserName}");

            // 检查资源是否存在
            var material = await _context.Materials.FirstOrDefaultAsync(m => m.Id == materialId);
            if (material == null)
            {
                Console.WriteLine($"资源不存在: {materialId}");
                // 输出所有可用的资源ID用于调试
                var allMaterials = await _context.Materials.ToListAsync();
                Console.WriteLine($"数据库中所有资源: {string.Join(", ", allMaterials.Select(m => m.Id))}");
                
                return new PurchaseResultDto
                {
                    Success = false,
                    Message = "资源不存在",
                    RemainingCoins = 0
                };
            }
            Console.WriteLine($"资源存在: {material.Description}, 价格: {material.Price}");

            // 检查用户是否已拥有该资源
            var existingInventory = await _context.UserInventories
                .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.MaterialId == materialId && ui.IsActive == 1);
            if (existingInventory != null)
            {
                return new PurchaseResultDto
                {
                    Success = false,
                    Message = "您已拥有该资源",
                    RemainingCoins = 0
                };
            }

            // 获取用户虚拟币余额
            var userCoin = await _context.UserCoins.FirstOrDefaultAsync(uc => uc.UserId == userId);
            if (userCoin == null)
            {
                return new PurchaseResultDto
                {
                    Success = false,
                    Message = "用户虚拟币账户不存在",
                    RemainingCoins = 0
                };
            }

            // 检查余额是否足够
            if (userCoin.Coin < material.Price)
            {
                return new PurchaseResultDto
                {
                    Success = false,
                    Message = $"虚拟币余额不足，需要{material.Price}虚拟币，当前余额{userCoin.Coin}虚拟币",
                    RemainingCoins = userCoin.Coin
                };
            }

            // 扣除虚拟币
            var temp = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (temp.Category == "VIP")
            {
                userCoin.Coin -= (int)(material.Price * 0.7m);
            }
            else
            {
                userCoin.Coin -= (int)material.Price;
            }
            

            // 生成订单ID
            var orderId = Guid.NewGuid().ToString();

            // 添加到用户库存
            var userInventory = new UserInventory
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                MaterialId = materialId,
                PurchaseDate = DateTime.Now,
                PurchasePrice = material.Price,
                OrderId = orderId,
                IsActive = 1
            };

            _context.UserInventories.Add(userInventory);
            await _context.SaveChangesAsync();

            return new PurchaseResultDto
            {
                Success = true,
                Message = $"购买成功！已扣除{material.Price}虚拟币",
                RemainingCoins = userCoin.Coin,
                OrderId = orderId
            };
        }

        public async Task<List<InventoryItemDto>> GetUserInventoryAsync(string userId)
        {
            var inventoryItems = await _context.UserInventories
                .Where(ui => ui.UserId == userId && ui.IsActive == 1)
                .Join(_context.Materials,
                    ui => ui.MaterialId,
                    m => m.Id,
                    (ui, m) => new InventoryItemDto
                    {
                        Id = ui.Id,
                        MaterialId = ui.MaterialId,
                        MaterialType = m.MaterialType,
                        ExamType = m.ExamType,
                        SkillType = m.SkillType,
                        Description = m.Description ?? "",
                        Url = m.Url,
                        PreviewUrl = m.PreviewUrl,
                        PurchaseDate = ui.PurchaseDate,
                        PurchasePrice = ui.PurchasePrice,
                        OrderId = ui.OrderId
                    })
                .ToListAsync();

            return inventoryItems;
        }

        public async Task<bool> IsMaterialInInventoryAsync(string userId, string materialId)
        {
            var inventoryItem = await _context.UserInventories
                .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.MaterialId == materialId && ui.IsActive == 1);
            return inventoryItem != null;
        }

        public async Task<List<Material>> GetAvailableMaterialsAsync()
        {
            return await _context.Materials
                .Where(m => m.IsActive == 1)
                .ToListAsync();
        }
        // 返回用户类型
        public async Task<string?> GetUserCategoryAsync(string userId)
        {
            string sql = "SELECT Category FROM Users WHERE Id = :userId";

            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                try
                {
                    await conn.OpenAsync();

                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter(":userId", userId));

                        using (OracleDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return reader.GetString(0); // 返回 Category
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"查询用户类型出错：{ex.Message}");
                }
            }

            return null; // 查询不到或异常时返回 null
        }
        // 判断用户是否从VIP恢复为普通用户
        public async Task<string?> ISUserReinstate(string userId)
        {
            var today = DateTime.Today;
            var temp = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            // 检查是否为空
            if (temp == null)
            {
                return null;
            }
            // 管理员和普通用户直接返回
            if (temp.Category == "admin")
            {
                return "admin";
            }
            if (temp.Category == "user")
            {
                return "user";
            }

            var userCoin = await _context.UserCoins.FirstOrDefaultAsync(u => u.UserId == userId);
            // 判断是否为空
            if (userCoin?.LastSignDate == null)
            {
                temp.Category = "user";
                await _context.SaveChangesAsync();
                return "user";
            }

            // 时间判断逻辑
            var daysSinceLastSign = (today - userCoin.LastSignDate.Value).Days;

            if (daysSinceLastSign >= 2)
            {
                temp.Category = "user";
                userCoin.FirstSignDate = null;
                await _context.SaveChangesAsync(); 
                return "user";
            }

            return "VIP";
        }
    }
}
using AllEnBackend.Models;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Text;

namespace AllEnBackend.Services
{
    public class HomeService : IHomeService
    {
        private readonly string _connectionString;
        private readonly ILogger<HomeService> _logger;

        public HomeService(IConfiguration configuration, ILogger<HomeService> logger)
        {
            _connectionString = configuration.GetConnectionString("OracleDb")
                ?? throw new ArgumentNullException("OracleDb 连接字符串未配置");
            _logger = logger;
        }


        private static string DumpCommand(string sql, OracleParameterCollection? ps)
        {
            var sb = new StringBuilder();
            sb.AppendLine(sql);
            if (ps != null && ps.Count > 0)
            {
                sb.AppendLine("-- params --");
                foreach (OracleParameter p in ps)
                {
                    sb.AppendLine($"{p.ParameterName} = {p.Value ?? "NULL"} " +
                                  $"(DbType={p.OracleDbType}, Dir={p.Direction})");
                }
            }
            return sb.ToString();
        }

        public async Task<HomeCardsResponse> GetHomeCardsAsync(string userId)
        {
            var result = new HomeCardsResponse();

            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();


            // 热门课程
            string hotSql = @"
                SELECT * FROM (
                    SELECT 
                        m.ID,
                        m.DESCRIPTION,
                        m.SKILLTYPE,
                        m.EXAMTYPE,
                        NVL(m.PRICE,0) AS PRICE,
                        m.PREVIEWURL,
                        ROW_NUMBER() OVER (PARTITION BY m.SKILLTYPE ORDER BY m.PRICE DESC NULLS LAST) rn
                    FROM MATERIAL m
                    WHERE m.IS_ACTIVE = 1
                      AND m.SKILLTYPE IN ('听力','阅读','口语','写作')
                ) t
                WHERE t.rn = 1";

            try
            {
                using var hotCmd = new OracleCommand(hotSql, connection) { BindByName = true };
                _logger.LogInformation("Executing HOT SQL:\n{dump}", DumpCommand(hotSql, hotCmd.Parameters));

                using var reader = await hotCmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Hot.Add(new SimpleMaterialDto
                    {
                        Id = reader["ID"]?.ToString() ?? "",
                        Title = reader["DESCRIPTION"]?.ToString() ?? "",
                        SkillType = reader["SKILLTYPE"]?.ToString() ?? "",
                        ExamType = reader["EXAMTYPE"]?.ToString() ?? "",
                        Price = reader["PRICE"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["PRICE"]),
                        PreviewUrl = reader["PREVIEWURL"]?.ToString() ?? ""
                    });
                }

                _logger.LogInformation("HOT OK, rows={cnt}", result.Hot.Count);
            }
            catch (OracleException ex)
            {
                _logger.LogError(ex, "HOT SQL FAILED:\n{dump}", DumpCommand(hotSql, null));
                throw;
            }

            //  用户累计单词数

            long learnedWords = 0;
            const string wordCountSql =
                "SELECT NVL(SUM(LEARNED_WORD_COUNT),0) FROM USER_STUDY_PLAN WHERE USER_ID = :p_uid";

            try
            {
                using var wcCmd = new OracleCommand(wordCountSql, connection) { BindByName = true };
                wcCmd.Parameters.Add(":p_uid", OracleDbType.Varchar2, userId, ParameterDirection.Input);

                _logger.LogInformation("Executing WORDCOUNT SQL:\n{dump}", DumpCommand(wordCountSql, wcCmd.Parameters));

                var obj = await wcCmd.ExecuteScalarAsync();
                learnedWords = (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt64(obj);

                _logger.LogInformation("WORDCOUNT OK, sum={sum}", learnedWords);
            }
            catch (OracleException ex)
            {
                using var wcCmd = new OracleCommand(wordCountSql, connection);
                _logger.LogError(ex, "WORDCOUNT SQL FAILED:\n{dump}", DumpCommand(wordCountSql, wcCmd.Parameters));
                throw;
            }


            // 为你推荐

            string targetExam = learnedWords <= 10 ? "CET-4" : "托福"; 

            string forYouSql = @"
                SELECT ID, DESCRIPTION, SKILLTYPE, EXAMTYPE, NVL(PRICE,0) AS PRICE, PREVIEWURL
                FROM MATERIAL
                WHERE IS_ACTIVE = 1
                  AND EXAMTYPE = :p_exam
                ORDER BY PRICE DESC NULLS LAST
                FETCH FIRST 3 ROWS ONLY";

            try
            {
                using var fyCmd = new OracleCommand(forYouSql, connection) { BindByName = true };
                fyCmd.Parameters.Add(":p_exam", OracleDbType.Varchar2, targetExam, ParameterDirection.Input);

                _logger.LogInformation("Executing FORYOU SQL:\n{dump}", DumpCommand(forYouSql, fyCmd.Parameters));

                using var reader2 = await fyCmd.ExecuteReaderAsync();
                while (await reader2.ReadAsync())
                {
                    result.ForYou.Add(new SimpleMaterialDto
                    {
                        Id = reader2["ID"]?.ToString() ?? "",
                        Title = reader2["DESCRIPTION"]?.ToString() ?? "",
                        SkillType = reader2["SKILLTYPE"]?.ToString() ?? "",
                        ExamType = reader2["EXAMTYPE"]?.ToString() ?? "",
                        Price = reader2["PRICE"] == DBNull.Value ? 0 : Convert.ToDecimal(reader2["PRICE"]),
                        PreviewUrl = reader2["PREVIEWURL"]?.ToString() ?? ""
                    });
                }

                _logger.LogInformation("FORYOU OK, rows={cnt}, targetExam={exam}", result.ForYou.Count, targetExam);
            }
            catch (OracleException ex)
            {
                using var fyCmd = new OracleCommand(forYouSql, connection);
                _logger.LogError(ex, "FORYOU SQL FAILED:\n{dump}", DumpCommand(forYouSql, fyCmd.Parameters));
                throw;
            }

            return result;
        }
    }
}


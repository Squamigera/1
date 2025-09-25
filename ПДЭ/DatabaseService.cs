using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ПДЭ
{
    public class User
    {
        public int ID_сущности { get; set; }
        public string Логин { get; set; }
        public int? ID_партнера { get; set; }

    }

    public static class DatabaseService
    {
        private static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        // Получить всех партнёров
        public static List<Partner> GetPartners()
        {
            var partners = new List<Partner>();
            string query = @"
                SELECT partner_id, name, partner_type, manager_name, email, phone, 
                       legal_address, inn, rating 
                FROM partners 
                ORDER BY name";

            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        partners.Add(new Partner
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            PartnerType = reader.GetString(2),
                            ManagerName = reader.GetString(3),
                            Email = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            Phone = reader.IsDBNull(5) ? "" : reader.GetString(5),
                            LegalAddress = reader.GetString(6),
                            Inn = reader.GetString(7),
                            Rating = reader.GetInt32(8)
                        });
                    }
                }
            }
            return partners;
        }

        // Получить историю услуг по партнёру
        public static List<ServiceHistoryItem> GetServiceHistory(int partnerId)
{
            var history = new List<ServiceHistoryItem>();
            string query = @"
                SELECT s.service_name, s.service_code, h.quantity, h.execution_date
                FROM partner_service_facts h
                JOIN services s ON h.service_code = s.service_code
                WHERE h.partner_id = @partnerId
                ORDER BY h.execution_date DESC";

            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@partnerId", partnerId);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        history.Add(new ServiceHistoryItem
                        {
                            ServiceName = reader.GetString(0),
                            ServiceCode = reader.GetString(1), // ← ДОЛЖНО БЫТЬ!
                            Quantity = reader.GetInt32(2),
                            ExecutionDate = reader.GetDateTime(3)
                        });
                    }
                }
            }
            return history;
        }

        // Сохранить или обновить партнёра
        public static void SavePartner(Partner partner)
        {
            string query;
            if (partner.Id == 0)
            {
                // INSERT
                query = @"
                    INSERT INTO partners (name, partner_type, manager_name, email, phone, 
                                          legal_address, inn, rating)
                    VALUES (@name, @partnerType, @managerName, @email, @phone, 
                            @legalAddress, @inn, @rating)";
            }
            else
            {
                // UPDATE
                query = @"
                    UPDATE partners 
                    SET name = @name, partner_type = @partnerType, manager_name = @managerName,
                        email = @email, phone = @phone, legal_address = @legalAddress,
                        inn = @inn, rating = @rating
                    WHERE partner_id = @id";
            }

            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@name", partner.Name);
                cmd.Parameters.AddWithValue("@partnerType", partner.PartnerType);
                cmd.Parameters.AddWithValue("@managerName", partner.ManagerName);
                cmd.Parameters.AddWithValue("@email", string.IsNullOrEmpty(partner.Email) ? (object)DBNull.Value : partner.Email);
                cmd.Parameters.AddWithValue("@phone", string.IsNullOrEmpty(partner.Phone) ? (object)DBNull.Value : partner.Phone);
                cmd.Parameters.AddWithValue("@legalAddress", partner.LegalAddress);
                cmd.Parameters.AddWithValue("@inn", partner.Inn);
                cmd.Parameters.AddWithValue("@rating", partner.Rating);
                if (partner.Id != 0)
                    cmd.Parameters.AddWithValue("@id", partner.Id);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Проверка уникальности ИНН (кроме текущего партнёра)
        public static bool IsInnUnique(string inn, int? excludeId = null)
        {
            string query = "SELECT COUNT(*) FROM partners WHERE inn = @inn";
            if (excludeId.HasValue)
                query += " AND partner_id != @excludeId";

            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@inn", inn);
                if (excludeId.HasValue)
                    cmd.Parameters.AddWithValue("@excludeId", excludeId.Value);
                conn.Open();
                int count = (int)cmd.ExecuteScalar();
                return count == 0;
            }
        }

        // Расчёт себестоимости одной услуги
        public static decimal CalculateServiceCost(string serviceCode)
        {
            // Получаем норму времени (условно = 1 час на любую услугу)
            // Часовая ставка сотрудника (условно 500 руб/час)
            const decimal hourlyRate = 500m;

            // Трудозатраты = 1 час * ставка
            decimal laborCost = hourlyRate;

            // Стоимость материалов
            decimal materialCost = 0;

            string query = @"
                SELECT sm.quantity_per_unit, m.overuse_percent
                FROM service_materials sm
                JOIN materials m ON sm.material_id = m.material_id
                WHERE sm.service_code = @serviceCode";

            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@serviceCode", serviceCode);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        decimal quantity = reader.GetDecimal(0);
                        decimal overuse = reader.GetDecimal(1);
                        // Условно: цена материала = 100 руб за единицу
                        const decimal materialPrice = 100m;
                        decimal actualQuantity = quantity * (1 + overuse);
                        materialCost += actualQuantity * materialPrice;
                    }
                }
            }

            return laborCost + materialCost;
        }
    }

    public class Partner
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PartnerType { get; set; }
        public string ManagerName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string LegalAddress { get; set; }
        public string Inn { get; set; }
        public int Rating { get; set; }
    }

    public class ServiceHistoryItem
    {
        public string ServiceName { get; set; }
        public string ServiceCode { get; set; } // ← ДОЛЖНО БЫТЬ!
        public int Quantity { get; set; }
        public DateTime ExecutionDate { get; set; }
    }

}

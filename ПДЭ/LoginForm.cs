using System;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Configuration;

namespace ПДЭ
{
    public partial class LoginForm : Form
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните все поля", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            User user = AuthenticateUser(login, password);

            if (user == null)
            {
                MessageBox.Show("Неверный логин или пароль", "Ошибка входа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Закрываем форму входа
            this.Hide();

            // Если пользователь — менеджер (partner_id = NULL) → открываем список партнёров
            if (!user.ID_партнера.HasValue)
            {
                var partnersForm = new PartnersListForm();
                partnersForm.FormClosed += (s, args) => this.Close(); // Закрыть всё приложение при закрытии главной формы
                partnersForm.Show();
            }
            else
            {
                // Опционально: открыть личный кабинет партнёра
                // Но по ТЗ требуется только работа с партнёрами → менеджеру нужен список
                // Поэтому для учебного проекта можно всегда открывать PartnersListForm
                var partnersForm = new PartnersListForm();
                partnersForm.FormClosed += (s, args) => this.Close();
                partnersForm.Show();
            }
        }

        private User AuthenticateUser(string login, string password)
        {
            string query = @"
                SELECT user_id, login, partner_id 
                FROM users 
                WHERE login = @login AND password = @password";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@login", login);
                        command.Parameters.AddWithValue("@password", password);
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new User
                                {
                                    ID_сущности = reader.GetInt32(0),
                                    Логин = reader.GetString(1),
                                    ID_партнера = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к БД: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        // Обработка нажатия Enter в полях
        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                btnLogin_Click(sender, EventArgs.Empty);
            }
        }
    }
}
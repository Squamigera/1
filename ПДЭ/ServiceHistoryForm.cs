using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ПДЭ
{
    public partial class ServiceHistoryForm : Form
    {
        private int _partnerId;
        private List<ServiceHistoryItem> _history;

        public ServiceHistoryForm(int partnerId, string partnerName)
        {
            InitializeComponent();
            _partnerId = partnerId;
            Text = $"История услуг — {partnerName}";
            LoadHistory();
        }

        private void LoadHistory(int partnerId)
        {
            try
            {
                var history = DatabaseService.GetServiceHistory(partnerId);
                dataGridViewHistory.AutoGenerateColumns = false;
                dataGridViewHistory.DataSource = history;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки истории: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ServiceHistoryForm_Load(object sender, EventArgs e)
        {

        }

        private void LoadHistory()
        {
            try
            {
                _history = DatabaseService.GetServiceHistory(_partnerId);
                dataGridViewHistory.AutoGenerateColumns = true;
                dataGridViewHistory.DataSource = _history;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки истории: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (_history == null || !_history.Any())
                {
                    MessageBox.Show("Нет данных для расчёта.", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                decimal totalCost = 0;

                foreach (var item in _history)
                {
                    decimal costPerUnit = DatabaseService.CalculateServiceCost(item.ServiceCode);
                    totalCost += costPerUnit * item.Quantity;
                }

                MessageBox.Show(
                    $"Общая себестоимость оказанных услуг для партнёра:\n{totalCost:N2} руб.",
                    "Себестоимость",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка расчёта: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

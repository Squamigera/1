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
    public partial class PartnersListForm : Form
    {
        private List<Partner> _partners;

        public PartnersListForm()
        {
            InitializeComponent();
            LoadPartners();
        }

        private void LoadPartners()
        {
            try
            {
                _partners = DatabaseService.GetPartners();
                dataGridViewPartners.AutoGenerateColumns = true;
                dataGridViewPartners.DataSource = _partners;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки партнёров: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var editForm = new PartnerEditForm();
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadPartners(); // Обновить список
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridViewPartners.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите партнёра для редактирования.", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var partner = (Partner)dataGridViewPartners.SelectedRows[0].DataBoundItem;
            var editForm = new PartnerEditForm(partner);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadPartners();
            }
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            if (dataGridViewPartners.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите партнёра для просмотра истории.", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var partner = (Partner)dataGridViewPartners.SelectedRows[0].DataBoundItem;
            var historyForm = new ServiceHistoryForm(partner.Id, partner.Name);
            historyForm.ShowDialog();
        }

        private void dataGridViewPartners_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                btnEdit_Click(sender, e);
        }
    }
}

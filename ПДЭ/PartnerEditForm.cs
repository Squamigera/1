using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace ПДЭ
{
    public partial class PartnerEditForm : Form
    {
        private Partner _partner;
        private bool _isEditMode;

        public PartnerEditForm()
        {
            InitializeComponent();
            _isEditMode = false;
            Text = "Добавление партнёра";
        }

        public PartnerEditForm(Partner partner) : this()
        {
            _partner = partner;
            _isEditMode = true;
            Text = "Редактирование партнёра";
            LoadData();
        }

        private void LoadData()
        {
            txtName.Text = _partner.Name;
            cmbType.SelectedItem = _partner.PartnerType;
            txtManager.Text = _partner.ManagerName;
            txtEmail.Text = _partner.Email;
            txtPhone.Text = _partner.Phone;
            txtAddress.Text = _partner.LegalAddress;
            txtInn.Text = _partner.Inn;
            numRating.Value = _partner.Rating;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            var partner = new Partner
            {
                Id = _isEditMode ? _partner.Id : 0,
                Name = txtName.Text.Trim(),
                PartnerType = cmbType.SelectedItem.ToString(),
                ManagerName = txtManager.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                LegalAddress = txtAddress.Text.Trim(),
                Inn = txtInn.Text.Trim(),
                Rating = (int)numRating.Value
            };

            try
            {
                if (!DatabaseService.IsInnUnique(partner.Inn, _isEditMode ? (int?)partner.Id : null))
                {
                    MessageBox.Show("Партнёр с таким ИНН уже существует.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DatabaseService.SavePartner(partner);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Укажите наименование партнёра.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (cmbType.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип партнёра.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (txtInn.Text.Length != 10 && txtInn.Text.Length != 12)
            {
                MessageBox.Show("ИНН должен содержать 10 или 12 цифр.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}

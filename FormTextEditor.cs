using System;
using System.IO;
using System.Windows.Forms;

namespace FileManagerGUI
{
    public partial class FormTextEditor : Form
    {
        private string filePath; // Calea catre fisierul care se editeaza
        private bool isContentModified = false; // Flag pentru a verifica daca s-au facut modificari

        public FormTextEditor(string path)
        {
            InitializeComponent(); // Initializarea componentelor UI (generat de Designer.cs)
            filePath = path;
            this.Text = $"Editare: {Path.GetFileName(filePath)}"; // Titlul ferestrei
            LoadFileContent(); // Incarca continutul fisierului in RichTextBox

            // Atasam un handler la evenimentul TextChanged pentru a detecta modificarile
            richTextBoxContent.TextChanged += RichTextBoxContent_TextChanged;
        }

        // Incarca continutul fisierului in RichTextBox
        private void LoadFileContent()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    richTextBoxContent.Text = File.ReadAllText(filePath);
                    isContentModified = false; // Resetam flag-ul dupa incarcare initiala
                }
                else
                {
                    MessageBox.Show("Fisierul nu exista.", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close(); // Inchide formularul daca fisierul nu exista
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la incarcarea fisierului: {ex.Message}", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        // Eveniment: Click pe butonul "Save"
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                File.WriteAllText(filePath, richTextBoxContent.Text); // Salveaza continutul
                MessageBox.Show("Fisier salvat cu succes!", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                isContentModified = false; // Resetam flag-ul dupa salvare
                this.DialogResult = DialogResult.OK; // Seteaza rezultatul dialogului la OK
                this.Close(); // Inchide formularul
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la salvarea fisierului: {ex.Message}", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Eveniment: Click pe butonul "Cancel"
        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Verifica daca sunt modificari nesalvate inainte de a inchide
            if (isContentModified)
            {
                DialogResult result = MessageBox.Show("Ai modificari nesalvate. Vrei sa iesi fara a salva?", "Avertisment", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    this.DialogResult = DialogResult.Cancel; // Seteaza rezultatul dialogului la Cancel
                    this.Close(); // Inchide formularul
                }
            }
            else
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        // Eveniment: Textul din RichTextBox s-a schimbat
        private void RichTextBoxContent_TextChanged(object sender, EventArgs e)
        {
            isContentModified = true; // Seteaza flag-ul ca s-au facut modificari
        }

        // Eveniment: Formularul se inchide (ex: din butonul X sau ALT+F4)
        private void FormTextEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Verifica din nou modificarile nesalvate, daca nu s-a apasat deja Save
            if (isContentModified && this.DialogResult != DialogResult.OK)
            {
                DialogResult result = MessageBox.Show("Ai modificari nesalvate. Vrei sa iesi fara a salva?", "Avertisment", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                {
                    e.Cancel = true; // Anuleaza inchiderea formularului
                }
            }
        }
    }
}
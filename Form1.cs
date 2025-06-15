using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics; // Util pentru deschiderea fisierelor cu aplicatia implicita

namespace FileManagerGUI
{
    public partial class Form1 : Form
    {
        // Variabila care retine calea directorului curent de lucru
        private string currentPath = Directory.GetCurrentDirectory();

        public Form1()
        {
            InitializeComponent(); // Initializarea componentelor UI (generat de Designer.cs)
            SetupListView(); // Seteaza coloanele ListView-ului o singura data

            PopulateTreeView(); // Incarca initial TreeView-ul cu structura directoarelor
            UpdateListView(currentPath); // Incarca initial ListView-ul cu continutul directorului curent
            UpdateCurrentPathTextBox(); // Actualizeaza caseta text cu calea curenta
        }

        // --- Metode pentru Actualizarea UI ---

        // Seteaza coloanele pentru ListView (Nume, Tip, Dimensiune, Data Modificarii)
        private void SetupListView()
        {
            listViewContents.Columns.Add("Nume", 200);
            listViewContents.Columns.Add("Tip", 100);
            listViewContents.Columns.Add("Dimensiune", 100, HorizontalAlignment.Right); // Aliniat la dreapta
            listViewContents.Columns.Add("Data Modificarii", 150);
            listViewContents.View = View.Details; // Afisare in modul "Details" (cu coloane)
            listViewContents.FullRowSelect = true; // Selecteaza intregul rand la click
        }

        // Actualizeaza TextBox-ul care afiseaza calea curenta
        private void UpdateCurrentPathTextBox()
        {
            txtCurrentPath.Text = currentPath;
        }

        // Populeaza TreeView-ul cu directoarele sistemului de fisiere
        private void PopulateTreeView()
        {
            try
            {
                treeViewFolders.Nodes.Clear(); // Goleste nodurile existente

                // Obtine toate unitatile logice (C:, D:, etc.) si adauga-le ca noduri radacina
                foreach (string drive in Directory.GetLogicalDrives())
                {
                    TreeNode driveNode = new TreeNode(drive);
                    driveNode.Tag = drive; // Stocam calea completa in proprietatea Tag
                    treeViewFolders.Nodes.Add(driveNode);
                    // Adaugam un nod "dummy" pentru a permite extinderea (lazy loading)
                    driveNode.Nodes.Add(new TreeNode("Loading..."));
                }

                // Incearca sa selectezi nodul corespunzator directorului curent la pornire
                SelectNodeByPath(treeViewFolders.Nodes, currentPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la incarcarea structurii directoarelor: {ex.Message}", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Metoda recursiva pentru a popula subdirectoarele unui nod (utilizata pentru lazy loading in TreeView)
        private void PopulateSubDirectories(TreeNode parentNode)
        {
            try
            {
                string path = parentNode.Tag as string;
                if (string.IsNullOrEmpty(path)) return; // Asigura-te ca avem o cale valida

                // Adauga nodurile pentru directoare
                foreach (string directory in Directory.GetDirectories(path))
                {
                    // Ignora directoarele care pot da UnauthorizedAccessException la listare
                    // (de exemplu, System Volume Information, Recyle Bin)
                    if (Directory.Exists(directory)) // Verifica daca directorul inca exista
                    {
                        TreeNode subNode = new TreeNode(Path.GetFileName(directory));
                        subNode.Tag = directory; // Stocheaza calea completa
                        parentNode.Nodes.Add(subNode);
                        // Adauga un nod dummy pentru a permite extinderea ulterioara
                        subNode.Nodes.Add(new TreeNode("Loading..."));
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Ignoram aceste erori, nu le afisam utilizatorului direct in TreeView
                // pentru fiecare director inaccesibil, dar e bine de stiut ca se pot intampla.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la popularea subdirectoarelor '{parentNode.Tag}': {ex.Message}");
            }
        }

        // Actualizeaza ListView-ul cu fisierele si directoarele din calea specificata
        private void UpdateListView(string path)
        {
            try
            {
                listViewContents.Items.Clear(); // Goleste elementele existente

                // Adauga directoare
                foreach (string dir in Directory.GetDirectories(path))
                {
                    DirectoryInfo di = new DirectoryInfo(dir);
                    // Daca directorul este ascuns sau de sistem, il putem ignora, sau afisa diferit
                    if ((di.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden ||
                        (di.Attributes & FileAttributes.System) == FileAttributes.System)
                    {
                        // Continua la urmatorul director daca vrei sa le ascunzi
                        // continue;
                    }

                    ListViewItem item = new ListViewItem(di.Name);
                    item.SubItems.Add("Folder");
                    item.SubItems.Add(""); // Dimensiune goala pentru directoare
                    item.SubItems.Add(di.LastWriteTime.ToString());
                    item.Tag = di.FullName; // Stocam calea completa in Tag
                    listViewContents.Items.Add(item);
                }

                // Adauga fisiere
                foreach (string file in Directory.GetFiles(path))
                {
                    FileInfo fi = new FileInfo(file);
                    // Daca fisierul este ascuns sau de sistem
                    if ((fi.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden ||
                        (fi.Attributes & FileAttributes.System) == FileAttributes.System)
                    {
                        // continue;
                    }
                    ListViewItem item = new ListViewItem(fi.Name);
                    item.SubItems.Add("File");
                    item.SubItems.Add((fi.Length / 1024.0).ToString("F2") + " KB"); // Dimensiune in KB
                    item.SubItems.Add(fi.LastWriteTime.ToString());
                    item.Tag = fi.FullName; // Stocam calea completa in Tag
                    listViewContents.Items.Add(item);
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Nu ai permisiunile necesare pentru a accesa acest director.", "Acces Refuzat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show("Directorul specificat nu exista.", "Director Negăsit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"A aparut o eroare la actualizarea listei: {ex.Message}", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper pentru a selecta un nod in TreeView dupa cale
        private void SelectNodeByPath(TreeNodeCollection nodes, string path)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag != null && node.Tag.ToString().Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    treeViewFolders.SelectedNode = node;
                    node.EnsureVisible(); // Asigura ca nodul este vizibil in TreeView
                    return;
                }
                // Cauta recursiv in subnoduri
                SelectNodeByPath(node.Nodes, path);
            }
        }

        // --- Evenimente UI ---

        // Eveniment: Un nod a fost selectat in TreeView (click pe un director)
        private void treeViewFolders_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null && e.Node.Tag is string selectedPath)
            {
                currentPath = selectedPath; // Actualizeaza calea curenta
                UpdateCurrentPathTextBox();
                UpdateListView(currentPath); // Reimprospateaza ListView-ul
            }
        }

        // Eveniment: Un nod TreeView este pe cale sa fie extins (pentru lazy loading)
        private void treeViewFolders_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            // Daca nodul contine doar nodul "Loading...", il golim si populam subdirectoarele reale
            if (e.Node.Nodes.Count == 1 && e.Node.Nodes[0].Text == "Loading...")
            {
                e.Node.Nodes.Clear();
                PopulateSubDirectories(e.Node);
            }
        }

        // Eveniment: Dublu-click pe un item din ListView (fisier sau director)
        private void listViewContents_DoubleClick(object sender, EventArgs e)
        {
            if (listViewContents.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewContents.SelectedItems[0];
                string fullPath = selectedItem.Tag as string;

                if (Directory.Exists(fullPath))
                {
                    // Daca este un director, schimba directorul curent si actualizeaza UI
                    currentPath = fullPath;
                    UpdateCurrentPathTextBox();
                    UpdateListView(currentPath);
                    SelectNodeByPath(treeViewFolders.Nodes, currentPath); // Actualizeaza selectia in TreeView
                }
                else if (File.Exists(fullPath))
                {
                    // Daca este un fisier
                    string extension = Path.GetExtension(fullPath).ToLower();
                    // Deschide cu editorul nostru daca e un fisier text, altfel cu aplicatia implicita
                    if (extension == ".txt" || extension == ".log" || extension == ".ini" || extension == ".csv" || extension == ".json" || extension == ".xml")
                    {
                        EditTextFile(fullPath); // Apelam metoda noastra de editare
                    }
                    else
                    {
                        try
                        {
                            // Deschide fisierul cu aplicatia implicita a sistemului
                            Process.Start(fullPath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Nu s-a putut deschide fisierul: {ex.Message}", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        // --- Implementarea Actiunilor (Butoane) ---

        // Buton "Back" (întoarce-te la directorul părinte)
        private void btnBack_Click(object sender, EventArgs e)
        {
            try
            {
                DirectoryInfo parentDir = Directory.GetParent(currentPath);
                if (parentDir != null)
                {
                    currentPath = parentDir.FullName; // Seteaza noua cale curenta
                    UpdateCurrentPathTextBox();
                    UpdateListView(currentPath); // Reimprospateaza ListView
                    SelectNodeByPath(treeViewFolders.Nodes, currentPath); // Actualizeaza selectia in TreeView
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la intoarcere: {ex.Message}", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Buton "New Folder" (creeaza un director nou)
        private void btnNewFolder_Click(object sender, EventArgs e)
        {
            // Afiseaza un dialog pentru a cere numele noului director
            string folderName = InputBox.Show("Creeaza Director Nou", "Introdu numele noului director:");

            if (!string.IsNullOrWhiteSpace(folderName))
            {
                try
                {
                    FileOperations.CreateFolder(currentPath, folderName);
                    MessageBox.Show($"Directorul '{folderName}' a fost creat.", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateListView(currentPath); // Reimprospateaza ListView-ul
                    PopulateTreeView(); // Reimprospateaza TreeView-ul pentru a reflecta noul director
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Eroare la crearea directorului: {ex.Message}", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Buton "New File" (creeaza un fisier nou)
        private void btnNewFile_Click(object sender, EventArgs e)
        {
            string fileName = InputBox.Show("Creeaza Fisier Nou", "Introdu numele noului fisier:");

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                try
                {
                    FileOperations.CreateFile(currentPath, fileName);
                    MessageBox.Show($"Fisierul '{fileName}' a fost creat.", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateListView(currentPath); // Reimprospateaza ListView-ul
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Eroare la crearea fisierului: {ex.Message}", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Buton "Delete" (sterge fisier/director)
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (listViewContents.SelectedItems.Count == 0)
            {
                MessageBox.Show("Selecteaza un fisier sau director de sters.", "Atentie", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ListViewItem selectedItem = listViewContents.SelectedItems[0];
            string fullPathToDelete = selectedItem.Tag as string;
            string itemName = selectedItem.Text; // Numele afisat in UI

            DialogResult result = MessageBox.Show($"Esti sigur ca vrei sa stergi '{itemName}'?\nAceasta actiune este ireversibila!", "Confirma Stergere", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    FileOperations.DeleteFileOrDirectory(fullPathToDelete);
                    MessageBox.Show($"'{itemName}' a fost sters cu succes.", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateListView(currentPath); // Reimprospateaza lista
                    PopulateTreeView(); // Reimprospateaza TreeView-ul (in caz ca s-a sters un director)
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Eroare la stergere: {ex.Message}", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Buton "Rename" (redenumeste fisier/director)
        private void btnRename_Click(object sender, EventArgs e)
        {
            if (listViewContents.SelectedItems.Count == 0)
            {
                MessageBox.Show("Selecteaza un fisier sau director de redenumit.", "Atentie", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ListViewItem selectedItem = listViewContents.SelectedItems[0];
            string oldFullPath = selectedItem.Tag as string;
            string oldName = selectedItem.Text;

            string newName = InputBox.Show("Redenumeste", $"Introdu noul nume pentru '{oldName}':", oldName);

            if (!string.IsNullOrWhiteSpace(newName) && newName != oldName)
            {
                try
                {
                    string newFullPath = Path.Combine(Path.GetDirectoryName(oldFullPath), newName);
                    FileOperations.Rename(oldFullPath, newFullPath);
                    MessageBox.Show($"'{oldName}' a fost redenumit in '{newName}'.", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateListView(currentPath); // Reimprospateaza lista
                    PopulateTreeView(); // Reimprospateaza TreeView-ul
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Eroare la redenumire: {ex.Message}", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Buton "Copy" (copiaza fisier/director)
        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (listViewContents.SelectedItems.Count == 0)
            {
                MessageBox.Show("Selecteaza un fisier sau director de copiat.", "Atentie", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ListViewItem selectedItem = listViewContents.SelectedItems[0];
            string sourcePath = selectedItem.Tag as string;
            string sourceName = selectedItem.Text;

            string destinationPath = InputBox.Show("Copiaza", $"Introdu calea destinatie pentru '{sourceName}':", currentPath + "\\" + sourceName);

            if (!string.IsNullOrWhiteSpace(destinationPath))
            {
                try
                {
                    FileOperations.Copy(sourcePath, destinationPath);
                    MessageBox.Show($"'{sourceName}' a fost copiat la '{destinationPath}'.", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateListView(currentPath); // Reimprospateaza lista (daca destinatia e in dir. curent)
                    PopulateTreeView(); // Reimprospateaza TreeView-ul (daca s-a copiat un director nou)
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Eroare la copiere: {ex.Message}", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Buton "Move" (muta fisier/director)
        private void btnMove_Click(object sender, EventArgs e)
        {
            if (listViewContents.SelectedItems.Count == 0)
            {
                MessageBox.Show("Selecteaza un fisier sau director de mutat.", "Atentie", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ListViewItem selectedItem = listViewContents.SelectedItems[0];
            string sourcePath = selectedItem.Tag as string;
            string sourceName = selectedItem.Text;

            string destinationPath = InputBox.Show("Muta", $"Introdu calea destinatie pentru '{sourceName}':", currentPath + "\\" + sourceName);

            if (!string.IsNullOrWhiteSpace(destinationPath))
            {
                try
                {
                    FileOperations.Move(sourcePath, destinationPath);
                    MessageBox.Show($"'{sourceName}' a fost mutat la '{destinationPath}'.", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateListView(currentPath); // Reimprospateaza lista
                    PopulateTreeView(); // Reimprospateaza TreeView-ul
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Eroare la mutare: {ex.Message}", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Buton "Edit" (editeaza fisier text)
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (listViewContents.SelectedItems.Count == 0)
            {
                MessageBox.Show("Selecteaza un fisier de editat.", "Atentie", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ListViewItem selectedItem = listViewContents.SelectedItems[0];
            string fullPath = selectedItem.Tag as string;

            if (!File.Exists(fullPath))
            {
                MessageBox.Show("Fisierul selectat nu exista.", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Verifica daca este un fisier text (dupa extensie)
            string extension = Path.GetExtension(fullPath).ToLower();
            if (extension == ".txt" || extension == ".log" || extension == ".ini" || extension == ".csv" || extension == ".json" || extension == ".xml")
            {
                EditTextFile(fullPath); // Apelam metoda de editare
            }
            else
            {
                MessageBox.Show("Acest tip de fisier nu poate fi editat cu editorul intern.", "Informare", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Metoda care deschide Formularul de editare text
        private void EditTextFile(string filePath)
        {
            using (FormTextEditor editorForm = new FormTextEditor(filePath))
            {
                // ShowDialog() deschide formularul modal si blocheaza Form1 pana se inchide editorForm
                if (editorForm.ShowDialog() == DialogResult.OK)
                {
                    // Daca s-a salvat, putem reimprospata ListView-ul (desi nu e strict necesar pentru continut)
                    UpdateListView(currentPath);
                }
            }
        }

        // Buton "Refresh"
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            UpdateListView(currentPath);
            PopulateTreeView();
        }
    }

    // O clasa helper pentru InputBox-uri simple (nu face parte din .NET Framework standard)
    // Trebuie sa o creezi manual sau sa o integrezi ca un nou Form in proiect.
    // Pentru simplitate, am inclus-o aici, dar ideal ar fi un nou Form separat.
    public static class InputBox
    {
        public static string Show(string title, string promptText, string defaultValue = "")
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = defaultValue;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new System.Drawing.Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new System.Drawing.Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult result = form.ShowDialog();
            return result == DialogResult.OK ? textBox.Text : string.Empty;
        }
    }
}
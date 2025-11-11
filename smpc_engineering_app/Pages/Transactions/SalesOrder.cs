using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using smpc_engineering_app.Shared;
using smpc_engineering_app.Services.Transaction;
using smpc_engineering_app.Services.Helpers;
using smpc_engineering_app.Properties;
using System.IO;
using smpc_engineering_app.Models;

namespace smpc_engineering_app.Pages.Transactions
{
    public partial class SalesOrder : UserControl
    {
        readonly SalesOrderViewEngService salesOrderViewEngService = new SalesOrderViewEngService();
        private readonly string jobOrderPath = Settings.Default.JOBORDERPATH;
        private TreeNode selectedNode;
        private int _currentSOIndex = -1;
        private SalesOrderViewEngList _sodata;
        private List<SalesOrderViewEngModel> _salesOrders;
        private DataTable _soTable;

        private readonly string[] systemFolders =
        {
        "Quotation Versions",
        "Technical Evaluation Report",
        "Clarificatories",
        "Bid Bulletin",
        "Client Purchase Order"
        };

        public SalesOrder()
        {
            InitializeComponent();

            InitializeContextMenu();

            LoadDirectory(SALES_TV, jobOrderPath);

            // Create ImageList
            ImageList imageList = new ImageList();
            imageList.Images.Add("folder", Properties.Resources.FolderIcon);
            imageList.Images.Add("pdf", Properties.Resources.pdf);
            imageList.Images.Add("word", Properties.Resources.word);
            imageList.Images.Add("excel", Properties.Resources.excel);
            imageList.Images.Add("image", Properties.Resources.img);
            imageList.Images.Add("file", Properties.Resources.file);

            // Assign to TreeView
            SALES_TV.ImageList = imageList;
            SALES_LV.SmallImageList = imageList;

            // Enable drag and drop for ListView
            SALES_LV.AllowDrop = true;
            SALES_LV.DragEnter += SALES_LV_DragEnter;
            SALES_LV.DragDrop += SALES_LV_DragDrop;

            InitializeListViewContextMenu();
        }

        private void InitializeListViewContextMenu()
        {
            ContextMenuStrip lvContextMenu = new ContextMenuStrip();

            ToolStripMenuItem renameFileItem = new ToolStripMenuItem("Rename File");
            renameFileItem.Click += RenameFileItem_Click;

            ToolStripMenuItem deleteFileItem = new ToolStripMenuItem("Delete File");
            deleteFileItem.Click += DeleteFileItem_Click;

            lvContextMenu.Items.Add(renameFileItem);
            lvContextMenu.Items.Add(deleteFileItem);

            SALES_LV.ContextMenuStrip = lvContextMenu;
        }

        private async void SalesOrder_Load(object sender, EventArgs e)
        {
            try
            {
                Helpers.Loading.ShowLoading(dgv_order_sales, "Fetching data...");
                await LoadSalesOrders();
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to load: {ex.Message}");
            }
            finally
            {
                Helpers.Loading.HideLoading(dgv_order_sales);
            }
        }

        private async Task LoadSalesOrders()
        {
            pnl_Sales.Visible = true;

            // save current index before reload
            int oldIndex = _currentSOIndex;

            //fill this declared value by the receiving reports data
            _sodata = await salesOrderViewEngService.GetAsModel();

            // Reverse order so newest records appear first
            _sodata.sales_order_view.Reverse();

            if (_sodata != null && _sodata.sales_order_view != null && _sodata.sales_order_view.Count > 0)
            {
                //set this variable to the parent of the rr
                _salesOrders = _sodata.sales_order_view;

                // restore old index if valid, otherwise fallback to 0
                if (oldIndex >= 0 && oldIndex < _salesOrders.Count)
                    _currentSOIndex = oldIndex;
                else
                    _currentSOIndex = 0;

                ShowCurrentRecord();
            }
            else
            {
                _salesOrders = new List<SalesOrderViewEngModel>();
                _currentSOIndex = -1;
                dgv_order_sales.DataSource = null;
                btn_prev.Enabled = false;
                btn_next.Enabled = false;
            }
        }

        private void ShowCurrentRecord()
        {
            if (_currentSOIndex < 0 || _sodata == null || _sodata.sales_order_view == null || !_sodata.sales_order_view.Any())
                return;

            // Add prefix "SO#" to doc_no before binding
            foreach (var so in _sodata.sales_order_view)
            {
                if (!string.IsNullOrEmpty(so.doc_no) && !so.doc_no.StartsWith("SO#"))
                {
                    so.doc_no = "SO#" + so.doc_no;
                }
            }

            txt_created_by.Text = CacheData.CurrentUser.first_name + " " + CacheData.CurrentUser.last_name;

            // Convert receiving report list to DataTable using helper
            _soTable = Helpers.ToDataTable(_sodata.sales_order_view);

            //Clear and rebuild _irltable based on current record only
            var current = _salesOrders[_currentSOIndex];

            if (_soTable.Rows.Count == 0 || _currentSOIndex >= _soTable.Rows.Count)
                return;

            //Bind controls automatically (textboxes, checkboxes, etc.)
            Helpers.BindControls(new Panel[] { panel5 }, _soTable, _currentSOIndex);

            //Disable auto column generation before setting the data source
            dgv_order_sales.AutoGenerateColumns = false;

            //Bind child details (grids)
            if (_sodata?.sales_order_details_view != null)
            {
                var detailsForCurrent = new BindingList<SalesOrderDetailsViewEngModel>(_sodata.sales_order_details_view.Where(d => d.so_id == current.id).ToList());

                dgv_order_sales.DataSource = detailsForCurrent;
            }
            else
            {
                dgv_order_sales.DataSource = null;
            }

            //Enable/disable navigation buttons
            btn_prev.Enabled = _currentSOIndex > 0;
            btn_next.Enabled = _currentSOIndex < _salesOrders.Count - 1;

            // Load files filtered by SO#
            if (SALES_TV.SelectedNode != null)
            {
                string currentPath = SALES_TV.SelectedNode.Tag?.ToString();
                if (!string.IsNullOrEmpty(currentPath))
                {
                    LoadFiles(currentPath); // This will now filter by SO#
                }
            }

            LoadDirectory(SALES_TV, jobOrderPath);
        }

        public async void SetSalesOrder(string salesOrder)
        {
            try
            {
                Helpers.Loading.ShowLoading(dgv_order_sales, "Fetching data...");
                await LoadSalesOrders();
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to load: {ex.Message}");
            }
            finally
            {
                Helpers.Loading.HideLoading(dgv_order_sales);
            }

            if (string.IsNullOrEmpty(salesOrder) || _sodata == null || _sodata.sales_order_view == null)
                return;

            txt_created_by.Text = CacheData.CurrentUser.first_name + " " + CacheData.CurrentUser.last_name;

            // Ensure the prefix "SO#" is consistent
            string formattedSO = salesOrder.StartsWith("SO#") ? salesOrder : "SO#" + salesOrder;

            // Find the matching record index (case-insensitive match)
            int index = _sodata.sales_order_view.FindIndex(so =>
                string.Equals(so.doc_no, formattedSO, StringComparison.OrdinalIgnoreCase));

            if (index >= 0)
            {
                _currentSOIndex = index;
                ShowCurrentRecord();
            }
            else
            {
                Helpers.ShowDialogMessage("error", "Sales Order record not found.");
            }
        }

        private void ChangeRecord(int step)
        {
            if (_salesOrders == null || !_salesOrders.Any()) return;

            pnl_Sales.Visible = true;

            int newIndex = _currentSOIndex + step;
            if (newIndex >= 0 && newIndex < _salesOrders.Count)
            {
                _currentSOIndex = newIndex;
                ShowCurrentRecord();
            }
        }

        private void btn_next_Click(object sender, EventArgs e)
        {
            ChangeRecord(1);
        }

        private void btn_prev_Click(object sender, EventArgs e)
        {
            ChangeRecord(-1);
        }

        private void DeleteFileItem_Click(object sender, EventArgs e)
        {
            if (SALES_LV.SelectedItems.Count == 0 || SALES_LV.SelectedItems[0].Text == "No files found")
                return;

            string currentFile = Path.Combine(GetCurrentDirectory(), SALES_LV.SelectedItems[0].Text);

            if (!File.Exists(currentFile)) return;

            var result = MessageBox.Show($"Are you sure you want to delete the file '{Path.GetFileName(currentFile)}'?",
                                         "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    File.Delete(currentFile);

                    // Refresh the ListView
                    LoadFiles(GetCurrentDirectory());
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting file: {ex.Message}", "Error",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void RenameFileItem_Click(object sender, EventArgs e)
        {
            if (SALES_LV.SelectedItems.Count == 0 || SALES_LV.SelectedItems[0].Text == "No files found")
                return;

            string currentFile = Path.Combine(GetCurrentDirectory(), SALES_LV.SelectedItems[0].Text);

            if (!File.Exists(currentFile)) return;

            string currentFileName = Path.GetFileName(currentFile);
            string nameWithoutExt = Path.GetFileNameWithoutExtension(currentFileName);
            string extension = Path.GetExtension(currentFileName);

            // Extract SO# suffix
            string currentSO = txt_doc_no.Text.Replace("SO#", "").Trim();
            string soSuffix = $"_SO{currentSO}";

            if (!nameWithoutExt.EndsWith(soSuffix, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("This file is not associated with the current SO and cannot be renamed.",
                                "Rename Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Ask user for the new name (without SO# suffix)
            string nameWithoutSuffix = nameWithoutExt.Substring(0, nameWithoutExt.Length - soSuffix.Length);

            using (var dialog = new InputDialog("Rename File", "Enter new file name:", nameWithoutSuffix))
            {
                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.InputText))
                {
                    string newFileNameWithoutSuffix = dialog.InputText.Trim();
                    string newFileName = $"{newFileNameWithoutSuffix}{soSuffix}{extension}";
                    string newFilePath = Path.Combine(GetCurrentDirectory(), newFileName);

                    try
                    {
                        File.Move(currentFile, newFilePath);

                        // Refresh the ListView
                        LoadFiles(GetCurrentDirectory());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error renaming file: {ex.Message}", "Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void InitializeContextMenu()
        {
            // Create context menu items
            ToolStripMenuItem addFolderItem = new ToolStripMenuItem("Add Folder");
            ToolStripMenuItem renameItem = new ToolStripMenuItem("Rename");
            ToolStripMenuItem deleteItem = new ToolStripMenuItem("Delete");
            ToolStripSeparator separator = new ToolStripSeparator();

            // Add click events
            addFolderItem.Click += AddFolderItem_Click;
            renameItem.Click += RenameItem_Click;
            deleteItem.Click += DeleteItem_Click;

            // Add items to context menu
            treeViewContextMenu.Items.AddRange(new ToolStripItem[] {
                addFolderItem,
                separator,
                renameItem,
                deleteItem
            });

            // Assign context menu to TreeView
            SALES_TV.ContextMenuStrip = treeViewContextMenu;
        }

        private void SALES_TV_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Select the node under the mouse pointer
                selectedNode = SALES_TV.GetNodeAt(e.X, e.Y);
                if (selectedNode != null)
                {
                    SALES_TV.SelectedNode = selectedNode;

                    // Enable/disable menu items based on node type
                    bool isRoot = selectedNode.Parent == null;
                    bool isCategory = selectedNode.Text == "ACTIVE" || selectedNode.Text == "BENCHED";
                    bool isSystemFolder = IsSystemFolder(selectedNode);

                    treeViewContextMenu.Items[0].Enabled = !isRoot; // Add Folder
                    treeViewContextMenu.Items[2].Enabled = !isRoot && !isCategory && !isSystemFolder; // Rename
                    treeViewContextMenu.Items[3].Enabled = !isRoot && !isCategory && !isSystemFolder; // Delete
                }
            }
        }

        private bool IsSystemFolder(TreeNode node)
        {
            // Check if this is one of the predefined system folders
            string[] systemFolders = { "Quotation Versions", "Technical Evaluation Report", "Clarificatories", "Bid Bulletin", "Client Purchase Order" };
            return systemFolders.Contains(node.Text);
        }

        private void AddFolderItem_Click(object sender, EventArgs e)
        {
            if (selectedNode == null) return;

            string parentPath = selectedNode.Tag?.ToString();
            if (string.IsNullOrEmpty(parentPath)) return;

            using (var dialog = new InputDialog("Add New Folder", "Enter folder name:"))
            {
                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.InputText))
                {
                    string newFolderName = dialog.InputText.Trim();
                    string soNumber = txt_doc_no.Text.Replace("SO#", "").Trim();
                    newFolderName = $"{newFolderName}_SO{soNumber}";

                    string newFolderPath = Path.Combine(parentPath, newFolderName);

                    try
                    {
                        Directory.CreateDirectory(newFolderPath);

                        TreeNode newNode = new TreeNode(newFolderName)
                        {
                            Tag = newFolderPath,
                            ImageKey = "folder",
                            SelectedImageKey = "folder"
                        };
                        selectedNode.Nodes.Add(newNode);
                        selectedNode.Expand();

                        SALES_TV.SelectedNode = newNode;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error creating folder: {ex.Message}", "Error",
                                       MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void RenameItem_Click(object sender, EventArgs e)
        {
            if (selectedNode == null || selectedNode.Parent == null) return;

            string currentPath = selectedNode.Tag?.ToString();
            if (string.IsNullOrEmpty(currentPath)) return;

            string currentFolderName = Path.GetFileName(currentPath);

            // Extract SO# suffix
            string currentSO = txt_doc_no.Text.Replace("SO#", "").Trim();
            string soSuffix = $"_SO{currentSO}";

            // If folder doesn't have suffix, do not allow renaming
            if (!currentFolderName.EndsWith(soSuffix, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("This folder is not associated with the current SO and cannot be renamed.",
                                "Rename Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Ask user for new name (excluding suffix)
            string nameWithoutSuffix = currentFolderName.Substring(0, currentFolderName.Length - soSuffix.Length);

            using (var dialog = new InputDialog("Rename Folder", "Enter new folder name:", nameWithoutSuffix))
            {
                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.InputText))
                {
                    string newFolderNameWithoutSuffix = dialog.InputText.Trim();
                    string newFolderName = $"{newFolderNameWithoutSuffix}{soSuffix}";

                    string parentDirectory = Path.GetDirectoryName(currentPath);
                    string newFolderPath = Path.Combine(parentDirectory, newFolderName);

                    try
                    {
                        // Rename directory
                        Directory.Move(currentPath, newFolderPath);

                        // Update TreeView
                        selectedNode.Text = newFolderName;
                        selectedNode.Tag = newFolderPath;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error renaming folder: {ex.Message}", "Error",
                                       MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void DeleteItem_Click(object sender, EventArgs e)
        {
            if (selectedNode == null || selectedNode.Parent == null) return;

            string folderPath = selectedNode.Tag?.ToString();
            if (string.IsNullOrEmpty(folderPath)) return;

            var result = MessageBox.Show($"Are you sure you want to delete the folder '{selectedNode.Text}'?",
                                       "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // Delete directory (recursively)
                    Directory.Delete(folderPath, true);

                    // Remove from TreeView
                    selectedNode.Remove();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting folder: {ex.Message}", "Error",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // InputDialog class for getting user input
        private class InputDialog : Form
        {
            private TextBox textBox;
            private Button okButton;
            private Button cancelButton;

            public string InputText => textBox.Text;

            public InputDialog(string title, string prompt, string defaultValue = "")
            {
                InitializeComponents(title, prompt, defaultValue);
            }

            private void InitializeComponents(string title, string prompt, string defaultValue)
            {
                this.Text = title;
                this.Size = new Size(300, 150);
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.StartPosition = FormStartPosition.CenterParent;
                this.MaximizeBox = false;
                this.MinimizeBox = false;

                Label promptLabel = new Label
                {
                    Text = prompt,
                    Location = new Point(10, 10),
                    Size = new Size(260, 20)
                };

                textBox = new TextBox
                {
                    Text = defaultValue,
                    Location = new Point(10, 40),
                    Size = new Size(260, 20)
                };

                okButton = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Location = new Point(100, 70),
                    Size = new Size(75, 25)
                };

                cancelButton = new Button
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(180, 70),
                    Size = new Size(75, 25)
                };

                this.Controls.Add(promptLabel);
                this.Controls.Add(textBox);
                this.Controls.Add(okButton);
                this.Controls.Add(cancelButton);

                this.AcceptButton = okButton;
                this.CancelButton = cancelButton;
            }
        }

        // Drag and drop event handlers
        private void SALES_LV_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void SALES_LV_DragDrop(object sender, DragEventArgs e)
        {
            if (SALES_TV.SelectedNode == null)
            {
                MessageBox.Show("Please select a folder first to upload files.", "Info",
                               MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            string targetFolder = SALES_TV.SelectedNode.Tag?.ToString();

            if (!string.IsNullOrEmpty(targetFolder) && Directory.Exists(targetFolder))
            {
                UploadFiles(files, targetFolder);
            }
        }

        private void UploadFiles(string[] files, string targetFolder)
        {
            try
            {
                int successCount = 0;
                int errorCount = 0;
                string soNumber = txt_doc_no.Text.Replace("SO#", "").Trim();

                foreach (string file in files)
                {
                    try
                    {
                        if (File.Exists(file))
                        {
                            string originalFileName = Path.GetFileName(file);
                            string nameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
                            string extension = Path.GetExtension(originalFileName);

                            string newFileName = $"{nameWithoutExt}_SO{soNumber}{extension}";

                            string destinationPath = Path.Combine(targetFolder, newFileName);

                            // If file exists, ask to overwrite or rename
                            if (File.Exists(destinationPath))
                            {
                                var result = MessageBox.Show($"File '{newFileName}' already exists. Overwrite?",
                                                           "File Exists",
                                                           MessageBoxButtons.YesNoCancel,
                                                           MessageBoxIcon.Question);

                                if (result == DialogResult.No)
                                {
                                    // Add timestamp to filename
                                    newFileName = $"{nameWithoutExt}_{DateTime.Now:yyyyMMddHHmmss}_SO{soNumber}{extension}";
                                    destinationPath = Path.Combine(targetFolder, newFileName);
                                }
                                else if (result == DialogResult.Cancel)
                                {
                                    continue;
                                }
                            }

                            File.Copy(file, destinationPath, true);
                            successCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        Console.WriteLine($"Error uploading {file}: {ex.Message}");
                    }
                }

                // Refresh the file list
                LoadFiles(targetFolder);

                MessageBox.Show($"Files uploaded successfully: {successCount}\nFailed: {errorCount}",
                              "Upload Complete",
                              MessageBoxButtons.OK,
                              successCount > 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error uploading files: {ex.Message}", "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDirectory(TreeView treeView, string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Ensure ACTIVE and BENCHED exist with subfolders
            CreateSubDirectories(directoryPath);

            // Clear tree view and add root
            treeView.Nodes.Clear();
            treeView.ImageKey = "folder";
            treeView.SelectedImageKey = "folder";

            TreeNode rootNode = new TreeNode(directoryPath)
            {
                Tag = directoryPath,
                ImageKey = "folder",
                SelectedImageKey = "folder"
            };
            treeView.Nodes.Add(rootNode);

            // Add ACTIVE and BENCHED with subfolders
            LoadManualSubDirectories(directoryPath, rootNode);

            rootNode.ExpandAll();
        }

        private void CreateSubDirectories(string directoryPath)
        {
            string activeDir = Path.Combine(directoryPath, "ACTIVE");
            string benchedDir = Path.Combine(directoryPath, "BENCHED");

            if (!Directory.Exists(activeDir)) Directory.CreateDirectory(activeDir);
            if (!Directory.Exists(benchedDir)) Directory.CreateDirectory(benchedDir);

            foreach (string subDir in new[] { activeDir, benchedDir })
            {
                foreach (string folderName in systemFolders)
                {
                    string folderPath = Path.Combine(subDir, folderName);
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                }
            }
        }

        private void LoadManualSubDirectories(string path, TreeNode parentNode)
        {
            string currentSO = txt_doc_no.Text.Replace("SO#", "").Trim();
            string soSuffix = $"_SO{currentSO}";

            foreach (var category in new[] { "ACTIVE", "BENCHED" })
            {
                string categoryPath = Path.Combine(path, category);
                TreeNode categoryNode = new TreeNode(category)
                {
                    Tag = categoryPath,
                    ImageKey = "folder",
                    SelectedImageKey = "folder"
                };
                parentNode.Nodes.Add(categoryNode);

                foreach (var subFolder in Directory.GetDirectories(categoryPath))
                {
                    string folderName = Path.GetFileName(subFolder);

                    // Always show system folders
                    if (systemFolders.Contains(folderName))
                    {
                        TreeNode sysNode = new TreeNode(folderName)
                        {
                            Tag = subFolder,
                            ImageKey = "folder",
                            SelectedImageKey = "folder"
                        };
                        categoryNode.Nodes.Add(sysNode);

                        // Use recursive loader
                        LoadSubDirectoriesRecursive(sysNode, subFolder, soSuffix);

                        continue;
                    }

                    // For other folders → filter by SO suffix
                    if (!string.IsNullOrEmpty(currentSO) && !folderName.EndsWith(soSuffix, StringComparison.OrdinalIgnoreCase))
                        continue;

                    TreeNode subNode = new TreeNode(folderName)
                    {
                        Tag = subFolder,
                        ImageKey = "folder",
                        SelectedImageKey = "folder"
                    };
                    categoryNode.Nodes.Add(subNode);
                }
            }
        }

        private void LoadSubDirectoriesRecursive(TreeNode parentNode, string parentPath, string soSuffix)
        {
            foreach (var dir in Directory.GetDirectories(parentPath))
            {
                string folderName = Path.GetFileName(dir);

                // Apply SO filter if SO is selected
                if (!string.IsNullOrEmpty(soSuffix) &&
                    !folderName.EndsWith(soSuffix, StringComparison.OrdinalIgnoreCase) &&
                    !systemFolders.Contains(folderName)) // system folders always show
                {
                    continue;
                }

                TreeNode newNode = new TreeNode(folderName)
                {
                    Tag = dir,
                    ImageKey = "folder",
                    SelectedImageKey = "folder"
                };

                parentNode.Nodes.Add(newNode);

                //Recursive call to load subfolders inside this folder
                LoadSubDirectoriesRecursive(newNode, dir, soSuffix);
            }
        }

        private void SALES_TV_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Check if the node has children (means it's a parent node)
            if (e.Node.Nodes.Count > 0)
            {
                // Parent node → show panel
                pnl_Sales.Visible = true;
            }
            else
            {
                // Child node → hide panel
                pnl_Sales.Visible = false;
            }

            string path = GetFullPath(e.Node);
            LoadFiles(path);
        }

        private string GetFullPath(TreeNode node)
        {
            if (node.Parent == null) return node.Text;
            return Path.Combine(GetFullPath(node.Parent), node.Text);
        }

        private void LoadFiles(string path)
        {
            try
            {
                SALES_LV.Items.Clear();

                // Configure ListView for better appearance
                SALES_LV.View = View.Details;
                SALES_LV.FullRowSelect = true;
                SALES_LV.GridLines = false;
                SALES_LV.HeaderStyle = ColumnHeaderStyle.Nonclickable;

                // Ensure columns exist and are properly sized
                if (SALES_LV.Columns.Count == 0)
                {
                    SALES_LV.Columns.Add("File Name", 250);
                    SALES_LV.Columns.Add("Size", 80);
                    SALES_LV.Columns.Add("Modified", 120);
                    SALES_LV.Columns.Add("Type", 100);
                }

                if (Directory.Exists(path))
                {
                    // Get all files and sort by name
                    var files = Directory.GetFiles(path)
                                        .OrderBy(f => Path.GetFileName(f))
                                        .ToArray();

                    // Get current SO number for filtering
                    string currentSONumber = txt_doc_no.Text.Replace("SO#", "").Trim();
                    string soSuffix = $"_SO{currentSONumber}";

                    foreach (var file in files)
                    {
                        FileInfo fi = new FileInfo(file);
                        string fileName = fi.Name;
                        string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

                        // Filter: Only show files that end with the current SO# suffix
                        // OR show all files if no SO is selected (txt_doc is empty)
                        if (!string.IsNullOrEmpty(currentSONumber) &&
                            !nameWithoutExt.EndsWith(soSuffix, StringComparison.OrdinalIgnoreCase))
                        {
                            continue; // Skip files that don't match the SO# suffix
                        }

                        ListViewItem item = new ListViewItem(fileName);

                        // Format file size with appropriate units
                        string fileSize = FormatFileSize(fi.Length);

                        // Format date in a more readable format
                        string modifiedDate = fi.LastWriteTime.ToString("MMM dd, yyyy hh:mm tt");

                        // Get file type/extension
                        string fileType = fi.Extension.ToUpper().TrimStart('.');
                        if (string.IsNullOrEmpty(fileType)) fileType = "File";

                        item.SubItems.Add(fileSize);
                        item.SubItems.Add(modifiedDate);
                        item.SubItems.Add(fileType);

                        // Set appropriate icon based on file type
                        SetFileIcon(item, fi.Extension);

                        SALES_LV.Items.Add(item);
                    }

                    // Show message if no files found
                    if (SALES_LV.Items.Count == 0)
                    {
                        ListViewItem emptyItem = new ListViewItem("No files found");
                        emptyItem.SubItems.Add("");
                        emptyItem.SubItems.Add("");
                        emptyItem.SubItems.Add("");
                        SALES_LV.Items.Add(emptyItem);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading files: {ex.Message}", "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Add tooltip for drag and drop hint
        private void SALES_LV_MouseEnter(object sender, EventArgs e)
        {
            if (SALES_TV.SelectedNode != null)
            {
                toolTip1.SetToolTip(SALES_LV, "Drag and drop files here to upload to the selected folder");
            }
            else
            {
                toolTip1.SetToolTip(SALES_LV, "Select a folder first to upload files");
            }
        }

        //Add a button for traditional file upload
        private void btnUpload_Click(object sender, EventArgs e)
        {
            if (SALES_TV.SelectedNode == null)
            {
                MessageBox.Show("Please select a folder first to upload files.", "Info",
                               MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Multiselect = true;
                openFileDialog.Title = "Select files to upload";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string targetFolder = SALES_TV.SelectedNode.Tag?.ToString();
                    if (!string.IsNullOrEmpty(targetFolder) && Directory.Exists(targetFolder))
                    {
                        UploadFiles(openFileDialog.FileNames, targetFolder);
                    }
                }
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double len = bytes;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        private void SetFileIcon(ListViewItem item, string extension)
        {
            switch (extension.ToLower())
            {
                case ".pdf":
                    item.ImageKey = "pdf";
                    break;
                case ".doc":
                case ".docx":
                    item.ImageKey = "word";
                    break;
                case ".xls":
                case ".xlsx":
                    item.ImageKey = "excel";
                    break;
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                    item.ImageKey = "image";
                    break;
                default:
                    item.ImageKey = "file";
                    break;
            }
        }

        // Double-click to open files
        private void SALES_LV_DoubleClick(object sender, EventArgs e)
        {
            if (SALES_LV.SelectedItems.Count > 0 && SALES_LV.SelectedItems[0].Text != "No files found")
            {
                string selectedFile = Path.Combine(GetCurrentDirectory(), SALES_LV.SelectedItems[0].Text);
                try
                {
                    System.Diagnostics.Process.Start(selectedFile);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening file: {ex.Message}", "Error",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string GetCurrentDirectory()
        {
            if (SALES_TV.SelectedNode != null)
            {
                return SALES_TV.SelectedNode.Tag?.ToString() ?? string.Empty;
            }
            return string.Empty;
        }
    }
}

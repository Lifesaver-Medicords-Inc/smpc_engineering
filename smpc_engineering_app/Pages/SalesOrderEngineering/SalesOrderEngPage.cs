using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using smpc_engineering_app.Services;
using smpc_engineering_app.Models;
using smpc_engineering_app.Shared;
using smpc_engineering_app.Services.Helpers;
using smpc_engineering_app.Properties;
using System.IO;
using smpc_engineering_app.Pages.SalesOrderEngineering.SalesOrderEngineeringModals;

namespace smpc_engineering_app.Pages.SalesOrderEngineering
{
    public partial class SalesOrderEngPage : UserControl
    {
        public string SelectedSOId { get; private set; } = null;
        GeneralService<SalesOrderViewEngList> generalSalesOrder;
        private int _currentSOIndex = -1;
        private int _previousSOIndex = -1;
        private SalesOrderViewEngList _sodata;
        private List<SalesOrderViewEngModel> _salesOrders;
        private DataTable _soTable;
        private BindingList<SalesOrderDetailsViewEngModel> _currentDetails;
        private string _userName = CacheData.CurrentUser.first_name + " " + CacheData.CurrentUser.last_name;
        private readonly string jobOrderPath = Settings.Default.JOBORDERPATH;
        private TreeNode _selectedNode;

        private readonly string[] _systemFolders =
        {
        "Quotation Versions",
        "Technical Evaluation Report",
        "Clarificatories",
        "Bid Bulletin",
        "Client Purchase Order"
        };

        public SalesOrderEngPage()
        {
            InitializeComponent();

            _userName = CacheData.CurrentUser.first_name + " " + CacheData.CurrentUser.last_name;

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
            InitializeContextMenu();

            txt_sales_executive.Text = _userName;
        }

        private void ChangeRecord(int step)
        {
            if (_salesOrders == null || !_salesOrders.Any()) return;

            int newIndex = _currentSOIndex + step;
            if (newIndex >= 0 && newIndex < _salesOrders.Count)
            {
                _currentSOIndex = newIndex;
                ShowCurrentRecord();
            }
        }

        private void btn_prev_Click(object sender, EventArgs e)
        {
            ChangeRecord(-1);
        }

        private void btn_next_Click(object sender, EventArgs e)
        {
            ChangeRecord(1);
        }

        private async void btn_search_Click(object sender, EventArgs e)
        {
            if (_salesOrders == null || _salesOrders.Count == 0)
            {
                await LoadSalesOrders();
            }

            using (var searchForm = new SalesOrderEngSearch())
            {
                if (searchForm.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(searchForm.SelectedSOId))
                {
                    if (int.TryParse(searchForm.SelectedSOId, out int selectedId))
                    {
                        int index = _salesOrders.FindIndex(r => r.id == selectedId);
                        if (index >= 0)
                        {
                            _currentSOIndex = index;
                            await LoadSalesOrders();
                        }
                    }
                    else
                    {
                        Helpers.ShowDialogMessage("error", "Invalid record ID selected.");
                    }
                }
            }
        }

        private async void SalesOrderEngPage_Load(object sender, EventArgs e)
        {
            try
            {
                Helpers.Loading.ShowLoading(dgv_main, "Fetching data...");
                await LoadSalesOrders();
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to load: {ex.Message}");
            }
            finally
            {
                Helpers.Loading.HideLoading(dgv_main);
            }
        }

        public async void SetSalesOrder(string soId)
        {
            SelectedSOId = soId;

            // Ensure data is loaded
            if (_salesOrders == null || _salesOrders.Count == 0)
                await LoadSalesOrders();

            // Find the matching record by so_id
            if (int.TryParse(soId, out int id))
            {
                int index = _salesOrders.FindIndex(r => r.id == id);
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
        }

        private async Task LoadSalesOrders()
        {
            // save current index before reload
            int oldIndex = _currentSOIndex;

            generalSalesOrder = new GeneralService<SalesOrderViewEngList>(ApiEndPoints.SALES_ORDER_ENGINEER);

            //fill this declared value by the sales order data
            _sodata = await generalSalesOrder.GetAsModel();

            if (_sodata != null && _sodata.sales_order_view != null && _sodata.sales_order_view.Count > 0)
            {
                //set this variable to the parent of the sales order
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
                ClearSalesOrderUI();
            }
        }

        private void ShowCurrentRecord()
        {
            if (_currentSOIndex < 0 || _sodata == null || _sodata.sales_order_view == null || !_sodata.sales_order_view.Any())
                return;

            // Convert sales order list to DataTable using helper
            _soTable = Helpers.ToDataTable(_sodata.sales_order_view);

            Helpers.BindControls(new Panel[] { pnl_main }, _soTable, _currentSOIndex);

            // Format txt_doc_no with SO prefix and 8 digit number
            if (!string.IsNullOrEmpty(txt_doc_no.Text))
            {
                if (int.TryParse(txt_doc_no.Text, out int number))
                {
                    txt_doc_no.Text = "SO" + number.ToString("D8");
                }
            }

            //Disable auto column generation before setting the data source
            dgv_main.AutoGenerateColumns = false;

            var current = _salesOrders[_currentSOIndex];

            //Bind child details (grids)
            if (_sodata?.sales_order_view != null)
            {
                _currentDetails = new BindingList<SalesOrderDetailsViewEngModel>(
                    _sodata.sales_order_details_view
                        .Where(d => d.so_id == current.id)
                        .ToList()
                );

                dgv_main.DataSource = _currentDetails;
            }
            else
            {
                dgv_main.DataSource = null;
            }

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

            //Enable/disable navigation buttons
            btn_prev.Enabled = _currentSOIndex > 0;
            btn_next.Enabled = _currentSOIndex < _salesOrders.Count - 1;
        }

        private void ClearSalesOrderUI()
        {
            _salesOrders = new List<SalesOrderViewEngModel>();
            _currentSOIndex = -1;
            _previousSOIndex = -1;

            // Clear panel fields
            Helpers.ResetControls(new Panel[] { pnl_main });

            // Clear grid
            dgv_main.DataSource = null;
            dgv_main.Rows.Clear();

            // Disable navigation buttons
            btn_prev.Enabled = false;
            btn_next.Enabled = false;
        }

        private void dgv_main_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = sender as DataGridView;

            // Ensure the numbering column exists
            if (grid.Columns.Contains("numbering"))
            {
                grid.Rows[e.RowIndex].Cells["numbering"].Value = (e.RowIndex + 1).ToString();
            }
        }

        private void pnl_footer_Resize(object sender, EventArgs e)
        {
            int halfWidth = pnl_footer.Width / 2;

            panel2.Width = halfWidth;
            panel2.Dock = DockStyle.Left;

            panel3.Width = halfWidth;
            panel3.Dock = DockStyle.Right;
        }

        private void pnl_Sales_Resize(object sender, EventArgs e)
        {
            pictureBox1.Left = (panel3.Width - pictureBox1.Width) / 2;
            pictureBox1.Top = (panel3.Height - pictureBox1.Height) / 2;

            label27.Left = (panel3.Width - label27.Width) / 2;
            label27.Top = pictureBox1.Bottom + 5;
        }

        private void TV1_preview_Resize(object sender, EventArgs e)
        {
            pictureBox3.Left = (panel2.Width - pictureBox3.Width) / 2;
            pictureBox3.Top = (panel2.Height - pictureBox3.Height) / 2;

            label29.Left = (panel2.Width - label29.Width) / 2;
            label29.Top = pictureBox3.Bottom + 5;
        }

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

        private void SALES_TV_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Select the node under the mouse pointer
                _selectedNode = SALES_TV.GetNodeAt(e.X, e.Y);
                if (_selectedNode != null)
                {
                    SALES_TV.SelectedNode = _selectedNode;

                    // Enable/disable menu items based on node type
                    bool isRoot = _selectedNode.Parent == null;
                    bool isCategory = _selectedNode.Text == "ACTIVE" || _selectedNode.Text == "BENCHED";
                    bool isSystemFolder = IsSystemFolder(_selectedNode);

                    treeViewContextMenu.Items[0].Enabled = !isRoot; // Add Folder
                    treeViewContextMenu.Items[2].Enabled = !isRoot && !isCategory && !isSystemFolder; // Rename
                    treeViewContextMenu.Items[3].Enabled = !isRoot && !isCategory && !isSystemFolder; // Delete
                }
            }
        }

        //File Storage
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
                foreach (string folderName in _systemFolders)
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
            string currentSO = txt_id.Text;
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
                    if (_systemFolders.Contains(folderName))
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

                // Apply SO filter if SO is selected (check for suffix instead of prefix)
                if (!string.IsNullOrEmpty(soSuffix) &&
                    !folderName.EndsWith(soSuffix, StringComparison.OrdinalIgnoreCase) &&
                    !_systemFolders.Contains(folderName)) // system folders always show
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
                    string currentSONumber = txt_id.Text;
                    string soSuffix = $"_SO{currentSONumber}"; // Changed from SO{currentSONumber}_

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

        private void UploadFiles(string[] files, string targetFolder)
        {
            try
            {
                int successCount = 0;
                int errorCount = 0;
                string soNumber = txt_id.Text;

                foreach (string file in files)
                {
                    try
                    {
                        if (File.Exists(file))
                        {
                            string originalFileName = Path.GetFileName(file);
                            string nameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
                            string extension = Path.GetExtension(originalFileName);

                            // Change: Move SO# to suffix instead of prefix
                            string newFileName = $"{nameWithoutExt}_SO{soNumber}{extension}"; // Changed from SO{soNumber}_{originalFileName}

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

        private string GetFullPath(TreeNode node)
        {
            if (node.Parent == null) return node.Text;
            return Path.Combine(GetFullPath(node.Parent), node.Text);
        }

        private bool IsSystemFolder(TreeNode node)
        {
            // Check if this is one of the predefined system folders
            string[] systemFolders = _systemFolders;
            return systemFolders.Contains(node.Text);
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
            // You can expand this method to set different icons based on file type
            // For now, using a simple approach - you might want to use ImageList with icons

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

        private string GetCurrentDirectory()
        {
            if (SALES_TV.SelectedNode != null)
            {
                return SALES_TV.SelectedNode.Tag?.ToString() ?? string.Empty;
            }
            return string.Empty;
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

            // Extract SO# suffix (changed from prefix)
            string currentSO = txt_id.Text;
            string soSuffix = $"_SO{currentSO}"; // Changed from SO{currentSO}_

            if (!nameWithoutExt.EndsWith(soSuffix, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("This file is not associated with the current RR and cannot be renamed.",
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
                    string newFileName = $"{newFileNameWithoutSuffix}{soSuffix}{extension}"; // Changed from {soPrefix}{newFileNameWithoutPrefix}{extension}
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

        private void AddFolderItem_Click(object sender, EventArgs e)
        {
            if (_selectedNode == null) return;

            string parentPath = _selectedNode.Tag?.ToString();
            if (string.IsNullOrEmpty(parentPath)) return;

            using (var dialog = new InputDialog("Add New Folder", "Enter folder name:"))
            {
                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.InputText))
                {
                    string newFolderName = dialog.InputText.Trim();

                    // Change: Move SO# prefix to the end
                    string soNumber = txt_id.Text;
                    newFolderName = $"{newFolderName}_SO{soNumber}"; // Changed from SO#{soNumber}_{newFolderName}

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
                        _selectedNode.Nodes.Add(newNode);
                        _selectedNode.Expand();

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
            if (_selectedNode == null || _selectedNode.Parent == null) return;

            string currentPath = _selectedNode.Tag?.ToString();
            if (string.IsNullOrEmpty(currentPath)) return;

            string currentFolderName = Path.GetFileName(currentPath);

            // Extract SO# suffix (changed from prefix)
            string currentSO = txt_id.Text;
            string soSuffix = $"_SO{currentSO}"; // Changed from SO#{currentSO}_

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
                    string newFolderName = $"{newFolderNameWithoutSuffix}{soSuffix}"; // Changed from {soPrefix}{newFolderNameWithoutPrefix}

                    string parentDirectory = Path.GetDirectoryName(currentPath);
                    string newFolderPath = Path.Combine(parentDirectory, newFolderName);

                    try
                    {
                        // Rename directory
                        Directory.Move(currentPath, newFolderPath);

                        // Update TreeView
                        _selectedNode.Text = newFolderName;
                        _selectedNode.Tag = newFolderPath;
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
            if (_selectedNode == null || _selectedNode.Parent == null) return;

            string folderPath = _selectedNode.Tag?.ToString();
            if (string.IsNullOrEmpty(folderPath)) return;

            var result = MessageBox.Show($"Are you sure you want to delete the folder '{_selectedNode.Text}'?",
                                       "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // Delete directory (recursively)
                    Directory.Delete(folderPath, true);

                    // Remove from TreeView
                    _selectedNode.Remove();
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
    }
}

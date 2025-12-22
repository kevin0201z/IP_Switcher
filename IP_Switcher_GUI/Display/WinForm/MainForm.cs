using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using IP_Switcher.Models;
using IP_Switcher.Interfaces;
using IP_Switcher_GUI.Display.WinForm;

namespace IP_Switcher.Display.WinForm
{
    /// <summary>
    /// 主表单
    /// </summary>
    public partial class MainForm : Form
    {
        private List<NicInfo> nicList;
        private List<NetworkConfig> configList;
        private INetworkManager networkManager;
        private IConfigManager configManager;
        private ILogger logger;

        // 当前选择的网卡和配置
        private NicInfo selectedNic;
        private NetworkConfig selectedConfig;

        /// <summary>
        /// 构造函数
        /// </summary>
        public MainForm()
        {
            // 移除对InitializeComponent的调用，因为我们手动初始化UI
            this.Text = "IP切换程序 v1.0";
            this.Size = new System.Drawing.Size(800, 600);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // 初始化UI组件
            InitializeUI();
        }

        /// <summary>
        /// 初始化UI组件
        /// </summary>
        private void InitializeUI()
        {
            // 主表单背景色
            this.BackColor = System.Drawing.Color.FromArgb(240, 242, 245);
            
            // 布局容器
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                ColumnStyles = { new ColumnStyle(SizeType.Percent, 50F), new ColumnStyle(SizeType.Percent, 50F) },
                RowStyles = { 
                    new RowStyle(SizeType.Percent, 40F),
                    new RowStyle(SizeType.Percent, 40F),
                    new RowStyle(SizeType.Percent, 40F),
                    new RowStyle(SizeType.Percent, 12F)
                },
                Padding = new Padding(10),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                BackColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // 设置全局字体
            Font globalFont = new System.Drawing.Font("Microsoft YaHei", 9F);
            Font groupBoxFont = new System.Drawing.Font("Microsoft YaHei", 9.5F, System.Drawing.FontStyle.Bold);
            
            // 第1行：网卡选择
            GroupBox nicGroup = new GroupBox
            {
                Text = "网卡选择",
                Dock = DockStyle.Fill,
                Font = groupBoxFont,
                BackColor = System.Drawing.Color.White,
                ForeColor = System.Drawing.Color.FromArgb(51, 51, 51),
                FlatStyle = FlatStyle.Flat
            };
            
            ListBox nicListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Name = "nicListBox",
                Font = globalFont,
                BackColor = System.Drawing.Color.White,
                ForeColor = System.Drawing.Color.FromArgb(51, 51, 51),
                BorderStyle = BorderStyle.FixedSingle,
                SelectionMode = SelectionMode.One,
                ItemHeight = 20
            };
            nicListBox.DrawMode = DrawMode.OwnerDrawFixed;
            nicListBox.DrawItem += (sender, e) =>
            {
                if (e.Index >= 0)
                {
                    e.DrawBackground();
                    bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                    
                    if (isSelected)
                    {
                        e.Graphics.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(220, 235, 255)), e.Bounds);
                        e.Graphics.DrawRectangle(new Pen(System.Drawing.Color.FromArgb(150, 200, 255)), e.Bounds);
                    }
                    
                    e.Graphics.DrawString(nicListBox.Items[e.Index].ToString(), 
                        globalFont, 
                        new SolidBrush(isSelected ? System.Drawing.Color.FromArgb(0, 82, 204) : System.Drawing.Color.FromArgb(51, 51, 51)), 
                        new RectangleF(e.Bounds.X + 5, e.Bounds.Y + 2, e.Bounds.Width - 10, e.Bounds.Height - 4));
                    
                    e.DrawFocusRectangle();
                }
            };
            
            nicListBox.SelectedIndexChanged += NicListBox_SelectedIndexChanged;
            nicGroup.Controls.Add(nicListBox);
            mainLayout.Controls.Add(nicGroup, 0, 0);
            mainLayout.SetColumnSpan(nicGroup, 2);

            // 第2行：当前配置
            GroupBox currentGroup = new GroupBox
            {
                Text = "当前配置",
                Dock = DockStyle.Fill,
                Font = groupBoxFont,
                BackColor = System.Drawing.Color.White,
                ForeColor = System.Drawing.Color.FromArgb(51, 51, 51),
                FlatStyle = FlatStyle.Flat
            };
            
            TextBox currentConfigBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                Name = "currentConfigBox",
                ScrollBars = ScrollBars.Vertical,
                Font = globalFont,
                BackColor = System.Drawing.Color.FromArgb(250, 250, 250),
                ForeColor = System.Drawing.Color.FromArgb(51, 51, 51),
                BorderStyle = BorderStyle.FixedSingle
            };
            currentGroup.Controls.Add(currentConfigBox);
            mainLayout.Controls.Add(currentGroup, 0, 1);

            // 第2行：目标配置
            GroupBox targetGroup = new GroupBox
            {
                Text = "目标配置",
                Dock = DockStyle.Fill,
                Font = groupBoxFont,
                BackColor = System.Drawing.Color.White,
                ForeColor = System.Drawing.Color.FromArgb(51, 51, 51),
                FlatStyle = FlatStyle.Flat
            };
            
            TextBox targetConfigBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                Name = "targetConfigBox",
                ScrollBars = ScrollBars.Vertical,
                Font = globalFont,
                BackColor = System.Drawing.Color.FromArgb(250, 250, 250),
                ForeColor = System.Drawing.Color.FromArgb(51, 51, 51),
                BorderStyle = BorderStyle.FixedSingle
            };
            targetGroup.Controls.Add(targetConfigBox);
            mainLayout.Controls.Add(targetGroup, 1, 1);

            // 第3行：配置选择
            GroupBox configGroup = new GroupBox
            {
                Text = "配置选择",
                Dock = DockStyle.Fill,
                Font = groupBoxFont,
                BackColor = System.Drawing.Color.White,
                ForeColor = System.Drawing.Color.FromArgb(51, 51, 51),
                FlatStyle = FlatStyle.Flat
            };
            
            // 配置选择面板布局
            TableLayoutPanel configPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                RowStyles = {
                    new RowStyle(SizeType.Percent, 80F),
                    new RowStyle(SizeType.Percent, 40F)
                }
            };
            
            // 配置列表
            ListBox configListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Name = "configListBox",
                Font = globalFont,
                BackColor = System.Drawing.Color.White,
                ForeColor = System.Drawing.Color.FromArgb(51, 51, 51),
                BorderStyle = BorderStyle.FixedSingle,
                SelectionMode = SelectionMode.MultiExtended,
                ItemHeight = 20
            };
            
            configListBox.DrawMode = DrawMode.OwnerDrawFixed;
            configListBox.DrawItem += (sender, e) =>
            {
                if (e.Index >= 0)
                {
                    e.DrawBackground();
                    bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                    
                    if (isSelected)
                    {
                        e.Graphics.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(220, 235, 255)), e.Bounds);
                        e.Graphics.DrawRectangle(new Pen(System.Drawing.Color.FromArgb(150, 200, 255)), e.Bounds);
                    }
                    
                    e.Graphics.DrawString(configListBox.Items[e.Index].ToString(), 
                        globalFont, 
                        new SolidBrush(isSelected ? System.Drawing.Color.FromArgb(0, 82, 204) : System.Drawing.Color.FromArgb(51, 51, 51)), 
                        new RectangleF(e.Bounds.X + 5, e.Bounds.Y + 2, e.Bounds.Width - 10, e.Bounds.Height - 4));
                    
                    e.DrawFocusRectangle();
                }
            };
            
            configListBox.SelectedIndexChanged += ConfigListBox_SelectedIndexChanged;
            configPanel.Controls.Add(configListBox, 0, 0);
            
            // 配置操作按钮
            FlowLayoutPanel configButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 0, 5, 5),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true
            };
            
            Button addButton = new Button
            {
                Text = "新增配置",
                Width = 100,
                Height = 36,
                Name = "addButton",
                Font = globalFont,
                BackColor = System.Drawing.Color.White,
                ForeColor = System.Drawing.Color.FromArgb(51, 51, 51),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 5, 0),
                Padding = new Padding(10, 5, 10, 5),
                FlatAppearance = { BorderSize = 1, BorderColor = System.Drawing.Color.FromArgb(200, 200, 200) }
            };
            addButton.MouseEnter += (sender, e) => addButton.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            addButton.MouseLeave += (sender, e) => addButton.BackColor = System.Drawing.Color.White;
            addButton.Click += AddButton_Click;
            configButtons.Controls.Add(addButton);
            
            Button editButton = new Button
            {
                Text = "修改配置",
                Width = 100,
                Height = 36,
                Name = "editButton",
                Font = globalFont,
                BackColor = System.Drawing.Color.White,
                ForeColor = System.Drawing.Color.FromArgb(51, 51, 51),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(5, 0, 5, 0),
                Padding = new Padding(10, 5, 10, 5),
                FlatAppearance = { BorderSize = 1, BorderColor = System.Drawing.Color.FromArgb(200, 200, 200) }
            };
            editButton.MouseEnter += (sender, e) => editButton.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            editButton.MouseLeave += (sender, e) => editButton.BackColor = System.Drawing.Color.White;
            editButton.Click += EditButton_Click;
            configButtons.Controls.Add(editButton);
            
            Button deleteButton = new Button
            {
                Text = "删除配置",
                Width = 100,
                Height = 36,
                Name = "deleteButton",
                Font = globalFont,
                BackColor = System.Drawing.Color.White,
                ForeColor = System.Drawing.Color.FromArgb(51, 51, 51),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(5, 0, 5, 0),
                Padding = new Padding(10, 5, 10, 5),
                FlatAppearance = { BorderSize = 1, BorderColor = System.Drawing.Color.FromArgb(200, 200, 200) }
            };
            deleteButton.MouseEnter += (sender, e) => deleteButton.BackColor = System.Drawing.Color.FromArgb(255, 240, 240);
            deleteButton.MouseLeave += (sender, e) => deleteButton.BackColor = System.Drawing.Color.White;
            deleteButton.Click += DeleteButton_Click;
            configButtons.Controls.Add(deleteButton);
            
            configPanel.Controls.Add(configButtons, 0, 1);
            configGroup.Controls.Add(configPanel);
            mainLayout.Controls.Add(configGroup, 0, 2);

            // 第3行：状态信息
            GroupBox statusGroup = new GroupBox
            {
                Text = "状态信息",
                Dock = DockStyle.Fill,
                Font = groupBoxFont,
                BackColor = System.Drawing.Color.White,
                ForeColor = System.Drawing.Color.FromArgb(51, 51, 51),
                FlatStyle = FlatStyle.Flat
            };
            
            TextBox statusBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                Name = "statusBox",
                ScrollBars = ScrollBars.Vertical,
                Font = globalFont,
                BackColor = System.Drawing.Color.FromArgb(250, 250, 250),
                ForeColor = System.Drawing.Color.FromArgb(51, 51, 51),
                BorderStyle = BorderStyle.FixedSingle
            };
            statusGroup.Controls.Add(statusBox);
            mainLayout.Controls.Add(statusGroup, 1, 2);

            // 第4行：按钮区域
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(10, 5, 10, 5),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                BackColor = System.Drawing.Color.White
            };
            
            Button applyButton = new Button
            {
                Text = "应用配置",
                Width = 110,
                Name = "applyButton",
                Font = new System.Drawing.Font("Microsoft YaHei", 9.5F, System.Drawing.FontStyle.Regular),
                BackColor = System.Drawing.Color.FromArgb(51, 153, 255),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Padding = new Padding(15, 5, 15, 5),
                Margin = new Padding(5, 0, 0, 0),
                Height = 36,
                FlatAppearance = { BorderSize = 0 }
            };
            applyButton.MouseEnter += (sender, e) => applyButton.BackColor = System.Drawing.Color.FromArgb(0, 136, 255);
            applyButton.MouseLeave += (sender, e) => applyButton.BackColor = System.Drawing.Color.FromArgb(51, 153, 255);
            applyButton.Click += ApplyButton_Click;
            buttonPanel.Controls.Add(applyButton);
            
            Button refreshButton = new Button
            {
                Text = "刷新",
                Width = 110,
                Name = "refreshButton",
                Font = new System.Drawing.Font("Microsoft YaHei", 9.5F, System.Drawing.FontStyle.Regular),
                BackColor = System.Drawing.Color.White,
                ForeColor = System.Drawing.Color.FromArgb(51, 51, 51),
                FlatStyle = FlatStyle.Flat,
                Padding = new Padding(15, 5, 15, 5),
                Margin = new Padding(10, 0, 0, 0),
                Height = 36,
                FlatAppearance = { BorderSize = 1, BorderColor = System.Drawing.Color.FromArgb(200, 200, 200) }
            };
            refreshButton.MouseEnter += (sender, e) => refreshButton.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            refreshButton.MouseLeave += (sender, e) => refreshButton.BackColor = System.Drawing.Color.White;
            refreshButton.Click += RefreshButton_Click;
            buttonPanel.Controls.Add(refreshButton);
            
            mainLayout.Controls.Add(buttonPanel, 0, 3);
            mainLayout.SetColumnSpan(buttonPanel, 2);

            // 添加到表单
            this.Controls.Add(mainLayout);
        }

        /// <summary>
        /// 设置网络管理器
        /// </summary>
        /// <param name="networkManager">网络管理器</param>
        public void SetNetworkManager(INetworkManager networkManager)
        {
            this.networkManager = networkManager;
        }

        /// <summary>
        /// 设置配置管理器
        /// </summary>
        /// <param name="configManager">配置管理器</param>
        public void SetConfigManager(IConfigManager configManager)
        {
            this.configManager = configManager;
        }

        /// <summary>
        /// 设置日志管理器
        /// </summary>
        /// <param name="logger">日志管理器</param>
        public void SetLogger(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// 加载网卡列表
        /// </summary>
        public void LoadNicList()
        {
            if (networkManager == null || configManager == null) return;

            nicList = networkManager.GetAllNetworkAdapters() ?? new List<NicInfo>();
            
            // 查找控件并添加空引用检查
            Control[] nicListBoxControls = this.Controls.Find("nicListBox", true);
            if (nicListBoxControls.Length > 0)
            {
                if (nicListBoxControls[0] is ListBox nicListBox)
                {
                    nicListBox.Items.Clear();
                    foreach (var nic in nicList)
                    {
                        nicListBox.Items.Add(nic.Name);
                    }

                    // 选择默认网卡或第一个网卡
                    string defaultNicName = configManager.GetLastNic();
                    if (!string.IsNullOrEmpty(defaultNicName))
                    {
                        int index = nicListBox.FindStringExact(defaultNicName);
                        if (index != -1)
                        {
                            nicListBox.SelectedIndex = index;
                        }
                    }
                    else if (nicList.Count > 0)
                    {
                        nicListBox.SelectedIndex = 0;
                        // 触发选择变化事件
                        OnNicSelected(nicList[0]);
                    }
                }
            }
        }

        /// <summary>
        /// 处理网卡选中事件
        /// </summary>
        /// <param name="selectedNic">选中的网卡</param>
        private void OnNicSelected(NicInfo selectedNic)
        {
            if (selectedNic != null && networkManager != null)
            {
                try
                {
                    // 获取选中网卡的当前配置
                    NetworkConfig currentConfig = networkManager.GetCurrentIpConfig(selectedNic.Name);
                    // 更新当前配置显示
                    UpdateCurrentConfig(currentConfig);
                    // 保存选中的网卡为默认网卡
                    configManager?.SetLastNic(selectedNic.Name);
                }
                catch (Exception ex)
                {
                    // 记录错误但不显示用户消息
                    logger?.Error("获取网卡配置失败", ex);
                }
            }
        }

        /// <summary>
        /// 加载配置列表
        /// </summary>
        public void LoadConfigList()
        {
            if (configManager == null) return;

            configList = configManager.LoadConfig() ?? new List<NetworkConfig>();
            
            // 查找控件并添加空引用检查
            Control[] configListBoxControls = this.Controls.Find("configListBox", true);
            if (configListBoxControls.Length > 0)
            {
                if (configListBoxControls[0] is ListBox configListBox)
                {
                    configListBox.Items.Clear();
                    
                    // 添加DHCP作为独立选项，不计入configList
                    configListBox.Items.Add("DHCP自动获取");
                    
                    // 添加配置文件中的配置
                    foreach (var config in configList)
                    {
                        configListBox.Items.Add(config.Name);
                    }

                    if (configListBox.Items.Count > 0)
                    {
                        configListBox.SelectedIndex = 0;
                    }
                }
            }
        }

        /// <summary>
        /// 更新当前配置显示
        /// </summary>
        /// <param name="config">当前配置</param>
        public void UpdateCurrentConfig(NetworkConfig config)
        {
            Control[] currentConfigBoxControls = this.Controls.Find("currentConfigBox", true);
            if (currentConfigBoxControls.Length > 0)
            {
                if (currentConfigBoxControls[0] is TextBox currentConfigBox)
                {
                    if (config != null)
                    {
                        string configText = $"IP地址：{config.IPAddress}\r\n" +
                                          $"子网掩码：{config.SubnetMask}\r\n" +
                                          $"默认网关：{config.DefaultGateway}\r\n" +
                                          $"DNS服务器：{string.Join(", ", config.DnsServers)}\r\n";
                        currentConfigBox.Text = configText;
                    }
                    else
                    {
                        currentConfigBox.Text = "无法获取当前配置";
                    }
                }
            }
        }

        /// <summary>
        /// 更新目标配置显示
        /// </summary>
        /// <param name="config">目标配置</param>
        public void UpdateTargetConfig(NetworkConfig config)
        {
            Control[] targetConfigBoxControls = this.Controls.Find("targetConfigBox", true);
            if (targetConfigBoxControls.Length > 0)
            {
                if (targetConfigBoxControls[0] is TextBox targetConfigBox)
                {
                    if (config != null)
                    {
                        string configText = $"配置名称：{config.Name}\r\n" +
                                          $"IP地址：{config.IPAddress}\r\n" +
                                          $"子网掩码：{config.SubnetMask}\r\n" +
                                          $"默认网关：{config.DefaultGateway}\r\n" +
                                          $"DNS服务器：{string.Join(", ", config.DnsServers)}\r\n";
                        targetConfigBox.Text = configText;
                    }
                    else
                    {
                        targetConfigBox.Text = "未选择配置";
                    }
                }
            }
        }

        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="icon">消息图标</param>
        public void ShowMessage(string message, MessageBoxIcon icon)
        {
            // 先显示MessageBox，然后再更新状态框
            MessageBox.Show(this, message, "IP切换控制台", MessageBoxButtons.OK, icon);
            
            // 更新状态框（添加空引用检查）
            Control[] statusBoxControls = this.Controls.Find("statusBox", true);
            if (statusBoxControls.Length > 0)
            {
                if (statusBoxControls[0] is TextBox statusBox)
                {
                    statusBox.AppendText($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\r\n");
                }
            }
        }

        /// <summary>
        /// 网卡选择变化事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void NicListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender is ListBox nicListBox && nicListBox.SelectedIndex != -1 && nicList != null && nicList.Count > nicListBox.SelectedIndex)
            {
                selectedNic = nicList[nicListBox.SelectedIndex];
                // 调用OnNicSelected方法更新配置
                OnNicSelected(selectedNic);
            }
        }

        /// <summary>
        /// 配置选择变化事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void ConfigListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender is ListBox configListBox && configListBox.SelectedIndex != -1)
            {
                string selectedItem = configListBox.SelectedItem.ToString();
                
                if (selectedItem == "DHCP自动获取")
                {
                    // 创建DHCP配置对象
                    selectedConfig = new NetworkConfig
                    {
                        Name = "DHCP自动获取",
                        NicName = "",
                        IPAddress = "",
                        SubnetMask = "",
                        DefaultGateway = "",
                        DnsServers = new List<string>()
                    };
                }
                else if (configList != null && configList.Count > configListBox.SelectedIndex - 1)
                {
                    // 从配置列表中获取（减去DHCP选项）
                    selectedConfig = configList[configListBox.SelectedIndex - 1];
                }
                
                UpdateTargetConfig(selectedConfig);
            }
        }

        /// <summary>
        /// 应用配置按钮点击事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void ApplyButton_Click(object sender, EventArgs e)
        {
            if (selectedNic == null)
            {
                ShowMessage("请先选择网卡", MessageBoxIcon.Warning);
                return;
            }

            if (selectedConfig == null)
            {
                ShowMessage("请先选择配置", MessageBoxIcon.Warning);
                return;
            }

            if (networkManager == null)
            {
                ShowMessage("网络管理器未初始化", MessageBoxIcon.Error);
                return;
            }

            try
            {
                // 确保配置的网卡名称与选择的网卡一致
                selectedConfig.NicName = selectedNic.Name;
                
                // 确保DnsServers不为空
                if (selectedConfig.DnsServers == null)
                {
                    selectedConfig.DnsServers = new List<string>();
                }

                ShowMessage("正在应用配置...", MessageBoxIcon.Information);
                bool success = networkManager.SetIpConfig(selectedNic.Name, selectedConfig);

                if (success)
                {
                    ShowMessage($"成功将网卡 {selectedNic.Name} 的配置切换为 {selectedConfig.Name}！", MessageBoxIcon.Information);
                    // 更新当前配置显示
                    NetworkConfig currentConfig = networkManager.GetCurrentIpConfig(selectedNic.Name);
                    UpdateCurrentConfig(currentConfig);
                }
                else
                {
                    ShowMessage($"将网卡 {selectedNic.Name} 的配置切换为 {selectedConfig.Name} 失败！", MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"应用配置时出错：{ex.Message}", MessageBoxIcon.Error);
                logger?.Error("应用配置时出错", ex);
            }
        }

        /// <summary>
        /// 新增配置按钮点击事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void AddButton_Click(object sender, EventArgs e)
        {
            // 创建新的配置对象
            NetworkConfig newConfig = new NetworkConfig
            {
                Name = "新配置",
                NicName = selectedNic?.Name ?? "",
                IPAddress = "",
                SubnetMask = "",
                DefaultGateway = "",
                DnsServers = new List<string>()
            };
            
            // 创建并显示编辑表单，传入现有配置列表进行名称唯一性检查
            EditConfigForm editForm = new EditConfigForm(newConfig, configList);
            DialogResult result = editForm.ShowDialog(this);
            
            if (result == DialogResult.OK && !editForm.IsCanceled)
            {
                try
                {
                    // 将新配置添加到配置列表
                    configList.Add(newConfig);
                    // 保存配置
                    configManager.SaveConfig(configList);
                    ShowMessage("配置新增成功", MessageBoxIcon.Information);
                    // 重新加载配置列表
                    LoadConfigList();
                }
                catch (Exception ex)
                {
                    ShowMessage($"保存配置失败：{ex.Message}", MessageBoxIcon.Error);
                    logger?.Error("保存配置失败", ex);
                }
            }
        }
        
        /// <summary>
        /// 修改配置按钮点击事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void EditButton_Click(object sender, EventArgs e)
        {
            // 查找配置列表控件
            Control[] configListBoxControls = this.Controls.Find("configListBox", true);
            if (configListBoxControls.Length > 0 && configListBoxControls[0] is ListBox configListBox)
            {
                if (configListBox.SelectedIndices.Count == 0)
                {
                    ShowMessage("请先选择要修改的配置", MessageBoxIcon.Warning);
                    return;
                }
                
                if (configListBox.SelectedIndices.Count > 1)
                {
                    ShowMessage("只能同时修改一个配置", MessageBoxIcon.Warning);
                    return;
                }
                
                string selectedItem = configListBox.SelectedItem.ToString();
                if (selectedItem == "DHCP自动获取")
                {
                    ShowMessage("DHCP配置不能修改", MessageBoxIcon.Warning);
                    return;
                }
                
                // 获取要修改的配置对象
                NetworkConfig configToEdit = null;
                if (configListBox.SelectedIndex > 0 && configList != null && configList.Count > configListBox.SelectedIndex - 1)
                {
                    configToEdit = configList[configListBox.SelectedIndex - 1];
                    
                    if (configToEdit != null)
                    {
                        // 创建并显示编辑表单，传入现有配置列表进行名称唯一性检查
                        EditConfigForm editForm = new EditConfigForm(configToEdit, configList);
                        DialogResult result = editForm.ShowDialog(this);
                        
                        if (result == DialogResult.OK && !editForm.IsCanceled)
                        {
                            try
                            {
                                // 保存修改后的配置
                                configManager.SaveConfig(configList);
                                ShowMessage("配置修改成功", MessageBoxIcon.Information);
                                // 重新加载配置列表
                                LoadConfigList();
                            }
                            catch (Exception ex)
                            {
                                ShowMessage($"保存配置失败：{ex.Message}", MessageBoxIcon.Error);
                                logger?.Error("保存配置失败", ex);
                            }
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 删除配置按钮点击事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            // 查找配置列表控件
            Control[] configListBoxControls = this.Controls.Find("configListBox", true);
            if (configListBoxControls.Length > 0 && configListBoxControls[0] is ListBox configListBox)
            {
                if (configListBox.SelectedIndices.Count == 0)
                {
                    ShowMessage("请先选择要删除的配置", MessageBoxIcon.Warning);
                    return;
                }
                
                // 检查是否包含DHCP配置
                bool containsDhcp = false;
                foreach (int index in configListBox.SelectedIndices)
                {
                    string item = configListBox.Items[index].ToString();
                    if (item == "DHCP自动获取")
                    {
                        containsDhcp = true;
                        break;
                    }
                }
                
                if (containsDhcp)
                {
                    ShowMessage("DHCP配置不能删除", MessageBoxIcon.Warning);
                    return;
                }
                
                // 确认删除
                string confirmMessage = configListBox.SelectedIndices.Count > 1 ? 
                    $"确定要删除选中的{configListBox.SelectedIndices.Count}个配置吗？" : 
                    $"确定要删除配置 '{configListBox.SelectedItem}' 吗？";
                
                DialogResult confirmResult = MessageBox.Show(this, confirmMessage, "确认删除", 
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                
                if (confirmResult == DialogResult.OK)
                {
                    try
                    {
                        // 创建要删除的配置列表
                        List<NetworkConfig> configsToDelete = new List<NetworkConfig>();
                        
                        // 从后向前删除，避免索引变化导致错误
                        int[] selectedIndices = new int[configListBox.SelectedIndices.Count];
                        configListBox.SelectedIndices.CopyTo(selectedIndices, 0);
                        Array.Sort(selectedIndices, (a, b) => b.CompareTo(a)); // 降序排序
                        
                        foreach (int index in selectedIndices)
                        {
                            if (index > 0 && configList != null && configList.Count > index - 1)
                            {
                                configsToDelete.Add(configList[index - 1]);
                            }
                        }
                        
                        // 从配置列表中删除
                        foreach (var config in configsToDelete)
                        {
                            configList.Remove(config);
                        }
                        
                        // 保存修改后的配置
                        configManager.SaveConfig(configList);
                        ShowMessage(configsToDelete.Count > 1 ? 
                            $"成功删除{configsToDelete.Count}个配置" : 
                            "配置删除成功", 
                            MessageBoxIcon.Information);
                        
                        // 重新加载配置列表
                        LoadConfigList();
                    }
                    catch (Exception ex)
                    {
                        ShowMessage($"删除配置失败：{ex.Message}", MessageBoxIcon.Error);
                        logger?.Error("删除配置失败", ex);
                    }
                }
            }
        }
        
        /// <summary>
        /// 刷新按钮点击事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void RefreshButton_Click(object sender, EventArgs e)
        {
            // 确保网络管理器和配置管理器已初始化
            if (networkManager != null && configManager != null)
            {
                LoadNicList();
                LoadConfigList();
                ShowMessage("刷新完成", MessageBoxIcon.Information);
            }
            else
            {
                ShowMessage("网络或配置管理器未初始化", MessageBoxIcon.Error);
            }
        }
    }
}
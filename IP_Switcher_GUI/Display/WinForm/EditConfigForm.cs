using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using IP_Switcher.Models;

namespace IP_Switcher_GUI.Display.WinForm
{
    /// <summary>
    /// 配置编辑表单
    /// </summary>
    public partial class EditConfigForm : Form
    {
        // 样式常量定义
        // 字体配置
        private const string FontFamilyName = "Microsoft YaHei"; // 全局字体家族
        private const float FontSizeRegular = 9.5F; // 常规字体大小
        private const float FontSizeSmall = 8.5F; // 小号字体大小（用于提示信息）
        
        // 尺寸配置
        private const int FormWidth = 450; // 表单宽度
        private const int FormHeight = 450; // 表单高度
        private const int ButtonWidth = 80; // 按钮宽度
        private const int ButtonHeight = 36; // 按钮高度
        private const int TextBoxHeight = 30; // 文本框高度
        
        // 颜色配置
        private static readonly Color _foreColor = Color.FromArgb(51, 51, 51); // 主要文本颜色
        private static readonly Color _backColor = Color.White; // 背景颜色
        private static readonly Color BorderColor = Color.FromArgb(220, 220, 220); // 边框颜色
        private static readonly Color ButtonPrimaryColor = Color.FromArgb(51, 153, 255); // 主要按钮背景色
        private static readonly Color ButtonPrimaryHoverColor = Color.FromArgb(0, 136, 255); // 主要按钮悬停色
        private static readonly Color ButtonPrimaryPressedColor = Color.FromArgb(0, 119, 230); // 主要按钮按下色
        private static readonly Color ButtonSecondaryColor = Color.White; // 次要按钮背景色
        private static readonly Color ButtonSecondaryBorderColor = Color.FromArgb(200, 200, 200); // 次要按钮边框色
        private static readonly Color TipBackgroundColor = Color.FromArgb(245, 245, 245); // 提示信息背景色
        private static readonly Color TipForeColor = Color.FromArgb(128, 128, 128); // 提示信息文本色
        
        // 边距配置
        private static readonly Padding TextBoxPadding = new Padding(8, 6, 8, 6); // 常规文本框内边距
        private static readonly Padding DnsTextBoxPadding = new Padding(8, 8, 8, 8); // DNS多行文本框内边距
        private static readonly Padding LabelPadding = new Padding(0, 0, 10, 0); // 标签右内边距
        
        // 控件引用缓存
        private TextBox _nameTextBox;
        private TextBox _ipTextBox;
        private TextBox _maskTextBox;
        private TextBox _gatewayTextBox;
        private TextBox _dnsTextBox;
        
        /// <summary>
        /// 编辑的配置对象
        /// </summary>
        public NetworkConfig Config { get; private set; }
        
        /// <summary>
        /// 是否取消编辑
        /// </summary>
        public bool IsCanceled { get; private set; } = true;
        
        /// <summary>
        /// 所有现有配置列表，用于检查配置名称唯一性
        /// </summary>
        private readonly List<NetworkConfig> _existingConfigs;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="config">要编辑的配置对象</param>
        public EditConfigForm(NetworkConfig config)
        {
            InitializeComponent();
            Config = config;
            // 根据配置是否有IP地址判断是新增还是编辑
            Text = string.IsNullOrEmpty(config.IPAddress) && string.IsNullOrEmpty(config.SubnetMask) ? "新增网络配置" : "编辑网络配置";
            LoadConfigData();
        }
        
        /// <summary>
        /// 构造函数，支持传入现有配置列表用于名称唯一性检查
        /// </summary>
        /// <param name="config">要编辑的配置对象</param>
        /// <param name="existingConfigs">现有配置列表</param>
        public EditConfigForm(NetworkConfig config, List<NetworkConfig> existingConfigs)
            : this(config)
        {
            _existingConfigs = existingConfigs;
        }
        
        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitializeComponent()
        {
            // 设置表单基本属性
            Text = "编辑网络配置";
            Size = new Size(FormWidth, FormHeight);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = _backColor;
            ForeColor = _foreColor;
            
            // 设置全局字体
            Font globalFont = new Font(FontFamilyName, FontSizeRegular, FontStyle.Regular);
            Font labelFont = new Font(FontFamilyName, FontSizeRegular, FontStyle.Regular);
            Font tipFont = new Font(FontFamilyName, FontSizeSmall, FontStyle.Regular);
            
            // 布局容器
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 7,
                ColumnStyles = { 
                    new ColumnStyle(SizeType.Percent, 35F),
                    new ColumnStyle(SizeType.Percent, 65F) 
                },
                RowStyles = {
                    new RowStyle(SizeType.Percent, 12F),
                    new RowStyle(SizeType.Percent, 12F),
                    new RowStyle(SizeType.Percent, 12F),
                    new RowStyle(SizeType.Percent, 12F),
                    new RowStyle(SizeType.Percent, 28F),
                    new RowStyle(SizeType.Percent, 12F),
                    new RowStyle(SizeType.Percent, 12F)
                },
                Padding = new Padding(20, 20, 20, 20),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };
            
            // 添加半透明边框样式
            mainLayout.Paint += (sender, e) =>
            {
                using (Pen pen = new Pen(BorderColor, 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, mainLayout.Width - 1, mainLayout.Height - 1);
                }
            };
            
            // 配置名称
            Label nameLabel = new Label 
            {
                Text = "配置名称:", 
                Dock = DockStyle.Fill, 
                TextAlign = ContentAlignment.MiddleRight,
                Font = labelFont,
                ForeColor = ForeColor,
                Padding = LabelPadding,
                BackColor = BackColor
            };
            
            TextBox nameTextBox = new TextBox 
            {
                Dock = DockStyle.Fill, 
                Name = "nameTextBox",
                Font = globalFont,
                BackColor = BackColor,
                ForeColor = ForeColor,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = TextBoxPadding,
                Height = TextBoxHeight
            };
            
            nameTextBox.GotFocus += (sender, e) => nameTextBox.BorderStyle = BorderStyle.FixedSingle;
            nameTextBox.LostFocus += (sender, e) => nameTextBox.BorderStyle = BorderStyle.FixedSingle;
            
            mainLayout.Controls.Add(nameLabel, 0, 0);
            mainLayout.Controls.Add(nameTextBox, 1, 0);
            _nameTextBox = nameTextBox; // 缓存控件引用
            
            // IP地址
            Label ipLabel = new Label 
            {
                Text = "IP地址:", 
                Dock = DockStyle.Fill, 
                TextAlign = ContentAlignment.MiddleRight,
                Font = labelFont,
                ForeColor = ForeColor,
                Padding = LabelPadding,
                BackColor = BackColor
            };
            
            TextBox ipTextBox = new TextBox 
            {
                Dock = DockStyle.Fill, 
                Name = "ipTextBox",
                Font = globalFont,
                BackColor = BackColor,
                ForeColor = ForeColor,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = TextBoxPadding,
                Height = TextBoxHeight
            };
            
            ipTextBox.GotFocus += (sender, e) => ipTextBox.BorderStyle = BorderStyle.FixedSingle;
            ipTextBox.LostFocus += (sender, e) => ipTextBox.BorderStyle = BorderStyle.FixedSingle;
            
            mainLayout.Controls.Add(ipLabel, 0, 1);
            mainLayout.Controls.Add(ipTextBox, 1, 1);
            _ipTextBox = ipTextBox; // 缓存控件引用
            
            // 子网掩码
            Label maskLabel = new Label 
            {
                Text = "子网掩码:", 
                Dock = DockStyle.Fill, 
                TextAlign = ContentAlignment.MiddleRight,
                Font = labelFont,
                ForeColor = ForeColor,
                Padding = LabelPadding,
                BackColor = BackColor
            };
            
            TextBox maskTextBox = new TextBox 
            {
                Dock = DockStyle.Fill, 
                Name = "maskTextBox",
                Font = globalFont,
                BackColor = BackColor,
                ForeColor = ForeColor,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = TextBoxPadding,
                Height = TextBoxHeight
            };
            
            maskTextBox.GotFocus += (sender, e) => maskTextBox.BorderStyle = BorderStyle.FixedSingle;
            maskTextBox.LostFocus += (sender, e) => maskTextBox.BorderStyle = BorderStyle.FixedSingle;
            
            mainLayout.Controls.Add(maskLabel, 0, 2);
            mainLayout.Controls.Add(maskTextBox, 1, 2);
            _maskTextBox = maskTextBox; // 缓存控件引用
            
            // 默认网关
            Label gatewayLabel = new Label 
            {
                Text = "默认网关:", 
                Dock = DockStyle.Fill, 
                TextAlign = ContentAlignment.MiddleRight,
                Font = labelFont,
                ForeColor = ForeColor,
                Padding = LabelPadding,
                BackColor = BackColor
            };
            
            TextBox gatewayTextBox = new TextBox 
            {
                Dock = DockStyle.Fill, 
                Name = "gatewayTextBox",
                Font = globalFont,
                BackColor = BackColor,
                ForeColor = ForeColor,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = TextBoxPadding,
                Height = TextBoxHeight
            };
            
            gatewayTextBox.GotFocus += (sender, e) => gatewayTextBox.BorderStyle = BorderStyle.FixedSingle;
            gatewayTextBox.LostFocus += (sender, e) => gatewayTextBox.BorderStyle = BorderStyle.FixedSingle;
            
            mainLayout.Controls.Add(gatewayLabel, 0, 3);
            mainLayout.Controls.Add(gatewayTextBox, 1, 3);
            _gatewayTextBox = gatewayTextBox; // 缓存控件引用
            
            // DNS服务器
            Label dnsLabel = new Label 
            {
                Text = "DNS服务器:", 
                Dock = DockStyle.Fill, 
                TextAlign = ContentAlignment.MiddleRight,
                Font = labelFont,
                ForeColor = ForeColor,
                Padding = LabelPadding,
                BackColor = BackColor
            };
            
            // 使用多行文本框，支持每行一个DNS服务器
            TextBox dnsTextBox = new TextBox 
            {
                Dock = DockStyle.Fill, 
                Name = "dnsTextBox",
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = globalFont,
                BackColor = BackColor,
                ForeColor = ForeColor,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = DnsTextBoxPadding,
                AcceptsReturn = true,
                AcceptsTab = false,
                WordWrap = true
            };
            
            dnsTextBox.GotFocus += (sender, e) => dnsTextBox.BorderStyle = BorderStyle.FixedSingle;
            dnsTextBox.LostFocus += (sender, e) => dnsTextBox.BorderStyle = BorderStyle.FixedSingle;
            
            mainLayout.Controls.Add(dnsLabel, 0, 4);
            mainLayout.Controls.Add(dnsTextBox, 1, 4);
            _dnsTextBox = dnsTextBox; // 缓存控件引用
            
            // 提示信息
            Label tipLabel = new Label 
            {
                Text = "提示：每行输入一个DNS服务器地址，支持IPv4格式",
                Dock = DockStyle.Fill, 
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = TipForeColor,
                Font = tipFont,
                BackColor = TipBackgroundColor,
                Padding = new Padding(10, 5, 10, 5)
            };
            mainLayout.Controls.Add(tipLabel, 0, 5);
            mainLayout.SetColumnSpan(tipLabel, 2);
            
            // 按钮区域
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 5, 0, 5),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                BackColor = BackColor
            };
            
            Button okButton = new Button 
            {
                Text = "确定", 
                Width = ButtonWidth,
                Height = ButtonHeight,
                Name = "okButton",
                Font = new Font(FontFamilyName, FontSizeRegular, FontStyle.Regular),
                BackColor = ButtonPrimaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Padding = new Padding(15, 5, 15, 5),
                Margin = new Padding(10, 0, 0, 0),
                FlatAppearance = { BorderSize = 0 }
            };
            
            okButton.MouseEnter += (sender, e) => okButton.BackColor = ButtonPrimaryHoverColor;
            okButton.MouseLeave += (sender, e) => okButton.BackColor = ButtonPrimaryColor;
            okButton.MouseDown += (sender, e) => okButton.BackColor = ButtonPrimaryPressedColor;
            okButton.MouseUp += (sender, e) => okButton.BackColor = ButtonPrimaryHoverColor;
            okButton.FlatAppearance.BorderSize = 0;
            okButton.Click += OkButton_Click;
            
            Button cancelButton = new Button 
            {
                Text = "取消", 
                Width = ButtonWidth,
                Height = ButtonHeight,
                Name = "cancelButton",
                Font = new Font(FontFamilyName, FontSizeRegular, FontStyle.Regular),
                BackColor = ButtonSecondaryColor,
                ForeColor = ForeColor,
                FlatStyle = FlatStyle.Flat,
                Padding = new Padding(15, 5, 15, 5),
                Margin = new Padding(10, 0, 0, 0),
                FlatAppearance = { BorderSize = 1, BorderColor = ButtonSecondaryBorderColor }
            };
            
            cancelButton.MouseEnter += (sender, e) => cancelButton.BackColor = TipBackgroundColor;
            cancelButton.MouseLeave += (sender, e) => cancelButton.BackColor = ButtonSecondaryColor;
            cancelButton.Click += CancelButton_Click;
            
            buttonPanel.Controls.Add(okButton);
            buttonPanel.Controls.Add(cancelButton);
            
            mainLayout.Controls.Add(buttonPanel, 0, 6);
            mainLayout.SetColumnSpan(buttonPanel, 2);

            Controls.Add(mainLayout);
        }
        
        /// <summary>
        /// 加载配置数据到表单
        /// </summary>
        private void LoadConfigData()
        {
            try
            {
                // 设置表单标题，区分新增和编辑
                Text = string.IsNullOrEmpty(Config.IPAddress) && string.IsNullOrEmpty(Config.SubnetMask) ? "新增网络配置" : "编辑网络配置";

                // 配置名称
                if (_nameTextBox != null)
                {
                    _nameTextBox.Text = Config.Name;
                }

                // IP地址
                if (_ipTextBox != null)
                {
                    _ipTextBox.Text = Config.IPAddress;
                }

                // 子网掩码
                if (_maskTextBox != null)
                {
                    _maskTextBox.Text = Config.SubnetMask;
                }

                // 默认网关
                if (_gatewayTextBox != null)
                {
                    _gatewayTextBox.Text = Config.DefaultGateway;
                }

                // DNS服务器 - 多行文本框
                if (_dnsTextBox != null && Config.DnsServers != null)
                {
                    // 将DNS服务器列表转换为每行一个
                    _dnsTextBox.Text = string.Join(Environment.NewLine, Config.DnsServers);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"加载配置数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// 保存配置数据
        /// </summary>
        private void SaveConfigData()
        {
            try
            {
                // 配置名称
                if (_nameTextBox != null)
                {
                    Config.Name = _nameTextBox.Text.Trim();
                }

                // IP地址
                if (_ipTextBox != null)
                {
                    Config.IPAddress = _ipTextBox.Text.Trim();
                }

                // 子网掩码
                if (_maskTextBox != null)
                {
                    Config.SubnetMask = _maskTextBox.Text.Trim();
                }

                // 默认网关
                if (_gatewayTextBox != null)
                {
                    Config.DefaultGateway = _gatewayTextBox.Text.Trim();
                }

                // DNS服务器 - 多行文本框
                if (_dnsTextBox != null)
                {
                    Config.DnsServers = new List<string>();

                    // 按行分割，去除空行和空格
                    string[] dnsLines = _dnsTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in dnsLines)
                    {
                        string dns = line.Trim();
                        if (!string.IsNullOrEmpty(dns))
                        {
                            Config.DnsServers.Add(dns);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"保存配置数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw; // 重新抛出异常，以便调用者知道保存失败
            }
        }
        
        /// <summary>
        /// 验证IP地址格式
        /// </summary>
        /// <param name="ipAddress">要验证的IP地址</param>
        /// <returns>是否为有效的IP地址</returns>
        private bool IsValidIpAddress(string ipAddress)
        {
            return IPAddress.TryParse(ipAddress, out _);
        }
        
        /// <summary>
        /// 验证配置名称是否唯一
        /// </summary>
        /// <param name="name">配置名称</param>
        /// <returns>是否唯一</returns>
        private bool IsConfigNameUnique(string name)
        {
            if (_existingConfigs == null) return true;
            
            // 排除当前编辑的配置
            return !_existingConfigs.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && !ReferenceEquals(c, Config));
        }
        
        /// <summary>
        /// 验证配置数据
        /// </summary>
        /// <returns>是否验证通过</returns>
        private bool ValidateConfigData()
        {
            // 配置名称不能为空
            if (_nameTextBox != null)
            {
                string configName = _nameTextBox.Text.Trim();
                if (string.IsNullOrEmpty(configName))
                {
                    MessageBox.Show(this, "配置名称不能为空", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _nameTextBox.Focus();
                    return false;
                }

                // 验证配置名称唯一性
                if (!IsConfigNameUnique(configName))
                {
                    MessageBox.Show(this, "配置名称已存在，请使用其他名称", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _nameTextBox.Focus();
                    return false;
                }
            }

            // 如果不是DHCP配置，需要验证IP相关参数
            if (_ipTextBox != null && !string.IsNullOrEmpty(_ipTextBox.Text.Trim()))
            {
                // 验证IP地址格式
                string ipAddress = _ipTextBox.Text.Trim();
                if (!IsValidIpAddress(ipAddress))
                {
                    MessageBox.Show(this, "IP地址格式不正确", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _ipTextBox.Focus();
                    return false;
                }

                // 验证子网掩码格式
                if (_maskTextBox != null)
                {
                    string subnetMask = _maskTextBox.Text.Trim();
                    if (string.IsNullOrEmpty(subnetMask))
                    {
                        MessageBox.Show(this, "子网掩码不能为空", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _maskTextBox.Focus();
                        return false;
                    }

                    if (!IsValidIpAddress(subnetMask))
                    {
                        MessageBox.Show(this, "子网掩码格式不正确", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _maskTextBox.Focus();
                        return false;
                    }
                }

                // 验证默认网关格式（如果填写）
                if (_gatewayTextBox != null)
                {
                    string gateway = _gatewayTextBox.Text.Trim();
                    if (!string.IsNullOrEmpty(gateway) && !IsValidIpAddress(gateway))
                    {
                        MessageBox.Show(this, "默认网关格式不正确", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _gatewayTextBox.Focus();
                        return false;
                    }
                }
            }

            // 验证DNS服务器格式
            if (_dnsTextBox != null)
            {
                string[] dnsLines = _dnsTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in dnsLines)
                {
                    string dns = line.Trim();
                    if (!string.IsNullOrEmpty(dns) && !IsValidIpAddress(dns))
                    {
                        MessageBox.Show(this, $"DNS服务器 '{dns}' 格式不正确", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _dnsTextBox.Focus();
                        return false;
                    }
                }
            }

            return true;
        }
        
        /// <summary>
        /// 确定按钮点击事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void OkButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (ValidateConfigData())
                {
                    SaveConfigData();
                    IsCanceled = false;
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"操作失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// 取消按钮点击事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            IsCanceled = true;
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
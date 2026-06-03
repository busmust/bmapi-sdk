<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MainForm
    Inherits System.Windows.Forms.Form
    'Form 重写 Dispose，以清理组件列表。
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意: 以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。  
    '不要使用代码编辑器修改它。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.flowLayoutPanel1 = New System.Windows.Forms.FlowLayoutPanel()
        Me.Label15 = New System.Windows.Forms.Label()
        Me.hostComboBox = New System.Windows.Forms.ComboBox()
        Me.label1 = New System.Windows.Forms.Label()
        Me.channelComboBox = New ComboBoxCustomer.ComCheckBoxList()
        Me.tresCheck = New System.Windows.Forms.CheckBox()
        Me.label2 = New System.Windows.Forms.Label()
        Me.modeCombo = New System.Windows.Forms.ComboBox()
        Me.label3 = New System.Windows.Forms.Label()
        Me.nominalBitrateEdit = New System.Windows.Forms.TextBox()
        Me.label4 = New System.Windows.Forms.Label()
        Me.dataBitrateEdit = New System.Windows.Forms.TextBox()
        Me.label5 = New System.Windows.Forms.Label()
        Me.samplePosSpin = New System.Windows.Forms.NumericUpDown()
        Me.openButton = New System.Windows.Forms.Button()
        Me.label6 = New System.Windows.Forms.Label()
        Me.rxIdEdit = New System.Windows.Forms.TextBox()
        Me.label7 = New System.Windows.Forms.Label()
        Me.rxMaskEdit = New System.Windows.Forms.TextBox()
        Me.rxStandardCheck = New System.Windows.Forms.CheckBox()
        Me.rxExtendedCheck = New System.Windows.Forms.CheckBox()
        Me.rxAdvancedFilterCheck = New System.Windows.Forms.CheckBox()
        Me.label8 = New System.Windows.Forms.Label()
        Me.rxDataMaskEdit = New System.Windows.Forms.TextBox()
        Me.enumerateButton = New System.Windows.Forms.Button()
        Me.label9 = New System.Windows.Forms.Label()
        Me.txIdEdit = New System.Windows.Forms.TextBox()
        Me.label10 = New System.Windows.Forms.Label()
        Me.txTypeCombo = New System.Windows.Forms.ComboBox()
        Me.txFdCheck = New System.Windows.Forms.CheckBox()
        Me.txExtendedCheck = New System.Windows.Forms.CheckBox()
        Me.label16 = New System.Windows.Forms.Label()
        Me.sendComboBox = New System.Windows.Forms.ComboBox()
        Me.label11 = New System.Windows.Forms.Label()
        Me.txCycleEdit = New System.Windows.Forms.TextBox()
        Me.label12 = New System.Windows.Forms.Label()
        Me.txCountEdit = New System.Windows.Forms.TextBox()
        Me.label13 = New System.Windows.Forms.Label()
        Me.txLengthEdit = New System.Windows.Forms.TextBox()
        Me.label14 = New System.Windows.Forms.Label()
        Me.txDataEdit = New System.Windows.Forms.TextBox()
        Me.txButton = New System.Windows.Forms.Button()
        Me.msgListView = New System.Windows.Forms.ListView()
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader3 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader4 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader5 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader6 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader7 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader8 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.rxTimer = New System.Windows.Forms.Timer(Me.components)
        Me.txTimer = New System.Windows.Forms.Timer(Me.components)
        Me.flowLayoutPanel1.SuspendLayout()
        CType(Me.samplePosSpin, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'flowLayoutPanel1
        '
        Me.flowLayoutPanel1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.flowLayoutPanel1.Controls.Add(Me.Label15)
        Me.flowLayoutPanel1.Controls.Add(Me.hostComboBox)
        Me.flowLayoutPanel1.Controls.Add(Me.label1)
        Me.flowLayoutPanel1.Controls.Add(Me.channelComboBox)
        Me.flowLayoutPanel1.Controls.Add(Me.tresCheck)
        Me.flowLayoutPanel1.Controls.Add(Me.label2)
        Me.flowLayoutPanel1.Controls.Add(Me.modeCombo)
        Me.flowLayoutPanel1.Controls.Add(Me.label3)
        Me.flowLayoutPanel1.Controls.Add(Me.nominalBitrateEdit)
        Me.flowLayoutPanel1.Controls.Add(Me.label4)
        Me.flowLayoutPanel1.Controls.Add(Me.dataBitrateEdit)
        Me.flowLayoutPanel1.Controls.Add(Me.label5)
        Me.flowLayoutPanel1.Controls.Add(Me.samplePosSpin)
        Me.flowLayoutPanel1.Controls.Add(Me.openButton)
        Me.flowLayoutPanel1.Controls.Add(Me.label6)
        Me.flowLayoutPanel1.Controls.Add(Me.rxIdEdit)
        Me.flowLayoutPanel1.Controls.Add(Me.label7)
        Me.flowLayoutPanel1.Controls.Add(Me.rxMaskEdit)
        Me.flowLayoutPanel1.Controls.Add(Me.rxStandardCheck)
        Me.flowLayoutPanel1.Controls.Add(Me.rxExtendedCheck)
        Me.flowLayoutPanel1.Controls.Add(Me.rxAdvancedFilterCheck)
        Me.flowLayoutPanel1.Controls.Add(Me.label8)
        Me.flowLayoutPanel1.Controls.Add(Me.rxDataMaskEdit)
        Me.flowLayoutPanel1.Controls.Add(Me.enumerateButton)
        Me.flowLayoutPanel1.Controls.Add(Me.label9)
        Me.flowLayoutPanel1.Controls.Add(Me.txIdEdit)
        Me.flowLayoutPanel1.Controls.Add(Me.label10)
        Me.flowLayoutPanel1.Controls.Add(Me.txTypeCombo)
        Me.flowLayoutPanel1.Controls.Add(Me.txFdCheck)
        Me.flowLayoutPanel1.Controls.Add(Me.txExtendedCheck)
        Me.flowLayoutPanel1.Controls.Add(Me.label16)
        Me.flowLayoutPanel1.Controls.Add(Me.sendComboBox)
        Me.flowLayoutPanel1.Controls.Add(Me.label11)
        Me.flowLayoutPanel1.Controls.Add(Me.txCycleEdit)
        Me.flowLayoutPanel1.Controls.Add(Me.label12)
        Me.flowLayoutPanel1.Controls.Add(Me.txCountEdit)
        Me.flowLayoutPanel1.Controls.Add(Me.label13)
        Me.flowLayoutPanel1.Controls.Add(Me.txLengthEdit)
        Me.flowLayoutPanel1.Controls.Add(Me.label14)
        Me.flowLayoutPanel1.Controls.Add(Me.txDataEdit)
        Me.flowLayoutPanel1.Controls.Add(Me.txButton)
        Me.flowLayoutPanel1.Location = New System.Drawing.Point(15, 364)
        Me.flowLayoutPanel1.MinimumSize = New System.Drawing.Size(917, 89)
        Me.flowLayoutPanel1.Name = "flowLayoutPanel1"
        Me.flowLayoutPanel1.Size = New System.Drawing.Size(1245, 89)
        Me.flowLayoutPanel1.TabIndex = 4
        '
        'Label15
        '
        Me.Label15.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.Label15.AutoSize = True
        Me.Label15.Location = New System.Drawing.Point(3, 8)
        Me.Label15.MinimumSize = New System.Drawing.Size(35, 0)
        Me.Label15.Name = "Label15"
        Me.Label15.Size = New System.Drawing.Size(35, 12)
        Me.Label15.TabIndex = 42
        Me.Label15.Text = "Host"
        '
        'hostComboBox
        '
        Me.hostComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.hostComboBox.FormattingEnabled = True
        Me.hostComboBox.Items.AddRange(New Object() {"Local USB Devices"})
        Me.hostComboBox.Location = New System.Drawing.Point(44, 3)
        Me.hostComboBox.Name = "hostComboBox"
        Me.hostComboBox.Size = New System.Drawing.Size(132, 20)
        Me.hostComboBox.TabIndex = 41
        '
        'label1
        '
        Me.label1.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.label1.AutoSize = True
        Me.label1.Location = New System.Drawing.Point(182, 8)
        Me.label1.Name = "label1"
        Me.label1.Size = New System.Drawing.Size(47, 12)
        Me.label1.TabIndex = 0
        Me.label1.Text = "Channel"
        '
        'channelComboBox
        '
        Me.channelComboBox.DataSource = Nothing
        Me.channelComboBox.Location = New System.Drawing.Point(235, 2)
        Me.channelComboBox.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.channelComboBox.Name = "channelComboBox"
        Me.channelComboBox.Size = New System.Drawing.Size(310, 20)
        Me.channelComboBox.TabIndex = 5
        '
        'tresCheck
        '
        Me.tresCheck.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.tresCheck.AutoSize = True
        Me.tresCheck.Checked = True
        Me.tresCheck.CheckState = System.Windows.Forms.CheckState.Checked
        Me.tresCheck.Location = New System.Drawing.Point(551, 6)
        Me.tresCheck.Name = "tresCheck"
        Me.tresCheck.Size = New System.Drawing.Size(54, 16)
        Me.tresCheck.TabIndex = 5
        Me.tresCheck.Text = "T-Res"
        Me.tresCheck.UseVisualStyleBackColor = True
        '
        'label2
        '
        Me.label2.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.label2.AutoSize = True
        Me.label2.Location = New System.Drawing.Point(611, 8)
        Me.label2.Name = "label2"
        Me.label2.Size = New System.Drawing.Size(53, 12)
        Me.label2.TabIndex = 2
        Me.label2.Text = "CAN Mode"
        '
        'modeCombo
        '
        Me.modeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.modeCombo.FormattingEnabled = True
        Me.modeCombo.Items.AddRange(New Object() {"Normal", "Reserved", "Internal Loopback", "Listen Only", "Reserved", "External Loopback", "CAN Only", "Reserved"})
        Me.modeCombo.Location = New System.Drawing.Point(670, 3)
        Me.modeCombo.Name = "modeCombo"
        Me.modeCombo.Size = New System.Drawing.Size(76, 20)
        Me.modeCombo.TabIndex = 3
        '
        'label3
        '
        Me.label3.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.label3.AutoSize = True
        Me.label3.Location = New System.Drawing.Point(752, 8)
        Me.label3.Name = "label3"
        Me.label3.Size = New System.Drawing.Size(77, 12)
        Me.label3.TabIndex = 6
        Me.label3.Text = "Nominal kbps"
        '
        'nominalBitrateEdit
        '
        Me.nominalBitrateEdit.Location = New System.Drawing.Point(835, 3)
        Me.nominalBitrateEdit.Name = "nominalBitrateEdit"
        Me.nominalBitrateEdit.Size = New System.Drawing.Size(24, 21)
        Me.nominalBitrateEdit.TabIndex = 7
        Me.nominalBitrateEdit.Text = "500"
        '
        'label4
        '
        Me.label4.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.label4.AutoSize = True
        Me.label4.Location = New System.Drawing.Point(865, 8)
        Me.label4.Name = "label4"
        Me.label4.Size = New System.Drawing.Size(59, 12)
        Me.label4.TabIndex = 8
        Me.label4.Text = "Data kbps"
        '
        'dataBitrateEdit
        '
        Me.dataBitrateEdit.Location = New System.Drawing.Point(930, 3)
        Me.dataBitrateEdit.Name = "dataBitrateEdit"
        Me.dataBitrateEdit.Size = New System.Drawing.Size(32, 21)
        Me.dataBitrateEdit.TabIndex = 10
        Me.dataBitrateEdit.Text = "2000"
        '
        'label5
        '
        Me.label5.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.label5.AutoSize = True
        Me.label5.Location = New System.Drawing.Point(968, 8)
        Me.label5.Name = "label5"
        Me.label5.Size = New System.Drawing.Size(77, 12)
        Me.label5.TabIndex = 11
        Me.label5.Text = "Sample pos %"
        '
        'samplePosSpin
        '
        Me.samplePosSpin.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.samplePosSpin.Increment = New Decimal(New Integer() {5, 0, 0, 0})
        Me.samplePosSpin.Location = New System.Drawing.Point(1051, 4)
        Me.samplePosSpin.Minimum = New Decimal(New Integer() {60, 0, 0, 0})
        Me.samplePosSpin.Name = "samplePosSpin"
        Me.samplePosSpin.Size = New System.Drawing.Size(45, 21)
        Me.samplePosSpin.TabIndex = 12
        Me.samplePosSpin.Value = New Decimal(New Integer() {75, 0, 0, 0})
        '
        'openButton
        '
        Me.openButton.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.flowLayoutPanel1.SetFlowBreak(Me.openButton, True)
        Me.openButton.Location = New System.Drawing.Point(1102, 3)
        Me.openButton.Name = "openButton"
        Me.openButton.Size = New System.Drawing.Size(100, 23)
        Me.openButton.TabIndex = 13
        Me.openButton.Text = "Open"
        Me.openButton.UseVisualStyleBackColor = True
        '
        'label6
        '
        Me.label6.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.label6.AutoSize = True
        Me.label6.Location = New System.Drawing.Point(3, 37)
        Me.label6.Name = "label6"
        Me.label6.Size = New System.Drawing.Size(35, 12)
        Me.label6.TabIndex = 14
        Me.label6.Text = "RX ID"
        '
        'rxIdEdit
        '
        Me.rxIdEdit.Location = New System.Drawing.Point(44, 32)
        Me.rxIdEdit.Name = "rxIdEdit"
        Me.rxIdEdit.Size = New System.Drawing.Size(132, 21)
        Me.rxIdEdit.TabIndex = 15
        Me.rxIdEdit.Text = "00000000"
        '
        'label7
        '
        Me.label7.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.label7.AutoSize = True
        Me.label7.Location = New System.Drawing.Point(182, 37)
        Me.label7.MinimumSize = New System.Drawing.Size(47, 0)
        Me.label7.Name = "label7"
        Me.label7.Size = New System.Drawing.Size(47, 12)
        Me.label7.TabIndex = 16
        Me.label7.Text = "Mask"
        '
        'rxMaskEdit
        '
        Me.rxMaskEdit.Location = New System.Drawing.Point(235, 32)
        Me.rxMaskEdit.Name = "rxMaskEdit"
        Me.rxMaskEdit.Size = New System.Drawing.Size(90, 21)
        Me.rxMaskEdit.TabIndex = 19
        Me.rxMaskEdit.Text = "00000000"
        '
        'rxStandardCheck
        '
        Me.rxStandardCheck.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.rxStandardCheck.AutoSize = True
        Me.rxStandardCheck.Checked = True
        Me.rxStandardCheck.CheckState = System.Windows.Forms.CheckState.Checked
        Me.rxStandardCheck.Location = New System.Drawing.Point(331, 35)
        Me.rxStandardCheck.Name = "rxStandardCheck"
        Me.rxStandardCheck.Size = New System.Drawing.Size(84, 16)
        Me.rxStandardCheck.TabIndex = 18
        Me.rxStandardCheck.Text = "Accept Std"
        Me.rxStandardCheck.UseVisualStyleBackColor = True
        '
        'rxExtendedCheck
        '
        Me.rxExtendedCheck.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.rxExtendedCheck.AutoSize = True
        Me.rxExtendedCheck.Checked = True
        Me.rxExtendedCheck.CheckState = System.Windows.Forms.CheckState.Checked
        Me.rxExtendedCheck.Location = New System.Drawing.Point(421, 35)
        Me.rxExtendedCheck.Name = "rxExtendedCheck"
        Me.rxExtendedCheck.Size = New System.Drawing.Size(84, 16)
        Me.rxExtendedCheck.TabIndex = 20
        Me.rxExtendedCheck.Text = "Accept Ext"
        Me.rxExtendedCheck.UseVisualStyleBackColor = True
        '
        'rxAdvancedFilterCheck
        '
        Me.rxAdvancedFilterCheck.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.rxAdvancedFilterCheck.AutoSize = True
        Me.rxAdvancedFilterCheck.Enabled = False
        Me.rxAdvancedFilterCheck.Location = New System.Drawing.Point(511, 35)
        Me.rxAdvancedFilterCheck.MinimumSize = New System.Drawing.Size(235, 0)
        Me.rxAdvancedFilterCheck.Name = "rxAdvancedFilterCheck"
        Me.rxAdvancedFilterCheck.Size = New System.Drawing.Size(235, 16)
        Me.rxAdvancedFilterCheck.TabIndex = 21
        Me.rxAdvancedFilterCheck.Text = "Advanced RX Filter"
        Me.rxAdvancedFilterCheck.UseVisualStyleBackColor = True
        '
        'label8
        '
        Me.label8.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.label8.AutoSize = True
        Me.label8.Location = New System.Drawing.Point(752, 37)
        Me.label8.Name = "label8"
        Me.label8.Size = New System.Drawing.Size(59, 12)
        Me.label8.TabIndex = 22
        Me.label8.Text = "Data Mask"
        '
        'rxDataMaskEdit
        '
        Me.rxDataMaskEdit.Enabled = False
        Me.rxDataMaskEdit.Location = New System.Drawing.Point(817, 32)
        Me.rxDataMaskEdit.Name = "rxDataMaskEdit"
        Me.rxDataMaskEdit.Size = New System.Drawing.Size(279, 21)
        Me.rxDataMaskEdit.TabIndex = 23
        Me.rxDataMaskEdit.Text = "xx xx xx xx xx xx xx xx"
        '
        'enumerateButton
        '
        Me.enumerateButton.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.flowLayoutPanel1.SetFlowBreak(Me.enumerateButton, True)
        Me.enumerateButton.Location = New System.Drawing.Point(1102, 32)
        Me.enumerateButton.Name = "enumerateButton"
        Me.enumerateButton.Size = New System.Drawing.Size(100, 23)
        Me.enumerateButton.TabIndex = 24
        Me.enumerateButton.Text = "Enumerate"
        Me.enumerateButton.UseVisualStyleBackColor = True
        '
        'label9
        '
        Me.label9.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.label9.AutoSize = True
        Me.label9.Location = New System.Drawing.Point(3, 66)
        Me.label9.Name = "label9"
        Me.label9.Size = New System.Drawing.Size(35, 12)
        Me.label9.TabIndex = 25
        Me.label9.Text = "TX ID"
        '
        'txIdEdit
        '
        Me.txIdEdit.Location = New System.Drawing.Point(44, 61)
        Me.txIdEdit.Name = "txIdEdit"
        Me.txIdEdit.Size = New System.Drawing.Size(132, 21)
        Me.txIdEdit.TabIndex = 26
        Me.txIdEdit.Text = "7FF"
        '
        'label10
        '
        Me.label10.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.label10.AutoSize = True
        Me.label10.Location = New System.Drawing.Point(182, 66)
        Me.label10.MinimumSize = New System.Drawing.Size(47, 0)
        Me.label10.Name = "label10"
        Me.label10.Size = New System.Drawing.Size(47, 12)
        Me.label10.TabIndex = 27
        Me.label10.Text = "Type"
        '
        'txTypeCombo
        '
        Me.txTypeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.txTypeCombo.FormattingEnabled = True
        Me.txTypeCombo.Items.AddRange(New Object() {"SW"})
        Me.txTypeCombo.Location = New System.Drawing.Point(235, 61)
        Me.txTypeCombo.Name = "txTypeCombo"
        Me.txTypeCombo.Size = New System.Drawing.Size(90, 20)
        Me.txTypeCombo.TabIndex = 28
        '
        'txFdCheck
        '
        Me.txFdCheck.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.txFdCheck.AutoSize = True
        Me.txFdCheck.Location = New System.Drawing.Point(331, 64)
        Me.txFdCheck.Name = "txFdCheck"
        Me.txFdCheck.Size = New System.Drawing.Size(36, 16)
        Me.txFdCheck.TabIndex = 29
        Me.txFdCheck.Text = "FD"
        Me.txFdCheck.UseVisualStyleBackColor = True
        '
        'txExtendedCheck
        '
        Me.txExtendedCheck.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.txExtendedCheck.AutoSize = True
        Me.txExtendedCheck.Location = New System.Drawing.Point(373, 64)
        Me.txExtendedCheck.Name = "txExtendedCheck"
        Me.txExtendedCheck.Size = New System.Drawing.Size(42, 16)
        Me.txExtendedCheck.TabIndex = 30
        Me.txExtendedCheck.Text = "Ext"
        Me.txExtendedCheck.UseVisualStyleBackColor = True
        '
        'label16
        '
        Me.label16.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.label16.AutoSize = True
        Me.label16.Location = New System.Drawing.Point(421, 66)
        Me.label16.Name = "label16"
        Me.label16.Size = New System.Drawing.Size(17, 12)
        Me.label16.TabIndex = 5
        Me.label16.Text = "CH"
        '
        'sendComboBox
        '
        Me.sendComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.sendComboBox.FormattingEnabled = True
        Me.sendComboBox.Location = New System.Drawing.Point(444, 61)
        Me.sendComboBox.Name = "sendComboBox"
        Me.sendComboBox.Size = New System.Drawing.Size(44, 20)
        Me.sendComboBox.TabIndex = 40
        '
        'label11
        '
        Me.label11.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.label11.AutoSize = True
        Me.label11.Location = New System.Drawing.Point(494, 66)
        Me.label11.Name = "label11"
        Me.label11.Size = New System.Drawing.Size(59, 12)
        Me.label11.TabIndex = 31
        Me.label11.Text = "Cycle(ms)"
        '
        'txCycleEdit
        '
        Me.txCycleEdit.Location = New System.Drawing.Point(559, 61)
        Me.txCycleEdit.Name = "txCycleEdit"
        Me.txCycleEdit.Size = New System.Drawing.Size(30, 21)
        Me.txCycleEdit.TabIndex = 32
        Me.txCycleEdit.Text = "1000"
        '
        'label12
        '
        Me.label12.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.label12.AutoSize = True
        Me.label12.Location = New System.Drawing.Point(595, 66)
        Me.label12.Name = "label12"
        Me.label12.Size = New System.Drawing.Size(35, 12)
        Me.label12.TabIndex = 33
        Me.label12.Text = "Count"
        '
        'txCountEdit
        '
        Me.txCountEdit.Location = New System.Drawing.Point(636, 61)
        Me.txCountEdit.Name = "txCountEdit"
        Me.txCountEdit.Size = New System.Drawing.Size(28, 21)
        Me.txCountEdit.TabIndex = 34
        Me.txCountEdit.Text = "1"
        '
        'label13
        '
        Me.label13.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.label13.AutoSize = True
        Me.label13.Location = New System.Drawing.Point(670, 66)
        Me.label13.Name = "label13"
        Me.label13.Size = New System.Drawing.Size(41, 12)
        Me.label13.TabIndex = 35
        Me.label13.Text = "Length"
        '
        'txLengthEdit
        '
        Me.txLengthEdit.Location = New System.Drawing.Point(717, 61)
        Me.txLengthEdit.Name = "txLengthEdit"
        Me.txLengthEdit.Size = New System.Drawing.Size(29, 21)
        Me.txLengthEdit.TabIndex = 36
        Me.txLengthEdit.Text = "8"
        '
        'label14
        '
        Me.label14.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.label14.AutoSize = True
        Me.label14.Location = New System.Drawing.Point(752, 66)
        Me.label14.MinimumSize = New System.Drawing.Size(59, 0)
        Me.label14.Name = "label14"
        Me.label14.Size = New System.Drawing.Size(59, 12)
        Me.label14.TabIndex = 37
        Me.label14.Text = "Data"
        '
        'txDataEdit
        '
        Me.txDataEdit.Location = New System.Drawing.Point(817, 61)
        Me.txDataEdit.Name = "txDataEdit"
        Me.txDataEdit.Size = New System.Drawing.Size(279, 21)
        Me.txDataEdit.TabIndex = 38
        Me.txDataEdit.Text = "01 23 45 67 89 AB CD EF"
        '
        'txButton
        '
        Me.txButton.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txButton.Enabled = False
        Me.flowLayoutPanel1.SetFlowBreak(Me.txButton, True)
        Me.txButton.Location = New System.Drawing.Point(1102, 61)
        Me.txButton.MaximumSize = New System.Drawing.Size(101, 30)
        Me.txButton.Name = "txButton"
        Me.txButton.Size = New System.Drawing.Size(101, 23)
        Me.txButton.TabIndex = 39
        Me.txButton.Text = "Transmit"
        Me.txButton.UseVisualStyleBackColor = True
        '
        'msgListView
        '
        Me.msgListView.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2, Me.ColumnHeader3, Me.ColumnHeader4, Me.ColumnHeader5, Me.ColumnHeader6, Me.ColumnHeader7, Me.ColumnHeader8})
        Me.msgListView.HideSelection = False
        Me.msgListView.Location = New System.Drawing.Point(16, 7)
        Me.msgListView.Margin = New System.Windows.Forms.Padding(2)
        Me.msgListView.Name = "msgListView"
        Me.msgListView.Size = New System.Drawing.Size(1244, 321)
        Me.msgListView.TabIndex = 5
        Me.msgListView.UseCompatibleStateImageBehavior = False
        Me.msgListView.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader1
        '
        Me.ColumnHeader1.Text = "Time(s)"
        Me.ColumnHeader1.Width = 75
        '
        'ColumnHeader2
        '
        Me.ColumnHeader2.Text = "UtcTime (us)"
        Me.ColumnHeader2.Width = 200
        '
        'ColumnHeader3
        '
        Me.ColumnHeader3.Text = "TX/Rx"
        '
        'ColumnHeader4
        '
        Me.ColumnHeader4.Text = "Channel"
        Me.ColumnHeader4.Width = 78
        '
        'ColumnHeader5
        '
        Me.ColumnHeader5.Text = "ID(HEX)"
        Me.ColumnHeader5.Width = 77
        '
        'ColumnHeader6
        '
        Me.ColumnHeader6.Text = "Type"
        Me.ColumnHeader6.Width = 81
        '
        'ColumnHeader7
        '
        Me.ColumnHeader7.Text = "Length (B)"
        Me.ColumnHeader7.Width = 116
        '
        'ColumnHeader8
        '
        Me.ColumnHeader8.Text = "Payload"
        Me.ColumnHeader8.Width = 765
        '
        'rxTimer
        '
        '
        'txTimer
        '
        '
        'MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1284, 461)
        Me.Controls.Add(Me.msgListView)
        Me.Controls.Add(Me.flowLayoutPanel1)
        Me.DoubleBuffered = True
        Me.Margin = New System.Windows.Forms.Padding(2)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(1300, 500)
        Me.MinimumSize = New System.Drawing.Size(1300, 466)
        Me.Name = "MainForm"
        Me.Text = "BUSMUST CAN Analyzer"
        Me.flowLayoutPanel1.ResumeLayout(False)
        Me.flowLayoutPanel1.PerformLayout()
        CType(Me.samplePosSpin, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Private WithEvents flowLayoutPanel1 As Windows.Forms.FlowLayoutPanel
    Private WithEvents label1 As Windows.Forms.Label
    Private WithEvents channelComboBox As ComboBoxCustomer.ComCheckBoxList
    Private WithEvents tresCheck As Windows.Forms.CheckBox
    Private WithEvents label2 As Windows.Forms.Label
    Private WithEvents modeCombo As Windows.Forms.ComboBox
    Private WithEvents label3 As Windows.Forms.Label
    Private WithEvents nominalBitrateEdit As Windows.Forms.TextBox
    Private WithEvents label4 As Windows.Forms.Label
    Private WithEvents dataBitrateEdit As Windows.Forms.TextBox
    Private WithEvents label5 As Windows.Forms.Label
    Private WithEvents samplePosSpin As Windows.Forms.NumericUpDown
    Private WithEvents openButton As Windows.Forms.Button
    Private WithEvents label6 As Windows.Forms.Label
    Private WithEvents rxIdEdit As Windows.Forms.TextBox
    Private WithEvents label7 As Windows.Forms.Label
    Private WithEvents rxMaskEdit As Windows.Forms.TextBox
    Private WithEvents rxStandardCheck As Windows.Forms.CheckBox
    Private WithEvents rxExtendedCheck As Windows.Forms.CheckBox
    Private WithEvents rxAdvancedFilterCheck As Windows.Forms.CheckBox
    Private WithEvents label8 As Windows.Forms.Label
    Private WithEvents rxDataMaskEdit As Windows.Forms.TextBox
    Private WithEvents enumerateButton As Windows.Forms.Button
    Private WithEvents label9 As Windows.Forms.Label
    Private WithEvents txIdEdit As Windows.Forms.TextBox
    Private WithEvents label10 As Windows.Forms.Label
    Private WithEvents txTypeCombo As Windows.Forms.ComboBox
    Private WithEvents txFdCheck As Windows.Forms.CheckBox
    Private WithEvents txExtendedCheck As Windows.Forms.CheckBox
    Private WithEvents label16 As Windows.Forms.Label
    Private WithEvents sendComboBox As Windows.Forms.ComboBox
    Private WithEvents label11 As Windows.Forms.Label
    Private WithEvents txCycleEdit As Windows.Forms.TextBox
    Private WithEvents label12 As Windows.Forms.Label
    Private WithEvents txCountEdit As Windows.Forms.TextBox
    Private WithEvents label13 As Windows.Forms.Label
    Private WithEvents txLengthEdit As Windows.Forms.TextBox
    Private WithEvents label14 As Windows.Forms.Label
    Private WithEvents txDataEdit As Windows.Forms.TextBox
    Private WithEvents txButton As Windows.Forms.Button
    Friend WithEvents msgListView As Windows.Forms.ListView
    Friend WithEvents ColumnHeader1 As Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader2 As Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader3 As Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader4 As Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader5 As Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader6 As Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader7 As Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader8 As Windows.Forms.ColumnHeader
    Friend WithEvents rxTimer As Windows.Forms.Timer
    Friend WithEvents txTimer As Windows.Forms.Timer
    Private WithEvents Label15 As Windows.Forms.Label
    Private WithEvents hostComboBox As Windows.Forms.ComboBox
End Class

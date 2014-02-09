namespace Clientix {
    partial class Clientix {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Clientix));
            this.enteredTextField = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.connectToManagerButton = new System.Windows.Forms.Button();
            this.log = new System.Windows.Forms.TextBox();
            this.sendText = new System.Windows.Forms.Button();
            this.connectToCloudButton = new System.Windows.Forms.Button();
            this.cloudIPField = new System.Windows.Forms.TextBox();
            this.managerIPField = new System.Windows.Forms.TextBox();
            this.cloudPortField = new System.Windows.Forms.TextBox();
            this.managerPortField = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.usernameField = new System.Windows.Forms.TextBox();
            this.setUsernameButton = new System.Windows.Forms.Button();
            this.getOtherClients = new System.Windows.Forms.Button();
            this.connectWithClientButton = new System.Windows.Forms.Button();
            this.disconnectWithClient = new System.Windows.Forms.Button();
            this.DisconnectButton = new System.Windows.Forms.Button();
            this.SaveConfigButton = new System.Windows.Forms.Button();
            this.ClientHostNumberField = new System.Windows.Forms.TextBox();
            this.ClientSubnetworkNumberField = new System.Windows.Forms.TextBox();
            this.setClientNumber = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.ClientNetworkNumberField = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.selectedClientBox = new System.Windows.Forms.ComboBox();
            this.controlCloudPortTextBox = new System.Windows.Forms.TextBox();
            this.portLabel = new System.Windows.Forms.Label();
            this.conLabel = new System.Windows.Forms.Label();
            this.controlCloudIPTextBox = new System.Windows.Forms.TextBox();
            this.conToCloudButton = new System.Windows.Forms.Button();
            this.clientSpeedBox = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.chooseTextFile = new System.Windows.Forms.Button();
            this.howToSendComboBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // enteredTextField
            // 
            this.enteredTextField.Location = new System.Drawing.Point(15, 25);
            this.enteredTextField.Name = "enteredTextField";
            this.enteredTextField.Size = new System.Drawing.Size(199, 20);
            this.enteredTextField.TabIndex = 0;
            this.enteredTextField.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.sendMessage_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(144, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Tu wprowadź tekst wysyłany";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(368, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "IP chmury kablowej";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(368, 51);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(108, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Port chmury kablowej";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(476, 50);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Port zarządcy";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(474, 11);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(62, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "IP zarządcy";
            // 
            // connectToManagerButton
            // 
            this.connectToManagerButton.Location = new System.Drawing.Point(477, 92);
            this.connectToManagerButton.Name = "connectToManagerButton";
            this.connectToManagerButton.Size = new System.Drawing.Size(100, 44);
            this.connectToManagerButton.TabIndex = 11;
            this.connectToManagerButton.Text = "Połącz z zarządcą";
            this.connectToManagerButton.UseVisualStyleBackColor = true;
            this.connectToManagerButton.Click += new System.EventHandler(this.connectToManager);
            // 
            // log
            // 
            this.log.BackColor = System.Drawing.SystemColors.Window;
            this.log.Location = new System.Drawing.Point(16, 92);
            this.log.Multiline = true;
            this.log.Name = "log";
            this.log.ReadOnly = true;
            this.log.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.log.Size = new System.Drawing.Size(333, 153);
            this.log.TabIndex = 12;
            // 
            // sendText
            // 
            this.sendText.Enabled = false;
            this.sendText.Location = new System.Drawing.Point(15, 51);
            this.sendText.Name = "sendText";
            this.sendText.Size = new System.Drawing.Size(334, 35);
            this.sendText.TabIndex = 13;
            this.sendText.Text = "Wyślij tekst na wybranym PortVPIVCI";
            this.sendText.UseVisualStyleBackColor = true;
            this.sendText.Click += new System.EventHandler(this.sendMessage);
            // 
            // connectToCloudButton
            // 
            this.connectToCloudButton.Location = new System.Drawing.Point(371, 92);
            this.connectToCloudButton.Name = "connectToCloudButton";
            this.connectToCloudButton.Size = new System.Drawing.Size(100, 44);
            this.connectToCloudButton.TabIndex = 19;
            this.connectToCloudButton.Text = "Połącz z chmurą";
            this.connectToCloudButton.UseVisualStyleBackColor = true;
            this.connectToCloudButton.Click += new System.EventHandler(this.connectToCloud);
            // 
            // cloudIPField
            // 
            this.cloudIPField.Location = new System.Drawing.Point(371, 25);
            this.cloudIPField.Name = "cloudIPField";
            this.cloudIPField.Size = new System.Drawing.Size(100, 20);
            this.cloudIPField.TabIndex = 20;
            this.cloudIPField.Text = "127.0.0.1";
            // 
            // managerIPField
            // 
            this.managerIPField.Location = new System.Drawing.Point(477, 27);
            this.managerIPField.Name = "managerIPField";
            this.managerIPField.Size = new System.Drawing.Size(100, 20);
            this.managerIPField.TabIndex = 21;
            this.managerIPField.Text = "127.0.0.1";
            // 
            // cloudPortField
            // 
            this.cloudPortField.Location = new System.Drawing.Point(371, 67);
            this.cloudPortField.Name = "cloudPortField";
            this.cloudPortField.Size = new System.Drawing.Size(100, 20);
            this.cloudPortField.TabIndex = 22;
            this.cloudPortField.Text = "13000";
            // 
            // managerPortField
            // 
            this.managerPortField.Location = new System.Drawing.Point(477, 66);
            this.managerPortField.Name = "managerPortField";
            this.managerPortField.Size = new System.Drawing.Size(100, 20);
            this.managerPortField.TabIndex = 23;
            this.managerPortField.Text = "8002";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(368, 141);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(75, 13);
            this.label7.TabIndex = 24;
            this.label7.Text = "Nazwa Klienta";
            // 
            // usernameField
            // 
            this.usernameField.Location = new System.Drawing.Point(371, 157);
            this.usernameField.Name = "usernameField";
            this.usernameField.Size = new System.Drawing.Size(100, 20);
            this.usernameField.TabIndex = 26;
            // 
            // setUsernameButton
            // 
            this.setUsernameButton.Location = new System.Drawing.Point(371, 183);
            this.setUsernameButton.Name = "setUsernameButton";
            this.setUsernameButton.Size = new System.Drawing.Size(100, 44);
            this.setUsernameButton.TabIndex = 27;
            this.setUsernameButton.Text = "Ustal nazwę klienta";
            this.setUsernameButton.UseVisualStyleBackColor = true;
            this.setUsernameButton.Click += new System.EventHandler(this.setUsernameButton_Click);
            // 
            // getOtherClients
            // 
            this.getOtherClients.Location = new System.Drawing.Point(371, 233);
            this.getOtherClients.Name = "getOtherClients";
            this.getOtherClients.Size = new System.Drawing.Size(100, 44);
            this.getOtherClients.TabIndex = 28;
            this.getOtherClients.Text = "Pobierz nazwy innych klientów";
            this.getOtherClients.UseVisualStyleBackColor = true;
            this.getOtherClients.Click += new System.EventHandler(this.getOtherClients_Click);
            // 
            // connectWithClientButton
            // 
            this.connectWithClientButton.Location = new System.Drawing.Point(477, 183);
            this.connectWithClientButton.Name = "connectWithClientButton";
            this.connectWithClientButton.Size = new System.Drawing.Size(100, 44);
            this.connectWithClientButton.TabIndex = 31;
            this.connectWithClientButton.Text = "Połącz z klientem";
            this.connectWithClientButton.UseVisualStyleBackColor = true;
            this.connectWithClientButton.Click += new System.EventHandler(this.connectWithClientButton_Click);
            // 
            // disconnectWithClient
            // 
            this.disconnectWithClient.Location = new System.Drawing.Point(477, 233);
            this.disconnectWithClient.Name = "disconnectWithClient";
            this.disconnectWithClient.Size = new System.Drawing.Size(100, 44);
            this.disconnectWithClient.TabIndex = 32;
            this.disconnectWithClient.Text = "Rozłącz z klientem";
            this.disconnectWithClient.UseVisualStyleBackColor = true;
            this.disconnectWithClient.Click += new System.EventHandler(this.disconnectWithClient_Click);
            // 
            // DisconnectButton
            // 
            this.DisconnectButton.Location = new System.Drawing.Point(243, 251);
            this.DisconnectButton.Name = "DisconnectButton";
            this.DisconnectButton.Size = new System.Drawing.Size(106, 26);
            this.DisconnectButton.TabIndex = 40;
            this.DisconnectButton.Text = "Rozłącz z siecią";
            this.DisconnectButton.UseVisualStyleBackColor = true;
            this.DisconnectButton.Click += new System.EventHandler(this.DisconnectButton_Click);
            // 
            // SaveConfigButton
            // 
            this.SaveConfigButton.Location = new System.Drawing.Point(15, 251);
            this.SaveConfigButton.Name = "SaveConfigButton";
            this.SaveConfigButton.Size = new System.Drawing.Size(109, 26);
            this.SaveConfigButton.TabIndex = 41;
            this.SaveConfigButton.Text = "Zapisz konfigurację";
            this.SaveConfigButton.UseVisualStyleBackColor = true;
            this.SaveConfigButton.Click += new System.EventHandler(this.SaveConfigButton_Click);
            // 
            // ClientHostNumberField
            // 
            this.ClientHostNumberField.Location = new System.Drawing.Point(666, 199);
            this.ClientHostNumberField.Name = "ClientHostNumberField";
            this.ClientHostNumberField.Size = new System.Drawing.Size(34, 20);
            this.ClientHostNumberField.TabIndex = 46;
            // 
            // ClientSubnetworkNumberField
            // 
            this.ClientSubnetworkNumberField.Location = new System.Drawing.Point(624, 199);
            this.ClientSubnetworkNumberField.Name = "ClientSubnetworkNumberField";
            this.ClientSubnetworkNumberField.Size = new System.Drawing.Size(36, 20);
            this.ClientSubnetworkNumberField.TabIndex = 45;
            // 
            // setClientNumber
            // 
            this.setClientNumber.Location = new System.Drawing.Point(584, 233);
            this.setClientNumber.Name = "setClientNumber";
            this.setClientNumber.Size = new System.Drawing.Size(115, 44);
            this.setClientNumber.TabIndex = 44;
            this.setClientNumber.Text = "Ustal numer klienta";
            this.setClientNumber.UseVisualStyleBackColor = true;
            this.setClientNumber.Click += new System.EventHandler(this.setClientNumber_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(583, 183);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(69, 13);
            this.label8.TabIndex = 43;
            this.label8.Text = "Adres Klienta";
            // 
            // ClientNetworkNumberField
            // 
            this.ClientNetworkNumberField.Location = new System.Drawing.Point(582, 199);
            this.ClientNetworkNumberField.Name = "ClientNetworkNumberField";
            this.ClientNetworkNumberField.Size = new System.Drawing.Size(36, 20);
            this.ClientNetworkNumberField.TabIndex = 42;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(474, 141);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(73, 13);
            this.label6.TabIndex = 29;
            this.label6.Text = "Klienty w sieci";
            // 
            // selectedClientBox
            // 
            this.selectedClientBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selectedClientBox.FormattingEnabled = true;
            this.selectedClientBox.Location = new System.Drawing.Point(477, 155);
            this.selectedClientBox.Name = "selectedClientBox";
            this.selectedClientBox.Size = new System.Drawing.Size(100, 21);
            this.selectedClientBox.TabIndex = 30;
            this.selectedClientBox.SelectedIndexChanged += new System.EventHandler(this.selectedClientBoxs_SelectedIndexChanged);
            // 
            // controlCloudPortTextBox
            // 
            this.controlCloudPortTextBox.Location = new System.Drawing.Point(582, 66);
            this.controlCloudPortTextBox.Name = "controlCloudPortTextBox";
            this.controlCloudPortTextBox.Size = new System.Drawing.Size(118, 20);
            this.controlCloudPortTextBox.TabIndex = 51;
            this.controlCloudPortTextBox.Text = "14000";
            // 
            // portLabel
            // 
            this.portLabel.AutoSize = true;
            this.portLabel.Location = new System.Drawing.Point(582, 50);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(117, 13);
            this.portLabel.TabIndex = 50;
            this.portLabel.Text = "Port chmury sterowania";
            // 
            // conLabel
            // 
            this.conLabel.AutoSize = true;
            this.conLabel.Location = new System.Drawing.Point(581, 11);
            this.conLabel.Name = "conLabel";
            this.conLabel.Size = new System.Drawing.Size(108, 13);
            this.conLabel.TabIndex = 49;
            this.conLabel.Text = "IP chmury sterowania";
            // 
            // controlCloudIPTextBox
            // 
            this.controlCloudIPTextBox.Location = new System.Drawing.Point(582, 27);
            this.controlCloudIPTextBox.Name = "controlCloudIPTextBox";
            this.controlCloudIPTextBox.Size = new System.Drawing.Size(118, 20);
            this.controlCloudIPTextBox.TabIndex = 48;
            this.controlCloudIPTextBox.Text = "127.0.0.1";
            // 
            // conToCloudButton
            // 
            this.conToCloudButton.Location = new System.Drawing.Point(582, 92);
            this.conToCloudButton.Name = "conToCloudButton";
            this.conToCloudButton.Size = new System.Drawing.Size(118, 43);
            this.conToCloudButton.TabIndex = 47;
            this.conToCloudButton.Text = "Połącz z chmurą sterowania";
            this.conToCloudButton.UseVisualStyleBackColor = true;
            this.conToCloudButton.Click += new System.EventHandler(this.conToCloudButton_Click);
            // 
            // clientSpeedBox
            // 
            this.clientSpeedBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.clientSpeedBox.FormattingEnabled = true;
            this.clientSpeedBox.Location = new System.Drawing.Point(585, 157);
            this.clientSpeedBox.Name = "clientSpeedBox";
            this.clientSpeedBox.Size = new System.Drawing.Size(115, 21);
            this.clientSpeedBox.TabIndex = 52;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(582, 141);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(96, 13);
            this.label9.TabIndex = 53;
            this.label9.Text = "Prędkość w Mbit/s";
            // 
            // chooseTextFile
            // 
            this.chooseTextFile.Location = new System.Drawing.Point(130, 251);
            this.chooseTextFile.Name = "chooseTextFile";
            this.chooseTextFile.Size = new System.Drawing.Size(107, 26);
            this.chooseTextFile.TabIndex = 54;
            this.chooseTextFile.Text = "Wczytaj config";
            this.chooseTextFile.UseVisualStyleBackColor = true;
            this.chooseTextFile.Click += new System.EventHandler(this.chooseTextFile_Click);
            // 
            // howToSendComboBox
            // 
            this.howToSendComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.howToSendComboBox.FormattingEnabled = true;
            this.howToSendComboBox.Location = new System.Drawing.Point(220, 24);
            this.howToSendComboBox.Name = "howToSendComboBox";
            this.howToSendComboBox.Size = new System.Drawing.Size(129, 21);
            this.howToSendComboBox.TabIndex = 55;
            // 
            // Clientix
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(710, 285);
            this.Controls.Add(this.howToSendComboBox);
            this.Controls.Add(this.chooseTextFile);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.clientSpeedBox);
            this.Controls.Add(this.controlCloudPortTextBox);
            this.Controls.Add(this.portLabel);
            this.Controls.Add(this.conLabel);
            this.Controls.Add(this.controlCloudIPTextBox);
            this.Controls.Add(this.conToCloudButton);
            this.Controls.Add(this.ClientHostNumberField);
            this.Controls.Add(this.ClientSubnetworkNumberField);
            this.Controls.Add(this.setClientNumber);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.ClientNetworkNumberField);
            this.Controls.Add(this.SaveConfigButton);
            this.Controls.Add(this.DisconnectButton);
            this.Controls.Add(this.disconnectWithClient);
            this.Controls.Add(this.connectWithClientButton);
            this.Controls.Add(this.selectedClientBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.getOtherClients);
            this.Controls.Add(this.setUsernameButton);
            this.Controls.Add(this.usernameField);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.managerPortField);
            this.Controls.Add(this.cloudPortField);
            this.Controls.Add(this.managerIPField);
            this.Controls.Add(this.cloudIPField);
            this.Controls.Add(this.connectToCloudButton);
            this.Controls.Add(this.sendText);
            this.Controls.Add(this.log);
            this.Controls.Add(this.connectToManagerButton);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.enteredTextField);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(601, 323);
            this.Name = "Clientix";
            this.Text = "Clientix";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Clientix_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox enteredTextField;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button connectToManagerButton;
        private volatile System.Windows.Forms.TextBox log;
        private System.Windows.Forms.Button sendText;
        private System.Windows.Forms.Button connectToCloudButton;
        private System.Windows.Forms.TextBox cloudIPField;
        private System.Windows.Forms.TextBox managerIPField;
        private System.Windows.Forms.TextBox cloudPortField;
        private System.Windows.Forms.TextBox managerPortField;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox usernameField;
        private System.Windows.Forms.Button setUsernameButton;
        private System.Windows.Forms.Button getOtherClients;
        private System.Windows.Forms.Button connectWithClientButton;
        private System.Windows.Forms.Button disconnectWithClient;
        private System.Windows.Forms.Button DisconnectButton;
        private System.Windows.Forms.Button SaveConfigButton;
        private System.Windows.Forms.TextBox ClientHostNumberField;
        private System.Windows.Forms.TextBox ClientSubnetworkNumberField;
        private System.Windows.Forms.Button setClientNumber;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox ClientNetworkNumberField;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox selectedClientBox;
        private System.Windows.Forms.TextBox controlCloudPortTextBox;
        private System.Windows.Forms.Label portLabel;
        private System.Windows.Forms.Label conLabel;
        private System.Windows.Forms.TextBox controlCloudIPTextBox;
        private System.Windows.Forms.Button conToCloudButton;
        private System.Windows.Forms.ComboBox clientSpeedBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button chooseTextFile;
        private System.Windows.Forms.ComboBox howToSendComboBox;
    }
}


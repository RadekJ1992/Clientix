namespace Clientix {
    partial class Client {
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
            this.enteredTextField = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.CloudIPField = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.CloudPortField = new System.Windows.Forms.TextBox();
            this.ManagerPortField = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.ManagerIPField = new System.Windows.Forms.TextBox();
            this.connectToCloud = new System.Windows.Forms.Button();
            this.connectToManager = new System.Windows.Forms.Button();
            this.log = new System.Windows.Forms.TextBox();
            this.sendText = new System.Windows.Forms.Button();
            this.clientList = new System.Windows.Forms.CheckedListBox();
            this.SuspendLayout();
            // 
            // enteredTextField
            // 
            this.enteredTextField.Location = new System.Drawing.Point(15, 25);
            this.enteredTextField.Name = "enteredTextField";
            this.enteredTextField.Size = new System.Drawing.Size(334, 20);
            this.enteredTextField.TabIndex = 0;
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
            // CloudIPField
            // 
            this.CloudIPField.Location = new System.Drawing.Point(368, 25);
            this.CloudIPField.Name = "CloudIPField";
            this.CloudIPField.Size = new System.Drawing.Size(125, 20);
            this.CloudIPField.TabIndex = 2;
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
            this.label3.Location = new System.Drawing.Point(368, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(108, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Port chmury kablowej";
            // 
            // CloudPortField
            // 
            this.CloudPortField.Location = new System.Drawing.Point(373, 74);
            this.CloudPortField.Name = "CloudPortField";
            this.CloudPortField.Size = new System.Drawing.Size(120, 20);
            this.CloudPortField.TabIndex = 5;
            // 
            // ManagerPortField
            // 
            this.ManagerPortField.Location = new System.Drawing.Point(373, 233);
            this.ManagerPortField.Name = "ManagerPortField";
            this.ManagerPortField.Size = new System.Drawing.Size(120, 20);
            this.ManagerPortField.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(368, 217);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Port zarządcy";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(368, 169);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(62, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "IP zarządcy";
            // 
            // ManagerIPField
            // 
            this.ManagerIPField.Location = new System.Drawing.Point(368, 184);
            this.ManagerIPField.Name = "ManagerIPField";
            this.ManagerIPField.Size = new System.Drawing.Size(125, 20);
            this.ManagerIPField.TabIndex = 6;
            // 
            // connectToCloud
            // 
            this.connectToCloud.Location = new System.Drawing.Point(373, 108);
            this.connectToCloud.Name = "connectToCloud";
            this.connectToCloud.Size = new System.Drawing.Size(119, 48);
            this.connectToCloud.TabIndex = 10;
            this.connectToCloud.Text = "Połącz z chmurą";
            this.connectToCloud.UseVisualStyleBackColor = true;
            // 
            // connectToManager
            // 
            this.connectToManager.Location = new System.Drawing.Point(373, 271);
            this.connectToManager.Name = "connectToManager";
            this.connectToManager.Size = new System.Drawing.Size(119, 48);
            this.connectToManager.TabIndex = 11;
            this.connectToManager.Text = "Połącz z zarządcą";
            this.connectToManager.UseVisualStyleBackColor = true;
            // 
            // log
            // 
            this.log.BackColor = System.Drawing.SystemColors.Window;
            this.log.Location = new System.Drawing.Point(16, 92);
            this.log.Multiline = true;
            this.log.Name = "log";
            this.log.ReadOnly = true;
            this.log.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.log.Size = new System.Drawing.Size(333, 232);
            this.log.TabIndex = 12;
            // 
            // sendText
            // 
            this.sendText.Location = new System.Drawing.Point(15, 51);
            this.sendText.Name = "sendText";
            this.sendText.Size = new System.Drawing.Size(333, 35);
            this.sendText.TabIndex = 13;
            this.sendText.Text = "Wyślij tekst do zaznaczonego klienta";
            this.sendText.UseVisualStyleBackColor = true;
            // 
            // clientList
            // 
            this.clientList.FormattingEnabled = true;
            this.clientList.Location = new System.Drawing.Point(504, 16);
            this.clientList.Name = "clientList";
            this.clientList.Size = new System.Drawing.Size(145, 304);
            this.clientList.TabIndex = 14;
            // 
            // Client
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(662, 337);
            this.Controls.Add(this.clientList);
            this.Controls.Add(this.sendText);
            this.Controls.Add(this.log);
            this.Controls.Add(this.connectToManager);
            this.Controls.Add(this.connectToCloud);
            this.Controls.Add(this.ManagerPortField);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.ManagerIPField);
            this.Controls.Add(this.CloudPortField);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.CloudIPField);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.enteredTextField);
            this.Name = "Client";
            this.Text = "Client";
            this.Load += new System.EventHandler(this.Client_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox enteredTextField;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox CloudIPField;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox CloudPortField;
        private System.Windows.Forms.TextBox ManagerPortField;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox ManagerIPField;
        private System.Windows.Forms.Button connectToCloud;
        private System.Windows.Forms.Button connectToManager;
        private System.Windows.Forms.TextBox log;
        private System.Windows.Forms.Button sendText;
        private System.Windows.Forms.CheckedListBox clientList;
    }
}


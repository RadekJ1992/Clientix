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
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.connectToManagerButton = new System.Windows.Forms.Button();
            this.log = new System.Windows.Forms.TextBox();
            this.sendText = new System.Windows.Forms.Button();
            this.clientList = new System.Windows.Forms.CheckedListBox();
            this.connectToCloudButton = new System.Windows.Forms.Button();
            this.cloudIPField = new System.Windows.Forms.TextBox();
            this.managerIPField = new System.Windows.Forms.TextBox();
            this.cloudPortField = new System.Windows.Forms.TextBox();
            this.managerPortField = new System.Windows.Forms.TextBox();
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
            this.label4.Location = new System.Drawing.Point(370, 182);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Port zarządcy";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(368, 143);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(62, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "IP zarządcy";
            // 
            // connectToManagerButton
            // 
            this.connectToManagerButton.Location = new System.Drawing.Point(371, 224);
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
            this.log.Size = new System.Drawing.Size(333, 176);
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
            this.sendText.Click += new System.EventHandler(this.sendMessage);
            // 
            // clientList
            // 
            this.clientList.FormattingEnabled = true;
            this.clientList.Location = new System.Drawing.Point(482, 9);
            this.clientList.Name = "clientList";
            this.clientList.Size = new System.Drawing.Size(142, 259);
            this.clientList.TabIndex = 14;
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
            this.cloudIPField.Location = new System.Drawing.Point(371, 28);
            this.cloudIPField.Name = "cloudIPField";
            this.cloudIPField.Size = new System.Drawing.Size(100, 20);
            this.cloudIPField.TabIndex = 20;
            // 
            // managerIPField
            // 
            this.managerIPField.Location = new System.Drawing.Point(371, 159);
            this.managerIPField.Name = "managerIPField";
            this.managerIPField.Size = new System.Drawing.Size(100, 20);
            this.managerIPField.TabIndex = 21;
            // 
            // cloudPortField
            // 
            this.cloudPortField.Location = new System.Drawing.Point(371, 67);
            this.cloudPortField.Name = "cloudPortField";
            this.cloudPortField.Size = new System.Drawing.Size(100, 20);
            this.cloudPortField.TabIndex = 22;
            // 
            // managerPortField
            // 
            this.managerPortField.Location = new System.Drawing.Point(371, 198);
            this.managerPortField.Name = "managerPortField";
            this.managerPortField.Size = new System.Drawing.Size(100, 20);
            this.managerPortField.TabIndex = 23;
            // 
            // Client
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(636, 279);
            this.Controls.Add(this.managerPortField);
            this.Controls.Add(this.cloudPortField);
            this.Controls.Add(this.managerIPField);
            this.Controls.Add(this.cloudIPField);
            this.Controls.Add(this.connectToCloudButton);
            this.Controls.Add(this.clientList);
            this.Controls.Add(this.sendText);
            this.Controls.Add(this.log);
            this.Controls.Add(this.connectToManagerButton);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.enteredTextField);
            this.Name = "Client";
            this.Text = "Client";
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
        private System.Windows.Forms.TextBox log;
        private System.Windows.Forms.Button sendText;
        private System.Windows.Forms.CheckedListBox clientList;
        private System.Windows.Forms.Button connectToCloudButton;
        private System.Windows.Forms.TextBox cloudIPField;
        private System.Windows.Forms.TextBox managerIPField;
        private System.Windows.Forms.TextBox cloudPortField;
        private System.Windows.Forms.TextBox managerPortField;
    }
}


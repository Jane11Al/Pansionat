namespace Pasionat
{
    partial class SubjAreaForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGridViewSubjectAreas = new System.Windows.Forms.DataGridView();
            this.label29 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonEdit = new System.Windows.Forms.Button();
            this.textBoxAreaName = new System.Windows.Forms.TextBox();
            this.textBoxAreaCode = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSubjectAreas)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridViewSubjectAreas
            // 
            this.dataGridViewSubjectAreas.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewSubjectAreas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewSubjectAreas.Location = new System.Drawing.Point(26, 28);
            this.dataGridViewSubjectAreas.Name = "dataGridViewSubjectAreas";
            this.dataGridViewSubjectAreas.RowHeadersWidth = 51;
            this.dataGridViewSubjectAreas.RowTemplate.Height = 24;
            this.dataGridViewSubjectAreas.Size = new System.Drawing.Size(1354, 204);
            this.dataGridViewSubjectAreas.TabIndex = 54;
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label29.Location = new System.Drawing.Point(242, 278);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(53, 25);
            this.label29.TabIndex = 56;
            this.label29.Text = "Код:";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label25.Location = new System.Drawing.Point(190, 333);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(105, 25);
            this.label25.TabIndex = 58;
            this.label25.Text = "Название:";
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(1208, 260);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(172, 43);
            this.buttonAdd.TabIndex = 60;
            this.buttonAdd.Text = "Добавить";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // buttonDelete
            // 
            this.buttonDelete.Location = new System.Drawing.Point(1208, 313);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(172, 44);
            this.buttonDelete.TabIndex = 61;
            this.buttonDelete.Text = "Удалить";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(1208, 427);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(172, 44);
            this.buttonSave.TabIndex = 62;
            this.buttonSave.Text = "Сохранить";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonEdit
            // 
            this.buttonEdit.Location = new System.Drawing.Point(1208, 370);
            this.buttonEdit.Name = "buttonEdit";
            this.buttonEdit.Size = new System.Drawing.Size(172, 44);
            this.buttonEdit.TabIndex = 63;
            this.buttonEdit.Text = "Изменить";
            this.buttonEdit.UseVisualStyleBackColor = true;
            this.buttonEdit.Click += new System.EventHandler(this.buttonEdit_Click);
            // 
            // textBoxAreaName
            // 
            this.textBoxAreaName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBoxAreaName.Location = new System.Drawing.Point(341, 333);
            this.textBoxAreaName.Name = "textBoxAreaName";
            this.textBoxAreaName.Size = new System.Drawing.Size(308, 30);
            this.textBoxAreaName.TabIndex = 65;
            // 
            // textBoxAreaCode
            // 
            this.textBoxAreaCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBoxAreaCode.Location = new System.Drawing.Point(341, 278);
            this.textBoxAreaCode.Name = "textBoxAreaCode";
            this.textBoxAreaCode.Size = new System.Drawing.Size(308, 30);
            this.textBoxAreaCode.TabIndex = 68;
            // 
            // SubjAreaForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1414, 643);
            this.Controls.Add(this.textBoxAreaCode);
            this.Controls.Add(this.textBoxAreaName);
            this.Controls.Add(this.buttonEdit);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.label25);
            this.Controls.Add(this.label29);
            this.Controls.Add(this.dataGridViewSubjectAreas);
            this.Name = "SubjAreaForm";
            this.Text = "Сведения о напрвлениях развития";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSubjectAreas)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridViewSubjectAreas;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonEdit;
        private System.Windows.Forms.TextBox textBoxAreaName;
        private System.Windows.Forms.TextBox textBoxAreaCode;
    }
}
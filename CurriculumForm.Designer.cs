namespace Pasionat
{
    partial class CurriculumForm
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
            this.txtProgramName = new System.Windows.Forms.TextBox();
            this.label24 = new System.Windows.Forms.Label();
            this.txtProgramCode = new System.Windows.Forms.TextBox();
            this.label27 = new System.Windows.Forms.Label();
            this.dataGridViewPrograms = new System.Windows.Forms.DataGridView();
            this.buttonEdit = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPrograms)).BeginInit();
            this.SuspendLayout();
            // 
            // txtProgramName
            // 
            this.txtProgramName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtProgramName.Location = new System.Drawing.Point(327, 321);
            this.txtProgramName.Name = "txtProgramName";
            this.txtProgramName.Size = new System.Drawing.Size(308, 30);
            this.txtProgramName.TabIndex = 59;
            this.txtProgramName.TextChanged += new System.EventHandler(this.txtProgramName_TextChanged);
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label24.Location = new System.Drawing.Point(176, 326);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(105, 25);
            this.label24.TabIndex = 58;
            this.label24.Text = "Название:";
            this.label24.Click += new System.EventHandler(this.label24_Click);
            // 
            // txtProgramCode
            // 
            this.txtProgramCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtProgramCode.Location = new System.Drawing.Point(327, 266);
            this.txtProgramCode.Name = "txtProgramCode";
            this.txtProgramCode.Size = new System.Drawing.Size(308, 30);
            this.txtProgramCode.TabIndex = 57;
            this.txtProgramCode.TextChanged += new System.EventHandler(this.txtProgramCode_TextChanged);
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label27.Location = new System.Drawing.Point(153, 271);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(168, 25);
            this.label27.TabIndex = 56;
            this.label27.Text = "Код программы:";
            this.label27.Click += new System.EventHandler(this.label27_Click);
            // 
            // dataGridViewPrograms
            // 
            this.dataGridViewPrograms.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewPrograms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewPrograms.Location = new System.Drawing.Point(12, 21);
            this.dataGridViewPrograms.Name = "dataGridViewPrograms";
            this.dataGridViewPrograms.RowHeadersWidth = 51;
            this.dataGridViewPrograms.RowTemplate.Height = 24;
            this.dataGridViewPrograms.Size = new System.Drawing.Size(1354, 204);
            this.dataGridViewPrograms.TabIndex = 54;
            // 
            // buttonEdit
            // 
            this.buttonEdit.Location = new System.Drawing.Point(1153, 350);
            this.buttonEdit.Name = "buttonEdit";
            this.buttonEdit.Size = new System.Drawing.Size(172, 44);
            this.buttonEdit.TabIndex = 53;
            this.buttonEdit.Text = "Изменить";
            this.buttonEdit.UseVisualStyleBackColor = true;
            this.buttonEdit.Click += new System.EventHandler(this.buttonEdit_Click_1);
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(1153, 407);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(172, 44);
            this.buttonSave.TabIndex = 52;
            this.buttonSave.Text = "Сохранить";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click_1);
            // 
            // buttonDelete
            // 
            this.buttonDelete.Location = new System.Drawing.Point(1153, 293);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(172, 44);
            this.buttonDelete.TabIndex = 51;
            this.buttonDelete.Text = "Удалить";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click_1);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(1153, 240);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(172, 43);
            this.buttonAdd.TabIndex = 50;
            this.buttonAdd.Text = "Добавить";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click_1);
            // 
            // CurriculumForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1394, 659);
            this.Controls.Add(this.txtProgramName);
            this.Controls.Add(this.label24);
            this.Controls.Add(this.txtProgramCode);
            this.Controls.Add(this.label27);
            this.Controls.Add(this.dataGridViewPrograms);
            this.Controls.Add(this.buttonEdit);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.buttonAdd);
            this.Name = "CurriculumForm";
            this.Text = "Сведения о программе";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPrograms)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtProgramName;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.TextBox txtProgramCode;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.DataGridView dataGridViewPrograms;
        private System.Windows.Forms.Button buttonEdit;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Button buttonAdd;
    }
}
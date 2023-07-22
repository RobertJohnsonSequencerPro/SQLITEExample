
using System.Data;
using System.Data.SQLite;


namespace SQLITE
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            dbFullPath = Path + "\\" + dbFileName;
            ConnString = string.Format("Data Source = {0}", dbFullPath);
            CreateDB();
            System.Data.DataTable dtData = new System.Data.DataTable();
            PopulateADataTable(ref dtData, "dtUsers");
            dgvData.DataSource = dtData;
        }

        private void Form1_Load(object sender, EventArgs e)
        {


        }

        public enum DataTypes
        {
            //This enum is for use in conjuction with other enums where the SQL data type needs to be attached to each item in the enum.
            //For example, when creating a data table from an enum, these 3 letter prefixes can be added to each enum item
            //in order to designate the intended SQL data type for the column in the data table. See the enum dtAccounts for an example.
            //txt is TEXT, boo is BIT, num is REAL (convert to needed type at point of use) and dat is DATETIME2.
            txt,
            boo,
            num,
            dat
        }

        public enum dtUsers
        {
            txtFirstName,
            txtLastName,
            txtUserID,
            booGoodAtComedy,
            datRecordCreated
        }

        string Path = "C:\\ProgramData\\SQLITETEST";
        string dbFileName = "SQLITETEST.db";
        string dbFullPath;
        string ConnString;
        SQLiteDataAdapter dbAdapter = new SQLiteDataAdapter();

        public void CreateDatabaseTableFromEnum(Type Enumeration)
        {

            try
            {
                string query = "CREATE table if Not EXISTS '" + Enumeration.Name + "' ('txtGUID' STRING PRIMARY KEY UNIQUE, 'booIsDeleted' INTEGER, 'datRecordCreated' STRING);";
                SQLiteConnection conn = new SQLiteConnection(ConnString);
                SQLiteCommand cmd = new SQLiteCommand(query, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                string[] EnumItems = Enum.GetNames(Enumeration);
                foreach (string item in EnumItems)
                {
                    AddAColumnToATableInDB(Enumeration.Name, item);
                }
                AddDataToDataBase();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in clsDBComm.CreateDatabaseTableFromEnum. Error is: " + ex.Message);
            }
        }

        public void AddDataToDataBase()
        {
            System.Data.DataTable dtData = new System.Data.DataTable();
            PopulateADataTable(ref dtData, "dtUsers");
            foreach (DataRow Row in dtData.Rows)
            {
                if (Row[dtUsers.txtFirstName.ToString()].ToString() == "Robert")
                {
                    break;
                }
            }

            DataRow dr = dtData.NewRow();
            dr[dtUsers.txtFirstName.ToString()] = "Robert";
            dr[dtUsers.txtLastName.ToString()] = "Johnson";
            dr[dtUsers.txtUserID.ToString()] = "Corban 2.0";
            dr[dtUsers.booGoodAtComedy.ToString()] = false;
            dr[dtUsers.datRecordCreated.ToString()] = DateTime.Now;
            dtData.Rows.Add(dr);
            SaveADataTable(dtData, "dtUsers");


            System.Data.DataTable dataTable = new System.Data.DataTable();
            PopulateADataTable(ref dataTable, "dtUsers");
            MessageBox.Show(dataTable.Rows.Count.ToString());
        }

        /// <summary>
        /// The first three letters of the ColumnName indicate the intended data type. txt = string, boo = boolean, etc.
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="ColumnName"></param>
        public void AddAColumnToATableInDB(string TableName, string ColumnName)
        {
            try
            {
                SQLiteConnection conn = new SQLiteConnection(ConnString);
                conn.Open();
                System.Data.DataTable dt = new System.Data.DataTable();
                string query = "SELECT * FROM '" + TableName + "'";
                SQLiteDataAdapter dbAdapter = new SQLiteDataAdapter(query, ConnString);
                SQLiteCommandBuilder cmdBuilder = new SQLiteCommandBuilder(dbAdapter);
                dbAdapter.Fill(dt);
                if (dt.Columns.Contains(ColumnName))
                {
                    //The column already exists so no need to add it.
                    return;
                }
                string DataType = ColumnName.Substring(0, 3);
                string AddColumnQuery = "";

                if (DataType == DataTypes.txt.ToString())
                {
                    AddColumnQuery = "ALTER TABLE " + TableName + " ADD COLUMN '" + ColumnName + "' " + "TEXT";
                }
                else
                {
                    if (DataType == DataTypes.boo.ToString())
                    {
                        AddColumnQuery = "ALTER TABLE " + TableName + " ADD COLUMN '" + ColumnName + "' " + "BIT";
                    }
                    else
                    {
                        if (DataType == DataTypes.num.ToString())
                        {
                            AddColumnQuery = "ALTER TABLE " + TableName + " ADD COLUMN '" + ColumnName + "' " + "REAL";
                        }
                        else
                        {
                            if (DataType == DataTypes.dat.ToString())
                            {
                                AddColumnQuery = "ALTER TABLE " + TableName + " ADD COLUMN '" + ColumnName + "' " + "DATETIME2";
                            }
                            else
                            {
                                MessageBox.Show("Error in clsDBComm.AddAColumnToATableInDB. DataType " + DataType + " not recognized.");
                                return;
                            }
                        }
                    }
                }

                SQLiteCommand cmd = new SQLiteCommand(AddColumnQuery, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in clsDBComm.AddAColumnToATableInDB. Error is " + ex.Message);
            }
        }


        public void CreateInMemoryDataTableFromEnum(Type Enumeration)
        {

        }

        public void CreateDB()
        {
            CreateDirectory(Path);

            if (!System.IO.File.Exists(dbFullPath))
            {
                SQLiteConnection.CreateFile(dbFullPath);
            }
            CreateDatabaseTableFromEnum(typeof(dtUsers));
        }

        public void PopulateADataTable(ref System.Data.DataTable dtTable, string TableNameInDB)
        {
            try
            {
                SQLiteConnection SQLiteConn = new SQLiteConnection(ConnString);
                SQLiteConn.Open();
                SQLiteDataAdapter dbAdapter = new SQLiteDataAdapter("SELECT * FROM " + TableNameInDB, SQLiteConn);
                SQLiteCommandBuilder cmdBuilder = new SQLiteCommandBuilder(dbAdapter);
                dbAdapter.Fill(dtTable);
                SQLiteConn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in clsDBComm.PopulateADataTable. TableNameInDB = " + TableNameInDB + ". Error is " + ex.Message);
            }
        }

        public void PopulateADataTable(ref System.Data.DataTable dtTable, string TableNameInDB, string FirstFilterColumnName, string FirstFilterValue)
        {
            try
            {
                SQLiteConnection SQLiteConn = new SQLiteConnection(ConnString);
                SQLiteConn.Open();
                SQLiteDataAdapter dbAdapter = new SQLiteDataAdapter("SELECT * FROM " + TableNameInDB + " WHERE " + FirstFilterColumnName + " = '" + FirstFilterValue + "'", SQLiteConn);
                SQLiteCommandBuilder cmdBuilder = new SQLiteCommandBuilder(dbAdapter);
                dbAdapter.Fill(dtTable);
                SQLiteConn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in clsDBComm.PopulateADataTable. TableNameInDB = " + TableNameInDB + ". Error is " + ex.Message);
            }
        }

        public void PopulateADataTable(ref System.Data.DataTable dtTable, string TableNameInDB, string FirstFilterColumnName, string FirstFilterValue, string SecondFilterColumnName, string SecondFilterValue)
        {
            try
            {
                SQLiteConnection SQLiteConn = new SQLiteConnection(ConnString);
                SQLiteConn.Open();
                SQLiteDataAdapter dbAdapter = new SQLiteDataAdapter("SELECT * FROM " + TableNameInDB + " WHERE " + FirstFilterColumnName + " = '" + FirstFilterValue + "' AND " + SecondFilterColumnName + " = '" + SecondFilterValue + "'", SQLiteConn);
                SQLiteCommandBuilder cmdBuilder = new SQLiteCommandBuilder(dbAdapter);
                dbAdapter.Fill(dtTable);
                SQLiteConn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in clsDBComm.PopulateADataTable. TableNameInDB = " + TableNameInDB + ". Error is " + ex.Message);
            }
        }

        public void PopulateADataTable(ref System.Data.DataTable dtTable, string TableNameInDB, string FirstFilterColumnName, string FirstFilterValue, string SecondFilterColumnName, string SecondFilterValue, string ThirdFilterColumnName, string ThirdFilterValue)
        {
            try
            {
                SQLiteConnection SQLiteConn = new SQLiteConnection(ConnString);
                SQLiteConn.Open();
                SQLiteDataAdapter dbAdapter = new SQLiteDataAdapter("SELECT * FROM " + TableNameInDB + " WHERE " + FirstFilterColumnName + " = '" + FirstFilterValue + "' AND " + SecondFilterColumnName + " = '" + SecondFilterValue + "' AND " + ThirdFilterColumnName + " = '" + ThirdFilterValue + "'", SQLiteConn);
                SQLiteCommandBuilder cmdBuilder = new SQLiteCommandBuilder(dbAdapter);
                dbAdapter.Fill(dtTable);
                SQLiteConn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in clsDBComm.PopulateADataTable. TableNameInDB = " + TableNameInDB + ". Error is " + ex.Message);
            }
        }

        public void PopulateADataTable(ref System.Data.DataTable dtTable, string TableNameInDB, string FirstFilterColumnName, string FirstFilterValue, string SecondFilterColumnName, string SecondFilterValue, string ThirdFilterColumnName, string ThirdFilterValue, string FourthFilterColumnName, string FourthFilterValue)
        {
            try
            {
                SQLiteConnection SQLiteConn = new SQLiteConnection(ConnString);
                SQLiteConn.Open();
                SQLiteDataAdapter dbAdapter = new SQLiteDataAdapter("SELECT * FROM " + TableNameInDB + " WHERE " + FirstFilterColumnName + " = '" + FirstFilterValue + "' AND " + SecondFilterColumnName + " = '" + SecondFilterValue + "' AND " + ThirdFilterColumnName + " = '" + ThirdFilterValue + "' AND " + FourthFilterColumnName + " = '" + FourthFilterValue + "'", SQLiteConn);
                SQLiteCommandBuilder cmdBuilder = new SQLiteCommandBuilder(dbAdapter);
                dbAdapter.Fill(dtTable);
                SQLiteConn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in clsDBComm.PopulateADataTable. TableNameInDB = " + TableNameInDB + ". Error is " + ex.Message);
            }
        }

        public void SaveADataTable(System.Data.DataTable dtTable, string TableNameInDB)
        {
            try
            {
                SQLiteConnection SQLiteConn = new SQLiteConnection(ConnString);
                SQLiteConn.Open();
                SQLiteDataAdapter dbAdapter = new SQLiteDataAdapter("SELECT * FROM " + TableNameInDB, SQLiteConn);
                SQLiteCommandBuilder cmdBuilder = new SQLiteCommandBuilder(dbAdapter);
                dbAdapter.DeleteCommand = cmdBuilder.GetDeleteCommand();
                dbAdapter.InsertCommand = cmdBuilder.GetInsertCommand();
                dbAdapter.UpdateCommand = cmdBuilder.GetUpdateCommand();
                dbAdapter.Update(dtTable);
                SQLiteConn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in clsDBComm.SaveADataTable. TableNameInDB = " + TableNameInDB + ". Error is " + ex.Message);
            }
        }

        public void CreateDirectory(string Path)
        {
            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }
        }
    }
}
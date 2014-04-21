using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSeriesAnalysis
{
    public class CsvStreamReader
    {
        private ArrayList rowAL;         
        private string fileName;        

        private Encoding encoding;      

        public CsvStreamReader()
        {
            this.rowAL = new ArrayList();
            this.fileName = "";
            this.encoding = Encoding.Default;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="fileName">file name</param>
        public CsvStreamReader(string fileName)
        {
            this.rowAL = new ArrayList();
            this.fileName = fileName;
            this.encoding = Encoding.Default;
            LoadCsvFile();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="fileName">file name</param>
        /// <param name="encoding">file code</param>
        public CsvStreamReader(string fileName, Encoding encoding)
        {
            this.rowAL = new ArrayList();
            this.fileName = fileName;
            this.encoding = encoding;
            LoadCsvFile();
        }

        /// <summary>
        /// file name
        /// </summary>
        public string FileName
        {
            set
            {
                this.fileName = value;
                LoadCsvFile();
            }
        }

        /// <summary>
        /// file code
        /// </summary>

        public Encoding FileEncoding
        {
            set
            {
                this.encoding = value;
            }
        }

        /// <summary>
        /// get the row count
        /// </summary>
        public int RowCount
        {
            get
            {
                return this.rowAL.Count;
            }
        }

        /// <summary>
        /// get the col count
        /// </summary>
        public int ColCount
        {
            get
            {
                int maxCol;

                maxCol = 0;
                for (int i = 0; i < this.rowAL.Count; i++)
                {
                    ArrayList colAL = (ArrayList)this.rowAL[i];

                    maxCol = (maxCol > colAL.Count) ? maxCol : colAL.Count;
                }

                return maxCol;
            }
        }


        /// <summary>
        ///get the data

        /// </summary>
        public string this[int row, int col]
        {
            get
            {
                //varify the validity

                CheckRowValid(row);
                CheckColValid(col);
                ArrayList colAL = (ArrayList)this.rowAL[row - 1];


                if (colAL.Count < col)
                {
                    return "";
                }

                return colAL[col - 1].ToString();
            }
        }


        /// <summary>

        /// maxrow: -1
        /// maxcol: -1
        /// </summary>
        public DataTable this[int minRow, int maxRow, int minCol, int maxCol]
        {
            get
            {

                CheckRowValid(minRow);
                CheckMaxRowValid(maxRow);
                CheckColValid(minCol);
                CheckMaxColValid(maxCol);
                if (maxRow == -1)
                {
                    maxRow = RowCount;
                }
                if (maxCol == -1)
                {
                    maxCol = ColCount;
                }
                if (maxRow < minRow)
                {
                    throw new Exception("Wrong");
                }
                if (maxCol < minCol)
                {
                    throw new Exception("Wrong");
                }
                DataTable csvDT = new DataTable();
                int i;
                int col;
                int row;

                //add col

                for (i = minCol; i <= maxCol; i++)
                {
                    csvDT.Columns.Add(i.ToString());
                }
                for (row = minRow; row <= maxRow; row++)
                {
                    DataRow csvDR = csvDT.NewRow();

                    i = 0;
                    for (col = minCol; col <= maxCol; col++)
                    {
                        csvDR[i] = this[row, col];
                        i++;
                    }
                    csvDT.Rows.Add(csvDR);
                }

                return csvDT;
            }
        }


        /// <summary>

        /// </summary>
        /// <param name="col"></param>  
        private void CheckRowValid(int row)
        {
            if (row <= 0)
            {
                throw new Exception("wrong");
            }
            if (row > RowCount)
            {
                throw new Exception("wrong");
            }
        }

        /// <summary>
        /// validate max row 

        /// </summary>
        /// <param name="col"></param>  
        private void CheckMaxRowValid(int maxRow)
        {
            if (maxRow <= 0 && maxRow != -1)
            {
                throw new Exception("wrong");
            }
            if (maxRow > RowCount)
            {
                throw new Exception("wrong");
            }
        }

        /// <summary>

        /// </summary>
        /// <param name="col"></param>  
        private void CheckColValid(int col)
        {
            if (col <= 0)
            {
                throw new Exception("wrong");
            }
            if (col > ColCount)
            {
                throw new Exception("wrong");
            }
        }

        /// <summary>

        /// </summary>
        /// <param name="col"></param>  
        private void CheckMaxColValid(int maxCol)
        {
            if (maxCol <= 0 && maxCol != -1)
            {
                throw new Exception("wrong");
            }
            if (maxCol > ColCount)
            {
                throw new Exception("wrong");
            }
        }

        /// <summary>
        /// load CSV
        /// </summary>
        private void LoadCsvFile()
        {

            if (this.fileName == null)
            {
                throw new Exception("the file name is null");
            }
            else if (!File.Exists(this.fileName))
            {
                throw new Exception("wrong with the file");
            }
            else
            {
            }
            if (this.encoding == null)
            {
                this.encoding = Encoding.Default;
            }

            StreamReader sr = new StreamReader(this.fileName, this.encoding);
            string csvDataLine;

            csvDataLine = "";
            while (true)
            {
                string fileDataLine;

                fileDataLine = sr.ReadLine();
                if (fileDataLine == null)
                {
                    break;
                }
                if (csvDataLine == "")
                {
                    csvDataLine = fileDataLine;//GetDeleteQuotaDataLine(fileDataLine);
                }
                else
                {
                    csvDataLine += "\r\n" + fileDataLine;//GetDeleteQuotaDataLine(fileDataLine);
                }

                if (!IfOddQuota(csvDataLine))
                {
                    AddNewDataLine(csvDataLine);
                    csvDataLine = "";
                }
            }
            sr.Close();

            if (csvDataLine.Length > 0)
            {
                throw new Exception("CSV file wrong");
            }
        }


        /// <param name="fileDataLine">file data row</param>
        /// <returns></returns>
        private string GetDeleteQuotaDataLine(string fileDataLine)
        {
            return fileDataLine.Replace("\"\"", "\"");
        }


        private bool IfOddQuota(string dataLine)
        {
            int quotaCount;
            bool oddQuota;

            quotaCount = 0;
            for (int i = 0; i < dataLine.Length; i++)
            {
                if (dataLine[i] == '\"')
                {
                    quotaCount++;
                }
            }

            oddQuota = false;
            if (quotaCount % 2 == 1)
            {
                oddQuota = true;
            }

            return oddQuota;
        }

        /// <param name="dataCell"></param>
        /// <returns></returns>
        private bool IfOddStartQuota(string dataCell)
        {
            int quotaCount;
            bool oddQuota;

            quotaCount = 0;
            for (int i = 0; i < dataCell.Length; i++)
            {
                if (dataCell[i] == '\"')
                {
                    quotaCount++;
                }
                else
                {
                    break;
                }
            }

            oddQuota = false;
            if (quotaCount % 2 == 1)
            {
                oddQuota = true;
            }

            return oddQuota;
        }

        /// <param name="dataCell"></param>
        /// <returns></returns>
        private bool IfOddEndQuota(string dataCell)
        {
            int quotaCount;
            bool oddQuota;

            quotaCount = 0;
            for (int i = dataCell.Length - 1; i >= 0; i--)
            {
                if (dataCell[i] == '\"')
                {
                    quotaCount++;
                }
                else
                {
                    break;
                }
            }

            oddQuota = false;
            if (quotaCount % 2 == 1)
            {
                oddQuota = true;
            }

            return oddQuota;
        }


        /// <param name="newDataLine">add new row</param>
        private void AddNewDataLine(string newDataLine)
        {

            ArrayList colAL = new ArrayList();
            string[] dataArray = newDataLine.Split(',');
            bool oddStartQuota;        

            string cellData;

            oddStartQuota = false;
            cellData = "";
            for (int i = 0; i < dataArray.Length; i++)
            {
                if (oddStartQuota)
                {
                    cellData += "," + dataArray[i];
                    if (IfOddEndQuota(dataArray[i]))
                    {
                        colAL.Add(GetHandleData(cellData));
                        oddStartQuota = false;
                        continue;
                    }
                }
                else
                {

                    if (IfOddStartQuota(dataArray[i]))
                    {

                        if (IfOddEndQuota(dataArray[i]) && dataArray[i].Length > 2 && !IfOddQuota(dataArray[i]))
                        {
                            colAL.Add(GetHandleData(dataArray[i]));
                            oddStartQuota = false;
                            continue;
                        }
                        else
                        {

                            oddStartQuota = true;
                            cellData = dataArray[i];
                            continue;
                        }
                    }
                    else
                    {
                        colAL.Add(GetHandleData(dataArray[i]));
                    }
                }
            }
            if (oddStartQuota)
            {
                throw new Exception("data wrong");
            }
            this.rowAL.Add(colAL);
        }


        /// <summary>

        /// </summary>
        /// <param name="fileCellData"></param>
        /// <returns></returns>
        private string GetHandleData(string fileCellData)
        {
            if (fileCellData == "")
            {
                return "";
            }
            if (IfOddStartQuota(fileCellData))
            {
                if (IfOddEndQuota(fileCellData))
                {
                    return fileCellData.Substring(1, fileCellData.Length - 2).Replace("\"\"", "\"");
                }
                else
                {
                    throw new Exception("data wrong" + fileCellData);
                }
            }
            else
            {
                if (fileCellData.Length > 2 && fileCellData[0] == '\"')
                {
                    fileCellData = fileCellData.Substring(1, fileCellData.Length - 2).Replace("\"\"", "\""); 
                }
            }

            return fileCellData;
        }


    }
}

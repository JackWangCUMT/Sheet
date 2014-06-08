using Sheet.Util.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Util
{
    public class CsvDatabaseController : IDatabaseController
    {
        #region Properties

        private string name = null;
        private string[] columns = null;
        private List<string[]> data = null;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
            }
        }

        public string[] Columns
        {
            get { return columns; }
            set
            {
                columns = value;
            }
        }

        public List<string[]> Data
        {
            get { return data; }
            set
            {
                data = value;
            }
        }

        #endregion

        #region Constructor

        public CsvDatabaseController(string name)
        {
            Name = name;
        }

        #endregion

        #region IDatabaseController

        public string[] Get(int index)
        {
            return data.Where(x => int.Parse(x[0]) == index).FirstOrDefault();
        }

        public bool Update(int index, string[] item)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (int.Parse(data[i][0]) == index)
                {
                    data[i] = item;
                    return true;
                }
            }
            return false;
        }

        public int Add(string[] item)
        {
            int index = data.Max((x) => int.Parse(x[0])) + 1;
            item[0] = index.ToString();
            data.Add(item);
            return index;
        }

        #endregion
    }
}

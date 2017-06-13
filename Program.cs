using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Data;



namespace MedicalReports
{
    class Program
    {
        private static DataTable table = new DataTable();
        private const char DELIM = ',';

        // App Parameters
        private const string PARAM_FILE = "file";
        private const string PARAM_SORT = "sort";
        private const string PARAM_SEARCH = "search";

        private static string value_file = "";
        private static string value_sort = "";
        private static string value_search = "";

        private static bool readingFile = false;


        static void Main(string[] args)
        {

            // Tests
            // ---------------------------------------------------------------------------
            // simple data output
            /**
            args = new string[2];
            args[0] = "-file";
            args[1] = "C:\\Users\\sutto\\Documents\\MedicalReports.txt";
            **/

            // sorting
            /**
            args = new string[4];
            args[0] = "-file";
            args[1] = "C:\\Users\\sutto\\Documents\\MedicalReports.txt";
            args[2] = "-sort";
            args[3] = "LastName";
            **/

            // searching
            /**
            args = new string[4];
            args[0] = "-file";
            args[1] = "C:\\Users\\sutto\\Documents\\MedicalReports.txt";
            args[2] = "-search";
            args[3] = "Bryan";
            **/

            // sorting and seaching
            /**
            args = new string[6];
            args[0] = "-file";
            args[1] = "C:\\Users\\sutto\\Documents\\MedicalReports.txt";
            args[2] = "-sort";
            args[3] = "LastName";
            args[4] = "-search";
            args[5] = "Bryan";
            **/

            // expecting file not found
            //args = new string[0];

            // expecting invalid parameter
            /**
            args = new string[2];
            args[0] = "-notParameter";
            args[1] = "test";
            **/

            // expecting Invalid value
            /**
            args = new string[3];
            args[0] = "-file";
            args[1] = "-sort";
            args[2] = "Name";
            **/
            // ---------------------------------------------------------------------------

            run(args);


        }

        private static void run(string[] args)
        {
            // Process Parameters

            for (int i = 0; i < args.Length; i++)
            {
                String arg = args[i];
                if (arg.StartsWith("-"))
                {
                    // Get parameter field name
                    String argField = arg.Remove(0, 1).ToLower();

                    try
                    {
                        // Save the appropriate value
                        i++;
                        switch (argField)
                        {
                            case PARAM_FILE:
                                value_file = args[i];
                                break;
                            case PARAM_SORT:
                                value_sort = args[i].ToLower();
                                break;
                            case PARAM_SEARCH:
                                value_search = args[i];
                                break;
                            default:
                                quit("Invalid Parameter", -1);
                                break;

                        }
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        // Argument call does not have value associated with it
                        quit("Argument does not have a corresponding value", -1);
                        return;
                    }
                }
                else
                {
                    // Argument call with - expected but no received
                    quit("Invalid Input", -1);
                    return;
                }
            }

            if (value_file.Equals(""))
            {
                quit("No File Supplied", -1);
                return;
            }

            //Process the file
            processFile(value_file);
            while (readingFile)
            {
                // wait for reading and processing to finish
            }

            DataView view = new DataView(table);

            string query = "";

            // search parameter is included
            // build a query to search the fields for each column
            if (!value_search.Equals(""))
            {
                bool started = false;
                foreach(DataColumn c in table.Columns)
                {
                    if (started)
                    {
                        query += " OR ";
                    }
                    else
                    {
                        started = true;
                    }
                    query += c.ColumnName + " LIKE '%" + value_search + "%'"; 
                }
            }

            // sort parameter is included
            // include sort on Select call and send to output
            if (!value_sort.Equals(""))
            {
                output(table.Select(query, value_sort));
            }
            else
            // otherwise, run Select without a sort
            {
                output(table.Select(query));
            }

            successQuit();
        }

        // Reads input file and builds datatable
        // Input file is assumed to be CSV
        private static void processFile(String location)
        {
            readingFile = true;
            try
            {
                using (StreamReader sr = new StreamReader(location))
                {
                    bool first = true;
                    string line;
                    string[] header = new string[0];
                    
                    // reading the file
                    while ((line = sr.ReadLine()) != null)
                    {
                        // on first line, build the columns and add to table
                        if (first)
                        {
                            header = parseLine(line, DELIM);
                            foreach(string c in header)
                            {
                                DataColumn column = new DataColumn();
                                column.ColumnName = c.ToLower();
                                table.Columns.Add(column);
                            }
                            first = false;
                        }
                        // add remaining lines as datarows
                        else
                        {
                            string[] r = parseLine(line, DELIM);
                            DataRow row = table.NewRow();
                            for (int i = 0; i < r.Length; i++)
                            {
                                row[header[i]] = r[i];
                            }
                            table.Rows.Add(row);
                        }
                    }
                    readingFile = false;

                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static String[] parseLine(String line, Char delim)
        {
            return line.Split(delim);
        }

        private static void output()
        {
            output(table.Columns, table.Select());
        }

        private static void output(DataRow[] rows)
        {
            output(table.Columns, rows);
        }

        private static void output(DataColumnCollection columns, DataRow[] rows)
        {
            foreach (DataColumn c in columns)
            {
                Console.Write("{0},", c.ColumnName);
            }
            Console.WriteLine();
            foreach (DataRow r in rows)
            {
                foreach (string v in r.ItemArray)
                {
                    Console.Write("{0},", v);
                }
                Console.WriteLine();
            }
        }

        private static void quit(string message, int code)
        {
            Console.WriteLine(message);
            Environment.Exit(code);
        }

        private static void successQuit()
        {
            quit("", 0);
        }
    }
}

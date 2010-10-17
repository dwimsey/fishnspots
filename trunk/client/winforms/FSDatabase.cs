using System;
using System.Collections.Generic;
using System.Data.Common;

#if !MONO
using System.Data.SQLite;
#else
   using SQLiteConnection=Mono.Data.SqliteClient.SqliteConnection;
   using SQLiteTransaction=Mono.Data.SqliteClient.SqliteTransaction;
   using SQLiteCommand=Mono.Data.SqliteClient.SqliteCommand;
   using SQLiteDataReader=Mono.Data.SqliteClient.SqliteDataReader;
   using SQLiteParameter=Mono.Data.SqliteClient.SqliteParameter;
#endif
using System.IO;
using System.Text;
using System.Diagnostics;

namespace FishnSpots
{
    
    class FSDatabase
    {
        private SQLiteConnection dbConn;
		private String dbFile;
		private FSEngine fsEngine;

		internal FSDatabase(FSEngine fsEngine)
		{
			this.fsEngine = fsEngine;
		    dbFile = fsEngine.prefs.DataDir + Path.DirectorySeparatorChar + "Data.fsdb";

			if (File.Exists(dbFile)) {
				try {
					File.Delete(dbFile);
				} catch {
					int i = 0;
					i++;
				}
			}

			if (!File.Exists(dbFile)) {
				CreateEmptyDB();
            }
			OpenDB();
        }

		private bool CreateEmptyDB()
        {
            bool ret = true;
            try {
				string dir = Path.GetDirectoryName(dbFile);
                if (!Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }

                using (SQLiteConnection cn = new SQLiteConnection()) {
#if !MONO
					cn.ConnectionString = string.Format("Data Source=\"{0}\";FailIfMissing=False;", dbFile);
#else
               cn.ConnectionString = string.Format("Version=3,URI=file://{0},FailIfMissing=False", dbFile);
#endif
					cn.Open();
                    {
                        using (DbTransaction tr = cn.BeginTransaction())
                        {
                            try
                            {
								using (DbCommand cmd = cn.CreateCommand())
								{
									cmd.Transaction = tr;
									cmd.CommandText = "CREATE TABLE [TileCache] (" +
	"[Ip] varchar(15) PRIMARY KEY NOT NULL," +
	"[CountryName] text NOT NULL," +
	"[RegionName] text NOT NULL," +
	"[City] text NOT NULL," +
	"[Latitude] float NOT NULL," +
	"[Longitude] float NOT NULL," +
	"[Time] datetime NOT NULL);";
									cmd.ExecuteNonQuery();
								}

								using (DbCommand cmd = cn.CreateCommand())
								{
									cmd.Transaction = tr;
									cmd.CommandText = "CREATE TABLE [Tracks] (" +
	"[tid] INTEGER PRIMARY KEY NOT NULL," +
	"[TrackName] text NOT NULL," +
	"[TimeCreated] datetime NOT NULL);";
									cmd.ExecuteNonQuery();
								}

								using (DbCommand cmd = cn.CreateCommand())
								{
									cmd.Transaction = tr;
									cmd.CommandText = "CREATE TABLE [Trackpoints] (" +
	"[tpid] INTEGER PRIMARY KEY NOT NULL," +	// Unique ID for this point
	"[tid] INTEGER REFERENCES Tracks(tid)," +					// Id of the entry in the Tracks table this track belongs too
	"[Altitude] float NOT NULL," +
	"[Latitude] float NOT NULL," +
	"[Longitude] float NOT NULL," +
	"[Heading] float NOT NULL," +
	"[Pressure] float NOT NULL," +
	"[Time] datetime NOT NULL);";
									cmd.ExecuteNonQuery();
								}

								using (DbCommand cmd = cn.CreateCommand())
								{
									cmd.Transaction = tr;
									cmd.CommandText = "CREATE TABLE [Waypoints] (" +
	"[wpid] INTEGER PRIMARY KEY NOT NULL," +
	"[WaypointName] text NOT NULL," +
	"[Altitude] float NOT NULL," +
	"[Latitude] float NOT NULL," +
	"[Longitude] float NOT NULL," +
	"[Heading] float NOT NULL," +
	"[Pressure] float NOT NULL," +
	"[Time] datetime NOT NULL);";
									cmd.ExecuteNonQuery();
								}

								tr.Commit();
                            }
                            catch (Exception exx)
                            {
                                Console.WriteLine("CreateEmptyDB: " + exx.ToString());
                                Debug.WriteLine("CreateEmptyDB: " + exx.ToString());

                                tr.Rollback();
                                ret = false;
                            }
                        }
                        cn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
#if MONO
            Console.WriteLine("CreateEmptyDB: " + ex.ToString());
#endif
                Debug.WriteLine("CreateEmptyDB: " + ex.ToString());
                ret = false;
            }
            return ret;
        }


		private bool OpenDB()
		{

			bool ret = true;
			try {
				this.dbConn = new SQLiteConnection();
#if !MONO
				this.dbConn.ConnectionString = string.Format("Data Source=\"{0}\";FailIfMissing=True;", dbFile);
#else
				this.dbConn.ConnectionString = string.Format("Version=3,URI=file://{0},FailIfMissing=True", dbFile);
#endif
				this.dbConn.Open();
			} catch (Exception ex) {
#if MONO
				Console.WriteLine("OpenDB: " + ex.ToString());
#endif
				Debug.WriteLine("OpenDB: " + ex.ToString());
				ret = false;
			}
			return ret;
		}
    }
}

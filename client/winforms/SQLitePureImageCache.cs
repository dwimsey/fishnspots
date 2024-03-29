﻿
namespace GMap.NET.CacheProviders
{
#if SQLite
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
   using System;
   using System.Diagnostics;
   using System.Globalization;

   /// <summary>
   /// ultra fast cache system for tiles
   /// </summary>
   internal class SQLitePureImageCache : PureImageCache
   {
      string cache;
      string gtileCache;
      string db;
      bool Created = false;

      public string GtileCache
      {
         get
         {
            return gtileCache;
         }
      }

      /// <summary>
      /// local cache location
      /// </summary>
      public string CacheLocation
      {
         get
         {
            return cache;
         }
         set
         {
            cache = value;
            gtileCache = cache + "TileDBv3" + Path.DirectorySeparatorChar;

            string dir = gtileCache + GMaps.Instance.Language.ToString() + Path.DirectorySeparatorChar;

            // precrete dir
            if(!Directory.Exists(dir))
            {
               Directory.CreateDirectory(dir);
            }

            // make empty db
            {
               db = dir + "Data.gmdb";

               if(!File.Exists(db))
               {
                  Created = CreateEmptyDB(db);
               }
               else
               {
                  Created = AlterDBAddTimeColumn(db);
               }
#if !MONO
               ConnectionString = string.Format("Data Source=\"{0}\";Page Size=32768;Pooling=True", db);
#else
               ConnectionString = string.Format("Version=3,URI=file://{0},FailIfMissing=True,Page Size=32768,Pooling=True", db);
#endif
            }
         }
      }

      #region -- import / export --
      public static bool CreateEmptyDB(string file)
      {
         bool ret = true;

         try
         {
            string dir = Path.GetDirectoryName(file);
            if(!Directory.Exists(dir))
            {
               Directory.CreateDirectory(dir);
            }

            using(SQLiteConnection cn = new SQLiteConnection())
            {
#if !MONO
               cn.ConnectionString = string.Format("Data Source=\"{0}\";FailIfMissing=False;Page Size=32768;Pooling=True", file);
#else
               cn.ConnectionString = string.Format("Version=3,URI=file://{0},FailIfMissing=False,Page Size=32768,Pooling=True", file);
#endif
               cn.Open();
               {
                  using(DbTransaction tr = cn.BeginTransaction())
                  {
                     try
                     {
                        using(DbCommand cmd = cn.CreateCommand())
                        {
                           cmd.Transaction = tr;
#if !PocketPC
                           cmd.CommandText = Properties.Resources.CreateTileDb;
#else
                           cmd.CommandText = GMap.NET.WindowsMobile.Properties.Resources.CreateTileDb;
#endif
                           cmd.ExecuteNonQuery();
                        }
                        tr.Commit();
                     }
                     catch(Exception exx)
                     {
#if MONO
                        Console.WriteLine("CreateEmptyDB: " + exx.ToString());
#endif
                        Debug.WriteLine("CreateEmptyDB: " + exx.ToString());

                        tr.Rollback();
                        ret = false;
                     }
                  }
                  cn.Close();
               }
            }
         }
         catch(Exception ex)
         {
#if MONO
            Console.WriteLine("CreateEmptyDB: " + ex.ToString());
#endif
            Debug.WriteLine("CreateEmptyDB: " + ex.ToString());
            ret = false;
         }
         return ret;
      }

      private static bool AlterDBAddTimeColumn(string file)
      {
         bool ret = true;

         try
         {
            if(File.Exists(file))
            {
               using(SQLiteConnection cn = new SQLiteConnection())
               {
#if !MONO
                  cn.ConnectionString = string.Format("Data Source=\"{0}\";FailIfMissing=False;Page Size=32768;Pooling=True", file);
#else
                  cn.ConnectionString = string.Format("Version=3,URI=file://{0},FailIfMissing=False,Page Size=32768,Pooling=True", file);
#endif
                  cn.Open();
                  {
                     using(DbTransaction tr = cn.BeginTransaction())
                     {
                        bool? NoCacheTimeColumn = null;

                        try
                        {
                           using(DbCommand cmd = new SQLiteCommand("SELECT CacheTime FROM Tiles", cn))
                           {
                              cmd.Transaction = tr;

                              using(DbDataReader rd = cmd.ExecuteReader())
                              {
                                 rd.Close();
                              }
                              NoCacheTimeColumn = false;
                           }
                        }
                        catch(Exception ex)
                        {
                           if(ex.Message.Contains("no such column: CacheTime"))
                           {
                              NoCacheTimeColumn = true;
                           }
                           else
                           {
                              throw ex;
                           }
                        }

                        try
                        {
                           if(NoCacheTimeColumn.HasValue && NoCacheTimeColumn.Value)
                           {
                              using(DbCommand cmd = cn.CreateCommand())
                              {
                                 cmd.Transaction = tr;

                                 cmd.CommandText = "ALTER TABLE Tiles ADD CacheTime DATETIME";

                                 cmd.ExecuteNonQuery();
                              }
                              tr.Commit();
                              NoCacheTimeColumn = false;
                           }
                        }
                        catch(Exception exx)
                        {
#if MONO                   
                           Console.WriteLine("AlterDBAddTimeColumn: " + exx.ToString());
#endif
                           Debug.WriteLine("AlterDBAddTimeColumn: " + exx.ToString());

                           tr.Rollback();
                           ret = false;
                        }
                     }
                     cn.Close();
                  }
               }
            }
            else
            {
               ret = false;
            }
         }
         catch(Exception ex)
         {
#if MONO
            Console.WriteLine("AlterDBAddTimeColumn: " + ex.ToString());
#endif
            Debug.WriteLine("AlterDBAddTimeColumn: " + ex.ToString());
            ret = false;
         }
         return ret;
      }

      public static bool VacuumDb(string file)
      {
         bool ret = true;

         try
         {
            using(SQLiteConnection cn = new SQLiteConnection())
            {
#if !MONO
               cn.ConnectionString = string.Format("Data Source=\"{0}\";FailIfMissing=True;Page Size=32768;Pooling=True", file);
#else
               cn.ConnectionString = string.Format("Version=3,URI=file://{0},FailIfMissing=True,Page Size=32768,Pooling=True", file);
#endif
               cn.Open();
               {
                  using(DbCommand cmd = cn.CreateCommand())
                  {
                     cmd.CommandText = "vacuum;";
                     cmd.ExecuteNonQuery();
                  }
                  cn.Close();
               }
            }
         }
         catch(Exception ex)
         {
            Debug.WriteLine("VacuumDb: " + ex.ToString());
            ret = false;
         }
         return ret;
      }

      public static bool ExportMapDataToDB(string sourceFile, string destFile)
      {
         bool ret = true;

         try
         {
            if(!File.Exists(destFile))
            {
               ret = CreateEmptyDB(destFile);
            }

            if(ret)
            {
               using(SQLiteConnection cn1 = new SQLiteConnection())
               {
#if !MONO
                  cn1.ConnectionString = string.Format("Data Source=\"{0}\";Page Size=32768;Pooling=True", sourceFile);
#else
                  cn1.ConnectionString = string.Format("Version=3,URI=file://{0},FailIfMissing=True,Page Size=32768,Pooling=True", sourceFile);
#endif

                  cn1.Open();
                  if(cn1.State == System.Data.ConnectionState.Open)
                  {
                     using(SQLiteConnection cn2 = new SQLiteConnection())
                     {
#if !MONO
                        cn2.ConnectionString = string.Format("Data Source=\"{0}\";Page Size=32768;Pooling=True", destFile);
#else
                        cn2.ConnectionString = string.Format("Version=3,URI=file://{0},FailIfMissing=True,Page Size=32768,Pooling=True", destFile);
#endif
                        cn2.Open();
                        if(cn2.State == System.Data.ConnectionState.Open)
                        {
                           using(SQLiteCommand cmd = new SQLiteCommand(string.Format("ATTACH DATABASE \"{0}\" AS Source", sourceFile), cn2))
                           {
                              cmd.ExecuteNonQuery();
                           }

#if !MONO
                           using(SQLiteTransaction tr = cn2.BeginTransaction())
#else
                           using(DbTransaction tr = cn2.BeginTransaction())
#endif
                           {
                              try
                              {
                                 List<long> add = new List<long>();
                                 using(SQLiteCommand cmd = new SQLiteCommand("SELECT id, X, Y, Zoom, Type FROM Tiles;", cn1))
                                 {
                                    using(SQLiteDataReader rd = cmd.ExecuteReader())
                                    {
                                       while(rd.Read())
                                       {
                                          long id = rd.GetInt64(0);
                                          using(SQLiteCommand cmd2 = new SQLiteCommand(string.Format("SELECT id FROM Tiles WHERE X={0} AND Y={1} AND Zoom={2} AND Type={3};", rd.GetInt32(1), rd.GetInt32(2), rd.GetInt32(3), rd.GetInt32(4)), cn2))
                                          {
                                             using(SQLiteDataReader rd2 = cmd2.ExecuteReader())
                                             {
                                                if(!rd2.Read())
                                                {
                                                   add.Add(id);
                                                }
                                             }
                                          }
                                       }
                                    }
                                 }

                                 foreach(long id in add)
                                 {
                                    using(SQLiteCommand cmd = new SQLiteCommand(string.Format("INSERT INTO Tiles(X, Y, Zoom, Type) SELECT X, Y, Zoom, Type FROM Source.Tiles WHERE id={0}; INSERT INTO TilesData(id, Tile) Values((SELECT last_insert_rowid()), (SELECT Tile FROM Source.TilesData WHERE id={0}));", id), cn2))
                                    {
                                       cmd.Transaction = tr;
                                       cmd.ExecuteNonQuery();
                                    }
                                 }
                                 add.Clear();

                                 tr.Commit();
                              }
                              catch
                              {
                                 tr.Rollback();
                                 ret = false;
                              }
                           }
                        }
                     }
                  }
               }
            }
         }
         catch(Exception ex)
         {
            Debug.WriteLine("ExportMapDataToDB: " + ex.ToString());
            ret = false;
         }
         return ret;
      }
      #endregion

      #region PureImageCache Members

      bool PureImageCache.PutImageToCache(MemoryStream tile, MapType type, Point pos, int zoom)
      {
         bool ret = true;
         if(Created)
         {
            try
            {
               using(SQLiteConnection cn = new SQLiteConnection())
               {
                  cn.ConnectionString = ConnectionString;
                  cn.Open();
                  {
                     using(DbTransaction tr = cn.BeginTransaction())
                     {
                        try
                        {
                           using(DbCommand cmd = cn.CreateCommand())
                           {
                              cmd.Transaction = tr;
                              cmd.CommandText = "INSERT INTO Tiles(X, Y, Zoom, Type, CacheTime) VALUES(@p1, @p2, @p3, @p4, @p5)";

                              cmd.Parameters.Add(new SQLiteParameter("@p1", pos.X));
                              cmd.Parameters.Add(new SQLiteParameter("@p2", pos.Y));
                              cmd.Parameters.Add(new SQLiteParameter("@p3", zoom));
                              cmd.Parameters.Add(new SQLiteParameter("@p4", (int) type));
                              cmd.Parameters.Add(new SQLiteParameter("@p5", DateTime.UtcNow));

                              cmd.ExecuteNonQuery();
                           }

                           using(DbCommand cmd = cn.CreateCommand())
                           {
                              cmd.Transaction = tr;

                              cmd.CommandText = "INSERT INTO TilesData(id, Tile) VALUES((SELECT last_insert_rowid()), @p1)";
                              cmd.Parameters.Add(new SQLiteParameter("@p1", tile.GetBuffer()));

                              cmd.ExecuteNonQuery();
                           }
                           tr.Commit();
                        }
                        catch(Exception ex)
                        {
#if MONO
                        Console.WriteLine("PutImageToCache: " + ex.ToString());
#endif
                           Debug.WriteLine("PutImageToCache: " + ex.ToString());

                           tr.Rollback();
                           ret = false;
                        }
                     }
                  }
                  cn.Close();
               }
            }
            catch(Exception ex)
            {
#if MONO
            Console.WriteLine("PutImageToCache: " + ex.ToString());
#endif
               Debug.WriteLine("PutImageToCache: " + ex.ToString());
               ret = false;
            }
         }
         return ret;
      }

      static readonly string sqlSelect = "SELECT Tile FROM TilesData WHERE id = (SELECT id FROM Tiles WHERE X={0} AND Y={1} AND Zoom={2} AND Type={3})";
      string ConnectionString;

      PureImage PureImageCache.GetImageFromCache(MapType type, Point pos, int zoom)
      {
         PureImage ret = null;
         try
         {
            using(SQLiteConnection cn = new SQLiteConnection())
            {
               cn.ConnectionString = ConnectionString;
               cn.Open();
               {
                  using(DbCommand com = cn.CreateCommand())
                  {
                     com.CommandText = string.Format(sqlSelect, pos.X, pos.Y, zoom, (int) type);

                     using(DbDataReader rd = com.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                     {
                        if(rd.Read())
                        {
                           long length = rd.GetBytes(0, 0, null, 0, 0);
                           byte[] tile = new byte[length];
                           rd.GetBytes(0, 0, tile, 0, tile.Length);
                           {
                              if(GMaps.Instance.ImageProxy != null)
                              {
                                 MemoryStream stm = new MemoryStream(tile, 0, tile.Length, false, true);

                                 ret = GMaps.Instance.ImageProxy.FromStream(stm);
                                 if(ret != null)
                                 {
                                    ret.Data = stm;
                                 }
                              }
                           }
                           tile = null;
                        }
                        rd.Close();
                     }
                  }
               }
               cn.Close();
            }
         }
         catch(Exception ex)
         {
#if MONO
            Console.WriteLine("GetImageFromCache: " + ex.ToString());
#endif
            Debug.WriteLine("GetImageFromCache: " + ex.ToString());
            ret = null;
         }

         return ret;
      }

      #endregion
   }
#endif
}

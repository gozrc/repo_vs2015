﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Commons.CustomHttpManager;
using WebScrapper.Entities;
using WebScrapper.Scrappers;
using WebScrapper.Scrappers.Pelispedia;
using WebScrapper.Servers;
using Commons.CustomDatabaseManager;

namespace WebScrapper
{
    class Program
    {
        static void Main (string[] args)
        {
            // ---------------------------------------------------------------
            //testearBaseDeDatos();
            // ---------------------------------------------------------------

            // ---------------------------------------------------------------
            //testearPelicula ("https://www.pelispedia.tv/pelicula/ibiza/");
            //testearPelicula("https://www.pelispedia.tv/pelicula/entre-sombras/", "Entre Sombras");
            // ---------------------------------------------------------------

            // ---------------------------------------------------------------
            IWebScrapper scrapper = new ScrapPelispedia();

            scrapper.onLog   += Scrapper_onLog;
            scrapper.onMovie += Scrapper_onMovie;

            scrapper.scrapMovies ();
            // ---------------------------------------------------------------

            Console.ReadKey();
        }


        static void testearPelicula (string urlPelicula)
        {
            IWebScrapper scrapper = new ScrapPelispedia();

            scrapper.onLog += Scrapper_onLog;

            Sources sources = new Sources();

            scrapper.getMovieSources(urlPelicula, ref sources);

            foreach (Source s in sources)
                Console.WriteLine("{0} {1}", s.name_server, s.url_source);
        }

        static void testearBaseDeDatos ()
        {
            string error = string.Empty;

            CustomDatabase customDb = new CustomDatabase(
                Config.dbServer, Config.dbName, Config.dbUser, Config.dbPassword);

            if (customDb.open(ref error))
            {
                string sql = Persistence.DbPersistence.sqlInsertSource(1, "Servidor", "direccion_pelicula", "direccion_subtitulo", "descripcion pelicula", "HASDASDASDASDAS", DateTime.Now);

                if (customDb.execute(sql, ref error))
                    Console.WriteLine("Ok");
            }

            customDb.close();

            if (error.Length > 0)
                Console.WriteLine(error);
        }

        static void log (string title, string description)
        {
            string text = string.Format(
                "[{0}] :: {1} :: {2}",
                DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                title,
                description
            );

            Console.WriteLine(text);
        }



        static void Scrapper_onLog (string title, string description)
        {
            log ("ERROR", title + " - " + description);

            //----------------------------------------------

            string error = string.Empty;

            CustomDatabase customDb = new CustomDatabase(
                Config.dbServer, Config.dbName, Config.dbUser, Config.dbPassword);

            if (customDb.open(ref error))
            {
                string sql = Persistence.DbPersistence.sqlInsertAudit(
                    title, description, DateTime.Now);

                customDb.execute (sql, ref error);
            }

            customDb.close ();

            if (error.Length > 0)
                log ("DATABASE", error);
        }

        static void Scrapper_onMovie (Movie movie)
        {
            log ("INFO", "");
            log ("INFO", movie.title);

            foreach (Source s in movie.sources)
                log ("INFO", "   " + s.name_server);

            //----------------------------------------------

            string error = string.Empty;

            CustomDatabase customDb = new CustomDatabase(
                Config.dbServer, Config.dbName, Config.dbUser, Config.dbPassword);

            if (customDb.open(ref error))
            {
                string sql = Persistence.DbPersistence.sqlInsertMovie(
                    movie.title, movie.description, movie.url_image, movie.url_web, DateTime.Now);

                if (customDb.execute (sql, ref error))
                {
                    sql = Persistence.DbPersistence.sqlSelectMovie(movie.url_web);
                    int id_movie = 0;

                    if (customDb.executeScalar(sql, ref id_movie, ref error))
                    {
                        foreach (Source s in movie.sources)
                        {
                            string hash = Cryptography.HelperMD5.calculateHashMD5(s.url_source);

                            sql = Persistence.DbPersistence.sqlInsertSource(
                                id_movie, s.name_server, s.url_source, s.url_subtitles, s.description, hash, DateTime.Now);

                            if (!customDb.execute (sql, ref error))
                                log("DATABASE", error);

                            error = string.Empty;
                        }

                    }
                }
            }

            customDb.close ();

            if (error.Length > 0)
                log ("DATABASE", error);
        }
    }
}

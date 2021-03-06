﻿using System;
using System.Collections.Generic;
using WebScrapper.Entities;
using WebScrapper.Cryptography;
using Commons.CustomHttpManager;
using WebScrapper.Servers;

namespace WebScrapper.Scrappers.Pelispedia
{
    public class ScrapPelispedia : IWebScrapper
    {
        const string URL_MOVIES = "https://www.pelispedia.tv/api/morex.php?rangeStart={0}&flagViewMore={1}&letra={2}&year={3}&genre={4}";

        public override string name ()
        {
            return "PELISPEDIA";
        }


        public override void scrapMovies ()
        {
            int    index  = 0;
            string buffer = string.Empty;
            string error  = string.Empty;

            while (HttpManager.requestGet(string.Format(URL_MOVIES, index, "true", "", "", ""), null, ref buffer, ref error))
            {
                string[] movies = buffer.MatchRegexs("<li.*?</li>", false, true);

                if (movies.Length == 0)
                    break;

                foreach (string li in movies)
                {
                    string errorItem = string.Empty;
                    Movie  movie     = null;

                    if (createMovie(li, ref movie, ref errorItem))
                    {
                        getMovieSources (movie.url_web, ref movie.sources);
                        runOnMovie      (movie);
                    }
                    else
                    {
                        runOnLog (name(), "ERROR", error);
                    }

                    index++;
                }
            }

            if (error.Length > 0)
                runOnLog (name(), "ERROR", error);
        }

        public override void getMovieSources (string urlMovie, ref Sources sources)
        {
            string       buffer          = string.Empty;
            string       keyMovie        = string.Empty;
            List<string> lSources        = new List<string>();
            string       urlDecoded      = string.Empty;
            string       error           = string.Empty;

            if (0 == error.Length)
                HttpManager.requestGet(urlMovie, null, ref buffer, ref error);

            if (0 == error.Length)
                PelispediaHelper.getKeyMovie(urlMovie, buffer, ref keyMovie, ref error);

            if (0 == error.Length)
                PelispediaHelper.getUrlOptions(urlMovie, keyMovie, ref buffer, ref error);

            if (0 == error.Length)
                PelispediaHelper.getSources(buffer, ref lSources, ref error);

            if (0 == error.Length)
            {
                foreach (string source in lSources)
                {
                    if (source.StartsWith("https://load.pelispedia.vip/embed"))
                    {
                        error = string.Empty;

                        if (decodeSource(source, ref urlDecoded, ref error))
                            ServerScrapper.scrap(urlDecoded, ref sources, ref error);

                        if (error.Length > 0)
                            runOnLog (name(), "ERROR (" + urlMovie + ")", error);
                    }
                }
            }
            else
            {
                runOnLog (name(), "ERROR (" + urlMovie + ")", error);
            }
        }


        bool createMovie (string htmlCode, ref Movie movie, ref string error)
        {
            try
            {
                string url_web      = htmlCode.MatchRegex("<a href=\"([^\"]*)\" alt=");
                string title        = htmlCode.MatchRegex("<h2 class=\"mb10\">([^<]*)<br><span");
                string url_image    = htmlCode.MatchRegex("url=(https://[^\"]*)\" alt=");
                string description  = htmlCode.MatchRegex("<p class=\"font12\">([^<]*)</p>");

                if (url_web.Length == 0)
                    throw new Exception("Url web not found");

                if (title.Length == 0)
                    throw new Exception("Title not found");

                if (url_image.Length == 0)
                    throw new Exception("Url image not found");

                if (description.Length == 0)
                    throw new Exception("Description not found");

                movie = new Movie(HelperMD5.calculateHashMD5(url_web));

                movie.title       = title;
                movie.url_image   = url_image;
                movie.url_web     = url_web;
                movie.description = description;
            }
            catch (Exception ex)
            {
                error = "createMovie -> " + ex.Message;
            }

            return (0 == error.Length);
        }

        bool decodeSource (string urlCodedSource, ref string urlDecodedSource, ref string error)
        {
            string codigo = string.Empty;
            string buffer = string.Empty;
            string urlAux = string.Empty;

            HttpHeaders responseHeaders = new HttpHeaders();

            if (0 == error.Length)
                HttpManager.requestGet(urlCodedSource, null, ref buffer, ref error);

            if (0 == error.Length)
                PelispediaHelper.getCode(buffer, ref codigo, ref error);

            if (0 == error.Length)
                PelispediaHelper.decryptUrl(urlCodedSource, codigo, ref urlAux, ref error);

            if (0 == error.Length)
                HttpManager.requestGetSR(urlAux, null, ref buffer, ref responseHeaders, ref error);

            if (0 == error.Length)
                if (!responseHeaders.exist("Location"))
                    error = "Location header missing";

            if (0 == error.Length)
                urlDecodedSource = responseHeaders.value("Location");

            if (error.Length > 0)
                error = "decodeSource -> " + error;

            return (0 == error.Length);
        }

































        public override Series scrapSeries ()
        {
            throw new NotImplementedException();
        }
    }
}

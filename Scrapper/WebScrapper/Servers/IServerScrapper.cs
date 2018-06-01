﻿using System;
using System.Text.RegularExpressions;

namespace WebScrapper.Servers
{
    public abstract class IServerScrapper
    {
        public abstract bool scrappear (string url, ref Sources serverLinks, ref string error);


        protected bool obtenerCodigo (string buffer, ref string codigo, ref string error)
        {
            try
            {
                Regex rgxCode = new Regex(
                    "<meta name=\"google-site-verification.+>",
                    RegexOptions.IgnoreCase
                );

                foreach (Match codeMatch in rgxCode.Matches(buffer))
                    codigo = "\"" + codeMatch.Value.Split('\"')[3] + "\"";

                if (codigo.Length == 0)
                    throw new Exception("No se encontró el codigo de la película.");
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            if (error.Length > 0)
                error = "IServerScrapper.obtenerCodigo -> " + error;

            return (0 == error.Length);
        }

        protected bool esArchivoValido (string urlVideo)
        {
            // TODO:
            return true;

            //string error        = string.Empty;
            //string dataResponse = string.Empty;

            //if (urlVideo.Substring(urlVideo.Length - 4, 1) != ".")
            //    return false;

            //if (!HttpHelper.requestHead(urlVideo, null, ref dataResponse, ref error))
            //    return false;

            //return true;
        }
    }
}

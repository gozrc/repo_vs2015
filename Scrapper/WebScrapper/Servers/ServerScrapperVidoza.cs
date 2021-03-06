﻿using System.Collections.Generic;
using Commons.CustomHttpManager;

namespace WebScrapper.Servers
{
    public class ServerScrapperVidoza : IServerScrapper
    {
        public override string name ()
        {
            return "VIDOZA";
        }

        public override bool scrappear (string url, ref Sources serverLinks, ref string error)
        {
            string buffer  = string.Empty;
            string urlSubs = string.Empty;

            List<string> urlVideos = new List<string>();
            List<string> urlDescs  = new List<string>();

            if (0 == error.Length)
                HttpManager.requestGet(url, null, ref buffer, ref error);

            if (0 == error.Length)
                getUrlSubs(buffer, ref urlSubs, ref error);

            if (0 == error.Length)
                getUrlVideos(buffer, ref urlVideos, ref urlDescs, ref error);

            if (0 == error.Length)
                for (int k = 0; k < urlVideos.Count; k++)
                    serverLinks.Add(new Source(name(), urlVideos[k], urlSubs, urlDescs[k]));

            return (0 == error.Length);
        }


        bool getUrlSubs (string buffer, ref string urlSubs, ref string error)
        {
            urlSubs = buffer.MatchRegex("<track kind=\"subtitles\" src=\"(https://[^\"]*)\" srclang");

            if (urlSubs.Length == 0)
                error = "Subtitles link not found";

            return (0 == error.Length);
        }

        bool getUrlVideos (string buffer, ref List<string> urlVideos, ref List<string> urlDescs, ref string error)
        {
            urlVideos = new List<string>();
            urlDescs  = new List<string>();

            urlVideos.AddRange (buffer.MatchRegexs("src: \"(https://[^\"]*)\", type:"));
            urlDescs.AddRange  (buffer.MatchRegexs("type: 'video/mp4', label:'[^']*', res:'([^']*)'"));

            if (urlVideos.Count == 0)
                error = "Link video not found";

            return (0 == error.Length);
        }
    }
}

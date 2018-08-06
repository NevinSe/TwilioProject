using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;

namespace TwilioProject
{
    internal class YoutubeSearch
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                new YoutubeSearch().Run("korn").Wait();
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }
        }

        public async Task<List<string>> Run(string searchTerm)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = Keys.YoutubeApiKey,
                ApplicationName = "TwilioProject"
            });

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = searchTerm;
            searchListRequest.MaxResults = 10;
            
            var searchListResponse = await searchListRequest.ExecuteAsync();

            List<string> videos = new List<string>();
            
            foreach (var searchResult in searchListResponse.Items)
            {
                videos.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.VideoId));
            }
            return videos;
        }
    }
}
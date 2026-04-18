using examBE_MovieCatalog_DH.DTOs;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Net;
using System.Text.Json;


namespace examBE_MovieCatalog_DH
{
    public class Tests
    {

        private RestClient client;
        private static string lastCreateadId;

        private const string BaseUrl = "http://144.91.123.158:5000";
        private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiI1M2VkYTg1ZS1lMjdhLTRhODctYWE0Yi05NjhiYjkyMmRiODgiLCJpYXQiOiIwNC8xOC8yMDI2IDA2OjEzOjU1IiwiVXNlcklkIjoiMDRjMmRmOGUtYmU2OS00OGE5LTYyM2YtMDhkZTc2OTcxYWI5IiwiRW1haWwiOiJkaEBleGFtcGxlLmNvbSIsIlVzZXJOYW1lIjoiZXhhbURIIiwiZXhwIjoxNzc2NTE0NDM1LCJpc3MiOiJNb3ZpZUNhdGFsb2dfQXBwX1NvZnRVbmkiLCJhdWQiOiJNb3ZpZUNhdGFsb2dfV2ViQVBJX1NvZnRVbmkifQ.7rxI320IqAn-9aLutDUsnpbplf3zr6LNScGlB0JKfnY";

        private const string LoginEmail = "dh@example.com";
        private const string LoginPassword = "123123";




        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken("dh@example.com", "123123");
            RestClientOptions options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };
            client = new RestClient(options);
        }


        private string GetJwtToken(string email, string password)
        {
            RestClient client = new RestClient(BaseUrl);
            RestRequest request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });

            RestResponse response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Response: {response.Content}");
            }
        }




        [Order(1)]
        [Test]
        public void CreateMovieWithRequiredFields_ShouldSucceed()
        {
            var movieInfo = new MovieDTO
            {
                Title = "Test Movie1",
                Description = "This is a test movie1."
            };
            var request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movieInfo);
            
            var response = client.Execute(request);
            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            // Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(createResponse.Movie, Is.TypeOf<MovieDTO>(), "Expected response to be of type MovieDTO.");
            Assert.That(createResponse.Msg, Is.EqualTo("Movie created successfully!"));
            Assert.That(createResponse.Movie.Id, Is.Not.Null);
            Assert.That(createResponse.Movie.Id, Is.Not.Empty);

            lastCreateadId = createResponse.Movie.Id;
        }


        [Order(2)]
        [Test]
        public void EditLastCreatedMovie_ShouldSucceed()
        {
            var editMovie = new MovieDTO
            {
                Title = "Edited Title",
                Description = "Edited Description."
            };
            var request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", lastCreateadId);
            request.AddJsonBody(editMovie);

            var response = client.Execute(request);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(editResponse.Msg, Is.EqualTo("Movie edited successfully!"));
        }

        [Order(3)]
        [Test]
        public void GetAllMovies_ShouldReturnNonEmptyArray()
        {
            var request = new RestRequest("/api/Catalog/All", Method.Get);
            var response = client.Execute(request);

            var moviesResponse = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(moviesResponse, Is.Not.Empty);
        }


        [Order(4)]
        [Test]
        public void DeleteLastCreatedMovie_ShouldSucceed()
        {
            var request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", lastCreateadId);

            var response = client.Execute(request);

            var moviesResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(moviesResponse.Msg, Is.EqualTo("Movie deleted successfully!"));
        }

        [Order(5)]
        [Test]
        public void CreateMovieWithoutRequiredFields_ShouldReturnBadRequest()
        {
            var movieInfo = new MovieDTO
            {
                Title = "",
                Description = ""
            };
            var request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movieInfo);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400.");
        }


        [Order(6)]
        [Test]
        public void EditNonExistingMovie_ShouldReturnBadRequest()
        {
            var nonExistingMovieId = "a";
            var movieInfo = new MovieDTO
            {
                Title = "12aa",
                Description = "aaa aa"
            };
            var request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", nonExistingMovieId);
            request.AddJsonBody(movieInfo);

            var response = client.Execute(request);

            //var moviesResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400.");
            Assert.That(response.Content, Is.EqualTo("{\"msg\":\"Unable to edit the movie! Check the movieId parameter or user verification!\"}"));
        }

        [Order(7)]
        [Test]
        public void DeleteNonExistingMovie_ShouldReturnBadRequest()
        {
            var nonExistingMovieId = "1";

            var request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", nonExistingMovieId);
            var response = client.Execute(request);

            //var moviesResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request.");
            Assert.That(response.Content, Is.EqualTo("{\"msg\":\"Unable to delete the movie! Check the movieId parameter or user verification!\"}"));
        }


        [OneTimeTearDown]

        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}
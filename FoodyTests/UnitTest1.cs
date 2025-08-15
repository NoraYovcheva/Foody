using FoodyTests.Models;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Diagnostics.Contracts;
using System.Net;
using System.Text.Json;


namespace FoodyTests
{
    [TestFixture]
    public class FoodyTests
    {
        private RestClient client;
        private static string createFoodId;
        private const string baseUrl = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:86";

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJtwToken("noraivo", "ivo1ivo1");

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        private string GetJtwToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);

            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString() ?? string.Empty;
        }

        [Order(1)]
        [Test]
        public void CreateNewFood_ShouldReturnCreated()
        {
            var food = new
            {
                Name = "New food",
                Description = "Test description",
                Url = ""
            };

            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(food);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            createFoodId = json.GetProperty("foodId").GetString() ?? string.Empty;

            Assert.That(createFoodId, Is.Not.Null.And.Not.Empty, "Food ID should not be null or empty");
        }

        [Order(2)]
        [Test]
        
        public void EditFoodTitle_ShouldReturnOk()
        {
            var changes = new[]
            {
                new { path = "/name", op = "replace", value = "Updated food name" }
            };

            var request = new RestRequest($"/api/Food/Edit/{createFoodId}", Method.Patch);
            request.AddJsonBody(changes);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Response status should be OK");
            Assert.That(response.Content, Does.Contain("Successfully edited"));
            
        }

        [Order(3)]
        [Test]

        public void GetAllFoods_ShouldReturnList()
        {
            var request = new RestRequest($"/api/Food/All", Method.Get);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Response status should be OK");
            var foods = JsonSerializer.Deserialize<List<object>>(response.Content);

            Assert.That(foods, Is.Not.Null.And.Not.Empty, "Food ID should not be null or empty");
        }

        [Order(4)]
        [Test]

        public void DeleteFood_ShouldReturnOk()
        {
            var request = new RestRequest($"/api/Food/Delete/{createFoodId}", Method.Delete);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Response status should be OK");
            Assert.That(response.Content, Does.Contain("Deleted successfully!"));
        }

        [Order(5)]
        [Test]

        public void CreateFoodWithoutRequiredFields__ShouldReturnBadRequest()
        {
            var food = new
            {
                Name = "",
                Description = ""
            };
            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(food);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Response status should be Bad Request");
        }

        [Order(6)]
        [Test]

        public void EditNonExistingFood_ReturnNotFound()
        {
            // string fakeId = "1234567890"; // Non-existing food ID
            var changes = new[]
            {
                new { path = "/name", op = "replace", value = "Updated food name" }
            };
            var request = new RestRequest("/api/Food/Edit/1234567890", Method.Patch);
            // var request = new RestRequest($"/api/Food/Edit/{fakeId}", Method.Patch);
            request.AddJsonBody(changes);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), "No food revues 1234567890");
        }

        [Order(7)]
        [Test]

        public void DeleteNonExistingFood_ReturnNotFound()
        {
            var request = new RestRequest("/api/Food/Delete/1234567890", Method.Delete);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Unable to delete this food revue!");
        }


        [OneTimeTearDown] 
        public void Cleanup()
        {
            client?.Dispose();
        }
       
  
    }

}    


using Microsoft.EntityFrameworkCore;
using ModelsLib;
using Server.Contexts;
using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;

using var listener = new HttpListener();

listener.Prefixes.Add(@"http://localhost:27001/");

var _cache = new ConcurrentDictionary<char, KeyValue>();

listener.Start();
while (true)
{
    var context = await listener.GetContextAsync();

    var request = context.Request;




    switch (request.HttpMethod)
    {
        case "GET":

            var response = context.Response;
            var key = request.QueryString["key"].ToCharArray()[0];


            try
            {
                if (_cache.TryGetValue(key, out var cachedValue))
                {
                    response.ContentType = "application/json";
                    response.Headers.Add("Content-Type", "text/plain");
                    response.Headers.Add("Content-Type", "text/html");
                    response.StatusCode = 200;

                    var writer = new StreamWriter(response.OutputStream);
                    var jsonStr = JsonSerializer.Serialize<KeyValue>(cachedValue);
                    await writer.WriteLineAsync(jsonStr);

                    writer.Close();
                }
                else
                {

                    var dbContext = new CacheDbContext();
                    var x = await dbContext.KeyValues.FindAsync(key);
                    if (x != null)
                    {
                        response.ContentType = "application/json";
                        response.Headers.Add("Content-Type", "text/plain");
                        response.Headers.Add("Content-Type", "text/html");
                        response.StatusCode = 200;


                        var writer = new StreamWriter(response.OutputStream);
                        var jsonStr = JsonSerializer.Serialize<KeyValue>(x);
                        await writer.WriteLineAsync(jsonStr);

                        writer.Close();
                        _cache.TryAdd(key, x);
                    }
                    else
                        response.StatusCode = (int)HttpStatusCode.NotFound;

                }
                response.Close();
            }
            catch (Exception)
            {

                throw;
            }


            break;

        case "POST":

            var stream = request.InputStream;
            var reader = new StreamReader(stream);

            var json = reader.ReadToEnd();

            var keyValue = JsonSerializer.Deserialize<KeyValue>(json);

            var response1 = context.Response;

            try
            {

                var dbContext1 = new CacheDbContext();

                if (dbContext1.Find<KeyValue>(keyValue.Key) == null)
                {
                    dbContext1.Add(keyValue);
                    dbContext1.SaveChanges();

                    _cache.TryAdd(keyValue.Key, keyValue);
                    response1.StatusCode = (int)HttpStatusCode.OK;
                }
                else
                    response1.StatusCode = (int)HttpStatusCode.Found;

                response1.Close();

            }
            catch (Exception)
            {

                throw;
            }
            break;
        case "PUT":

            var streamPut = request.InputStream;
            var readerPut = new StreamReader(streamPut);

            var jsonPut = readerPut.ReadToEnd();

            Console.WriteLine(jsonPut);

            var keyValuePut = JsonSerializer.Deserialize<KeyValue>(jsonPut);

            var responsePut = context.Response;

            try
            {
                var dbContextPut = new CacheDbContext();
                var c = dbContextPut.KeyValues.Find(keyValuePut.Key);
                if (c != null)
                {
                    c.Value = keyValuePut.Value;
                    dbContextPut.SaveChanges();
                    _cache[keyValuePut.Key] = keyValuePut;
                    responsePut.StatusCode = (int)HttpStatusCode.OK;
                }
                else
                    responsePut.StatusCode = (int)HttpStatusCode.NotFound;

                responsePut.Close();

            }
            catch (Exception)
            {

                throw;
            }
            break;
        case "DELETE":
            var responseDelete = context.Response;
            var keyDelete = request.QueryString["key"].ToCharArray()[0];
            try
            {
                using (var dbContextDelete = new CacheDbContext())
                {
                    var entityToDelete = await dbContextDelete.KeyValues.FindAsync(keyDelete);
                    if (entityToDelete != null)
                    {
                        dbContextDelete.KeyValues.Remove(entityToDelete);
                        dbContextDelete.SaveChanges();
                        _cache.TryRemove(keyDelete, out _);
                        responseDelete.StatusCode = (int)HttpStatusCode.OK;
                    }
                    else
                        responseDelete.StatusCode = (int)HttpStatusCode.Found;
                }
                responseDelete.Close();
            }
            catch (Exception)
            {

                throw;
            }

            break;
    }


}


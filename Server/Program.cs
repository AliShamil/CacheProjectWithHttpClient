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

    var requestGet = context.Request;

    switch (requestGet.HttpMethod)
    {
        case "GET":

            var responseGet = context.Response;
            var key = requestGet.QueryString["key"].ToCharArray()[0];

            try
            {
                if (_cache.TryGetValue(key, out var cachedValue))
                {
                    responseGet.ContentType = "application/json";
                    responseGet.Headers.Add("Content-Type", "text/plain");
                    responseGet.Headers.Add("Content-Type", "text/html");
                    responseGet.StatusCode = 200;

                    var writer = new StreamWriter(responseGet.OutputStream);
                    var jsonStr = JsonSerializer.Serialize<KeyValue>(cachedValue);
                    await writer.WriteLineAsync(jsonStr);
                    writer.Close();
                    Console.WriteLine($@"Successfully sent to Client From Cache! ({DateTime.Now})");
                    Console.WriteLine();
                }
                else
                {

                    var dbContext = new CacheDbContext();
                    var x = await dbContext.KeyValues.FindAsync(key);
                    if (x != null)
                    {
                        responseGet.ContentType = "application/json";
                        responseGet.Headers.Add("Content-Type", "text/plain");
                        responseGet.Headers.Add("Content-Type", "text/html");
                        responseGet.StatusCode = 200;


                        var writer = new StreamWriter(responseGet.OutputStream);
                        var jsonGet = JsonSerializer.Serialize<KeyValue>(x);
                        await writer.WriteLineAsync(jsonGet);

                        writer.Close();
                        _cache.TryAdd(key, x);
                        Console.WriteLine($@"Successfully sent to Client From DB! ({DateTime.Now})");
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine($@"Not Founded! ({DateTime.Now})");
                        Console.WriteLine();
                        responseGet.StatusCode = (int)HttpStatusCode.NotFound;
                    }

                }
                responseGet.Close();
            }
            catch (Exception)
            {

                throw;
            }


            break;

        case "POST":

            var streamPost = requestGet.InputStream;
            var readerPost = new StreamReader(streamPost);

            var jsonPost = readerPost.ReadToEnd();

            var keyValue = JsonSerializer.Deserialize<KeyValue>(jsonPost);

            var responsePost = context.Response;

            try
            {

                var dbContextPost = new CacheDbContext();
                var dataPost = await dbContextPost.KeyValues.FindAsync(keyValue.Key);
                if (dataPost == null)
                {
                    dbContextPost.Add(keyValue);
                    dbContextPost.SaveChanges();

                    _cache.TryAdd(keyValue.Key, keyValue);
                    responsePost.StatusCode = (int)HttpStatusCode.OK;
                    Console.WriteLine($@"Successfully Posted! ({DateTime.Now})");
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine($@"Not Founded! ({DateTime.Now})");
                    Console.WriteLine();
                    responsePost.StatusCode = (int)HttpStatusCode.Found;
                }

                responsePost.Close();

            }
            catch (Exception)
            {
                throw;
            }
            break;
        case "PUT":

            var streamPut = requestGet.InputStream;
            var readerPut = new StreamReader(streamPut);

            var jsonPut = readerPut.ReadToEnd();

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
                    Console.WriteLine($@"Successfully Putted! ({DateTime.Now})");
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine($"Not Founded! ({DateTime.Now})");
                    Console.WriteLine();
                    responsePut.StatusCode = (int)HttpStatusCode.NotFound;
                }

                responsePut.Close();
            }
            catch (Exception)
            {
                throw;
            }
            break;
        case "DELETE":
            var responseDelete = context.Response;
            var keyDelete = requestGet.QueryString["key"].ToCharArray()[0];
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
                        Console.WriteLine($@"Successfully Deleted! ({DateTime.Now})");
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine($@"Not Founded! ({DateTime.Now})");
                        Console.WriteLine();
                        responseDelete.StatusCode = (int)HttpStatusCode.Found;
                    }
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


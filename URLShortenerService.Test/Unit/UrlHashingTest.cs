using System;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using URLShortenerService.Hash;
using URLShortenerService.Models;
using System.Collections.Generic;

namespace URLShortenerService.Test;

public class UrlHashingTest
{
    private UrlHashingService? HashingService { get; set; }

    [SetUp]
    public void Setup()
    {
        HashingService = new UrlHashingService();
    }

    [Test]
    [Category("Unit")]
    public void TestEncodeDecodeOfIdInteger()
    {
        int id = 1234567;
        var encodedId = HashingService.Encode(id);
        Console.WriteLine($"encodedId: {encodedId}");

        var decodedId = HashingService.Decode(encodedId);
        Console.WriteLine($"decodedId: {decodedId}");

        Console.WriteLine($"decodedId: {decodedId} == id: {id}");
        Assert.AreEqual(decodedId, id);
    }

    [Test]
    [Category("Unit")]
    public void TestEncodeDecodeFor1000000Operations()
    {
        for (var i = 0; i < 1000000; i++)
        {
            var encodedId = HashingService.Encode(i);
            var decodedId = HashingService.Decode(encodedId);

            Console.WriteLine($"{i} => {encodedId}");

            if (HashingService.Decode(encodedId) != i)
            {
                Console.WriteLine("{0} != {1}", HashingService.Encode(i), i);
                break;
            }
        }

        Assert.Pass();
    }
}
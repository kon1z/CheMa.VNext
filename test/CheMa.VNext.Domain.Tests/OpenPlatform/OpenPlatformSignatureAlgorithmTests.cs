using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Shouldly;
using Xunit;

namespace CheMa.VNext.OpenPlatform;

public class OpenPlatformSignatureAlgorithmTests
{
    [Fact]
    public void ComputeBodyHash_Should_Return_Sha256_Hex()
    {
        var body = "{\"vehicleId\":\"A1001\"}";
        var bodyBytes = Encoding.UTF8.GetBytes(body);

        var bodyHash = OpenPlatformSignatureAlgorithm.ComputeBodyHash(bodyBytes);
        var expectedBodyHash = Convert.ToHexString(SHA256.HashData(bodyBytes)).ToLowerInvariant();

        bodyHash.ShouldBe(expectedBodyHash);
    }

    [Fact]
    public void BuildCanonicalString_Should_Sort_Query_And_Use_Newline_Format()
    {
        var query = new[]
        {
            new KeyValuePair<string, IEnumerable<string?>>("z", new[] { "last" }),
            new KeyValuePair<string, IEnumerable<string?>>("name", new[] { "che ma" }),
            new KeyValuePair<string, IEnumerable<string?>>("a", new[] { "1", "2" })
        };

        var canonical = OpenPlatformSignatureAlgorithm.BuildCanonicalString(
            "post",
            "/api/open/vehicles",
            query,
            "hash-value",
            "client-1",
            "1710000000",
            "nonce-1");

        canonical.ShouldBe("POST\n/api/open/vehicles\na=1&a=2&name=che%20ma&z=last\nhash-value\nclient-1\n1710000000\nnonce-1");
    }

    [Fact]
    public void ComputeSignature_Should_Return_Stable_Base64_HmacSha256()
    {
        var canonical = "POST\n/api/open/vehicles\na=1\nhash-value\nclient-1\n1710000000\nnonce-1";

        var signature = OpenPlatformSignatureAlgorithm.ComputeSignature("secret-123", canonical);

        signature.ShouldBe("MSqv1ax92+BDwDU/nLzjiECbX79lSvygr5EgwHESKxQ=");
    }

    [Fact]
    public void BuildCanonicalString_Should_Handle_Empty_Query_And_BodyHash()
    {
        var canonical = OpenPlatformSignatureAlgorithm.BuildCanonicalString(
            "get",
            "/api/open/orders",
            Array.Empty<KeyValuePair<string, IEnumerable<string?>>>(),
            OpenPlatformSignatureAlgorithm.ComputeBodyHash(Array.Empty<byte>()),
            "client-2",
            "1710001234",
            "nonce-2");

        canonical.ShouldBe("GET\n/api/open/orders\n\ne3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\nclient-2\n1710001234\nnonce-2");
    }

    [Fact]
    public void FixedTimeEquals_Should_Return_Expected_Result()
    {
        OpenPlatformSignatureAlgorithm.FixedTimeEquals("abc", "abc").ShouldBeTrue();
        OpenPlatformSignatureAlgorithm.FixedTimeEquals("abc", "abd").ShouldBeFalse();
    }
}

using OasisHyperDriveClient.Core.Models;

namespace OasisHyperDriveClient.Tests;

public class HolonViewModelTests
{
    [Theory]
    [InlineData("File",   "📄")]
    [InlineData("NFT",    "🖼")]
    [InlineData("GeoNFT", "🌍")]
    [InlineData("Avatar", "💠")]
    [InlineData("Keys",   "🔑")]
    [InlineData("Holon",  "🔷")]
    public void DisplayIcon_ReturnsCorrectEmoji(string holonType, string expectedIcon)
    {
        var vm = new HolonViewModel { HolonType = holonType };
        Assert.Equal(expectedIcon, vm.DisplayIcon);
    }

    [Theory]
    [InlineData(512,          "512 B")]
    [InlineData(2048,         "2.0 KB")]
    [InlineData(1536 * 1024,  "1.5 MB")]
    public void SizeDisplay_FormatsCorrectly(long bytes, string expected)
    {
        var vm = new HolonViewModel { SizeBytes = bytes };
        Assert.Equal(expected, vm.SizeDisplay);
    }

    [Fact]
    public void FromHolon_MapsAllFields()
    {
        var holon = new Holon
        {
            Id = Guid.NewGuid(),
            Name = "TestHolon",
            HolonType = "File",
            ProviderUniqueStorageKey = new Dictionary<string, string>
            {
                ["IPFS"] = "QmHash123",
                ["Holochain"] = "hc-abc"
            },
            CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ModifiedDate = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var vm = HolonViewModel.FromHolon(holon);

        Assert.Equal(holon.Id, vm.Id);
        Assert.Equal("TestHolon", vm.Name);
        Assert.Equal("File", vm.HolonType);
        Assert.Equal(2, vm.ReplicatedProviders.Count);
        Assert.Contains("IPFS", vm.ReplicatedProviders);
        Assert.Contains("Holochain", vm.ReplicatedProviders);
    }

    [Fact]
    public void FromHolon_DefaultsHolonType_WhenEmpty()
    {
        var holon = new Holon { HolonType = string.Empty };
        var vm = HolonViewModel.FromHolon(holon);
        Assert.Equal("Holon", vm.HolonType);
    }
}

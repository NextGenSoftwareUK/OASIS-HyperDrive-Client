using OasisHyperDriveClient.Core.Models;

namespace OasisHyperDriveClient.Core.Api;

public class DataService
{
    private readonly OasisApiClient _api;

    public DataService(OasisApiClient api)
    {
        _api = api;
    }

    public async Task<IReadOnlyList<Holon>> LoadAllHolonsAsync(
        string holonType = "All",
        string? provider = null,
        CancellationToken ct = default)
    {
        var result = await _api.PostAsync<List<Holon>>("api/data/load-all-holons",
            new { HolonType = holonType, Provider = provider }, ct);

        if (result.IsError || result.Result is null)
            return [];

        return result.Result;
    }

    public async Task<Holon?> LoadHolonAsync(Guid holonId, string? provider = null, CancellationToken ct = default)
    {
        var result = await _api.PostAsync<Holon>("api/data/load-holon",
            new { HolonId = holonId, ProviderTypeString = provider }, ct);

        return result.IsError ? null : result.Result;
    }

    public async Task<byte[]?> LoadFileAsync(Guid fileId, string? provider = null, CancellationToken ct = default)
    {
        var result = await _api.PostAsync<byte[]>("api/data/load-file",
            new { Id = fileId, Provider = provider }, ct);

        return result.IsError ? null : result.Result;
    }

    public async Task<Guid?> SaveFileAsync(
        byte[] data, string fileName, string fileExtension, string mimeType,
        Guid? avatarId = null, string? provider = null, CancellationToken ct = default)
    {
        var result = await _api.PostAsync<Guid>("api/data/save-file", new
        {
            Data = data,
            FileName = fileName,
            FileExtension = fileExtension,
            MimeType = mimeType,
            AvatarId = avatarId,
            Provider = provider
        }, ct);

        return result.IsError ? null : result.Result;
    }

    public async Task<Holon?> SaveHolonAsync(Holon holon, string? provider = null, CancellationToken ct = default)
    {
        var result = await _api.PostAsync<Holon>("api/data/save-holon",
            new { HolonId = holon.Id, Holon = holon, ProviderTypeString = provider }, ct);

        return result.IsError ? null : result.Result;
    }

    public async Task<bool> DeleteHolonAsync(Guid holonId, bool softDelete = true, CancellationToken ct = default)
    {
        var result = await _api.DeleteAsync<Holon>("api/data/delete-holon",
            new { Id = holonId, SoftDelete = softDelete }, ct);

        return !result.IsError;
    }

    public async Task<IReadOnlyList<Holon>> LoadHolonsForParentAsync(
        Guid parentId, string holonType = "All", CancellationToken ct = default)
    {
        var result = await _api.PostAsync<List<Holon>>("api/data/load-holons-for-parent",
            new { Id = parentId, HolonType = holonType }, ct);

        return result.IsError || result.Result is null ? [] : result.Result;
    }
}

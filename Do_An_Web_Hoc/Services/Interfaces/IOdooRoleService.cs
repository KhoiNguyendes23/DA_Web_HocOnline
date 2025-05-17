namespace Do_An_Web_Hoc.Services.Interfaces
{
    public interface IOdooRoleService
    {
        Task<int?> GetOrCreateRoleAsync(string roleName, int expectedId);
    }
}

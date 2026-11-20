using PokeHub.Modules.Users.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PokeHub.Modules.Users.Infrastructure.Users;

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");

        builder.HasKey(p => p.Code);

        builder.Property(p => p.Code).HasMaxLength(100);

        builder.HasData(
            Permission.GetUser,
            Permission.ModifyUser,
            Permission.ReadPokedex,
            Permission.UpdatePokedex,
            Permission.SyncSpecies,
            Permission.GrantPokemon,
            Permission.ReadTrades,
            Permission.UpdateTrades,
            Permission.ManageGames);

        builder
            .HasMany<Role>()
            .WithMany()
            .UsingEntity(joinBuilder =>
            {
                joinBuilder.ToTable("role_permissions");

                joinBuilder.HasData(
                    // Member permissions
                    CreateRolePermission(Role.Member, Permission.GetUser),
                    CreateRolePermission(Role.Member, Permission.ModifyUser),
                    CreateRolePermission(Role.Member, Permission.ReadPokedex),
                    CreateRolePermission(Role.Member, Permission.UpdatePokedex),
                    CreateRolePermission(Role.Member, Permission.ReadTrades),
                    CreateRolePermission(Role.Member, Permission.UpdateTrades),

                    // Admin permissions
                    CreateRolePermission(Role.Administrator, Permission.GetUser),
                    CreateRolePermission(Role.Administrator, Permission.ModifyUser),
                    CreateRolePermission(Role.Administrator, Permission.ReadPokedex),
                    CreateRolePermission(Role.Administrator, Permission.UpdatePokedex),
                    CreateRolePermission(Role.Administrator, Permission.SyncSpecies),
                    CreateRolePermission(Role.Administrator, Permission.GrantPokemon),
                    CreateRolePermission(Role.Administrator, Permission.ReadTrades),
                    CreateRolePermission(Role.Administrator, Permission.UpdateTrades),
                    CreateRolePermission(Role.Administrator, Permission.ManageGames)
                );
            });
    }

    private static object CreateRolePermission(Role role, Permission permission)
    {
        return new
        {
            RoleName = role.Name,
            PermissionCode = permission.Code,
        };
    }
}

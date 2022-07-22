using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CharityMS.Migrations
{
    public partial class SeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"USE [CharityMS_DB]");

            // Seed User and Role
            migrationBuilder.Sql($"INSERT [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'd4cfc221-cfd3-4dc0-a55d-bf3f26291cdb', N'Staff', N'STAFF', N'e199dc9d-b495-4bee-af06-ea72492c5d28')");
            migrationBuilder.Sql($"INSERT [dbo].[AspNetUsers] ([Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [FullName], [Address]) VALUES (N'07df0a49-2de5-4ce0-b9f8-010d3230caaf', N'admin@mail.com', N'ADMIN@MAIL.COM', N'admin@mail.com', N'ADMIN@MAIL.COM', 1, N'AQAAAAEAACcQAAAAEJhEgP4NNVRUfz9M/KXJdAm/A8Fw4W5t0P1jUuWp+fOUpSyHpxAa1dEt32n6+URlPA==', N'IXG4HN6NKA3RQZ5KOIPNLWASEXLL77NP', N'89bfdb11-50d2-45ea-a546-d4beae1d30bc', NULL, 0, 0, NULL, 1, 0, N'admin', N'admin')");
            migrationBuilder.Sql($"INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'07df0a49-2de5-4ce0-b9f8-010d3230caaf', N'd4cfc221-cfd3-4dc0-a55d-bf3f26291cdb')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
           
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EnterprisePMO_PWA.Infrastructure.Data.Migrations
{
    public partial class AddSupabaseIntegration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add SupabaseId column to Users table
            migrationBuilder.AddColumn<string>(
                name: "SupabaseId",
                table: "Users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
            
            // Create index on SupabaseId for faster lookups
            migrationBuilder.CreateIndex(
                name: "IX_Users_SupabaseId",
                table: "Users",
                column: "SupabaseId");
            
            // Check if Holding department exists
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM ""Departments"" WHERE ""Name"" = 'Holding') THEN
                        INSERT INTO ""Departments"" (""Id"", ""Name"", ""Description"", ""CreatedDate"", ""IsActive"")
                        VALUES (uuid_generate_v4(), 'Holding', 'Default department for new users', NOW(), TRUE);
                    END IF;
                END $$;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove index on SupabaseId
            migrationBuilder.DropIndex(
                name: "IX_Users_SupabaseId",
                table: "Users");
            
            // Remove SupabaseId column from Users table
            migrationBuilder.DropColumn(
                name: "SupabaseId",
                table: "Users");
            
            // Note: We don't delete the Holding department when rolling back
            // as it might be in use by existing users
        }
    }
}
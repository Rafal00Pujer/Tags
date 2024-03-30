using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Tags.Context;

#nullable disable

namespace Tags.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                $"CREATE FUNCTION {TagsContext.CalcuateCountPercentFunctionName}(@count DECIMAL(38,18))" +
                " RETURNS FLOAT" +
                " AS" +
                " BEGIN" +
                " declare @sum DECIMAL(38,18)" +
                $" select @sum = SUM(CAST(\"{TagsContext.TagCountColumnName}\" AS DECIMAL(38,18)))" +
                $" from {TagsContext.TagTableName}" +
                " RETURN CAST(((@count / @sum) * 100.0) AS FLOAT)" +
                " END");


            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    CountPercent = table.Column<float>(type: "real", nullable: false, computedColumnSql: "dbo.CalculateCountPercent(CAST(\"Count\" AS DECIMAL(38,18)))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.Sql($"DROP FUNCTION {TagsContext.CalcuateCountPercentFunctionName}");
        }
    }
}

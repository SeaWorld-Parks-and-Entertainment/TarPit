using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SEA.DET.TarPit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateRateLimiterSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "callers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    externalidentifier = table.Column<string>(name: "external_identifier", type: "text", nullable: false),
                    difficulty = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_callers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "call_records",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    callerid = table.Column<int>(name: "caller_id", type: "integer", nullable: false),
                    calledat = table.Column<Instant>(name: "called_at", type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    nonce = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_call_records", x => x.id);
                    table.ForeignKey(
                        name: "fk_call_records_callers_caller_id",
                        column: x => x.callerid,
                        principalTable: "callers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_call_records_caller_id_called_at",
                table: "call_records",
                columns: new[] { "caller_id", "called_at" });

            migrationBuilder.CreateIndex(
                name: "ix_call_records_caller_id_nonce",
                table: "call_records",
                columns: new[] { "caller_id", "nonce" });

            migrationBuilder.CreateIndex(
                name: "ix_callers_external_identifier",
                table: "callers",
                column: "external_identifier",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "call_records");

            migrationBuilder.DropTable(
                name: "callers");
        }
    }
}

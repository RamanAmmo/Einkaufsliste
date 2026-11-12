using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace einkaufslisterichtig.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Artikel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, collation: "NOCASE"),
                    Menge = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Einheit = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, collation: "NOCASE"),
                    Notiz = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, defaultValue: ""),
                    Erledigt = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artikel", x => x.Id);
                    table.CheckConstraint("CK_Artikel_Menge", "CAST(\"Menge\" AS REAL) >= 0.01 AND CAST(\"Menge\" AS REAL) <= 10000");
                });

            migrationBuilder.CreateTable(
                name: "Kategorien",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kategorien", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Zutaten",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zutaten", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Gerichte",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false, collation: "NOCASE"),
                    KategorieId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gerichte", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gerichte_Kategorien_KategorieId",
                        column: x => x.KategorieId,
                        principalTable: "Kategorien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GerichtZutaten",
                columns: table => new
                {
                    GerichtId = table.Column<int>(type: "INTEGER", nullable: false),
                    ZutatId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GerichtZutaten", x => new { x.GerichtId, x.ZutatId });
                    table.ForeignKey(
                        name: "FK_GerichtZutaten_Gerichte_GerichtId",
                        column: x => x.GerichtId,
                        principalTable: "Gerichte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GerichtZutaten_Zutaten_ZutatId",
                        column: x => x.ZutatId,
                        principalTable: "Zutaten",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Artikel",
                columns: new[] { "Id", "Einheit", "Erledigt", "Menge", "Name", "Notiz" },
                values: new object[,]
                {
                    { 1, "Liter", false, 2m, "Milch", "Vollmilch" },
                    { 2, "Stück", false, 1m, "Brot", "" },
                    { 3, "Stück", false, 6m, "Äpfel", "Jonagold" }
                });

            migrationBuilder.InsertData(
                table: "Kategorien",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Hauptgericht" },
                    { 2, "Dessert" },
                    { 3, "Frühstück" }
                });

            migrationBuilder.InsertData(
                table: "Zutaten",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Mehl" },
                    { 2, "Milch" },
                    { 3, "Eier" },
                    { 4, "Butter" },
                    { 5, "Spaghetti" },
                    { 6, "Hackfleisch" },
                    { 7, "Tomaten" },
                    { 8, "Zwiebeln" },
                    { 9, "Zucker" },
                    { 10, "Kakao" },
                    { 11, "Haferflocken" },
                    { 12, "Bananen" }
                });

            migrationBuilder.InsertData(
                table: "Gerichte",
                columns: new[] { "Id", "KategorieId", "Name" },
                values: new object[,]
                {
                    { 1, 3, "Pfannkuchen" },
                    { 2, 1, "Spaghetti Bolognese" },
                    { 3, 3, "Porridge" },
                    { 4, 2, "Schokopudding" }
                });

            migrationBuilder.InsertData(
                table: "GerichtZutaten",
                columns: new[] { "GerichtId", "ZutatId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 1, 3 },
                    { 1, 4 },
                    { 2, 5 },
                    { 2, 6 },
                    { 2, 7 },
                    { 2, 8 },
                    { 3, 2 },
                    { 3, 11 },
                    { 3, 12 },
                    { 4, 2 },
                    { 4, 9 },
                    { 4, 10 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Artikel_Name_Einheit",
                table: "Artikel",
                columns: new[] { "Name", "Einheit" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Gerichte_KategorieId",
                table: "Gerichte",
                column: "KategorieId");

            migrationBuilder.CreateIndex(
                name: "IX_Gerichte_Name",
                table: "Gerichte",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GerichtZutaten_ZutatId",
                table: "GerichtZutaten",
                column: "ZutatId");

            migrationBuilder.CreateIndex(
                name: "IX_Kategorien_Name",
                table: "Kategorien",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Zutaten_Name",
                table: "Zutaten",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Artikel");

            migrationBuilder.DropTable(
                name: "GerichtZutaten");

            migrationBuilder.DropTable(
                name: "Gerichte");

            migrationBuilder.DropTable(
                name: "Zutaten");

            migrationBuilder.DropTable(
                name: "Kategorien");
        }
    }
}
